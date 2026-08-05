#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA1849 // Call async methods when in an async method

using FFCEI.DotNetStack.Compliance;
using FFCEI.DotNetStack.Configuration;
using FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;
using FFCEI.DotNetStack.DevelopmentTests.Library.MessageBroker;
using FFCEI.DotNetStack.DevelopmentTests.Library.MessageHandlers;
using FFCEI.DotNetStack.DevelopmentTests.Library.Models;
using FFCEI.DotNetStack.DevelopmentTests.Library.TestModels;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.EntityFramework.Configurations;
using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Logging;
using FFCEI.DotNetStack.Logging.Configurations;
using FFCEI.DotNetStack.MessageBroker;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Secrets;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.WebApi;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
        var logger = BootstrapLogger.Instance;

        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        var builder = Host.CreateDefaultBuilder();

        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        _ = builder.ConfigureServices((hostContext, services) =>
        {
            _ = services.AddEnvFiles(
                logger: logger);

#if TEST_EFCORE
            _ = services.AddSingleton(serviceProvider =>
            {
                var sqliteConnection = new SqliteConnection("DataSource=:memory:");

                sqliteConnection.Open();
                sqliteConnection.CreateCollation("UNICODE_CI_AI", (x, y) => string.CompareOrdinal(x, 0, y, 0, int.Max(x.Length, y.Length)));

                return sqliteConnection;
            });

            _ = services.AddEFCoreCachingViaEFSecondLevelCache(
                new EFCoreCachingSecondLevelCacheOptions(),
                logger: logger);

            _ = services.AddEFDataProtectionUsingStandardKeyContextDatabaseTableKeyStore(
                (serviceProvider, configureDbContextOptions) => { configureDbContextOptions.ConfigureDbContextConnection(serviceProvider); },
                configureOptions => { },
                logger: logger);

            _ = services.AddDockerSecretManagerRepository(
                logger: logger);

            _ = services.AddDbContextFactoryAndRepositoryContainer<BloggingContext, BloggingEntityModelRepositoryContainer>(
                (serviceProvider, configureDbContextOptions) => { configureDbContextOptions.ConfigureDbContextConnection(serviceProvider); },
                logger: logger);
#endif
#if TEST_MESSAGE_BROKER
            _ = services.AddMessageBrokerConnectorFactory(
                MessageBrokerSettings.Default,
                factory => factory.ConfigureCallbacks(callbacks =>
                {
                    callbacks.OnUnknownMessageReceived += (envelope) =>
                    {
                        Console.WriteLine($"Receiver unknown message envelope: {JsonSerializer.FormattedForScreenInstance.SerializedToString(envelope)}");

                        BlogRowMessageHandler.IncrementReceived();

                        if (BlogRowMessageHandler.Received == 2)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Messages received...");
                        }
                    };

                    callbacks.OnMessageHandlingException += (e, envelope) =>
                    {
                        Console.WriteLine($"Receiver handling error: {e}\nMessage envelope: {JsonSerializer.FormattedForScreenInstance.SerializedToString(envelope)}");

                        BlogRowMessageHandler.IncrementReceived();

                        if (BlogRowMessageHandler.Received == 2)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Messages received...");
                        }

                        return true;
                    };

                    callbacks.OnUnhandledMessage += (envelope) =>
                    {
                        Console.WriteLine($"Receiver unhandled message envelope: {JsonSerializer.FormattedForScreenInstance.SerializedToString(envelope)}");

                        BlogRowMessageHandler.IncrementReceived();

                        if (BlogRowMessageHandler.Received == 2)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Messages received...");
                        }
                    };
                })
                , logger: logger);

#endif
            _ = services.AddComplianceRedaction(
                logger: logger);

            _ = services.AddLoggerFactoryUsingSerilog(
                configuration: new LoggerFactoryConfiguration()
                {
                    Console = new(),
                    Debugger = new()
                },
                logger: logger);

            _ = services.ReplicateStandardLoggerFilters();
        });

        _ = builder.AddForwardedHeaders();

        var app = builder.Build();

        ArgumentNullException.ThrowIfNull(app, nameof(app));

#if TEST_MESSAGE_BROKER
        await app.Services.UseMessageBrokerConnectorsAsync();
#endif

#if TEST_EFCORE
#if TEST_EFCORE_MAPPINGS
        MappingRules.CheckRules();
#endif

        app.Services.UseDataProtection();

        using (var repository = app.Services.GetRequiredService<BloggingEntityModelRepositoryContainer>())
#endif
        {
#if TEST_EFCORE
            var provider = repository.DatabaseProvider.ToString();
            var providerName = repository.DatabaseProviderName;

            Console.WriteLine($"Using database provider: {provider} (name: {providerName})");

            ExceptionProtector.ExecuteAction(() => repository.EnsureDeleted());

#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Database tables dropped!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            ExceptionProtector.ExecuteAction(() => repository.EnsureCreated());

            app.Services.EnsureDataProtectionTableCreated();

#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Database tables created!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            try
            {
                await repository.ExecuteSqlRawAsync("CREATE VIEW FullMappingView AS SELECT * FROM FullMapping");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                ExceptionProtector.ExecuteAction(() => repository.ExecuteSqlRaw("CREATE VIEW \"FullMappingView\" AS SELECT * FROM \"FullMapping\""));
            }

#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Database views created!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            /*
            await repository.MigrateAsync();

            Console.WriteLine("Database tables migrated!");
            */

            _ = Console.ReadLine();
#endif

#if TEST_EFCORE_MAPPINGS
            {
#if TEST_EFCORE
                var fullMappingQueried = await repository.FullMappingModels.AsNoTracking().FirstOrDefaultAsync();
#endif
                var fullMappingAdded = new FullMapping();

                fullMappingAdded.Fill();

#if TEST_EFCORE
                await repository.FullMappingModels.AddNewAsync(fullMappingAdded);

                fullMappingQueried = await repository.FullMappingModels.AsNoTracking().FirstAsync();
#else
        var fullMappingQueried = fullMappingAdded;
#endif

                Console.WriteLine("");
                Console.WriteLine($"Added: {JsonSerializer.FormattedForScreenInstance.SerializedToString(fullMappingAdded)}");
                Console.WriteLine($"Queried: {JsonSerializer.FormattedForScreenInstance.SerializedToString(fullMappingQueried)}");

#if TEST_EFCORE
                try
                {
                    var fullMappingViewQueried = await repository.FullMappingViewModels.FirstOrDefaultAsync();

                    Console.WriteLine($"View: {JsonSerializer.FormattedForScreenInstance.SerializedToString(fullMappingViewQueried)}");
                }
                catch
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    else
                    {
                        throw;
                    }
                }
#endif

                Console.WriteLine("");
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                Console.WriteLine("FullMapping test done!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

                _ = Console.ReadLine();
            }
#endif

            var blogObj = new Blog { Name = "Tech Tips", Url = "http://techtips.com" };
            var blogRow = new BlogRow() { Blog = blogObj, LineText = "Welcome to Tech Tips!" };

#if TEST_MESSAGE_BROKER
            {
                MessageHandlerRegistry.Global.LoadAssemblyHandlerTypes(Assembly.GetExecutingAssembly());

                var blogRowMsg = new BlogRow();

                blogRowMsg.CopyEntityModelPropertiesFrom(blogRow);

                var messageBrokerConnectorFactory = app.Services.GetRequiredService<IMessageBrokerConnectorFactory>();

                Console.WriteLine($"Message to send: {JsonSerializer.FormattedForScreenInstance.SerializedToString(blogRowMsg)}");

                await using (var sender = await messageBrokerConnectorFactory.GetDefaultSenderAsync(MessageBrokerSettings.TestTargetAddress))
                {
                    await sender.SendAsync(blogRowMsg);

                    sender.Send(blogRowMsg);

                    Console.WriteLine("");
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                    Console.WriteLine("Messages sent...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                }

                while (BlogRowMessageHandler.Received != 2)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                }

                Console.WriteLine("");
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                Console.WriteLine("MessageBroker tests done");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

                _ = Console.ReadLine();
            }
#endif

#if TEST_EFCORE
            await repository.Blogs.AddNewAsync(blogObj, saveChanges: false);
            await repository.BlogRows.AddNewAsync(blogRow, saveChanges: true);

            var blogRowCopy1 = new BlogRow();

            blogRowCopy1.CopyEntityModelPropertiesFrom(blogRow, copyEntityModelIds: false);

            var blogRowCopy2 = new BlogRow();

            blogRowCopy2.CopyEntityModelPropertiesFrom(blogRow, copyEntityModelIds: true);

            var blogObjCopy1 = new Blog();

            blogObjCopy1.CopyEntityModelPropertiesFrom(blogObj, copyEntityModelIds: false);

            var blogObjCopy2 = new Blog();

            blogObjCopy2.CopyEntityModelPropertiesFrom(blogObj, copyEntityModelIds: true);

            blogObj = new Blog { Name = "Coding Daily", Url = "http://codingdaily.com" };

            await repository.Blogs.AddNewAsync(blogObj, saveChanges: false);

            blogRow = new BlogRow() { Blog = blogObj, LineText = "Welcome to Coding Daily!" };

            await repository.BlogRows.AddNewAsync(blogRow, saveChanges: false);

            _ = await repository.SaveChangesAsync(acceptAllChangesOnSuccess: true);

            var blogRow2 = new BlogRow() { LineText = "Welcome to Coding Daily 2!" };

            await repository.BlogRows.AddNewAsync(blogRow2, saveChanges: true);

            blogObj = new Blog { Name = "Coding Daily 3", Url = "http://codingdaily.com" };

            await repository.Blogs.AddNewAsync(blogObj);

            blogRow = new BlogRow() { Blog = blogObj, LineText = "Welcome to Coding Daily 3!", LineRow = 1 };

            await repository.BlogRows.AddNewAsync(blogRow);

            var blogRowR1 = blogRow;

            blogRow = new BlogRow() { Blog = blogObj, LineText = "Welcome to Coding Daily 4!", LineRow = 2 };

            await repository.BlogRows.AddNewAsync(blogRow);

            var blogRowR2 = blogRow;

            await repository.BlogRows.DeleteExistingAsync(blogRowR1);

            blogRow = new BlogRow() { Blog = blogObj, LineText = "Welcome to Coding Daily 3!", LineRow = 1 };

            await repository.BlogRows.AddNewAsync(blogRow, false);

            /*
            await repository.BlogRows.SaveChangesAsync();
            */

            _ = repository.BlogRows.SaveChanges();

            /*
            blogRow = await repository.BlogRows.FindExistingByUniqueKeyAsync(blogRow) as BlogRow;
            */

            blogRow.BlogId = 1;

            await repository.BlogRows.UpdateExistingAsync(blogRow);

            blogRow.BlogId = 2;

            await repository.BlogRows.UpdateExistingAsync(blogRow);

            blogRow.Blog = blogObj;

            await repository.BlogRows.UpdateExistingAsync(blogRow);

            await repository.Blogs.ReloadExistingAsync(blogObj);

            _ = blogObj.BlogRows;

            await repository.Blogs.DeleteExistingAsync(blogObj);
            await repository.Blogs.LogicallyUndeleteExistingAsync(blogObj);
            await repository.Blogs.DeleteExistingAsync(blogObj);

            var blogObjCopy = new Blog();

            blogObjCopy.CopyEntityModelPropertiesFrom(blogObj);

            foreach (var row in blogObj.BlogRows)
            {
                var blogRowCopy = new BlogRow();

                blogRowCopy.CopyEntityModelPropertiesFrom(row);

                blogObjCopy.BlogRows.Add(blogRowCopy);
            }

            await repository.Blogs.AddNewAsync(blogObjCopy);

            await repository.Blogs.LogicallyDeleteExistingAsync(blogObjCopy);
            await repository.Blogs.PhysicallyDeleteExistingAsync(blogObjCopy);
#endif

            var test = new TestModel()
            {
                BoolProperty = true,
                Int8Property = (sbyte)FullMappingExtensionMethods.NextRandom(),
                UInt8Property = (byte)FullMappingExtensionMethods.NextRandom(),
                Int16Property = (short)FullMappingExtensionMethods.NextRandom(),
                UInt16Property = (ushort)FullMappingExtensionMethods.NextRandom(),
                Int32Property = FullMappingExtensionMethods.NextRandom(),
                UInt32Property = (uint)FullMappingExtensionMethods.NextRandom(),
                Int64Property = FullMappingExtensionMethods.NextRandom(),
                UInt64Property = (ulong)FullMappingExtensionMethods.NextRandom(),
                FloatProperty = (float)FullMappingExtensionMethods.NextDoubleRandom(),
                DoubleProperty = FullMappingExtensionMethods.NextDoubleRandom(),
                DecimalProperty = (decimal)FullMappingExtensionMethods.NextDoubleRandom(),
                DateOnlyProperty = DateOnly.FromDateTime(DateTime.FromOADate(FullMappingExtensionMethods.NextDoubleRandom())),
                TimeOnlyProperty = TimeOnly.FromDateTime(DateTime.FromOADate(FullMappingExtensionMethods.NextDoubleRandom())),
                DateTimeProperty = DateTime.FromOADate(FullMappingExtensionMethods.NextDoubleRandom()),
                DateTimeOffsetProperty = new DateTimeOffset(DateTime.FromOADate(FullMappingExtensionMethods.NextDoubleRandom()), TimeSpan.FromMinutes(-180)),
                TimeSpanProperty = TimeSpan.FromTicks(FullMappingExtensionMethods.NextRandom()),
                GuidProperty = Guid.CreateVersion7(),
                StringProperty = RandomStringGenerator.GenerateWithSubsets(RandomStringGeneratorSubsets.AsciiUppercase | RandomStringGeneratorSubsets.HexadecimalDigitsLowercase, 256),
                CharArrayProperty = RandomStringGenerator.GenerateWithSubsets(RandomStringGeneratorSubsets.AsciiLowercase | RandomStringGeneratorSubsets.HexadecimalDigitsUppercase, 256).ToCharArray(),
                ByteArrayProperty = Encoding.UTF8.GetBytes(RandomStringGenerator.GenerateWithSubsets(RandomStringGeneratorSubsets.Digits, 256))
            };

#if TEST_EFCORE
            await repository.TestModels.AddNewAsync(test);
#endif

            var ntest = new NullableTestModel();
#if TEST_EFCORE
            await repository.NullableTestModels.AddNewAsync(ntest);
#endif

            var likeString = "i";
            var myBlogs =
#if TEST_EFCORE
                await repository.Blogs.AsNoTracking().Include(b => b.BlogRows).Where(b => b.Name.Contains(likeString)).ToListAsync();

            myBlogs ??=
#endif
                new List<IBlog> { blogObj };

            Console.WriteLine("");

            foreach (var blog in myBlogs ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Found Blog: {JsonSerializer.FormattedForScreenInstance.SerializedToString(blog)}");
                Console.WriteLine("");
            }

            var myBlogRows =
#if TEST_EFCORE
                await repository.BlogRows.AsNoTracking().ToListAsync();

            myBlogRows ??=
#endif
                new List<IBlogRow> { blogRow };


            Console.WriteLine("");

            foreach (var myBlogRow in myBlogRows ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Found BlogRow: {JsonSerializer.FormattedForScreenInstance.SerializedToString(myBlogRow)}");
                Console.WriteLine("");
            }

#if TEST_EFCORE
            myBlogRows = await repository.BlogRows.ToListAsync();

            Console.WriteLine("");

            foreach (var myBlogRow in myBlogRows ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Found BlogRow: {JsonSerializer.FormattedForScreenInstance.SerializedToString(myBlogRow)}");
                Console.WriteLine("");
            }

            myBlogRows = await repository.BlogRows.Include(br => br.Blog).ToListAsync();

            Console.WriteLine("");

            foreach (var myBlogRow in myBlogRows ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Found BlogRow: {JsonSerializer.FormattedForScreenInstance.SerializedToString(myBlogRow)}");
                Console.WriteLine("");
            }
#endif

            var tests =
#if TEST_EFCORE
                await repository.TestModels.AsNoTracking().ToListAsync();

            tests ??=
#endif
                new List<ITestModel> { test };

            Console.WriteLine("");

            foreach (var ttest in tests ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Found test: {JsonSerializer.FormattedForScreenInstance.SerializedToString(ttest)}");
                Console.WriteLine("");
            }

            var ntests =
#if TEST_EFCORE
                await repository.NullableTestModels.AsNoTracking().ToListAsync();

            ntests ??=
#endif
                new List<INullableTestModel> { ntest };

            Console.WriteLine("");

            foreach (var nttest in ntests ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Found nullable test: {JsonSerializer.FormattedForScreenInstance.SerializedToString(nttest)}");
                Console.WriteLine("");
            }

#if TEST_EFCORE
            myBlogRows = await repository.BlogRows.AsNoTracking().Include(br => br.Blog).Where(br => br.BlogId == 1).ToListAsync();

            Console.WriteLine("");

            foreach (var myBlogRow in myBlogRows ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Queried BlogRow: {JsonSerializer.FormattedForScreenInstance.SerializedToString(myBlogRow)}");
                Console.WriteLine("");
            }

            myBlogRows = await repository.BlogRows.AsNoTracking().Where(br => br.BlogId == 1).ToListAsync();

            Console.WriteLine("");

            foreach (var myBlogRow in myBlogRows ?? [])
            {
                Console.WriteLine("");
                Console.WriteLine($"Queried BlogRow: {JsonSerializer.FormattedForScreenInstance.SerializedToString(myBlogRow)}");
                Console.WriteLine("");
            }
#endif
        }

        Console.WriteLine("");
#pragma warning disable CA1303 // Do not pass literals as localized parameters
        Console.WriteLine("Test finished");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

        _ = Console.ReadLine();
    }
}

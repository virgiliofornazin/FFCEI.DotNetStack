using FFCEI.DotNetStack.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.Models;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Url), nameof(Id), IsUnique = false, IsDescending = [false, true])]
public class Blog
   : UniqueIdExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel
#if TEST_EFCORE_USE_INTERFACES
    , IBlog
#endif
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Url { get; set; } = string.Empty;

    public virtual Collection<BlogRow> BlogRows { get; } = [];
#if TEST_EFCORE_USE_INTERFACES
    IReadOnlyCollection<IBlogRow> IBlog.BlogRows => BlogRows;
#endif
}

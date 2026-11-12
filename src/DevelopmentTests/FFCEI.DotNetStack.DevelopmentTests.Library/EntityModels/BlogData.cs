using FFCEI.DotNetStack.EntityModels;
using FFCEI.DotNetStack.EntityModels.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;

[Index(nameof(ExternalId), IsUnique = true)]
[ForeignKeyReference(nameof(Blog), ReverseNavigationPropertyName = nameof(Blog.BlogData), DeleteBehavior = DeleteBehavior.Cascade)]
public class BlogData
   : DatabaseUniqueIdLogicallyDeletableTimestampTrackedEntityModel
#if TEST_EFCORE_USE_INTERFACES
    , IBlogData
#endif
{
    public string ExternalId { get; set; } = string.Empty;
    public string DataText { get; set; } = string.Empty;

    /*
    [ForeignKey(nameof(Blog))]
    */
    public long? BlogId { get; set; }
    public virtual Blog? Blog { get; set; }
#if TEST_EFCORE_USE_INTERFACES
    IBlog? IBlogData.Blog => Blog;
#endif
}

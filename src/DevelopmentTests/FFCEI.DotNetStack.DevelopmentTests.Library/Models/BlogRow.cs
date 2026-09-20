using FFCEI.DotNetStack.Models;
using FFCEI.DotNetStack.Models.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.Models;

[Index(nameof(LineRow), IsUnique = true)]
[ForeignKeyReference(nameof(Blog), ReverseNavigationPropertyName = nameof(Blog.BlogRows), DeleteBehavior = DeleteBehavior.Cascade)]
public class BlogRow
   : DatabaseIdLogicallyDeletableTimestampTrackedEntityModel
#if TEST_EFCORE_USE_INTERFACES
    , IBlogRow
#endif
{
    public long? LineRow { get; set; }
    public string LineText { get; set; } = string.Empty;

    /*
    [ForeignKey(nameof(Blog))]
    */
    public long? BlogId { get; set; }
    public virtual Blog? Blog { get; set; }
#if TEST_EFCORE_USE_INTERFACES
    IBlog? IBlogRow.Blog => Blog;
#endif
}

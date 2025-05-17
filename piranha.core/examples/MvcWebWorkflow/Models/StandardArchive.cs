using Piranha.AttributeBuilder;
using Piranha.Extend;
using Piranha.Models;
using Piranha.Extend.Fields;

namespace MvcWebWorkflow.Models;

[PageType(Title = "Blog archive", UseBlocks = false)]
public class StandardArchive : Page<StandardArchive>
{
    /// <summary>
    /// The currently loaded post archive.
    /// </summary>
    public PostArchive<PostInfo> Archive { get; set; }
}

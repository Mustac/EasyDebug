using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterHunt.addons.easydebug;

public class TrackedProperty
{
    public WeakReference<Node> NodeInstance { get; }
    public TrackOptions Options { get; }
    public string PropertyName { get; }
    public Func<object> ValueAccessor { get; }
    public TreeItem UiItem { get; set; }

    public TrackedProperty(Node node, TrackOptions options, string propertyName, Func<object> accessor)
    {
        NodeInstance = new WeakReference<Node>(node);
        Options = options ?? new TrackOptions();
        PropertyName = propertyName;
        ValueAccessor = accessor;
    }
}

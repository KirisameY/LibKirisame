using Godot;

using KirisameLib.Extensions;

namespace KirisameLib.Godot.Extensions;

public static class NodeExtensions
{
    public static void RemoveChildren(this Node node) => node.GetChildren().ForEach(node.RemoveChild);

    public static void RemoveChildren(this Node node, IEnumerable<Node> children) => children.ForEach(node.RemoveChild);

    public static void RemoveChildren(this Node node, Func<Node, bool> predicate) =>
        node.GetChildren().Where(predicate).ForEach(node.RemoveChild);
}
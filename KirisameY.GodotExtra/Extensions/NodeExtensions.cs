using Godot;

using KirisameY.Relinq.Extensions;

namespace KirisameY.GodotExtra.Extensions;

public static class NodeExtensions
{
    extension(Node node)
    {
        public void RemoveChildren() => node.GetChildren().ForEach(node.RemoveChild);

        public void RemoveChildren(IEnumerable<Node> children) => children.ForEach(node.RemoveChild);

        public void RemoveChildren(Func<Node, bool> predicate) =>
            node.GetChildren().Where(predicate).ForEach(node.RemoveChild);
    }
}
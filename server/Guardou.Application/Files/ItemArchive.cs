namespace Guardou.Application.Files;

public sealed class ItemArchive
{
    internal ItemArchive(string name, IReadOnlyList<TreeNode> nodes)
    {
        Name = name;
        Nodes = nodes;
    }

    public string Name { get; }

    internal IReadOnlyList<TreeNode> Nodes { get; }
}

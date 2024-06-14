using System.Collections.Generic;
using System.Linq;

public class Paths
{
    public Paths(PathContainer[] paths)
    {
        List<PathContainer> IdentifiedPath = new List<PathContainer>();

        foreach (var pathGroup in paths.GroupBy(p => p.Path.Id).OrderBy(i => i.Key))
        {
            if (pathGroup.Count() != 1) continue;

            IdentifiedPath.Add(pathGroup.Single());
        }

        _paths = IdentifiedPath.ToDictionary(ip => ip.Path.Id);
    }

    public PathContainer GetPath(short id)
    {
        return _paths[id];
    }

    private Dictionary<short, PathContainer> _paths;
}

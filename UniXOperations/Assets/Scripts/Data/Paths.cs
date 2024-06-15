using System.Collections.Generic;
using System.Linq;

public class Paths
{
    public Paths(AbstractPath[] paths)
    {
        List<AbstractPath> IdentifiedPath = new List<AbstractPath>();

        foreach (var pathGroup in paths.GroupBy(p => p.Id).OrderBy(i => i.Key))
        {
            if (pathGroup.Count() != 1) continue;

            IdentifiedPath.Add(pathGroup.Single());
        }

        _paths = IdentifiedPath.ToDictionary(ip => ip.Id);
    }

    public AbstractPath GetPath(short id)
    {
        return _paths[id];
    }

    private Dictionary<short, AbstractPath> _paths;
}

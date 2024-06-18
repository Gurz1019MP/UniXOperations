using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

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
        if (_paths.ContainsKey(id))
        {
            return _paths[id];
        }
        else
        {
            return NoPath.Instance;
        }
    }

    private Dictionary<short, AbstractPath> _paths;
}

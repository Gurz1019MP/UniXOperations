using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class TransformExtension
{
    public static IEnumerable<Transform> GetDescendantsWithParent(this Transform root)
    {
        yield return root;

        for (int i = 0; i < root.childCount; i++)
        {
            foreach(var child in root.GetChild(i).GetDescendantsWithParent())
            {
                yield return child;
            }
        }
    }

    public static IEnumerable<Transform> GetChildrens(this Transform root)
    {
        for(int i = 0; i < root.childCount; i++)
        {
            yield return root.GetChild(i);
        }
    }
}

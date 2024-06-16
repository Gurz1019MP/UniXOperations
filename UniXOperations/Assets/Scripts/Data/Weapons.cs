using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class Weapons
{
    public Weapons(Weapon[] weapons)
    {
        _weapons = weapons.ToArray();
    }

    public void ForEach(UnityAction<Weapon> action)
    {
        foreach (Weapon weapon in _weapons)
        {
            action(weapon);
        }
    }

    private readonly IReadOnlyList<Weapon> _weapons;
}

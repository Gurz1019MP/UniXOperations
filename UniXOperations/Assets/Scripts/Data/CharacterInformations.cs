using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterInformations
{
    public CharacterInformations(IEnumerable<CharacterInformation> characterInfomations)
    {
        _characterInfomations = characterInfomations?.ToArray() ?? throw new ArgumentNullException();
    }

    public CharacterInformation Find(short id)
    {
        return _characterInfomations.SingleOrDefault(i => i.IsId(id)) ?? _characterInfomations.First();
    }

    private CharacterInformation[] _characterInfomations;
}

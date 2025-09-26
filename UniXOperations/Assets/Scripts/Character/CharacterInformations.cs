using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterInformations
{
    public CharacterInformations(IReadOnlyList<CharacterInformation> characterInfomations)
    {
        _characterInfomations = characterInfomations?.ToArray() ?? throw new ArgumentNullException();
    }

    public CharacterInformation Find(short id)
    {
        return _characterInfomations.SingleOrDefault(i => i.IsId(id));
    }

    private CharacterInformation[] _characterInfomations;
}

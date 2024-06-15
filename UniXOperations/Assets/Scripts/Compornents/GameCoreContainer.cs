using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XOPS;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class GameCoreContainer : MonoBehaviour
{
    public virtual void Initialize(MissionInformation missionInformation)
    {
        _gameCore = new GameCore(gameObject, LoadBlock, missionInformation, CreateCharacterContainer, GetComponent<MissionEventManager>());
    }

    private void LoadBlock(BlockData blockData)
    {
        gameObject.GetComponent<MeshFilter>().mesh = blockData.Mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = blockData.Mesh;
        gameObject.GetComponent<MeshRenderer>().materials = blockData.Materials;
    }

    private void CreateCharacterContainer(Character character, CharacterInformations characterInformations, Paths paths, Characters characters)
    {
        GameObject prefab = Instantiate(MissionPrefabProvider.Instance.PrefabCharacter, character.PointData.Position * transform.localScale.x + Vector3.up * 0.6f, character.PointData.Rotation);
        CharacterContainer container = prefab.GetComponent<CharacterContainer>();
        CharacterInformation info = characterInformations.Find(character.PointData.Data2);

        AIInputter inputter = new AIInputter(container, CharacterSpec.GetCharacterSpec(info.Kind).AISkill, paths, characters, FindCharacterContainer);
        character.SetInputterState(new InputterState(null, inputter));

        container.InitCharacterContainer(character);

        _characterContainers.Add(container);
    }

    protected CharacterContainer FindCharacterContainer(Character character)
    {
        return _characterContainers.SingleOrDefault(cc => cc.Character == character);
    }

    protected GameCore _gameCore;
    private List<CharacterContainer> _characterContainers = new List<CharacterContainer>();
}

using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.TextCore.Text;
using XOPS;

public class GameCoreContainer : MonoBehaviour
{
    public GameObject stage;

    public virtual void Initialize(MissionInformation missionInformation)
    {
        if (stage == null) throw new InvalidOperationException("Stageが設定されていません");

        _gameCore = new GameCore(missionInformation);
        LoadBlock(_gameCore.BlockData);
        _gameCore.Paths.ForEach(ScalePaths);
        _gameCore.Characters.ForEach(CreateCharacterContainer);
        _gameCore.Articles.ForEach(CreateArticleContainer);
        _gameCore.MissionEvents.ForEach(ScaleMissionEvent);
        _gameCore.Weapons.ForEach(CreateWeaponPickup);

        MissionEventManager missionEventManager = GetComponent<MissionEventManager>();
        if (missionEventManager != null)
        {
            missionEventManager.InitEvents(_gameCore.MissionEvents, _gameCore.Characters);
        }
    }

    private void LoadBlock(BlockData blockData)
    {
        stage.GetComponent<MeshFilter>().mesh = blockData.Mesh;
        stage.GetComponent<MeshCollider>().sharedMesh = blockData.Mesh;
        stage.GetComponent<MeshRenderer>().materials = blockData.Materials;
    }

    private void ScalePaths(AbstractPath abstractPath)
    {
        abstractPath.ScalePosition(stage.transform.localScale.x);
    }

    private void CreateCharacterContainer(Character character)
    {
        GameObject prefab = Instantiate(MissionPrefabProvider.Instance.PrefabCharacter, character.PointData.Position * stage.transform.localScale.x + Vector3.up * 0.6f, character.PointData.Rotation);
        CharacterContainer container = prefab.GetComponent<CharacterContainer>();
        CharacterInformation info = _gameCore.CharacterInformations.Find(character.PointData.Data2);

        container.InitCharacterContainer(character);

        AIInputter inputter = new AIInputter(container, CharacterSpec.GetCharacterSpec(info.Kind).AISkill, _gameCore.Paths, _gameCore.Characters, FindCharacterContainer);
        character.SetInputterState(new InputterState(null, inputter));

        _characterContainers.Add(container);
    }

    private void CreateArticleContainer(Article article)
    {
        GameObject prefab = Instantiate(MissionPrefabProvider.Instance.PrefabArticle, article.PointData.Position * stage.transform.localScale.x + Vector3.up * 0.6f, article.PointData.Rotation);
        ArticleContainer container = prefab.GetComponent<ArticleContainer>();

        container.InitArticleSpec(article);

        _articleContainers.Add(container);
    }

    private void ScaleMissionEvent(AbstractMissionEvent missionEvent)
    {
        missionEvent.ScalePosition(stage.transform.localScale.x);
    }

    private void CreateWeaponPickup(Weapon weapon)
    {
        GameObject prefab = Instantiate(MissionPrefabProvider.Instance.PrefabWeaponPickup, weapon.PointData.Position * stage.transform.localScale.x + Vector3.up * 0.6f, weapon.PointData.Rotation * Quaternion.Euler(0, 180, -90));
        WeaponPickup container = prefab.GetComponent<WeaponPickup>();

        container.Initialize(weapon);
    }

    protected CharacterContainer FindCharacterContainer(Character character)
    {
        return _characterContainers.SingleOrDefault(cc => cc.Character == character);
    }

    protected GameCore _gameCore;
    private List<CharacterContainer> _characterContainers = new List<CharacterContainer>();
    private List<ArticleContainer> _articleContainers = new List<ArticleContainer>();
}

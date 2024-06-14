using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine.UI;

public class Characters
{
    #region .ctor

    public Characters(CharacterContainer[] characters)
    {
        _characters = characters.ToList();

        foreach (var character in _characters)
        {
            character.OnDied.Subscribe(sender =>
            {
                _characters.Remove(sender);

                if (sender.Inputter is PlayerInputter)
                {
                    _onPlayerDiedSubject.OnNext(Unit.Default);
                    _onPlayerDiedSubject.OnCompleted();
                    _diedPlayerCombatStatistics = sender.CombatStatistics;
                }

                if (_characters.Select(p => p.Team).Distinct().Count() == 1)
                {
                    _onAllEnemyEliminatedSubject.OnNext(Unit.Default);
                    _onAllEnemyEliminatedSubject.OnCompleted();
                }
            }).AddTo(character.gameObject);
        }
    }

    #endregion

    #region Property

    public CharacterContainer Player => _characters.SingleOrDefault(c => c.ID == 0);

    public CombatStatistics PlayerCombatStatistics => _diedPlayerCombatStatistics == null ?
        _characters.Single(c => c.ID == 0).CombatStatistics : _diedPlayerCombatStatistics;

    public IObservable<Unit> OnPlayerDied => _onPlayerDiedSubject;

    public IObservable<Unit> OnAllEnemyEliminated => _onAllEnemyEliminatedSubject;

    #endregion

    #region Method

    public void AppendAIInputter(CharacterInformations characterInformations, Paths paths)
    {
        foreach(CharacterContainer character in _characters)
        {
            CharacterInformation info = characterInformations.Find(character.Point2);

            //AIInputterの生成
            var aiInputter = new AIInputter(character, CharacterSpec.GetAISkill(info.GetKind()), paths, this);

            // 各種コンポーネントの初期化
            character.InputterContainer.Initialize(aiInputter);
        }
    }

    public IReadOnlyList<CharacterContainer> WhereId(short id)
    {
        return _characters.Where(c => c.ID == id).ToArray();
    }

    public CharacterContainer GetCharacter(short id)
    {
        return WhereId(id).SingleOrDefault();
    }

    public int NextCharacter(int id)
    {
        return id == (_characters.Count - 1) ? 0 : id + 1;
    }

    public int PreCharacter(int id)
    {
        return id == 0 ? _characters.Count - 1 : id - 1;
    }

    public bool NoOneId(short id)
    {
        return !WhereId(id).Any();
    }

    public IEnumerable<CharacterContainer> Enemies(Team team)
    {
        return _characters.Where(c => c.Team != team).Where(c => c.gameObject != null);
    }

    public int GetIndex(CharacterContainer character)
    {
        return _characters.IndexOf(character);
    }

    public CharacterContainer GetCharacterAtIndex(int index)
    {
        return _characters[index];
    }

    #endregion

    #region Field

    private readonly List<CharacterContainer> _characters;
    private CombatStatistics _diedPlayerCombatStatistics;
    private Subject<Unit> _onPlayerDiedSubject = new Subject<Unit>();
    private Subject<Unit> _onAllEnemyEliminatedSubject = new Subject<Unit>();

    #endregion
}

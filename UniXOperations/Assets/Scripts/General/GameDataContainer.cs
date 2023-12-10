using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UniRx;

[Serializable]
public class GameDataContainer
{
    public IReadOnlyList<Character> Characters
    {
        get { return _characters; }
    }

    public IReadOnlyList<ArticleContainer> Articles
    {
        get { return _articles; }
    }

    public IReadOnlyList<MissionEventContainer> MissionEvents { get; private set; }

    public CombatStatistics PlayerCombatStatistics
    {
        get
        {
            if (_diedPlayerCombatStatistics == null)
            {
                return Characters.Single(c => c.ID == 0).CombatStatistics;
            }
            else
            {
                return _diedPlayerCombatStatistics;
            }
        }
    }

    public IReadOnlyDictionary<short, string> EventMessasge
    {
        get { return _eventMessage; }
    }

    public IObservable<Unit> OnPlayerDied => _onPlayerDiedSubject;

    public IObservable<Unit> OnAllEnemyEliminated => _onAllEnemyEliminatedSubject;

    public void InitPaths(PathContainer[] paths)
    {
        List<PathContainer> IdentifiedPath = new List<PathContainer>();

        foreach (var pathGroup in paths.GroupBy(p => p.Path.Id).OrderBy(i => i.Key))
        {
            if (pathGroup.Count() != 1) continue;

            IdentifiedPath.Add(pathGroup.Single());
        }

        _paths = IdentifiedPath.ToDictionary(ip => ip.Path.Id);
    }

    public void InitCharacters(Character[] characters)
    {
        _characters = characters.ToList();

        foreach (var character in Characters)
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

                if (Characters.Select(p => p.Team).Distinct().Count() == 1)
                {
                    _onAllEnemyEliminatedSubject.OnNext(Unit.Default);
                    _onAllEnemyEliminatedSubject.OnCompleted();
                }
            }).AddTo(character.gameObject);
        }
    }

    public void InitArticles(ArticleContainer[] articles)
    {
        _articles = articles.ToList();

        foreach (var article in Articles)
        {
            article.OnDestroyed.Subscribe(sender =>
            {
                _articles.Remove(sender);
            }).AddTo(article.gameObject);
        }
    }

    public void InitMissionEvents(MissionEventContainer[] missionEvents)
    {
        MissionEvents = missionEvents;
    }

    public void InitEventMessage(string[] messages)
    {
        _eventMessage = messages.Select((m, i) => new { id = i, message = m }).ToDictionary(a => (short)a.id, a => a.message);
    }

    public PathContainer GetPath(short id)
    {
        if (_paths == null) throw new InvalidOperationException("パスマネージャが初期化されていません。");

        return _paths[id];
    }

    private List<Character> _characters;
    private Dictionary<short, PathContainer> _paths;
    private List<ArticleContainer> _articles;
    private CombatStatistics _diedPlayerCombatStatistics;
    private Dictionary<short, string> _eventMessage;
    private Subject<Unit> _onPlayerDiedSubject = new Subject<Unit>();
    private Subject<Unit> _onAllEnemyEliminatedSubject = new Subject<Unit>();
}

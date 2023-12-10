public interface IAIBehaviorData
{
    Character CharacterState { get; }
    GameDataContainer GameDataContainer { get; }
    AISkill AISkill { get; }
    AIController Controller { get; }
    PathContainer FirstPath { get; }
}

public class AIBehaviorData : IAIBehaviorData
{
    public AIBehaviorData(AIController controller, Character characterState, PathContainer firstPath, GameDataContainer gameDataContainer, AISkill aISkill)
    {
        CharacterState = characterState;
        GameDataContainer = gameDataContainer;
        AISkill = aISkill;
        Controller = controller;
        FirstPath = firstPath;
    }

    public Character CharacterState { get; }
    public GameDataContainer GameDataContainer { get; }
    public AISkill AISkill { get; }
    public AIController Controller { get; }
    public PathContainer FirstPath { get; }
}

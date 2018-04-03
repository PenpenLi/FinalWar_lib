namespace FinalWar
{
    public enum AuraType
    {
        FIX_INT,
        CAST_SKILL,
        SET_INT,
    }

    public enum AuraTarget
    {
        OWNER,
        OWNER_NEIGHBOUR_ALLY,
        OWNER_NEIGHBOUR_ENEMY,
        TRIGGER,
        TRIGGER_TARGET,
        OWNER_NEIGHBOUR,
        OWNER_ALLY,
        OWNER_ENEMY,
    }

    public enum AuraConditionCompare
    {
        NULL,
        EQUAL,
        BIGGER,
        SMALLER
    }

    public interface IAuraSDS
    {
        int GetID();
        string GetEventName();
        AuraType GetEffectType();
        int GetPriority();
        AuraTarget GetEffectTarget();
        AuraTarget GetTriggerTarget();
        AuraConditionCompare GetConditionCompare();
        Hero.HeroData[] GetConditionType();
        int[] GetConditionData();
        int GetEffectTargetNum();
        int[] GetEffectData();
        string[] GetRemoveEventNames();
    }
}
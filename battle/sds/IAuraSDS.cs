namespace FinalWar
{
    public enum AuraType
    {
        FIX_INT,
        FIX_BOOL,
        CAST_SKILL
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
        int GetPriority();
        AuraTarget GetTriggerTarget();
        AuraConditionCompare GetConditionCompare();
        Hero.HeroData[] GetConditionType();
        AuraTarget[] GetConditionTarget();
        int[] GetConditionData();
        AuraType GetEffectType();
        AuraTarget GetEffectTarget();
        AuraConditionCompare GetTargetConditionCompare();
        Hero.HeroData[] GetTargetConditionType();
        AuraTarget[] GetTargetConditionTarget();
        int[] GetTargetConditionData();
        int GetEffectTargetNum();
        int[] GetEffectData();
        string[] GetRemoveEventNames();
    }
}
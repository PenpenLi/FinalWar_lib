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
    }

    public enum AuraConditionCompare
    {
        NULL,
        EQUAL,
        BIGGER,
        SMALLER
    }

    public enum AuraConditionType
    {
        DATA,
        NOWHP,
        MAXHP,
        LEVEL,
        ATTACK,
        NEIGHBOUR_ALLY_NUM,
        NEIGHBOUR_ENEMY_NUM,
        NEIGHBOUR_NUM,
    }

    public interface IAuraSDS
    {
        string GetEventName();
        int GetPriority();
        AuraTarget GetTriggerTarget();
        AuraConditionCompare GetConditionCompare();
        AuraConditionType[] GetConditionType();
        AuraTarget[] GetConditionTarget();
        int[] GetConditionData();
        AuraType GetEffectType();
        AuraTarget[] GetEffectTarget();
        int[] GetEffectTargetNum();
        int GetEffectData();
        string[] GetRemoveEventNames();
        string GetDesc();
    }
}
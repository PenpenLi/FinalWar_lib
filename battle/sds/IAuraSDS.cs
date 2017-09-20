public enum AuraType
{
    FIX_INT,
    FIX_BOOL,
    CAST_SKILL
}

public enum AuraTarget
{
    NULL,
    OWNER,
    OWNER_NEIGHBOUR_ALLY,
    OWNER_NEIGHBOUR_ENEMY,
    TRIGGER,
    TRIGGER_TARGET
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
    NEIGHBOUR_ENEMY_NUM
}

public interface IAuraSDS
{
    string GetEventName();
    int GetEventPriority();
    AuraTarget GetTriggerTarget();
    AuraConditionCompare GetConditionCompare();
    AuraConditionType[] GetConditionType();
    AuraTarget[] GetConditionTarget();
    int[] GetConditionData();
    AuraType GetEffectType();
    AuraTarget GetEffectTarget();
    int GetEffectTargetNum();
    int[] GetEffectData();
}
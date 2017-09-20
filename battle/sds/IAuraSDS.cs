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
    AuraTarget GetAuraTrigger();
    AuraConditionCompare GetAuraConditionCompare();
    AuraConditionType[] GetAuraConditionType();
    AuraTarget[] GetAuraConditionTarget();
    int[] GetAuraConditionData();
    AuraType GetAuraType();
    AuraTarget GetAuraTarget();
    int GetAuraTargetNum();
    int[] GetAuraData();
}

public interface IAuraConditionSDS
{
    AuraConditionType GetConditionType();
    int GetConditionData();
    AuraTarget GetConditionTarget();
}

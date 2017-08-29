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

public enum AuraCondition
{
    NULL,
    INJURED,
    HEALTHY,
    LEVEL_HIGHER,
    LEVEL_LOWER,
}

public interface IAuraSDS
{
    string GetEventName();
    AuraTarget GetAuraTrigger();
    AuraCondition GetAuraCondition();
    AuraTarget[] GetAuraConditionTarget();
    int GetAuraConditionData();
    AuraType GetAuraType();
    AuraTarget GetAuraTarget();
    int[] GetAuraData();
}

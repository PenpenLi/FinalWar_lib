public enum AuraType
{
    FIX_INT,
    FIX_BOOL,
    CAST_SKILL
}

public enum AuraTarget
{
    NULL,
    SELF,
    ALLY,
    ENEMY,
    TRIGGER,
    TARGET
}

public enum AuraCondition
{
    NULL,
    INJURED,
    HEALTHY,
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

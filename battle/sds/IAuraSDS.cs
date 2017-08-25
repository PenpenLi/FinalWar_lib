public enum AuraType
{
    FIX_INT,
    FIX_BOOL,
    CAST_SKILL
}

public enum AuraTarget
{
    SELF,
    ALLY,
    ENEMY,
    TRIGGER
}

public enum AuraCondition
{
    INJURED,
}

public interface IAuraSDS
{
    string GetEventName();
    AuraCondition[] GetAuraCondition();
    int[] GetAuraConditionData();
    AuraType GetAuraType();
    AuraTarget GetAuraTarget();
    int[] GetAuraData();
}

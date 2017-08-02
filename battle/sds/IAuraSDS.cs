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

public interface IAuraSDS
{
    string GetEventName();
    AuraType GetAuraType();
    AuraTarget GetAuraTarget();
    int[] GetAuraData();
}

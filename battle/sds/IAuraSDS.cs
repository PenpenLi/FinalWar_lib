public enum AuraEffect
{
    FIX_ALLY_ATTACK,
    FIX_ALLY_SPEED,
    FIX_SELF_SPEED,
    FIX_SELF_ATTACK,
    FIX_ENEMY_CAN_MOVE,
    FIX_SELF_CAN_PIERCE_SHIELD,
    FIX_ENEMY_SPEED,
    FIX_ENEMY_ATTACK,
    FIX_ALLY_CAN_PIERCE_SHIELD,
}

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
    ENEMY
}

public interface IAuraSDS
{
    string GetEventName();
    bool GetIsEventWithUid();
    AuraType GetAuraType();
    AuraTarget GetAuraTarget();




    AuraEffect GetAuraEffect();
    int GetAuraData();
}

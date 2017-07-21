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

public interface IAuraSDS
{
    AuraEffect GetAuraEffect();
    int GetAuraData();
}

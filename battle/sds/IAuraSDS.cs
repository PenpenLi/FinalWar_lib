public enum AuraEffect
{
    FIX_ALLY_ATTACK,
    FIX_ALLY_SPEED,
    FIX_SELF_SPEED,
}

public interface IAuraSDS
{
    AuraEffect GetAuraEffect();
    int GetAuraData();
}

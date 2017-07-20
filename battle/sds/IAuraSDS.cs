public enum AuraEffect
{
    FIX_ATTACK,
    FIX_SPEED,
}

public interface IAuraSDS
{
    AuraEffect GetAuraEffect();
    int GetAuraData();
}

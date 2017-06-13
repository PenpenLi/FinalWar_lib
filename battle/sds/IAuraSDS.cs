public enum AuraEffect
{
    FIX_ATTACK,
}

public interface IAuraSDS
{
    AuraEffect GetAuraEffect();
    int GetAuraData();
}

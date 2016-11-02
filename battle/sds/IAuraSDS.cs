public enum AuraTarget
{
    ALLY,
    ENEMY
}

public enum AuraEffect
{
    FIX_ATTACK,
    FIX_SHOOT,
}

public interface IAuraSDS
{
    AuraTarget GetAuraTarget();
    AuraEffect GetAuraEffect();
    int[] GetAuraDatas();
}

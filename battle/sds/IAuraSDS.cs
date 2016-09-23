public enum AuraTarget
{
    ALLY,
    ENEMY
}

public enum AuraEffect
{
    FIX_ATTACK,
    FIX_SHOOT,
    FIX_COUNTER,
    FIX_DEFENSE,
}

public interface IAuraSDS
{
    AuraTarget GetAuraTarget();
    AuraEffect GetAuraEffect();
    float[] GetAuraDatas();
}

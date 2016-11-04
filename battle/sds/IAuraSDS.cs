public enum AuraTarget
{
    SELF,
    ALLY,
    ENEMY
}

public enum AuraEffect
{
    FIX_ATTACK,
    FIX_SHOOT,
    FIX_SHOOT_DAMAGE
}

public interface IAuraSDS
{
    AuraTarget GetAuraTarget();
    AuraEffect GetAuraEffect();
    int[] GetAuraDatas();
}

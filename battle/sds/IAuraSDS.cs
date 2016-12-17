public enum AuraTarget
{
    SELF,
    ALLY,
    ENEMY
}

public enum AuraEffect
{
    FIX_ATTACK,
    FIX_ABILITY,
    SILENT,
    DISABLE_RECOVER_SHIELD
}

public interface IAuraSDS
{
    AuraTarget GetAuraTarget();
    AuraEffect GetAuraEffect();
    int[] GetAuraDatas();
}

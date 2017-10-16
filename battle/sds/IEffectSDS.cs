public enum Effect
{
    DAMAGE,
    HP_CHANGE,
    SHIELD_CHANGE,
    SILENCE,
    CHANGE_HERO,
    ADD_MONEY,
    AURA,
}

public interface IEffectSDS
{
    Effect GetEffect();
    int GetEffectPriority();
    int[] GetData();
}

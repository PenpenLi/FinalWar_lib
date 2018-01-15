namespace FinalWar
{
    public enum Effect
    {
        NULL,
        DAMAGE,
        HP_CHANGE,
        SHIELD_CHANGE,
        CHANGE_HERO,
        ADD_MONEY,
        ADD_AURA,
        BE_CLEAN,
    }

    public interface IEffectSDS
    {
        Effect GetEffect();
        int GetPriority();
        int[] GetData();
    }
}
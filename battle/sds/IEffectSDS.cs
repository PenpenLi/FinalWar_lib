namespace FinalWar
{
    public enum Effect
    {
        DAMAGE,
        HP_CHANGE,
        SHIELD_CHANGE,
        CHANGE_HERO,
        ADD_MONEY,
        ADD_AURA,
        BE_CLEANED,
        BE_KILLED,
        FORCE_FEAR,
    }

    public interface IEffectSDS
    {
        Effect GetEffect();
        int GetPriority();
        AuraConditionCompare GetConditionCompare();
        Hero.HeroData[] GetConditionType();
        int[] GetConditionData();
        int[] GetData();
    }
}
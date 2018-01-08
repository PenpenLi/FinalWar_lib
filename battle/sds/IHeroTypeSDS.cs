namespace FinalWar
{
    public enum FearType
    {
        PROBABILITY,
        ALWAYS,
        NEVER
    }

    public interface IHeroTypeSDS
    {
        int GetID();

        int GetAttackSpeed();
        int GetDefenseSpeed();
        int GetSupportSpeed();

        int GetAttackTimes();
        int GetThread();
        int GetFearValue();

        FearType GetFearType();

        int GetFearAttackWeight();
        int GetFearDefenseWeight();
    }
}
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

        bool GetCanAttack();

        int GetAttackSpeed();
        int GetDefenseSpeed();
        int GetSupportSpeed();

        int GetRecoverShield();
        int GetAttackTimes();
        int GetThread();
        int GetFearValue();

        FearType GetFearType();

        int GetFearAttackWeight();
        int GetFearDefenseWeight();
    }
}
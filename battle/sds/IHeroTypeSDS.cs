namespace FinalWar
{
    public interface IHeroTypeSDS
    {
        int GetID();

        int GetAttackSpeed();
        int GetDefenseSpeed();
        int GetSupportSpeed();

        int GetRecoverShield();
        int GetAttackTimes();
        int GetCounterTimes();
        int GetThread();
        int GetFearValue();

        int GetFearAttackWeight();
        int GetFearDefenseWeight();
    }
}
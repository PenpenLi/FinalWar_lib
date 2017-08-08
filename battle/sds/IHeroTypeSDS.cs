public interface IHeroTypeSDS
{
    int GetID();
    bool GetCanDoAction();

    int GetAttackSpeed();
    int GetDefenseSpeed();
    int GetSupportSpeed();

    int GetAttackTimes();
    int GetThread();
    int GetSupportSpeedBonus();
}
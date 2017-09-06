public enum FearType
{
    NEVER,
    ALWAYS,
    CHANCE
}

public interface IHeroTypeSDS
{
    int GetID();

    int GetAttackSpeed();
    int GetDefenseSpeed();
    int GetSupportSpeed();

    int GetAttackTimes();
    int GetThread();
    int GetSupportSpeedBonus();

    FearType GetFearType();

    int GetFearAttackWeight();
    int GetFearDefenseWeight();
}
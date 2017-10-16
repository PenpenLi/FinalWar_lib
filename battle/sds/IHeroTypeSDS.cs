public enum FearType
{
    PROBABILITY,
    ALWAYS
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
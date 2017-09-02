public enum FearType
{
    NEVER,
    ATTACK,
    DEFENSE,
    SUPPORT,
    ALWAYS_DEFENSE
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
}
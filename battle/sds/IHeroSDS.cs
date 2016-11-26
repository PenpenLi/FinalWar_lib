public enum AbilityType
{
    Null,
    Shoot,
    Support,
    Counter
}

public interface IHeroSDS
{
    int GetID();
    int GetHp();
    int GetShield();
    int GetCost();
    bool GetCanControl();
    bool GetCanMove();
    int GetLevelUp();
    int GetAttack();
    AbilityType GetAbilityType();
    int GetAbilityData();
    int[] GetSkills();
    int[] GetAuras();
}


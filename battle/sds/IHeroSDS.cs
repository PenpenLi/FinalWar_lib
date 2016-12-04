public enum AbilityType
{
    Null,
    Shoot,
    Support,
    Counter,
    Root
}

public interface IHeroSDS
{
    int GetID();
    int GetHp();
    int GetShield();
    int GetCost();
    bool GetCanControl();
    int GetLevelUp();
    int GetAttack();
    AbilityType GetAbilityType();
    int[] GetSkills();
    int[] GetAuras();
}


public enum AttackType
{
    NULL,
    SHOOT,
    SUPPORT
}

public interface IHeroSDS
{
    int GetID();
    int GetHp();
    int GetShield();
    int GetCost();
    bool GetCanControl();
    bool GetCanMove();
    bool GetThreat();
    int GetLevelUp();
    int GetAttack();
    AttackType GetAttackType();
    int[] GetSkills();
    int[] GetAuras();
}


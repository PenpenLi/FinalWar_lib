public enum AttackType
{
    NORMAL,
    ATTACK_FIRST,
    ATTACK_ONLY
}

public interface IHeroTypeSDS
{
    bool GetCanDoAction();
    int GetThread();
    bool GetCanDoDamageWhenDefense();
    bool GetCanDoDamageWhenSupport();
    AttackType GetAttackType();
    bool GetCanLendDamageWhenSupport();
}
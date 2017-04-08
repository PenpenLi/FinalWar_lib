public enum AdditionAttackType
{
    NULL,
    SHOOT,
    THROW,
}

public interface IHeroTypeSDS
{
    bool GetCanDoAction();
    int GetThread();
    bool GetCanDoDamageWhenDefense();
    bool GetCanAddAbilityWhenDefense();
    bool GetCanAddAbilityWhenAttack();
    bool GetCanDoDamageWhenSupport();
    bool GetWillBeDamageByDefense();
    bool GetWillBeDamageBySupport();
    bool GetCanLendDamageWhenSupport();
    AdditionAttackType GetAdditionAttackType();
    bool GetCanDoAdditionAttackWhenNextToEnemy();
}
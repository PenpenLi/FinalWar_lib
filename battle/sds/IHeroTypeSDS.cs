public interface IHeroTypeSDS
{
    bool GetCanDoAction();
    int GetThread();
    bool GetCanDoDamageWhenDefense();
    bool GetCanDoDamageWhenSupport();
    bool GetWillBeDamageByDefense();
    bool GetWillBeDamageBySupport();
    bool GetCanLendDamageWhenSupport();
}
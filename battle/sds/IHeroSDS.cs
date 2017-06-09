public interface IHeroSDS
{
    int GetID();
    int GetHp();
    int GetShield();
    int GetCost();
    int GetAttack();
    int GetAttackTimes();
    int GetSkill();
    int[] GetAuras();
    IHeroTypeSDS GetHeroType();
}


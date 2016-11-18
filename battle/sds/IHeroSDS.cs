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
    int GetShoot();
    int[] GetSkills();
    int[] GetAuras();
}


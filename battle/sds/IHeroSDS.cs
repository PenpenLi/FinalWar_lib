namespace FinalWar
{
    public interface IHeroSDS
    {
        int GetID();
        int GetHp();
        int GetShield();
        int GetCost();
        int GetAttack();
        int[] GetShootSkills();
        int[] GetSupportSkills();
        int[] GetAuras();
        int[] GetFeatures();
        int[] GetEffects();
        IHeroTypeSDS GetHeroType();
    }
}
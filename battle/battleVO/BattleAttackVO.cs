namespace FinalWar
{
    public struct BattleAttackVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public BattleHeroEffectVO vo;

        public BattleAttackVO(int _pos, int _attacker, int _defender, BattleHeroEffectVO _vo)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            vo = _vo;
        }
    }
}

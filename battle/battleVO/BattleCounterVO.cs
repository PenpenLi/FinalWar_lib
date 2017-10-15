namespace FinalWar
{
    public struct BattleCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public BattleHeroEffectVO vo;

        public BattleCounterVO(int _pos, int _attacker, int _defender, BattleHeroEffectVO _vo)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            vo = _vo;
        }
    }
}

namespace FinalWar
{
    public struct BattleRushVO
    {
        public int attacker;
        public int stander;
        public BattleHeroEffectVO vo;

        public BattleRushVO(int _attacker, int _stander, BattleHeroEffectVO _vo)
        {
            attacker = _attacker;
            stander = _stander;
            vo = _vo;
        }
    }
}

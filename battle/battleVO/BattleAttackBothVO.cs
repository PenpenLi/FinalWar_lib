namespace FinalWar
{
    public struct BattleAttackBothVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public bool attackerShield;
        public bool defenderShield;
        public BattleHeroEffectVO attackVO;
        public BattleHeroEffectVO defenseVO;

        public BattleAttackBothVO(int _pos, int _attacker, int _defender, bool _attackerShield, bool _defenderShield, BattleHeroEffectVO _attackVO, BattleHeroEffectVO _defenseVO)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            attackerShield = _attackerShield;
            defenderShield = _defenderShield;
            attackVO = _attackVO;
            defenseVO = _defenseVO;
        }
    }
}

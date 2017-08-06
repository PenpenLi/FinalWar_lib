using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleAttackAndCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public int attackDamage;
        public int defenseDamage;
        public List<BattleHeroEffectVO> attackerEffectList;
        public List<BattleHeroEffectVO> defenderEffectList;

        public BattleAttackAndCounterVO(int _pos, int _attacker, int _defender, int _attackDamage, int _defenseDamage, List<BattleHeroEffectVO> _attackerEffectList, List<BattleHeroEffectVO> _defenderEffectList)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            attackDamage = _attackDamage;
            defenseDamage = _defenseDamage;
            attackerEffectList = _attackerEffectList;
            defenderEffectList = _defenderEffectList;
        }
    }
}

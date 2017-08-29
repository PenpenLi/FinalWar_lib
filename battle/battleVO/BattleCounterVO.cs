using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public int damage;
        public List<BattleHeroEffectVO> attackerEffectList;
        public List<BattleHeroEffectVO> defenderEffectList;

        public BattleCounterVO(int _pos, int _attacker, int _defender, int _damage, List<BattleHeroEffectVO> _attackerEffectList, List<BattleHeroEffectVO> _defenderEffectList)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            damage = _damage;
            attackerEffectList = _attackerEffectList;
            defenderEffectList = _defenderEffectList;
        }
    }
}

using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public int attacker;
        public int stander;
        public List<BattleHeroEffectVO> attackerEffectList;
        public List<BattleHeroEffectVO> standerEffectList;

        public BattleRushVO(int _attacker, int _stander, List<BattleHeroEffectVO> _attackerEffectList, List<BattleHeroEffectVO> _standerEffectList)
        {
            attacker = _attacker;
            stander = _stander;
            attackerEffectList = _attackerEffectList;
            standerEffectList = _standerEffectList;
        }
    }
}

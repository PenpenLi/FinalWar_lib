using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public int attacker;
        public int stander;
        public List<BattleHeroEffectVO> effectList;

        public BattleRushVO(int _attacker, int _stander, List<BattleHeroEffectVO> _effectList)
        {
            attacker = _attacker;
            stander = _stander;
            effectList = _effectList;
        }
    }
}

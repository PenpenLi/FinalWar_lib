using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleTriggerAuraVO
    {
        public int pos;
        public Dictionary<int, BattleHeroEffectVO> data;

        public BattleTriggerAuraVO(int _pos, Dictionary<int, BattleHeroEffectVO> _data)
        {
            pos = _pos;
            data = _data;
        }
    }
}

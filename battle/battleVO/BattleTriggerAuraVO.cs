using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleTriggerAuraVO
    {
        public int pos;
        public Dictionary<int, List<BattleHeroEffectVO>> data;

        public BattleTriggerAuraVO(int _pos, Dictionary<int, List<BattleHeroEffectVO>> _data)
        {
            pos = _pos;
            data = _data;
        }
    }
}

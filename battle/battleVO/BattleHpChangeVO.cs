using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleHpChangeVO
    {
        public List<int> pos;
        public List<int> hpChange;

        public BattleHpChangeVO(List<int> _pos, List<int> _hpChange)
        {
            pos = _pos;
            hpChange = _hpChange;
        }
    }
}

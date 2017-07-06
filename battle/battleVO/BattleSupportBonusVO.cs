using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleSupportBonusVO
    {
        public int pos;
        public List<int> supperters;

        public BattleSupportBonusVO(int _pos, List<int> _supporters)
        {
            pos = _pos;
            supperters = _supporters;
        }
    }
}

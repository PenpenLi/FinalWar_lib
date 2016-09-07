using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleDeathVO
    {
        public List<int> deads;
        public Dictionary<int, int> powerChange;

        public BattleDeathVO(List<int> _deads, Dictionary<int, int> _powerChange)
        {
            deads = _deads;
            powerChange = _powerChange;
        }
    }
}

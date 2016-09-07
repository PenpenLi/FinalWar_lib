using System.Collections.Generic;

namespace FinalWar
{
    public struct BattlePowerChangeVO
    {
        public Dictionary<int, int> powerChanges;

        public BattlePowerChangeVO(Dictionary<int, int> _powerChanges)
        {
            powerChanges = _powerChanges;
        }
    }
}

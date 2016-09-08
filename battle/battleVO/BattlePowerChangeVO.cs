using System.Collections.Generic;

namespace FinalWar
{
    public struct BattlePowerChangeVO
    {
        public List<int> pos;
        public List<int> powerChange;
        public List<bool> isDizz;

        public BattlePowerChangeVO(List<int> _pos, List<int> _powerChange, List<bool> _isDizz)
        {
            pos = _pos;
            powerChange = _powerChange;
            isDizz = _isDizz;
        }
    }
}

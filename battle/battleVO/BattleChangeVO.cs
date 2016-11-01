using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleChangeVO
    {
        public List<int> pos;
        public List<int> shieldChange;
        public List<int> hpChange;

        public BattleChangeVO(List<int> _pos, List<int> _shieldChange, List<int> _hpChange)
        {
            pos = _pos;
            shieldChange = _shieldChange;
            hpChange = _hpChange;
        }
    }
}

using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleMoveVO
    {
        public Dictionary<int, int> moves;
        public Dictionary<int, Dictionary<int, int>> powerChange;

        public BattleMoveVO(Dictionary<int, int> _moves, Dictionary<int, Dictionary<int, int>> _powerChange)
        {
            moves = _moves;
            powerChange = _powerChange;
        }
    }
}

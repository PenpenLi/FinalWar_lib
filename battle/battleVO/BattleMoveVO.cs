using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleMoveVO
    {
        public Dictionary<int, int> moves;

        public BattleMoveVO(Dictionary<int, int> _moves)
        {
            moves = _moves;
        }
    }
}

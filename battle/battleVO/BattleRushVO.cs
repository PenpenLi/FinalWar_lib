using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public List<KeyValuePair<int, int>> attackers;
        public int stander;

        public BattleRushVO(List<KeyValuePair<int, int>> _attackers, int _stander)
        {
            attackers = _attackers;
            stander = _stander;
        }
    }
}

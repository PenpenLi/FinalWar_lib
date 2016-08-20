using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleShootVO
    {
        public List<KeyValuePair<int, int>> shooters;
        public int stander;

        public BattleShootVO(List<KeyValuePair<int, int>> _shooters, int _stander)
        {
            shooters = _shooters;
            stander = _stander;
        }
    }
}

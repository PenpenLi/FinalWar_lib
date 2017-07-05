using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public List<int> attackers;
        public List<List<int>> helpers;
        public int stander;
        public int hpDamage;

        public BattleRushVO(List<int> _attackers, List<List<int>> _helpers, int _stander, int _hpDamage)
        {
            attackers = _attackers;
            helpers = _helpers;
            stander = _stander;
            hpDamage = _hpDamage;
        }
    }
}

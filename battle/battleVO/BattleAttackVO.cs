using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalWar
{
    public struct BattleAttackVO
    {
        public List<KeyValuePair<int, int>> attackers;
        public List<KeyValuePair<int, int>> supporters;
        public int defender;
        public int defenderDamage;

        public BattleAttackVO(List<KeyValuePair<int, int>> _attackers, List<KeyValuePair<int, int>> _supporters, int _defender, int _defenderDamage)
        {
            attackers = _attackers;
            supporters = _supporters;
            defender = _defender;
            defenderDamage = _defenderDamage;
        }
    }
}

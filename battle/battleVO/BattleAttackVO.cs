using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleAttackVO
    {
        public int attacker;
        public int defender;
        public int supporter;
        public List<int> attackerHelpers;
        public List<int> defenderHelpers;
        public int attackerDamage;
        public int defenderDamage;

        public BattleAttackVO(int _attacker, int _defender, int _supporter, List<int> _attackerHelpers, List<int> _defenderHelpers, int _attackerDamage, int _defenderDamage)
        {
            attacker = _attacker;
            defender = _defender;
            supporter = _supporter;
            attackerHelpers = _attackerHelpers;
            defenderHelpers = _defenderHelpers;
            attackerDamage = _attackerDamage;
            defenderDamage = _defenderDamage;
        }
    }
}

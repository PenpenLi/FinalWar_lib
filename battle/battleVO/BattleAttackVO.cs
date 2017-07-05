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
        public int attackerShieldDamage;
        public int attackerHpDamage;
        public int defenderShieldDamage;
        public int defenderHpDamage;

        public BattleAttackVO(int _attacker, int _defender, int _supporter, List<int> _attackerHelpers, List<int> _defenderHelpers, int _attackerShieldDamage, int _attackerHpDamage, int _defenderShieldDamage, int _defenderHpDamage)
        {
            attacker = _attacker;
            defender = _defender;
            supporter = _supporter;
            attackerHelpers = _attackerHelpers;
            defenderHelpers = _defenderHelpers;
            attackerShieldDamage = _attackerShieldDamage;
            attackerHpDamage = _attackerHpDamage;
            defenderShieldDamage = _defenderShieldDamage;
            defenderHpDamage = _defenderHpDamage;
        }
    }
}

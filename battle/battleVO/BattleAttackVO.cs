using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleAttackVO
    {
        public List<int> attackers;
        public List<int> supporters;
        public int defender;
        public List<int> attackersShieldDamage;
        public List<int> attackersHpDamage;
        public List<int> supportersShieldDamage;
        public List<int> supportersHpDamage;
        public int defenderShieldDamage;
        public int defenderHpDamage;

        public BattleAttackVO(List<int> _attackers, List<int> _supporters, int _defender, List<int> _attackersShieldDamage, List<int> _attackersHpDamage, List<int> _supportersShieldDamage, List<int> _supportersHpDamage, int _defenderShieldDamage, int _defenderHpDamage)
        {
            attackers = _attackers;
            supporters = _supporters;
            defender = _defender;
            attackersShieldDamage = _attackersShieldDamage;
            attackersHpDamage = _attackersHpDamage;
            supportersShieldDamage = _supportersShieldDamage;
            supportersHpDamage = _supportersHpDamage;
            defenderShieldDamage = _defenderShieldDamage;
            defenderHpDamage = _defenderHpDamage;
        }
    }
}

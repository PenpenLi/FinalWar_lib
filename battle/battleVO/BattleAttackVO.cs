using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleAttackVO
    {
        public List<int> attackers;
        public List<int> supporters;
        public int defender;
        public List<int> attackersDamage;
        public List<int> supportersDamage;
        public int defenderDamage;
        
        public BattleAttackVO(List<int> _attackers, List<int> _supporters, int _defender, List<int> _attackersDamage, List<int> _supportersDamage, int _defenderDamage)
        {
            attackers = _attackers;
            supporters = _supporters;
            defender = _defender;
            attackersDamage = _attackersDamage;
            supportersDamage = _supportersDamage;
            defenderDamage = _defenderDamage;
        }
    }
}

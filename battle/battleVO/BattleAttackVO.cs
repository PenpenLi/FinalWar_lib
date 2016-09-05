using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public List<int> attackersPowerChange;
        public List<int> supportersPowerChange;
        public int defenderPowerChange;

        public BattleAttackVO(List<int> _attackers, List<int> _supporters, int _defender, List<int> _attackersDamage, List<int> _supportersDamage, int _defenderDamage, List<int> _attackersPowerChange, List<int> _supportersPowerChange, int _defenderPowerChange)
        {
            attackers = _attackers;
            supporters = _supporters;
            defender = _defender;
            attackersDamage = _attackersDamage;
            supportersDamage = _supportersDamage;
            defenderDamage = _defenderDamage;
            attackersPowerChange = _attackersPowerChange;
            supportersPowerChange = _supportersPowerChange;
            defenderPowerChange = _defenderPowerChange;
        }
    }
}

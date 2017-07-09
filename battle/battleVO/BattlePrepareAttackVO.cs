using System.Collections.Generic;

namespace FinalWar
{
    public struct BattlePrepareAttackVO
    {
        public int pos;

        public int attacker;
        public List<int> attackerSupperters;
        public int attackerSpeed;

        public int defender;
        public List<int> defenderSupporters;
        public int defenderSpeed;

        public BattlePrepareAttackVO(int _pos, int _attacker, List<int> _attackerSupporters, int _attackerSpeed,int _defender, List<int> _defenderSupporters, int _defenderSpeed)
        {
            pos = _pos;

            attacker = _attacker;
            attackerSupperters = _attackerSupporters;
            attackerSpeed = _attackerSpeed;

            defender = _defender;
            defenderSupporters = _defenderSupporters;
            defenderSpeed = _defenderSpeed;
        }
    }
}

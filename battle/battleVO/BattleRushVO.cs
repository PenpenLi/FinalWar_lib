using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public List<int> attackers;
        public int stander;
        public int damage;
        public List<int> attackersPowerChange;
        public int standerPowerChange;

        public BattleRushVO(List<int> _attackers, int _stander, int _damage, List<int> _attackersPowerChange, int _standerPowerChange)
        {
            attackers = _attackers;
            stander = _stander;
            damage = _damage;
            attackersPowerChange = _attackersPowerChange;
            standerPowerChange = _standerPowerChange;
        }
    }
}

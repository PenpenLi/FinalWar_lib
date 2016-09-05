using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleShootVO
    {
        public List<int> shooters;
        public int stander;
        public int damage;
        public List<int> shootersPowerChange;
        public int standerPowerChange;

        public BattleShootVO(List<int> _shooters, int _stander, int _damage, List<int> _shootersPowerChange, int _standerPowerChange)
        {
            shooters = _shooters;
            stander = _stander;
            damage = _damage;
            shootersPowerChange = _shootersPowerChange;
            standerPowerChange = _standerPowerChange;
        }
    }
}

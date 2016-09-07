using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleShootVO
    {
        public List<int> shooters;
        public int stander;
        public int damage;
        
        public BattleShootVO(List<int> _shooters, int _stander, int _damage)
        {
            shooters = _shooters;
            stander = _stander;
            damage = _damage;
        }
    }
}

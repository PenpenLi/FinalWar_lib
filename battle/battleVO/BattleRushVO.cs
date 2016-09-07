using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public List<int> attackers;
        public int stander;
        public int damage;
        
        public BattleRushVO(List<int> _attackers, int _stander, int _damage)
        {
            attackers = _attackers;
            stander = _stander;
            damage = _damage;
        }
    }
}

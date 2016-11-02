using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleShootVO
    {
        public List<int> shooters;
        public int stander;
        public int shieldDamage;
        public int hpDamage;
        
        public BattleShootVO(List<int> _shooters, int _stander, int _shieldDamage, int _hpDamage)
        {
            shooters = _shooters;
            stander = _stander;
            shieldDamage = _shieldDamage;
            hpDamage = _hpDamage;
        }
    }
}

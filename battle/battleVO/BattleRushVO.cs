using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public List<int> attackers;
        public int stander;
        public int shieldDamage;
        public int hpDamage;

        public BattleRushVO(List<int> _attackers, int _stander, int _shieldDamage, int _hpDamage)
        {
            attackers = _attackers;
            stander = _stander;
            shieldDamage = _shieldDamage;
            hpDamage = _hpDamage;
        }
    }
}

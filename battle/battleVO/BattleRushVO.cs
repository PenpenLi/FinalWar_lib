using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleRushVO
    {
        public int attacker;
        public int stander;
        public int damage;

        public BattleRushVO(int _attacker, int _stander, int _damage)
        {
            attacker = _attacker;
            stander = _stander;
            damage = _damage;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalWar
{
    public struct BattleAttackVO
    {
        public int attacker;
        public int defender;
        public int supporter;
        public int damage;
        public int damageSelf;

        public BattleAttackVO(int _attacker, int _defender, int _supporter, int _damage, int _damageSelf)
        {
            attacker = _attacker;
            defender = _defender;
            supporter = _supporter;
            damage = _damage;
            damageSelf = _damageSelf;
        }
    }
}

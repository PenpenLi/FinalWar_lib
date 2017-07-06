using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleShootVO
    {
        public int shooter;
        public int stander;
        public int damage;

        public BattleShootVO(int _shooter, int _stander, int _damage)
        {
            shooter = _shooter;
            stander = _stander;
            damage = _damage;
        }
    }
}

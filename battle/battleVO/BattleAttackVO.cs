﻿namespace FinalWar
{
    public struct BattleAttackVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public int damage;

        public BattleAttackVO(int _pos, int _attacker, int _defender, int _damage)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            damage = _damage;
        }
    }
}

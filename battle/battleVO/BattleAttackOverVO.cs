﻿namespace FinalWar
{
    public struct BattleAttackOverVO
    {
        public int pos;
        public int attacker;
        public int defender;

        public BattleAttackOverVO(int _pos, int _attacker, int _defender)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
        }
    }
}
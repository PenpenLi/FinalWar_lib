namespace FinalWar
{
    public struct BattleAttackAndCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;

        public BattleAttackAndCounterVO(int _pos, int _attacker, int _defender)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
        }
    }
}

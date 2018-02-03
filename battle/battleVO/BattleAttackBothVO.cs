namespace FinalWar
{
    public struct BattleAttackBothVO
    {
        public int pos;
        public int attacker;
        public int defender;

        public BattleAttackBothVO(int _pos, int _attacker, int _defender)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
        }
    }
}

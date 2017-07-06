namespace FinalWar
{
    public struct BattleCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public int damage;

        public BattleCounterVO(int _pos, int _attacker, int _defender, int _damage)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            damage = _damage;
        }
    }
}

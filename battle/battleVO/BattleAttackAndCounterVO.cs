namespace FinalWar
{
    public struct BattleAttackAndCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public int attackDamage;
        public int defenseDamage;

        public BattleAttackAndCounterVO(int _pos, int _attacker, int _defender, int _attackDamage, int _defenseDamage)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            attackDamage = _attackDamage;
            defenseDamage = _defenseDamage;
        }
    }
}

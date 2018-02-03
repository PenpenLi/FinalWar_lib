namespace FinalWar
{
    public struct BattleAttackOverVO
    {
        public int pos;
        public AttackType attackType;
        public int attacker;
        public int defender;

        public BattleAttackOverVO(int _pos, AttackType _attackType, int _attacker, int _defender)
        {
            pos = _pos;
            attackType = _attackType;
            attacker = _attacker;
            defender = _defender;
        }
    }
}

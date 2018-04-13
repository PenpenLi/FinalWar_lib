namespace FinalWar
{
    public enum AttackType
    {
        A_A,
        A_D,
        A_S,
    }

    public struct BattlePrepareAttackVO
    {
        public int pos;

        public AttackType attackType;

        public int attacker;
        public int attackerSpeed;

        public int defender;
        public int defenderSpeed;


        public BattlePrepareAttackVO(int _pos, AttackType _attackType, int _attacker, int _attackerSpeed, int _defender, int _defenderSpeed)
        {
            pos = _pos;

            attackType = _attackType;

            attacker = _attacker;
            attackerSpeed = _attackerSpeed;

            defender = _defender;
            defenderSpeed = _defenderSpeed;
        }
    }
}

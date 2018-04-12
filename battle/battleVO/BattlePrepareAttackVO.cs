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
        public string attackerSpeed;

        public int defender;
        public string defenderSpeed;


        public BattlePrepareAttackVO(int _pos, AttackType _attackType, int _attacker, string _attackerSpeed, int _defender, string _defenderSpeed)
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

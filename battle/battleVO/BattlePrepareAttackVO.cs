namespace FinalWar
{
    public struct BattlePrepareAttackVO
    {
        public int pos;

        public int attacker;
        public int attackerSpeed;

        public int defender;
        public int defenderSpeed;

        public BattlePrepareAttackVO(int _pos, int _attacker, int _attackerSpeed, int _defender, int _defenderSpeed)
        {
            pos = _pos;

            attacker = _attacker;
            attackerSpeed = _attackerSpeed;

            defender = _defender;
            defenderSpeed = _defenderSpeed;
        }
    }
}

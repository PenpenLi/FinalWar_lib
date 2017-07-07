namespace FinalWar
{
    public struct BattleShowSpeedVO
    {
        public int attacker;

        public int defender;

        public int attackerSpeed;

        public int defenderSpeed;

        public BattleShowSpeedVO(int _attacker, int _defender, int _attackerSpeed, int _defenderSpeed)
        {
            attacker = _attacker;

            defender = _defender;

            attackerSpeed = _attackerSpeed;

            defenderSpeed = _defenderSpeed;
        }
    }
}

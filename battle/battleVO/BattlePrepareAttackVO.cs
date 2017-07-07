namespace FinalWar
{
    public struct BattlePrepareAttackVO
    {
        public int pos;
        public int attacker;
        public int defender;

        public BattlePrepareAttackVO(int _pos, int _attacker, int _defender)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
        }
    }
}

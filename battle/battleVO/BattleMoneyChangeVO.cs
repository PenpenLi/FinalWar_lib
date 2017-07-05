namespace FinalWar
{
    public struct BattleMoneyChangeVO
    {
        public bool isMine;
        public int money;

        public BattleMoneyChangeVO(bool _isMine, int _money)
        {
            isMine = _isMine;
            money = _money;
        }
    }
}

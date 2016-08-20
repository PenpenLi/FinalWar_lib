namespace FinalWar
{
    public struct BattleSummonVO
    {
        public int cardUid;
        public int pos;

        public BattleSummonVO(int _cardUid, int _pos)
        {
            cardUid = _cardUid;
            pos = _pos;
        }
    }
}

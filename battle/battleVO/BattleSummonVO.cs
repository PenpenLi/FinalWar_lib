namespace FinalWar
{
    public struct BattleSummonVO
    {
        public int cardUid;
        public int heroID;
        public int pos;

        public BattleSummonVO(int _cardUid, int _heroID, int _pos)
        {
            cardUid = _cardUid;
            heroID = _heroID;
            pos = _pos;
        }
    }
}

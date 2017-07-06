namespace FinalWar
{
    public struct BattleSummonVO
    {
        public bool isMine;
        public int cardUid;
        public int heroID;
        public int pos;

        public BattleSummonVO(bool _isMine, int _cardUid, int _heroID, int _pos)
        {
            isMine = _isMine;
            cardUid = _cardUid;
            heroID = _heroID;
            pos = _pos;
        }
    }
}

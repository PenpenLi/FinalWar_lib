using System.IO;

namespace FinalWar
{
    public struct BattleSummonVO : IBattleVO
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

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(cardUid);

            _bw.Write(heroID);

            _bw.Write(pos);
        }

        public void FromBytes(BinaryReader _br)
        {
            cardUid = _br.ReadInt32();

            heroID = _br.ReadInt32();

            pos = _br.ReadInt32();
        }
    }
}

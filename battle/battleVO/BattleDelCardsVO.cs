using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleDelCardsVO : IBattleVO
    {
        public bool isMine;
        public List<int> delCards;

        public BattleDelCardsVO(bool _isMine, List<int> _delCards)
        {
            isMine = _isMine;
            delCards = _delCards;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(isMine);

            _bw.Write(delCards.Count);

            for (int m = 0; m < delCards.Count; m++)
            {
                _bw.Write(delCards[m]);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            isMine = _br.ReadBoolean();

            delCards = new List<int>();

            int num = _br.ReadInt32();

            for (int m = 0; m < num; m++)
            {
                int uid = _br.ReadInt32();

                delCards.Add(uid);
            }
        }
    }
}

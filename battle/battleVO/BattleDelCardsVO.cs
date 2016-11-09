using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleDelCardsVO : IBattleVO
    {
        public List<int> delCards;

        public BattleDelCardsVO(List<int> _delCards)
        {
            delCards = _delCards;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(delCards.Count);

            for (int m = 0; m < delCards.Count; m++)
            {
                _bw.Write(delCards[m]);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
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

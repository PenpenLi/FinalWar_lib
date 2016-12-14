using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleDelCardsVO : IBattleVO
    {
        public bool isMine;
        public LinkedList<int> delCards;

        public BattleDelCardsVO(bool _isMine, LinkedList<int> _delCards)
        {
            isMine = _isMine;
            delCards = _delCards;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(isMine);

            _bw.Write(delCards.Count);

            LinkedList<int>.Enumerator enumerator = delCards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _bw.Write(enumerator.Current);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            isMine = _br.ReadBoolean();

            delCards = new LinkedList<int>();

            int num = _br.ReadInt32();

            for (int m = 0; m < num; m++)
            {
                int uid = _br.ReadInt32();

                delCards.AddLast(uid);
            }
        }
    }
}

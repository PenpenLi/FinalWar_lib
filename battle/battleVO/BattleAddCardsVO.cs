using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleAddCardsVO : IBattleVO
    {
        public bool isMine;
        public Dictionary<int, int> addCards;

        public BattleAddCardsVO(bool _isMine, Dictionary<int, int> _addCards)
        {
            isMine = _isMine;
            addCards = _addCards;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(isMine);

            _bw.Write(addCards.Count);

            Dictionary<int, int>.Enumerator enumerator = addCards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                _bw.Write(pair.Key);

                if (isMine == _isMine)
                {
                    _bw.Write(pair.Value);
                }
                else
                {
                    _bw.Write(0);
                }
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            isMine = _br.ReadBoolean();

            addCards = new Dictionary<int, int>();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                addCards.Add(uid, id);
            }
        }
    }
}

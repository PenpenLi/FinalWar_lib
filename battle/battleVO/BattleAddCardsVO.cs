using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleAddCardsVO : IBattleVO
    {
        public Dictionary<int, int> addCards;

        public BattleAddCardsVO(Dictionary<int, int> _addCards)
        {
            addCards = _addCards;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(addCards.Count);

            Dictionary<int, int>.Enumerator enumerator = addCards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                _bw.Write(pair.Key);

                _bw.Write(pair.Value);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            addCards = new Dictionary<int, int>();

            int num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                addCards.Add(uid, id);
            }
        }
    }
}

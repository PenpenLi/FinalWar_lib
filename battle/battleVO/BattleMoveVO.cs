using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleMoveVO : IBattleVO
    {
        public Dictionary<int, int> moves;

        public BattleMoveVO(Dictionary<int, int> _moves)
        {
            moves = _moves;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(moves.Count);

            Dictionary<int, int>.Enumerator enumerator = moves.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _bw.Write(enumerator.Current.Key);

                _bw.Write(enumerator.Current.Value);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            moves = new Dictionary<int, int>();

            int moveNum = _br.ReadInt32();

            for (int m = 0; m < moveNum; m++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                moves.Add(pos, targetPos);
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleDeathVO : IBattleVO
    {
        public LinkedList<int> deads;

        public BattleDeathVO(LinkedList<int> _deads)
        {
            deads = _deads;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(deads.Count);

            LinkedList<int>.Enumerator enumerator = deads.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _bw.Write(enumerator.Current);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            deads = new LinkedList<int>();

            int deadsNum = _br.ReadInt32();

            for (int m = 0; m < deadsNum; m++)
            {
                int deadPos = _br.ReadInt32();

                deads.AddLast(deadPos);
            }
        }
    }
}

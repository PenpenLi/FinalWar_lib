using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleDeathVO : IBattleVO
    {
        public List<int> deads;

        public BattleDeathVO(List<int> _deads)
        {
            deads = _deads;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(deads.Count);

            for (int i = 0; i < deads.Count; i++)
            {
                _bw.Write(deads[i]);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            deads = new List<int>();

            int deadsNum = _br.ReadInt32();

            for (int m = 0; m < deadsNum; m++)
            {
                int deadPos = _br.ReadInt32();

                deads.Add(deadPos);
            }
        }
    }
}

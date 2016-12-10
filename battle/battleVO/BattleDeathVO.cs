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

            for (int m = 0; m < deads.Count; m++)
            {
                _bw.Write(deads[m]);
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

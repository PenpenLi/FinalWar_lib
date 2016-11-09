using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleChangeVO : IBattleVO
    {
        public List<int> pos;
        public List<int> shieldChange;
        public List<int> hpChange;

        public BattleChangeVO(List<int> _pos, List<int> _shieldChange, List<int> _hpChange)
        {
            pos = _pos;
            shieldChange = _shieldChange;
            hpChange = _hpChange;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(pos.Count);

            for (int m = 0; m < pos.Count; m++)
            {
                _bw.Write(pos[m]);

                _bw.Write(shieldChange[m]);

                _bw.Write(hpChange[m]);
            }
        }

        public void FromBytes(BinaryReader _br)
        {
            pos = new List<int>();

            shieldChange = new List<int>();

            hpChange = new List<int>();

            int changeNum = _br.ReadInt32();

            for (int m = 0; m < changeNum; m++)
            {
                int tmpPos = _br.ReadInt32();

                pos.Add(tmpPos);

                int tmpShieldChange = _br.ReadInt32();

                shieldChange.Add(tmpShieldChange);

                int tmpHpChange = _br.ReadInt32();

                hpChange.Add(tmpHpChange);
            }
        }
    }
}

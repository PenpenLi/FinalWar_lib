using System.IO;

namespace FinalWar
{
    public struct BattleRecoverShieldVO : IBattleVO
    {
        public int pos;

        public BattleRecoverShieldVO(int _pos)
        {
            pos = _pos;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(pos);
        }

        public void FromBytes(BinaryReader _br)
        {
            pos = _br.ReadInt32();
        }
    }
}

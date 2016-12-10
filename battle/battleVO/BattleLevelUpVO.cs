using System.IO;

namespace FinalWar
{
    public struct BattleLevelUpVO : IBattleVO
    {
        public int pos;
        public int id;

        public BattleLevelUpVO(int _pos, int _id)
        {
            pos = _pos;
            id = _id;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(pos);
            _bw.Write(id);
        }

        public void FromBytes(BinaryReader _br)
        {
            pos = _br.ReadInt32();
            id = _br.ReadInt32();
        }
    }
}

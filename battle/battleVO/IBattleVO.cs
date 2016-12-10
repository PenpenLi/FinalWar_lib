using System.IO;

namespace FinalWar
{
    public interface IBattleVO
    {
        void ToBytes(bool _isMine, BinaryWriter _bw);

        void FromBytes(BinaryReader _br);
    }
}

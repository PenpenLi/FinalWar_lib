using System.IO;

namespace FinalWar
{
    public interface IBattleVO
    {
        void ToBytes(BinaryWriter _bw);

        void FromBytes(BinaryReader _br);
    }
}

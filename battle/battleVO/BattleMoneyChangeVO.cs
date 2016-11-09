using System.IO;

namespace FinalWar
{
    public struct BattleMoneyChangeVO : IBattleVO
    {
        public int money;

        public BattleMoneyChangeVO(int _money)
        {
            money = _money;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(money);
        }

        public void FromBytes(BinaryReader _br)
        {
            money = _br.ReadInt32();
        }
    }
}

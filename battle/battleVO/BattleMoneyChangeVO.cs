using System.IO;

namespace FinalWar
{
    public struct BattleMoneyChangeVO : IBattleVO
    {
        public bool isMine;
        public int money;

        public BattleMoneyChangeVO(bool _isMine, int _money)
        {
            isMine = _isMine;
            money = _money;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(isMine);
            _bw.Write(money);
        }

        public void FromBytes(BinaryReader _br)
        {
            isMine = _br.ReadBoolean();
            money = _br.ReadInt32();
        }
    }
}

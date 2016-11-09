using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleShootVO : IBattleVO
    {
        public List<int> shooters;
        public int stander;
        public int shieldDamage;
        public int hpDamage;

        public BattleShootVO(List<int> _shooters, int _stander, int _shieldDamage, int _hpDamage)
        {
            shooters = _shooters;
            stander = _stander;
            shieldDamage = _shieldDamage;
            hpDamage = _hpDamage;
        }

        public void ToBytes(BinaryWriter _bw)
        {
            _bw.Write(shooters.Count);

            for (int m = 0; m < shooters.Count; m++)
            {
                _bw.Write(shooters[m]);
            }

            _bw.Write(stander);

            _bw.Write(shieldDamage);

            _bw.Write(hpDamage);
        }

        public void FromBytes(BinaryReader _br)
        {
            shooters = new List<int>();

            int shooterNum = _br.ReadInt32();

            for (int m = 0; m < shooterNum; m++)
            {
                int shooter = _br.ReadInt32();

                shooters.Add(shooter);
            }

            stander = _br.ReadInt32();

            shieldDamage = _br.ReadInt32();

            hpDamage = _br.ReadInt32();
        }
    }
}

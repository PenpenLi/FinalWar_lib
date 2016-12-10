using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleRushVO : IBattleVO
    {
        public List<int> attackers;
        public int stander;
        public int shieldDamage;
        public int hpDamage;

        public BattleRushVO(List<int> _attackers, int _stander, int _shieldDamage, int _hpDamage)
        {
            attackers = _attackers;
            stander = _stander;
            shieldDamage = _shieldDamage;
            hpDamage = _hpDamage;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(attackers.Count);

            for (int m = 0; m < attackers.Count; m++)
            {
                _bw.Write(attackers[m]);
            }

            _bw.Write(stander);

            _bw.Write(shieldDamage);

            _bw.Write(hpDamage);
        }

        public void FromBytes(BinaryReader _br)
        {
            attackers = new List<int>();

            int attackerNum = _br.ReadInt32();

            for (int m = 0; m < attackerNum; m++)
            {
                int rusher = _br.ReadInt32();

                attackers.Add(rusher);
            }

            stander = _br.ReadInt32();

            shieldDamage = _br.ReadInt32();

            hpDamage = _br.ReadInt32();
        }
    }
}

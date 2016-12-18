using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleRushVO : IBattleVO
    {
        public List<int> attackers;
        public List<List<int>> helpers;
        public int stander;
        public int shieldDamage;
        public int hpDamage;

        public BattleRushVO(List<int> _attackers, List<List<int>> _helpers, int _stander, int _shieldDamage, int _hpDamage)
        {
            attackers = _attackers;
            helpers = _helpers;
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

                List<int> tmpList = helpers[m];

                _bw.Write(tmpList.Count);

                for (int i = 0; i < tmpList.Count; i++)
                {
                    _bw.Write(tmpList[i]);
                }
            }

            _bw.Write(stander);

            _bw.Write(shieldDamage);

            _bw.Write(hpDamage);
        }

        public void FromBytes(BinaryReader _br)
        {
            attackers = new List<int>();

            helpers = new List<List<int>>();

            int attackerNum = _br.ReadInt32();

            for (int m = 0; m < attackerNum; m++)
            {
                int rusher = _br.ReadInt32();

                attackers.Add(rusher);

                List<int> tmpList = new List<int>();

                helpers.Add(tmpList);

                int num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    tmpList.Add(_br.ReadInt32());
                }
            }

            stander = _br.ReadInt32();

            shieldDamage = _br.ReadInt32();

            hpDamage = _br.ReadInt32();
        }
    }
}

using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public struct BattleAttackVO : IBattleVO
    {
        public List<int> attackers;
        public List<List<int>> helpers;
        public List<int> supporters;
        public int defender;
        public List<int> attackersShieldDamage;
        public List<int> attackersHpDamage;
        public List<int> supportersShieldDamage;
        public List<int> supportersHpDamage;
        public int defenderShieldDamage;
        public int defenderHpDamage;

        public BattleAttackVO(List<int> _attackers, List<List<int>> _helpers, List<int> _supporters, int _defender, List<int> _attackersShieldDamage, List<int> _attackersHpDamage, List<int> _supportersShieldDamage, List<int> _supportersHpDamage, int _defenderShieldDamage, int _defenderHpDamage)
        {
            attackers = _attackers;
            helpers = _helpers;
            supporters = _supporters;
            defender = _defender;
            attackersShieldDamage = _attackersShieldDamage;
            attackersHpDamage = _attackersHpDamage;
            supportersShieldDamage = _supportersShieldDamage;
            supportersHpDamage = _supportersHpDamage;
            defenderShieldDamage = _defenderShieldDamage;
            defenderHpDamage = _defenderHpDamage;
        }

        public void ToBytes(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(attackers.Count);

            for (int m = 0; m < attackers.Count; m++)
            {
                _bw.Write(attackers[m]);

                _bw.Write(attackersShieldDamage[m]);

                _bw.Write(attackersHpDamage[m]);

                List<int> tmpList = helpers[m];

                _bw.Write(tmpList.Count);

                for (int i = 0; i < tmpList.Count; i++)
                {
                    _bw.Write(tmpList[i]);
                }
            }

            _bw.Write(supporters.Count);

            for (int m = 0; m < supporters.Count; m++)
            {
                _bw.Write(supporters[m]);

                _bw.Write(supportersShieldDamage[m]);

                _bw.Write(supportersHpDamage[m]);
            }

            _bw.Write(defender);

            _bw.Write(defenderShieldDamage);

            _bw.Write(defenderHpDamage);
        }

        public void FromBytes(BinaryReader _br)
        {
            attackers = new List<int>();

            helpers = new List<List<int>>();

            attackersShieldDamage = new List<int>();

            attackersHpDamage = new List<int>();

            supporters = new List<int>();

            supportersShieldDamage = new List<int>();

            supportersHpDamage = new List<int>();

            int num = _br.ReadInt32();

            for (int m = 0; m < num; m++)
            {
                int pos = _br.ReadInt32();

                attackers.Add(pos);

                int attackerShieldDamage = _br.ReadInt32();

                attackersShieldDamage.Add(attackerShieldDamage);

                int attackerHpDamage = _br.ReadInt32();

                attackersHpDamage.Add(attackerHpDamage);

                List<int> tmpList = new List<int>();

                helpers.Add(tmpList);

                int num2 = _br.ReadInt32();

                for (int i = 0; i < num2; i++)
                {
                    tmpList.Add(_br.ReadInt32());
                }
            }

            num = _br.ReadInt32();

            for (int m = 0; m < num; m++)
            {
                int pos = _br.ReadInt32();

                supporters.Add(pos);

                int supporterShieldDamage = _br.ReadInt32();

                supportersShieldDamage.Add(supporterShieldDamage);

                int supporterHpDamage = _br.ReadInt32();

                supportersHpDamage.Add(supporterHpDamage);
            }

            defender = _br.ReadInt32();

            defenderShieldDamage = _br.ReadInt32();

            defenderHpDamage = _br.ReadInt32();
        }
    }
}

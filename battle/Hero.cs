using System;

namespace FinalWar
{
    public class Hero
    {
        internal enum HeroAction
        {
            ATTACK,
            ATTACKOVER,
            SHOOT,
            SUPPORT,
            DEFENSE,
            NULL
        }

        private static readonly int[] HeroActionPower = new int[]
        {
            6000,
            6000,
            4000,
            4000,
            2000,
            0
        };

        private static readonly Random random = new Random();

        private const int MAX_POWER = 10000;

        private const int MAX_DEFENSE = 100;

        private const int MAX_LEADER = 100;

        private const float DEFENSE_FIX = 0.7f;

        private const float DAMAGE_FIX_WITH_POWER_RANGE = 0.3f;

        private const float DAMAGE_FIX_WITH_RANDOM_RANGE = 0.05f;

        private const float POWER_FIX_WITH_LEADER_RANGE = 0.3f;

        private const float POWER_FIX_WITH_RANDOM_RANGE = 0.05f;

        public bool isMine;

        public IHeroSDS sds;

        public int pos;
        public int nowHp { get; private set; }
        public int nowPower { get; private set; }

        internal HeroAction action { get; private set; }

        internal int actionTarget { get; private set; }

        private bool beDamaged = false;

        internal Hero(bool _isMine, IHeroSDS _sds, int _pos)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = sds.GetHp();
            nowPower = sds.GetPower();

            action = HeroAction.NULL;
        }

        internal Hero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowPower)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = _nowHp;
            nowPower = _nowPower;

            action = HeroAction.NULL;
        }

        internal void SetAction(HeroAction _action, int _actionTarget)
        {
            action = _action;

            actionTarget = _actionTarget;
        }

        internal void SetAction(HeroAction _action)
        {
            action = _action;
        }

        internal bool HpChange(int _value)
        {
            nowHp += _value;

            if(nowHp > sds.GetHp())
            {
                nowHp = sds.GetHp();
            }
            else if(nowHp < 0)
            {
                nowHp = 0;
            }

            return nowHp < 1;
        }

        internal bool PowerChange(int _value)
        {
            nowPower += _value;

            if(nowPower > 100)
            {
                nowPower = 100;
            }
            else if(nowPower < 0)
            {
                nowPower = 0;
            }

            if(_value < 0 && action != HeroAction.NULL && nowPower < HeroActionPower[(int)action])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal int GetShootDamage()
        {
            return FixDamageWithPowerAndRandom(sds.GetShoot());
        }

        internal int GetAttackDamage()
        {
            return FixDamageWithPowerAndRandom(sds.GetAttack());
        }

        internal int GetCounterDamage()
        {
            return FixDamageWithPowerAndRandom(sds.GetCounter());
        }

        private int FixDamageWithPowerAndRandom(int _damage)
        {
            int damage = (int)(nowHp * _damage * (1 + ((nowPower * 2 / MAX_POWER) - 1) * DAMAGE_FIX_WITH_POWER_RANGE));

            int minDamage = (int)(damage * (1 - DAMAGE_FIX_WITH_RANDOM_RANGE));

            int maxDamage = (int)(damage * (1 + DAMAGE_FIX_WITH_RANDOM_RANGE));

            damage = random.Next(minDamage, maxDamage);

            if(damage < 1)
            {
                damage = 1;
            }

            return damage;
        }

        internal int BeDamage(int _damage)
        {
            float fix = FixDefense();

            int tmpDamage = (int)(_damage / fix);

            if (tmpDamage > nowHp)
            {
                tmpDamage = nowHp;
            }
            else if (tmpDamage < 1)
            {
                tmpDamage = 1;
            }

            beDamaged = true;

            return tmpDamage;
        }

        internal int BeDamage(ref int _damage)
        {
            float fix = FixDefense();

            int tmpDamage = (int)(_damage / fix);

            if (tmpDamage > nowHp)
            {
                tmpDamage = nowHp;
            }
            else if (tmpDamage < 1)
            {
                tmpDamage = 1;
            }

            _damage -= (int)(tmpDamage * fix);

            beDamaged = true;

            return tmpDamage;
        }

        private float FixDefense()
        {
            return (int)(sds.GetDefense() * DEFENSE_FIX + MAX_DEFENSE * (1 - DEFENSE_FIX)) * 2;
        }

        private int FixPowerChange(int _powerChange)
        {
            int minPowerChange;

            int maxPowerChange;

            if(_powerChange > 0)
            {
                minPowerChange = (int)(_powerChange * (1 - ((sds.GetLeader() * 2 / MAX_LEADER) - 1) * POWER_FIX_WITH_LEADER_RANGE));

                maxPowerChange = (int)(_powerChange * (1 + ((sds.GetLeader() * 2 / MAX_LEADER) - 1) * POWER_FIX_WITH_LEADER_RANGE));
            }
            else
            {
                minPowerChange = (int)(_powerChange * (1 + ((sds.GetLeader() * 2 / MAX_LEADER) - 1) * POWER_FIX_WITH_LEADER_RANGE));

                maxPowerChange = (int)(_powerChange * (1 - ((sds.GetLeader() * 2 / MAX_LEADER) - 1) * POWER_FIX_WITH_LEADER_RANGE));
            }

            return random.Next(minPowerChange, maxPowerChange);
        }

        internal int Shoot()
        {
            return 0;
        }

        internal int BeShoot(int _shooterNum)
        {
            return FixPowerChange(-300 * _shooterNum);
        }

        internal int SummonHero()
        {
            return FixPowerChange(500);
        }

        internal int Rush()
        {
            return FixPowerChange(1200);
        }

        internal int BeRush(int _rusherNum)
        {
            return FixPowerChange(-1500 * _rusherNum);
        }

        internal int Attack(int _attackerNum, int _defenderNum)
        {
            return FixPowerChange((_attackerNum - _defenderNum) * 500);
        }

        internal int BeAttack(int _attackerNum, int _defenderNum)
        {
            return FixPowerChange((_defenderNum - _attackerNum) * 500);
        }

        internal int OtherHeroDie(bool _isMine)
        {
            if (isMine == _isMine)
            {
                return FixPowerChange(-1000);
            }
            else
            {
                return FixPowerChange(800);
            }
        }

        internal int MapBelongChange(bool _isMineNow)
        {
            if(isMine == _isMineNow)
            {
                return FixPowerChange(800);
            }
            else
            {
                return FixPowerChange(-1000);
            }
        }

        internal int RecoverPower()
        {
            int powerChange;

            if (beDamaged)
            {
                powerChange = FixPowerChange(300);

                beDamaged = false;
            }
            else
            {
                powerChange = FixPowerChange(600);
            }

            return powerChange;
        }

        internal bool CheckCanDoAction(HeroAction _action)
        {
            if(nowPower < HeroActionPower[(int)_action])
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

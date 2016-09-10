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
            60,
            60,
            40,
            40,
            20,
            0
        };

        public bool isMine;

        public IHeroSDS sds;

        public int pos;
        public int nowHp { get; private set; }
        public int nowPower { get; private set; }

        internal HeroAction action { get; private set; }

        internal int actionTarget { get; private set; }

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

            if(_value < 0 && action != HeroAction.NULL)
            {
                if(nowPower < HeroActionPower[(int)action])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal int GetShootDamage()
        {
            return nowHp * sds.GetShoot();
        }

        internal int GetAttackDamage()
        {
            return nowHp * sds.GetAttack();
        }

        internal int GetCounterDamage()
        {
            return nowHp * sds.GetCounter();
        }

        internal int BeDamage(int _damage)
        {
            float fix = sds.GetDefense() * 2;

            int tmpDamage = (int)(_damage / fix);

            if (tmpDamage > nowHp)
            {
                tmpDamage = nowHp;
            }

            return tmpDamage;
        }

        internal int BeDamage(ref int _damage)
        {
            float fix = sds.GetDefense() * 2;

            int tmpDamage = (int)(_damage / fix);

            if (tmpDamage > nowHp)
            {
                tmpDamage = nowHp;
            }

            _damage -= (int)(tmpDamage * fix);

            return tmpDamage;
        }

        internal int Shoot()
        {
            return 0;
        }

        internal int BeShoot(int _shooterNum)
        {
            return 0;
        }
        internal int Rush()
        {
            return 0;
        }

        internal int BeRush(int _rusherNum)
        {
            return 0;
        }

        internal int Attack(int _attackerNum, int _defenderNum)
        {
            return 0;
        }

        internal int BeAttack(int _attackerNum, int _defenderNum)
        {
            return 0;
        }

        internal int OtherHeroDie(bool _isMine)
        {
            return 0;
        }

        internal int MapBelongChange(bool _isMineNow)
        {
            return 0;
        }

        internal int RecoverPower()
        {
            return 10;
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

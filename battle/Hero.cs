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

        public bool isMine;

        public IHeroSDS sds;

        public int pos;
        public int nowHp { get; private set; }
        public int nowPower { get; private set; }

        internal HeroAction action;

        internal int actionTarget;

        internal Hero(bool _isMine, IHeroSDS _sds, int _pos)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = sds.GetHp();
            nowPower = sds.GetPower();
        }

        internal Hero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowPower)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = _nowHp;
            nowPower = _nowPower;
        }

        internal void HpChange(int _value)
        {
            nowHp += _value;
        }

        internal bool PowerChange(int _value)
        {
            nowPower += _value;

            return false;
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

        internal int Shoot(int _damage)
        {
            return 0;
        }

        internal int BeShoot(int _damage)
        {
            return 0;
        }
        internal int Rush(int _damage)
        {
            return 0;
        }

        internal int BeRush(int _damage)
        {
            return 0;
        }

        internal int Attack(int _attackerNum, int _defenderNum, int _attackDamage, int _defenseDamage)
        {
            return 0;
        }

        internal int BeAttack(int _attackerNum, int _defenderNum, int _attackDamage, int _defenseDamage)
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
            return 0;
        }
    }
}

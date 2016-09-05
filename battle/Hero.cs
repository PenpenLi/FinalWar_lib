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
        public int nowHp;

        internal HeroAction action;

        internal int actionTarget;

        public Hero(bool _isMine, IHeroSDS _sds, int _pos)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = sds.GetHp();
        }

        public Hero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = _nowHp;
        }

        public int GetShootDamage()
        {
            return nowHp * sds.GetShoot();
        }

        public int GetAttackDamage()
        {
            return nowHp * sds.GetAttack();
        }

        public int GetCounterDamage()
        {
            return nowHp * sds.GetCounter();
        }

        public int BeDamage(ref int _damage)
        {
            int tmpDamage = _damage / sds.GetDefense();

            if(tmpDamage > nowHp)
            {
                tmpDamage = nowHp;
            }

            _damage -= tmpDamage * sds.GetDefense();

            return tmpDamage;
        }
    }
}

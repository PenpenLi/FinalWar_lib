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
    }
}

namespace FinalWar
{
    public class Hero
    {
        public enum HeroAction
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

        public bool isSummon;

        public HeroAction action;

        public int actionTarget;

        public Hero(bool _isMine, IHeroSDS _sds, int _pos)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = sds.GetHp();

            isSummon = true;
        }

        public Hero(bool _isMine,IHeroSDS _sds,int _pos,int _nowHp)
        {
            isMine = _isMine;
            sds = _sds;
            pos = _pos;
            nowHp = _nowHp;

            isSummon = false;
        }
    }
}

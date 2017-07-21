using superEvent;

namespace FinalWar
{
    internal static partial class HeroAura
    {
        internal static void Init(Battle _battle, SuperEventListener _eventListener, Hero _hero)
        {
            if (_hero.sds.GetAuras().Length == 0)
            {
                return;
            }

            int[] ids = new int[_hero.sds.GetAuras().Length + 1];

            for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
            {
                int id = _hero.sds.GetAuras()[i];

                IAuraSDS sds = Battle.GetAuraData(id);

                ids[i] = RegisterAura(sds.GetAuraEffect(), _battle, _eventListener, _hero, sds.GetAuraData());
            }

            SuperEventListener.SuperFunctionCallBack1<Hero> dele = delegate (int _index, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        _eventListener.RemoveListener(ids[i]);
                    }
                }
            };

            ids[ids.Length - 1] = _eventListener.AddListener(REMOVE_AURA, dele, SuperEventListener.MAX_PRIORITY - 1);
        }
    }
}

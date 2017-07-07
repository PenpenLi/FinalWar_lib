using System.Collections.Generic;
using superEvent;

namespace FinalWar
{
    internal static class HeroAura
    {
        internal const string FIX_ATTACK = "fixAttack";

        internal const string FIX_SPEED = "fixSpeed";

        internal const string DIE = "die";

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

                switch (sds.GetAuraEffect())
                {
                    case AuraEffect.FIX_ATTACK:

                        ids[i] = FixAttack(_battle, _eventListener, _hero, sds.GetAuraData());

                        break;
                }
            }

            SuperEventListener.SuperFunctionCallBack1<Hero> dele = delegate (int _index, Hero _triggerHero)
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    _eventListener.RemoveListener(ids[i]);
                }
            };

            ids[ids.Length - 1] = _eventListener.AddListener(DIE, dele, SuperEventListener.MAX_PRIORITY - 1);
        }

        private static int FixAttack(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _attackFix, Hero _triggerHero)
             {
                 if (_triggerHero != _hero && _triggerHero.isMine == _hero.isMine)
                 {
                     List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                     if (tmpList.Contains(_triggerHero.pos))
                     {
                         _attackFix += _data;
                     }
                 }
             };

            return _eventListener.AddListener(FIX_ATTACK, dele);
        }
    }
}

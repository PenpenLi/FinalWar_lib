using System;
using superEvent;

namespace FinalWar
{
    internal static class HeroEffect
    {
        internal static BattleHeroEffectVO HeroTakeEffect(Battle _battle, Hero _hero, int _id)
        {
            IEffectSDS sds = Battle.GetEffectData(_id);

            switch (sds.GetEffect())
            {
                case Effect.DAMAGE:

                    _hero.BeDamage(sds.GetData()[0]);

                    break;

                case Effect.HP_CHANGE:

                    _hero.HpChange(sds.GetData()[0]);

                    break;

                case Effect.SHIELD_CHANGE:

                    _hero.ShieldChange(sds.GetData()[0]);

                    break;

                case Effect.DISABLE_MOVE:

                    FixBool(_battle, _hero, BattleConst.FIX_CAN_MOVE, false);

                    break;

                case Effect.DISABLE_RECOVER_SHIELD:

                    FixBool(_battle, _hero, BattleConst.FIX_CAN_RECOVER_SHIELD, false);

                    break;

                case Effect.FIX_ATTACK:

                    FixInt(_battle, _hero, BattleConst.FIX_ATTACK, sds.GetData()[0]);

                    break;

                case Effect.DISABLE_ACTION:

                    _hero.DisableAction();

                    break;

                case Effect.SILENCE:

                    _hero.Silence();

                    break;

                case Effect.FIX_SPEED:

                    FixInt(_battle, _hero, BattleConst.FIX_SPEED, sds.GetData()[0]);

                    break;

                case Effect.LEVEL_UP:

                    _hero.LevelUp(sds.GetData()[0]);

                    break;

                case Effect.ADD_MONEY:

                    _hero.MoneyChange(sds.GetData()[0]);

                    break;

                default:

                    throw new Exception("skill effect error:" + sds.GetEffect().ToString());
            }

            return new BattleHeroEffectVO(sds.GetEffect(), sds.GetData()[0]);
        }

        private static void FixBool(Battle _battle, Hero _hero, string _eventName, bool _result)
        {
            int id0 = 0;
            int id1 = 0;
            int id2 = 0;

            SuperEventListener.SuperFunctionCallBackV1<bool, Hero> dele = delegate (int _index, ref bool _b, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _b = _result;
                }
            };

            id0 = _battle.eventListener.AddListener(_eventName, dele);

            SuperEventListener.SuperFunctionCallBack1<Hero> dele2 = delegate (int _index, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _battle.eventListener.RemoveListener(id0);
                    _battle.eventListener.RemoveListener(id1);
                    _battle.eventListener.RemoveListener(id2);
                }
            };

            id1 = _battle.eventListener.AddListener(BattleConst.DIE, dele2, SuperEventListener.MAX_PRIORITY - 1);

            SuperEventListener.SuperFunctionCallBack dele3 = delegate (int _index)
            {
                _battle.eventListener.RemoveListener(id0);
                _battle.eventListener.RemoveListener(id1);
                _battle.eventListener.RemoveListener(id2);
            };

            id2 = _battle.eventListener.AddListener(BattleConst.ROUND_OVER, dele3, SuperEventListener.MAX_PRIORITY - 1);
        }

        private static void FixInt(Battle _battle, Hero _hero, string _eventName, int _result)
        {
            int id0 = 0;
            int id1 = 0;
            int id2 = 0;

            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _value, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _value += _result;
                }
            };

            id0 = _battle.eventListener.AddListener(_eventName, dele);

            SuperEventListener.SuperFunctionCallBack1<Hero> dele2 = delegate (int _index, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _battle.eventListener.RemoveListener(id0);
                    _battle.eventListener.RemoveListener(id1);
                    _battle.eventListener.RemoveListener(id2);
                }
            };

            id1 = _battle.eventListener.AddListener(BattleConst.DIE, dele2, SuperEventListener.MAX_PRIORITY - 1);

            SuperEventListener.SuperFunctionCallBack dele3 = delegate (int _index)
            {
                _battle.eventListener.RemoveListener(id0);
                _battle.eventListener.RemoveListener(id1);
                _battle.eventListener.RemoveListener(id2);
            };

            id2 = _battle.eventListener.AddListener(BattleConst.ROUND_OVER, dele3, SuperEventListener.MAX_PRIORITY - 1);
        }
    }
}

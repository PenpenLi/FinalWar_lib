using System;

namespace FinalWar
{
    internal static class HeroEffect
    {
        internal static BattleHeroEffectVO HeroTakeEffect(Battle _battle, Hero _hero, IEffectSDS _sds)
        {
            switch (_sds.GetEffect())
            {
                case Effect.DAMAGE:

                    _hero.BeDamage(_sds.GetData()[0]);

                    break;

                case Effect.HP_CHANGE:

                    _hero.HpChange(_sds.GetData()[0]);

                    break;

                case Effect.SHIELD_CHANGE:

                    _hero.ShieldChange(_sds.GetData()[0]);

                    break;

                case Effect.BE_SILENCE:

                    _hero.BeSilence();

                    break;

                case Effect.CHANGE_HERO:

                    _hero.ChangeHero(_sds.GetData()[0]);

                    break;

                case Effect.ADD_MONEY:

                    _hero.MoneyChange(_sds.GetData()[0]);

                    break;

                case Effect.BE_CLEAN:

                    _hero.BeClean();

                    break;

                case Effect.ADD_AURA:

                    HeroAura.Init(_battle, _hero, _sds.GetData()[0], false);

                    break;

                default:

                    throw new Exception("skill effect error:" + _sds.GetEffect().ToString());
            }

            return new BattleHeroEffectVO(_sds.GetEffect(), _sds.GetData());
        }
    }
}

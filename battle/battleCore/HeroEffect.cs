using System;

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

                case Effect.BE_SILENCE:

                    _hero.BeSilence();

                    break;

                case Effect.CHANGE_HERO:

                    _hero.ChangeHero(sds.GetData()[0]);

                    break;

                case Effect.ADD_MONEY:

                    _hero.MoneyChange(sds.GetData()[0]);

                    break;

                case Effect.BE_CLEAN:

                    _hero.BeClean();

                    break;

                case Effect.ADD_AURA:

                    HeroAura.Init(_battle, _hero, sds.GetData()[0]);

                    break;

                default:

                    throw new Exception("skill effect error:" + sds.GetEffect().ToString());
            }

            return new BattleHeroEffectVO(sds.GetEffect(), sds.GetData());
        }
    }
}

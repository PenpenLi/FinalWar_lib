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

                    _hero.BeDamage(GetData(_hero, _sds));

                    break;

                case Effect.HP_CHANGE:

                    _hero.HpChange(GetData(_hero, _sds));

                    break;

                case Effect.SHIELD_CHANGE:

                    _hero.ShieldChange(GetData(_hero, _sds));

                    break;

                case Effect.CHANGE_HERO:

                    _hero.ChangeHero(GetData(_hero, _sds));

                    break;

                case Effect.ADD_MONEY:

                    _hero.MoneyChange(GetData(_hero, _sds));

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

        private static int GetData(Hero _hero, IEffectSDS _sds)
        {
            Hero.HeroData heroData = (Hero.HeroData)_sds.GetData()[0];

            if (heroData == Hero.HeroData.DATA)
            {
                return _sds.GetData()[1];
            }
            else
            {
                return _hero.GetData(heroData);
            }
        }
    }
}

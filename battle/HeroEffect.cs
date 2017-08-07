using System;

namespace FinalWar
{
    internal static class HeroEffect
    {
        internal static BattleHeroEffectVO HeroTakeEffect(Hero _hero, int _id)
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

                    _hero.DisableMove();

                    break;

                case Effect.DISABLE_RECOVER_SHIELD:

                    _hero.DisableRecoverShield();

                    break;

                case Effect.FIX_ATTACK:

                    _hero.SetAttackFix(sds.GetData()[0]);

                    break;

                case Effect.DISABLE_ACTION:

                    _hero.DisableAction();

                    break;

                case Effect.SILENCE:

                    _hero.Silence();

                    break;

                case Effect.FIX_SPEED:

                    _hero.SetSpeedFix(sds.GetData()[0]);

                    break;

                default:

                    throw new Exception("skill effect error:" + sds.GetEffect().ToString());
            }

            return new BattleHeroEffectVO(sds.GetEffect(), sds.GetData()[0]);
        }
    }
}

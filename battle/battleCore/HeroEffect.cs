using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal static class HeroEffect
    {
        internal static List<BattleHeroEffectVO> HeroTakeEffect(Battle _battle, Hero _hero, IEffectSDS _sds)
        {
            List<BattleHeroEffectVO> result = new List<BattleHeroEffectVO>();

            int data = 0;

            switch (_sds.GetEffect())
            {
                case Effect.DAMAGE:

                    data = GetData(_hero, _sds);

                    int nowShield;

                    int nowHp;

                    _hero.ProcessDamage(out nowShield, out nowHp);

                    _hero.BeDamage(data);

                    int targetShield;

                    int targetHp;

                    _hero.ProcessDamage(out targetShield, out targetHp);

                    if (targetShield < nowShield)
                    {
                        result.Add(new BattleHeroEffectVO(Effect.SHIELD_CHANGE, targetShield - nowShield));
                    }

                    if (targetHp < nowHp)
                    {
                        result.Add(new BattleHeroEffectVO(Effect.HP_CHANGE, targetHp - nowHp));
                    }

                    return result;

                case Effect.HP_CHANGE:

                    data = GetData(_hero, _sds);

                    _hero.HpChange(data);

                    break;

                case Effect.SHIELD_CHANGE:

                    data = GetData(_hero, _sds);

                    _hero.ShieldChange(data);

                    break;

                case Effect.CHANGE_HERO:

                    data = _sds.GetData()[0];

                    _hero.ChangeHero(data);

                    break;

                case Effect.ADD_MONEY:

                    data = GetData(_hero, _sds);

                    _hero.MoneyChange(data);

                    break;

                case Effect.BE_CLEANED:

                    _hero.BeClean();

                    break;

                case Effect.ADD_AURA:

                    data = _sds.GetData()[0];

                    HeroAura.Init(_battle, _hero, data, HeroAura.AuraRegisterType.EFFECT);

                    break;

                case Effect.BE_KILLED:

                    _hero.BeKilled();

                    break;

                case Effect.FORCE_FEAR:

                    data = _sds.GetData()[0];

                    _hero.ForceFear(data);

                    break;

                default:

                    throw new Exception("skill effect error:" + _sds.GetEffect().ToString());
            }

            result.Add(new BattleHeroEffectVO(_sds.GetEffect(), data));

            return result;
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
                return _hero.GetData(heroData) * _sds.GetData()[1];
            }
        }
    }
}

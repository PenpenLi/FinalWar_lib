using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal static class HeroSkill
    {
        public static BattleShootVO CastSkill(Battle _battle, Hero _hero, Hero _target)
        {
            List<int> shooters = new List<int>() { _hero.pos };

            int stander = _target.pos;

            ISkillSDS sds = Battle.GetSkillData(_hero.sds.GetSkill());

            if (sds.GetIsStop())
            {
                _hero.DisableAction();
            }

            switch (sds.GetSkillEffect())
            {
                case SkillEffect.DAMAGE:

                    int shieldDamge;

                    int hpDamage;

                    _target.BeDamage(sds.GetSkillData(), out shieldDamge, out hpDamage);

                    return new BattleShootVO(shooters, stander, shieldDamge, hpDamage);

                case SkillEffect.HP_DAMAGE:

                    _target.BeHpDamage(sds.GetSkillData());

                    return new BattleShootVO(shooters, stander, 0, sds.GetSkillData());

                case SkillEffect.DISABLE_MOVE:

                    _target.DisableMove();

                    return new BattleShootVO(shooters, stander, 0, 0);

                case SkillEffect.DISABLE_RECOVER_SHIELD:

                    _target.DisableRecoverShield();

                    return new BattleShootVO(shooters, stander, 0, 0);

                case SkillEffect.FIX_ATTACK:

                    _target.SetAttackFix(sds.GetSkillData());

                    return new BattleShootVO(shooters, stander, 0, 0);

                case SkillEffect.DISABLE_ACTION:

                    _target.DisableAction();

                    return new BattleShootVO(shooters, stander, 0, 0);

                default:

                    throw new Exception("skill effect error:" + sds.GetSkillEffect());
            }

        }
    }
}

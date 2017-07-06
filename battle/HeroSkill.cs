using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal static class HeroSkill
    {
        public static BattleShootVO CastSkill(Battle _battle, Hero _hero, Hero _target)
        {
            int stander = _target.pos;

            ISkillSDS sds = Battle.GetSkillData(_hero.sds.GetSkill());

            if (sds.GetIsStop())
            {
                _hero.DisableAction();
            }

            switch (sds.GetSkillEffect())
            {
                case SkillEffect.DAMAGE:

                    _target.BeDamage(sds.GetSkillData());

                    return new BattleShootVO(_hero.pos, stander, sds.GetSkillData());

                case SkillEffect.HP_DAMAGE:

                    _target.BeHpDamage(sds.GetSkillData());

                    return new BattleShootVO(_hero.pos, stander, sds.GetSkillData());

                case SkillEffect.DISABLE_MOVE:

                    _target.DisableMove();

                    return new BattleShootVO(_hero.pos, stander, sds.GetSkillData());

                case SkillEffect.DISABLE_RECOVER_SHIELD:

                    _target.DisableRecoverShield();

                    return new BattleShootVO(_hero.pos, stander, sds.GetSkillData());

                case SkillEffect.FIX_ATTACK:

                    _target.SetAttackFix(sds.GetSkillData());

                    return new BattleShootVO(_hero.pos, stander, sds.GetSkillData());

                case SkillEffect.DISABLE_ACTION:

                    _target.DisableAction();

                    return new BattleShootVO(_hero.pos, stander, sds.GetSkillData());

                default:

                    throw new Exception("skill effect error:" + sds.GetSkillEffect());
            }

        }
    }
}

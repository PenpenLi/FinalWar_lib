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

                    break;

                case SkillEffect.HP_DAMAGE:

                    _target.BeHpDamage(sds.GetSkillData());

                    break;

                case SkillEffect.SHIELD_DAMAGE:

                    _target.BeShieldDamage(sds.GetSkillData());

                    break;

                case SkillEffect.DISABLE_MOVE:

                    _target.DisableMove();

                    break;

                case SkillEffect.DISABLE_RECOVER_SHIELD:

                    _target.DisableRecoverShield();

                    break;

                case SkillEffect.FIX_ATTACK:

                    _target.SetAttackFix(sds.GetSkillData());

                    break;

                case SkillEffect.DISABLE_ACTION:

                    _target.DisableAction();

                    break;

                case SkillEffect.SILENCE:

                    _target.Silence();

                    break;

                case SkillEffect.FIX_SPEED:

                    _target.SetSpeedFix(sds.GetSkillData());

                    break;

                default:

                    throw new Exception("skill effect error:" + sds.GetSkillEffect());
            }

            return new BattleShootVO(_hero.pos, stander, sds.GetSkillEffect(), sds.GetSkillData());
        }
    }
}

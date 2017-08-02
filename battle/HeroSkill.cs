using System;

namespace FinalWar
{
    internal static class HeroSkill
    {
        internal static BattleShootVO CastSkill(Battle _battle, Hero _hero, Hero _target)
        {
            int stander = _target.pos;

            ISkillSDS sds = Battle.GetSkillData(_hero.sds.GetSkill());

            if (sds.GetIsStop())
            {
                _hero.DisableAction();
            }

            CastSkill(_battle, _target, sds.GetSkillEffect(), sds.GetSkillData(), 0);

            return new BattleShootVO(_hero.pos, stander, sds.GetSkillEffect(), sds.GetSkillData()[0]);
        }

        internal static void CastSkill(Battle _battle, Hero _target, SkillEffect _skillEffect, int[] _skillData, int _skillDataIndex)
        {
            switch (_skillEffect)
            {
                case SkillEffect.DAMAGE:

                    _target.BeDamage(_skillData[_skillDataIndex]);

                    break;

                case SkillEffect.HP_DAMAGE:

                    _target.BeHpDamage(_skillData[_skillDataIndex]);

                    break;

                case SkillEffect.SHIELD_DAMAGE:

                    _target.BeShieldDamage(_skillData[_skillDataIndex]);

                    break;

                case SkillEffect.DISABLE_MOVE:

                    _target.DisableMove();

                    break;

                case SkillEffect.DISABLE_RECOVER_SHIELD:

                    _target.DisableRecoverShield();

                    break;

                case SkillEffect.FIX_ATTACK:

                    _target.SetAttackFix(_skillData[_skillDataIndex]);

                    break;

                case SkillEffect.DISABLE_ACTION:

                    _target.DisableAction();

                    break;

                case SkillEffect.SILENCE:

                    _target.Silence();

                    break;

                case SkillEffect.FIX_SPEED:

                    _target.SetSpeedFix(_skillData[_skillDataIndex]);

                    break;

                default:

                    throw new Exception("skill effect error:" + _skillEffect);
            }
        }
    }
}

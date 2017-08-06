using System.Collections.Generic;

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

            List<BattleHeroEffectVO> effectList = new List<BattleHeroEffectVO>();

            for (int i = 0; i < sds.GetEffects().Length; i++)
            {
                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_target, sds.GetEffects()[i]);

                effectList.Add(vo);
            }

            return new BattleShootVO(_hero.pos, stander, effectList);
        }
    }
}

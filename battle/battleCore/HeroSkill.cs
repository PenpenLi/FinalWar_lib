﻿using System.Collections.Generic;

namespace FinalWar
{
    internal static class HeroSkill
    {
        internal static List<BattleHeroEffectVO> CastSkill(Battle _battle, Hero _hero, Hero _target)
        {
            int stander = _target.pos;

            ISkillSDS sds = Battle.GetSkillData(_hero.sds.GetSkill());

            List<BattleHeroEffectVO> effectList = new List<BattleHeroEffectVO>();

            for (int i = 0; i < sds.GetEffects().Length; i++)
            {
                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _target, sds.GetEffects()[i]);

                effectList.Add(vo);
            }

            return effectList;
        }
    }
}

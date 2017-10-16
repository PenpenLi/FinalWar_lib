using System.Collections.Generic;

namespace FinalWar
{
    internal static class HeroSkill
    {
        internal static List<BattleHeroEffectVO> CastSkill(Battle _battle, Hero _hero, Hero _target, int[] _ids)
        {
            int stander = _target.pos;

            List<BattleHeroEffectVO> effectList = new List<BattleHeroEffectVO>();

            for (int i = 0; i < _ids.Length; i++)
            {
                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _target, _ids[i]);

                effectList.Add(vo);
            }

            return effectList;
        }
    }
}

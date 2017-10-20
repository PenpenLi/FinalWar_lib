using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal static class HeroSkill
    {
        internal static void CastSkill(Battle _battle, Hero _hero, Hero _target, int[] _ids, Dictionary<Hero, List<Func<BattleHeroEffectVO>>>[] _arr)
        {
            int stander = _target.pos;

            for (int i = 0; i < _ids.Length; i++)
            {
                int id = _ids[i];

                IEffectSDS sds = Battle.GetEffectData(id);

                Func<BattleHeroEffectVO> func = delegate ()
                {
                    return HeroEffect.HeroTakeEffect(_battle, _target, sds);
                };

                Dictionary<Hero, List<Func<BattleHeroEffectVO>>> dic = _arr[sds.GetPriority()];

                if (dic == null)
                {
                    dic = new Dictionary<Hero, List<Func<BattleHeroEffectVO>>>();

                    _arr[sds.GetPriority()] = dic;
                }

                List<Func<BattleHeroEffectVO>> list;

                if (!dic.TryGetValue(_hero, out list))
                {
                    list = new List<Func<BattleHeroEffectVO>>();

                    dic.Add(_hero, list);
                }

                list.Add(func);
            }
        }
    }
}

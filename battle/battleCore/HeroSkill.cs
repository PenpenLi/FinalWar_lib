using System.Collections.Generic;
using System;
using tuple;

namespace FinalWar
{
    internal static class HeroSkill
    {
        internal static void CastSkill(Battle _battle, Hero _hero, Hero _target, int[] _ids, LinkedList<Tuple<int, Hero, Func<BattleHeroEffectVO>>> _list)
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

                LinkedListNode<Tuple<int, Hero, Func<BattleHeroEffectVO>>> addNode = new LinkedListNode<Tuple<int, Hero, Func<BattleHeroEffectVO>>>(new Tuple<int, Hero, Func<BattleHeroEffectVO>>(sds.GetPriority(), _hero, func));

                LinkedListNode<Tuple<int, Hero, Func<BattleHeroEffectVO>>> node = _list.First;

                if (node == null)
                {
                    _list.AddFirst(addNode);
                }
                else
                {
                    while (true)
                    {
                        if (sds.GetPriority() > node.Value.first)
                        {
                            node = node.Next;

                            if (node == null)
                            {
                                _list.AddLast(addNode);

                                break;
                            }
                        }
                        else
                        {
                            _list.AddBefore(node, addNode);

                            break;
                        }
                    }
                }
            }
        }
    }
}

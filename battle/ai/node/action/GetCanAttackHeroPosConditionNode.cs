using System.Collections.Generic;
using bt;
using System;

namespace FinalWar
{
    internal class GetCanAttackHeroPosConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "GetCanAttackHeroPosConditionNode";

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = HeroAi.GetCanAttackHeroPos(_t, _u);

            if (posList != null)
            {
                _v.Add(key, posList);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
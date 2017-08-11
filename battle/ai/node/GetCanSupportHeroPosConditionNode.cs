using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class GetCanSupportHeroPosConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "GetCanSupportHeroPosConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = HeroAi.GetCanSupportHeroPos(_t, _u);

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
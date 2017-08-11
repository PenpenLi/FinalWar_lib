using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class GetCanShootHeroPosConditionConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "GetCanShootHeroPosConditionConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = HeroAi.GetCanShootHeroPos(_t, _u);

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
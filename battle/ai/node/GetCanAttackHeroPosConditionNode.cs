using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class GetCanAttackHeroPosConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        internal const string key = "GetCanAttackHeroPosConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiData _v)
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
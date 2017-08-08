using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class GetCanSupportPosConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        internal const string key = "GetCanSupportPosConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            List<int> posList = HeroAi.GetCanSupportPos(_t, _u);

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
using System.Collections.Generic;
using bt;
using System;

namespace FinalWar
{
    internal class GetCanAttackPosConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "GetCanAttackPosConditionNode";

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = HeroAi.GetCanAttackPos(_t, _u);

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
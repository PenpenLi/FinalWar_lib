using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal class GetCanShootHeroPosConditionConditionNode : GetCanAttackHeroPosConditionNode
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = BattlePublicTools.GetCanThrowHeroPos(_t, _u);

            return CheckResult(posList, _v);
        }
    }
}
using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal class GetCanSupportHeroPosConditionNode : GetCanAttackHeroPosConditionNode
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = BattlePublicTools.GetCanSupportCanBeAttackedHeroPos(_t, _u);

            return CheckResult(posList, _v);
        }
    }
}
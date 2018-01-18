using bt;
using System;

namespace FinalWar
{
    internal class CheckHeroCanBeAttackConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            return BattlePublicTools.CheckHeroCanBeAttacked(_t, _u);
        }
    }
}
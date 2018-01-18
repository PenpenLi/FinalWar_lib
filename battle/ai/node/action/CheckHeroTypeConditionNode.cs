using bt;
using System;

namespace FinalWar
{
    internal class CheckHeroTypeConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        private int value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            return _u.sds.GetHeroType().GetID() == value;
        }
    }
}

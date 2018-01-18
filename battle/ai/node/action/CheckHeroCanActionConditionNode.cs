using bt;
using System;

namespace FinalWar
{
    internal class CheckHeroCanActionConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            return _u.GetCanAction();
        }
    }
}

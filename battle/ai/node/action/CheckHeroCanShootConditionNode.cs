using bt;
using System;

namespace FinalWar
{
    internal class CheckHeroCanShootConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "CheckHeroCanShootConditionNode";

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            return _u.sds.GetSkill() != 0;
        }
    }
}

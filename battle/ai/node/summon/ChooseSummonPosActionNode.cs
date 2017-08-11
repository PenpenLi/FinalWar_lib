using bt;
using System;

namespace FinalWar
{
    internal class ChooseSummonPosActionNode : ActionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "ChooseSummonPosActionNode";

        private Func<int, int> getRandomValueCallBack;

        internal ChooseSummonPosActionNode(Func<int, int> _getRandomValueCallBack)
        {
            getRandomValueCallBack = _getRandomValueCallBack;
        }

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {
            return true;
        }
    }
}

using bt;
using System;

namespace FinalWar
{
    internal class ChooseSummonPosConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        private int value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            return _v.summonPosDic.ContainsKey(value - 1);
        }
    }
}

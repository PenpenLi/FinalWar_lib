using bt;
using System;

namespace FinalWar
{
    internal class CheckSummonHeroTypeConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        private int value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            IHeroSDS sds = Battle.GetHeroData(_t.GetCard(_u, _v.uid));

            return sds.GetHeroType().GetID() == value;
        }
    }
}

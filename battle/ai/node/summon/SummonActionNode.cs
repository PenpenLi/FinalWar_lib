using bt;
using System;

namespace FinalWar
{
    internal class SummonActionNode : ActionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "SummonActionNode";

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            _t.AddSummon(_u, _v.pair.Key, _v.summonPos);

            IHeroSDS sds = Battle.GetHeroData(_v.pair.Value);

            _v.money -= sds.GetCost();

            return true;
        }
    }
}

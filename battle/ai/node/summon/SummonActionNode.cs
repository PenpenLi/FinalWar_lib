using bt;

namespace FinalWar
{
    internal class SummonActionNode : ActionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "SummonActionNode";

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {
            _t.summon.Add(_v.pair.Key, _v.summonPos);

            IHeroSDS sds = Battle.GetHeroData(_v.pair.Value);

            _v.money -= sds.GetCost();

            return true;
        }
    }
}

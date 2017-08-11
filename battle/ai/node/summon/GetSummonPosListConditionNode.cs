using bt;

namespace FinalWar
{
    internal class GetSummonPosListConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "GetSummonPosListConditionNode";

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {
            _v.summonPosList = HeroAi.GetSummonPosList(_t, _u);

            return true;
        }
    }
}

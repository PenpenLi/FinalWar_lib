using bt;

namespace FinalWar
{
    internal class GetMoneyConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "GetMoneyConditionNode";

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {
            _v.money = _u ? _t.mMoney : _t.oMoney;

            return _v.money > 0;
        }
    }
}

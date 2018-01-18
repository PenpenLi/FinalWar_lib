using bt;
using System;

namespace FinalWar
{
    internal class GetSummonPosToEnemyHeroListConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        private int value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            _v.summonPosDic = BattleAi.GetSummonPosToEmemyHeroList(_t, _u, value);

            return _v.summonPosDic != null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bt;

namespace FinalWar
{
    internal class GetSummonHeroIdConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "GetSummonHeroIdConditionNode";

        private Func<int, int> getRandomValueCallBack;

        internal GetSummonHeroIdConditionNode(Func<int, int> _getRandomValueCallBack)
        {
            getRandomValueCallBack = _getRandomValueCallBack;
        }

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {


            return true;
        }
    }
}

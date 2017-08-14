using System;
using System.Collections.Generic;
using System.Linq;
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
            Dictionary<int, bool> dic = _u ? _t.mHandCards : _t.oHandCards;

            int index = getRandomValueCallBack(dic.Count);

            KeyValuePair<int, bool> pair = dic.ElementAt(index);

            int id = _t.GetCard(pair.Key);

            IHeroSDS sds = Battle.GetHeroData(id);

            if (sds.GetCost() > _v.money)
            {
                return false;
            }
            else
            {
                _v.pair = new KeyValuePair<int, int>(pair.Key, id);

                return true;
            }
        }
    }
}

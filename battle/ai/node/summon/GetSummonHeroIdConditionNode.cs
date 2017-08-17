using System;
using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class GetSummonHeroIdConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "GetSummonHeroIdConditionNode";

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            List<int> handCards = _u ? _t.mHandCards : _t.oHandCards;

            if (handCards.Count == 0)
            {
                return false;
            }

            int index = _getRandomValueCallBack(handCards.Count);

            int uid = handCards[index];

            int id = _t.GetCard(uid);

            IHeroSDS sds = Battle.GetHeroData(id);

            if (sds.GetCost() > _v.money)
            {
                return false;
            }
            else
            {
                _v.pair = new KeyValuePair<int, int>(uid, id);

                return true;
            }
        }
    }
}

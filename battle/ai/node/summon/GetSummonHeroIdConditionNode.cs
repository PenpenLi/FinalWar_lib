using System;
using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class GetSummonHeroIdConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            List<int> handCards = _u ? _t.mHandCards : _t.oHandCards;

            if (handCards.Count == 0)
            {
                return false;
            }

            handCards = new List<int>(handCards);

            for (int i = handCards.Count - 1; i > -1; i--)
            {
                if (_v.result.ContainsKey(handCards[i]))
                {
                    handCards.RemoveAt(i);
                }
            }

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
                _v.uid = uid;

                _v.id = id;

                return true;
            }
        }
    }
}

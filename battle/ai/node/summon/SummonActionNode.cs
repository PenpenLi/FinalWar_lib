using bt;
using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal class SummonActionNode : ActionNode<Battle, bool, AiSummonData>
    {
        private int value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            List<int> list = _v.summonPosDic[value - 1];

            int index = _getRandomValueCallBack(list.Count);

            int pos = list[index];

            _v.summon.Add(pos, _v.uid);

            list.RemoveAt(index);

            if (list.Count == 0)
            {
                _v.summonPosDic.Remove(value - 1);
            }

            IHeroSDS sds = Battle.GetHeroData(_t.GetCard(_u, _v.uid));

            _v.money -= sds.GetCost();

            return true;
        }
    }
}

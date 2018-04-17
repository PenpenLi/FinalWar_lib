using bt;
using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal class ChooseTargetActionNode : ActionNode<Battle, Hero, AiActionData>
    {
        private string value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> list = _v.dic[value];

            int index = _getRandomValueCallBack(list.Count);

            int target = list[index];

            _v.action.Add(_u.pos, target);

            return true;
        }
    }
}
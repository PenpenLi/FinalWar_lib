using bt;
using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal class MoveForwardActionNode : ActionNode<Battle, Hero, AiActionData>
    {
        private int value;

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            int target = _u.isMine ? _t.mapData.oBase : _t.mapData.mBase;

            List<int> list = BattleAStar.Find(_t.mapData, _u.pos, target, value, _getRandomValueCallBack);

            int pos = list[0];

            if (!_v.summon.ContainsValue(pos))
            {
                _v.action.Add(_u.pos, pos);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
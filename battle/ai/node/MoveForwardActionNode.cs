using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class MoveForwardActionNode : ActionNode<Battle, Hero, AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            int targetPos;

            if (_u.isMine)
            {
                targetPos = _t.mapData.moveMap[_u.pos].Key;
            }
            else
            {
                targetPos = _t.mapData.moveMap[_u.pos].Value;
            }

            _t.action.Add(new KeyValuePair<int, int>(_u.pos, targetPos));

            return true;
        }
    }
}

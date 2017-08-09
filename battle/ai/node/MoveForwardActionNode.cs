using bt;
using System.Collections.Generic;

namespace FinalWar
{
    internal class MoveForwardActionNode : ActionNode<Battle, Hero, AiData>
    {
        internal const string key = "MoveForwardActionNode";

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            int target = _u.isMine ? _t.mapData.oBase : _t.mapData.mBase;

            List<int> list = BattleAStar.Find(_t.mapData, _u.pos, target, 3);

            _t.action.Add(new KeyValuePair<int, int>(_u.pos, list[0]));

            return true;
        }
    }
}
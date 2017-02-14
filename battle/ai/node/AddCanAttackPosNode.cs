using bt;
using System.Collections.Generic;

namespace FinalWar
{
    internal class AddCanAttackPosNode : ConditionNode<Battle,Hero,AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            List<int> posList = _t.GetCanAttackPos(_u);

            if (posList.Count > 0)
            {
                _v.posList.InsertRange(_v.posList.Count, posList);
            }

            return true;
        }
    }
}

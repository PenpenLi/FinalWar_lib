#if !CLIENT
using bt;

namespace FinalWar
{
    internal class CheckHasTargetPosConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return _v.posList.Count > 0;
        }
    }
}
#endif

#if !CLIENT
using bt;

namespace FinalWar
{
    internal class CheckHeroCanBeAttackConditionNode : ConditionNode<Battle,Hero,AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return _t.CheckHeroCanBeAttack(_u);
        }
    }
}
#endif

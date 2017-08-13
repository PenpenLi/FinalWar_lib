using bt;

namespace FinalWar
{
    internal class CheckHeroCanShootConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "CheckHeroCanShootConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiActionData _v)
        {
            return _u.sds.GetSkill() != 0;
        }
    }
}

using bt;

namespace FinalWar
{
    internal class CheckHeroCanActionConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "CheckHeroCanActionConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiActionData _v)
        {
            return _u.GetCanAction();
        }
    }
}

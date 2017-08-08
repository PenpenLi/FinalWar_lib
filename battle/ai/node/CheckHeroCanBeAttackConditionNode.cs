using bt;

namespace FinalWar
{
    internal class CheckHeroCanBeAttackConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        internal const string key = "CheckHeroCanBeAttackConditionNode";

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return HeroAi.CheckHeroCanBeAttack(_t, _u);
        }
    }
}
using bt;

namespace FinalWar
{
    internal class CheckAbilityTypeConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        private AbilityType abilityType;

        public CheckAbilityTypeConditionNode(AbilityType _abilityType)
        {
            abilityType = _abilityType;
        }

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return _u.sds.GetAbilityType() == abilityType;
        }
    }
}

using bt;

namespace FinalWar
{
    internal class DefenseActionNode : ActionNode<Battle, Hero, AiData>
    {
        internal const string key = "DefenseActionNode";

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {


            return true;
        }
    }
}
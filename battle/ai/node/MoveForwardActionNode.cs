using bt;

namespace FinalWar
{
    internal class MoveForwardActionNode : ActionNode<Battle, Hero, AiData>
    {
        internal const string key = "MoveForwardActionNode";

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {


            return true;
        }
    }
}
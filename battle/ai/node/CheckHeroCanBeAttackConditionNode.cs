using bt;
using System.Xml;

namespace FinalWar
{
    internal class CheckHeroCanBeAttackConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return HeroAi.CheckHeroCanBeAttack(_t, _u);
        }

        public CheckHeroCanBeAttackConditionNode(XmlNode _node)
        {

        }
    }
}
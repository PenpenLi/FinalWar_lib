using bt;
using System.Xml;
using System;

namespace FinalWar
{
    internal class CheckHeroTypeConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        public int heroType;

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return _u.sds.GetHeroType().GetID() == heroType;
        }

        internal CheckHeroTypeConditionNode(XmlNode _node)
        {
            XmlAttribute heroTypeAtt = _node.Attributes["heroType"];

            if (heroTypeAtt == null)
            {
                throw new Exception("CheckHeroTypeConditionNode has not heroType attribute:" + _node.ToString());
            }

            heroType = int.Parse(heroTypeAtt.InnerText);
        }
    }
}

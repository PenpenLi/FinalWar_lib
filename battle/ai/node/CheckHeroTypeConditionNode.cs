using bt;
using System.Xml;
using System;

namespace FinalWar
{
    internal class CheckHeroTypeConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        internal const string key = "CheckHeroTypeConditionNode";

        private int value;

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            return _u.sds.GetHeroType().GetID() == value;
        }

        internal CheckHeroTypeConditionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("CheckHeroTypeConditionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }
    }
}

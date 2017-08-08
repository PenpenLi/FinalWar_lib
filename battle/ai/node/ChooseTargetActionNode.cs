using bt;
using System.Xml;
using System;

namespace FinalWar
{
    internal class ChooseTargetActionNode : ActionNode<Battle, Hero, AiData>
    {
        internal const string key = "ChooseTargetActionNode";

        private string value;

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {


            return true;
        }

        internal ChooseTargetActionNode(XmlNode _node)
        {
            XmlAttribute valueAtt = _node.Attributes["value"];

            if (valueAtt == null)
            {
                throw new Exception("ChooseTargetActionNode has not value attribute:" + _node.ToString());
            }

            value = valueAtt.InnerText;
        }
    }
}
using bt;
using System.Xml;
using System;

namespace FinalWar
{
    internal class AddDefenseConditionNode : ConditionNode<Battle, Hero, AiData>
    {
        public int weight;

        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            _v.dic.Add(_u.pos, weight);

            return true;
        }

        internal AddDefenseConditionNode(XmlNode _node)
        {
            XmlAttribute weightAtt = _node.Attributes["weight"];

            if (weightAtt == null)
            {
                throw new Exception("AddDefenseConditionNode has not weight attribute:" + _node.ToString());
            }

            weight = int.Parse(weightAtt.InnerText);
        }
    }
}
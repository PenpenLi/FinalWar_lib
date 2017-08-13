using bt;
using System.Xml;
using System;

namespace FinalWar
{
    internal class CheckSummonHeroTypeConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "CheckSummonHeroTypeConditionNode";

        private int value;

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {
            IHeroSDS sds = Battle.GetHeroData(_v.pair.Value);

            return sds.GetHeroType().GetID() == value;
        }

        internal CheckSummonHeroTypeConditionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("CheckSummonHeroTypeConditionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }
    }
}

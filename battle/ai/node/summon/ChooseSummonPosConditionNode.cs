using bt;
using System;
using System.Xml;
using System.Collections.Generic;

namespace FinalWar
{
    internal class ChooseSummonPosConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "ChooseSummonPosConditionNode";

        private int value;

        internal ChooseSummonPosConditionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("ChooseSummonPosConditionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            return _v.summonPosDic.ContainsKey(value - 1);
        }
    }
}

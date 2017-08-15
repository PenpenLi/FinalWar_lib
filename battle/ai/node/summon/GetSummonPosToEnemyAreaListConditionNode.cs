using bt;
using System;
using System.Xml;

namespace FinalWar
{
    internal class GetSummonPosToEnemyAreaListConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "GetSummonPosToEnemyAreaListConditionNode";

        private int value;

        internal GetSummonPosToEnemyAreaListConditionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("GetSummonPosToEnemyAreaListConditionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            _v.summonPosList = HeroAi.GetSummonPosToEmemyAreaList(_t, _u, value);

            return _v.summonPosList.Count > 0;
        }
    }
}

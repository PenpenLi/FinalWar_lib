using bt;
using System;
using System.Xml;

namespace FinalWar
{
    internal class GetSummonPosToEnemyHeroListConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "GetSummonPosToEnemyHeroListConditionNode";

        private int value;

        internal GetSummonPosToEnemyHeroListConditionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("GetSummonPosToEnemyHeroListConditionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            _v.summonPosDic = BattleAi.GetSummonPosToEmemyHeroList(_t, _u, value);

            return _v.summonPosDic != null;
        }
    }
}

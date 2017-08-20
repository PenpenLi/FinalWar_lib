using bt;
using System;
using System.Xml;

namespace FinalWar
{
    internal class SummonActionNode : ActionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "SummonActionNode";

        private int value;

        public SummonActionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("SummonActionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }

        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, bool _u, AiSummonData _v)
        {
            int pos = _v.summonPos[value - 1];

            _t.AddSummon(_u, _v.pair.Key, pos);

            _v.summonPosList[value - 1].Remove(pos);

            IHeroSDS sds = Battle.GetHeroData(_v.pair.Value);

            _v.money -= sds.GetCost();

            return true;
        }
    }
}

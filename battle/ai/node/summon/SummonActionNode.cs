using bt;
using System;
using System.Collections.Generic;
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
            List<int> list = _v.summonPosDic[value - 1];

            int index = _getRandomValueCallBack(list.Count);

            int pos = list[index];

            bool b = _t.AddSummon(_u, _v.uid, pos);

            if (!b)
            {
                throw new Exception("SummonActionNode error!");
            }

            list.RemoveAt(index);

            if (list.Count == 0)
            {
                _v.summonPosDic.Remove(value - 1);
            }

            IHeroSDS sds = Battle.GetHeroData(_v.id);

            _v.money -= sds.GetCost();

            return true;
        }
    }
}

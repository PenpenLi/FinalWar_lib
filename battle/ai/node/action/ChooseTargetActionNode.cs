using bt;
using System.Xml;
using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal class ChooseTargetActionNode : ActionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "ChooseTargetActionNode";

        private Func<int, int> getRandomValueCallBack;

        private string value;

        public override bool Enter(Battle _t, Hero _u, AiActionData _v)
        {
            List<int> list = _v.dic[value];

            int index = getRandomValueCallBack(list.Count);

            int target = list[index];

            _t.GetAction().Add(_u.pos, target);

            return true;
        }

        internal ChooseTargetActionNode(XmlNode _node, Func<int, int> _getRandomValueCallBack)
        {
            XmlAttribute valueAtt = _node.Attributes["value"];

            if (valueAtt == null)
            {
                throw new Exception("ChooseTargetActionNode has not value attribute:" + _node.ToString());
            }

            value = valueAtt.InnerText;

            getRandomValueCallBack = _getRandomValueCallBack;
        }
    }
}
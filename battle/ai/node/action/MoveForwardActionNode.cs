using bt;
using System.Collections.Generic;
using System.Xml;
using System;

namespace FinalWar
{
    internal class MoveForwardActionNode : ActionNode<Battle, Hero, AiActionData>
    {
        internal const string key = "MoveForwardActionNode";

        private int value;

        internal MoveForwardActionNode(XmlNode _node)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("MoveForwardActionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);
        }

        public override bool Enter(Battle _t, Hero _u, AiActionData _v)
        {
            int target = _u.isMine ? _t.mapData.oBase : _t.mapData.mBase;

            List<int> list = BattleAStar.Find(_t.mapData, _u.pos, target, value);

            _t.action.Add(new KeyValuePair<int, int>(_u.pos, list[0]));

            return true;
        }
    }
}
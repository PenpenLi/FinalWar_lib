using bt;
using System.Collections.Generic;
using System.Xml;
using System;

namespace FinalWar
{
    internal class FinalActionNode : ActionNode<Battle, Hero, AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {


            return true;
        }

        internal FinalActionNode(XmlNode _node)
        {
        }
    }
}
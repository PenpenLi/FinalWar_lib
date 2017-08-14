using bt;
using System;
using System.Xml;
using System.Collections.Generic;

namespace FinalWar
{
    internal class ChooseSummonPosConditionNode : ConditionNode<Battle, bool, AiSummonData>
    {
        internal const string key = "ChooseSummonPosConditionNode";

        private Func<int, int> getRandomValueCallBack;

        private int value;

        internal ChooseSummonPosConditionNode(XmlNode _node, Func<int, int> _getRandomValueCallBack)
        {
            XmlAttribute valueTypeAtt = _node.Attributes["value"];

            if (valueTypeAtt == null)
            {
                throw new Exception("ChooseSummonPosConditionNode has not value attribute:" + _node.ToString());
            }

            value = int.Parse(valueTypeAtt.InnerText);

            getRandomValueCallBack = _getRandomValueCallBack;
        }

        public override bool Enter(Battle _t, bool _u, AiSummonData _v)
        {
            IHeroSDS sds = Battle.GetHeroData(_v.pair.Value);

            if (_v.summonPosList.Count >= value)
            {
                List<int> tmpList = _v.summonPosList[value - 1];

                if (tmpList.Count > 0)
                {
                    List<int> finalList = new List<int>();

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        if (!_t.GetSummon().ContainsKey(pos))
                        {
                            finalList.Add(pos);
                        }
                    }

                    if (finalList.Count > 0)
                    {
                        int index = getRandomValueCallBack(finalList.Count);

                        _v.summonPos = finalList[index];

                        return true;
                    }
                }
            }

            return false;
        }
    }
}

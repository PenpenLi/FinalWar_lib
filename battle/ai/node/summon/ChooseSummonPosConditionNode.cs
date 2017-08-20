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

                        if (!_t.GetSummonContainsKey(pos))
                        {
                            finalList.Add(pos);
                        }
                    }

                    if (finalList.Count > 0)
                    {
                        int index = _getRandomValueCallBack(finalList.Count);

                        for (int i = _v.summonPos.Count; i < value; i++)
                        {
                            _v.summonPos.Add(0);
                        }

                        _v.summonPos[value - 1] = finalList[index];

                        return true;
                    }
                }
            }

            return false;
        }
    }
}

using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class AiSummonData : IClone
    {
        internal Dictionary<int, List<int>> summonPosDic;

        internal int uid;

        internal int money;

        internal Dictionary<int, int> action;

        internal Dictionary<int, int> summon;

        public IClone Clone()
        {
            AiSummonData clone = new AiSummonData();

            if (summonPosDic != null)
            {
                clone.summonPosDic = new Dictionary<int, List<int>>();

                IEnumerator<KeyValuePair<int, List<int>>> enumerator = summonPosDic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    clone.summonPosDic.Add(enumerator.Current.Key, new List<int>(enumerator.Current.Value));
                }

                clone.uid = uid;

                clone.money = money;

                if (action != null)
                {
                    clone.action = new Dictionary<int, int>();

                    IEnumerator<KeyValuePair<int, int>> enumerator2 = action.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        clone.action.Add(enumerator2.Current.Key, enumerator2.Current.Value);
                    }
                }

                if (summon != null)
                {
                    clone.summon = new Dictionary<int, int>();

                    IEnumerator<KeyValuePair<int, int>> enumerator2 = summon.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        clone.summon.Add(enumerator2.Current.Key, enumerator2.Current.Value);
                    }
                }
            }

            return clone;
        }
    }
}

using System.Collections.Generic;
using bt;

namespace FinalWar
{
    internal class AiActionData : IClone
    {
        internal Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();

        internal Dictionary<int, int> summon;

        internal Dictionary<int, int> action;

        internal void Add(string _key, List<int> _list)
        {
            dic.Add(_key, _list);
        }

        public IClone Clone()
        {
            AiActionData clone = new AiActionData();

            IEnumerator<KeyValuePair<string, List<int>>> enumerator = dic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                clone.dic.Add(enumerator.Current.Key, new List<int>(enumerator.Current.Value));
            }

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

            return clone;
        }
    }
}

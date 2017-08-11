using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal class AiActionData
    {
        private Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();

        private Func<int, int> getRandomValueCallBack;

        internal AiActionData(Func<int, int> _getRandomValueCallBack)
        {
            getRandomValueCallBack = _getRandomValueCallBack;
        }

        internal void Add(string _key, List<int> _list)
        {
            dic.Add(_key, _list);
        }

        internal int Get(string _key)
        {
            List<int> list;

            if (dic.TryGetValue(_key, out list))
            {
                int v = getRandomValueCallBack(list.Count);

                return list[v];
            }
            else
            {
                throw new Exception("Can not find key:" + _key);
            }
        }
    }
}

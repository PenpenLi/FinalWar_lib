using System.Collections.Generic;

namespace FinalWar
{
    internal class AiActionData
    {
        internal Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();

        internal Dictionary<int, int> summon;

        internal Dictionary<int, int> action;

        internal void Add(string _key, List<int> _list)
        {
            dic.Add(_key, _list);
        }
    }
}

using System.Collections.Generic;

namespace FinalWar
{
    internal class AiData
    {
        internal Dictionary<int, int> dic = new Dictionary<int, int>();

        internal void Add(int _pos, int _weight)
        {
            int value;

            if (dic.TryGetValue(_pos, out value))
            {
                dic[_pos] = value + _weight;
            }
            else
            {
                dic.Add(_pos, _weight);
            }
        }
    }
}

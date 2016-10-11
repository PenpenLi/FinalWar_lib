using System;
using System.Collections.Generic;

namespace publicTools
{
    public class PublicTools
    {
        public static void ShuffleList<T>(List<T> _list, Random _random)
        {
            List<T> tmpList = new List<T>(_list);

            for(int i = 0; i < _list.Count; i++)
            {
                int index = (int)(_random.NextDouble() * tmpList.Count);

                _list[i] = tmpList[index];

                tmpList.RemoveAt(index);
            }
        }

        public static int Choose(List<double> _list1, Random _random)
        {
            double d = 0;

            for(int i = 0; i < _list1.Count; i++)
            {
                d += _list1[i];
            }

            double value = _random.NextDouble() * d;

            for(int i = 0; i < _list1.Count; i++)
            {
                double v = _list1[i];

                if(value < v)
                {
                    return i;
                }
                else
                {
                    value -= v;
                }
            }

            return 0;
        }

        public static Dictionary<T, U> ConvertDic<T, V, U>(Dictionary<T, V> _dic) where V : U
        {
            Dictionary<T, U> result = new Dictionary<T, U>();

            Dictionary<T, V>.Enumerator enumerator = _dic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<T, V> pair = enumerator.Current;

                result.Add(pair.Key, pair.Value);
            }

            return result;
        }
    }
}

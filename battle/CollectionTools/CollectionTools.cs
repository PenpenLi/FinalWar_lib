using System;
using System.Collections.Generic;

namespace collectionTools
{
    public class CollectionTools
    {
        public static void ShuffleList<T>(List<T> _list, Random _random)
        {
            List<T> tmpList = new List<T>(_list);

            for (int i = 0; i < _list.Count; i++)
            {
                int index = _random.Next(tmpList.Count);

                _list[i] = tmpList[index];

                tmpList.RemoveAt(index);
            }
        }

        public static int Choose(List<double> _list1, Random _random)
        {
            double d = 0;

            for (int i = 0; i < _list1.Count; i++)
            {
                d += _list1[i];
            }

            double value = _random.NextDouble() * d;

            for (int i = 0; i < _list1.Count; i++)
            {
                double v = _list1[i];

                if (value < v)
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

        public static List<T> ChooseKeysFromDic<T, U>(Dictionary<T, U> _dic, int _num, Random _random)
        {
            List<T> list = new List<T>();

            Dictionary<T, U>.KeyCollection.Enumerator enumerator = _dic.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }

            List<T> result = new List<T>();

            for (int i = 0; i < _num && list.Count > 0; i++)
            {
                int index = _random.Next(list.Count);

                result.Add(list[index]);

                list.RemoveAt(index);
            }

            return result;
        }

        public static T ChooseOneKeyFromDic<T, U>(Dictionary<T, U> _dic, Random _random)
        {
            int index = _random.Next(_dic.Count);

            Dictionary<T, U>.KeyCollection.Enumerator enumerator = _dic.Keys.GetEnumerator();

            int i = 0;

            while (enumerator.MoveNext())
            {
                if(i == index)
                {
                    return enumerator.Current;
                }
                else
                {
                    i++;
                }
            }

            return enumerator.Current;
        }
    }
}

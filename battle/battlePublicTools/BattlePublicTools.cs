using System.Collections.Generic;

public class BattlePublicTools
{
    public static List<int> GetNeighbourPos(Dictionary<int, int[]> _neighbourPosMap, int _pos)
    {
        List<int> result = new List<int>();

        int[] arr = _neighbourPosMap[_pos];

        for(int i = 0; i < 6; i++)
        {
            if (arr[i] != -1)
            {
                result.Add(arr[i]);
            }
        }

        return result;
    }

    public static List<int> GetNeighbourPos2(Dictionary<int, int[]> _neighbourPosMap, int _pos)
    {
        List<int> result = new List<int>();

        int[] arr = _neighbourPosMap[_pos];

        for (int i = 0; i < 6; i++)
        {
            if (arr[i] != -1)
            {
                int[] arr2 = _neighbourPosMap[arr[i]];

                if (arr2[i] != -1)
                {
                    result.Add(arr2[i]);
                }
            }
        }

        return result;
    }

    public static void AccumulateDicData<T>(ref Dictionary<T,int> _dic,T _key,int _data)
    {
        if(_data != 0)
        {
            if (_dic == null)
            {
                _dic = new Dictionary<T, int>();

                _dic.Add(_key, _data);
            }
            else
            {
                if (_dic.ContainsKey(_key))
                {
                    _dic[_key] += _data;
                }
                else
                {
                    _dic.Add(_key, _data);
                }
            }
        }
    }
}


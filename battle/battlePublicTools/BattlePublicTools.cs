using System.Collections.Generic;
using System;

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

    public static int GetDistance(int _width, int _pos, int _targetPos)
    {
        int y0 = (int)(_pos / _width);
        int x0 = _pos % _width;

        int y1 = (int)(_targetPos / _width);
        int x1 = _targetPos % _width;

        int dy = y1 - y0;

        if(dy > 0)
        {
            int tt = _pos + dy * _width;

            int minx;

            int maxx;

            if (y0 % 2 == 0)
            {
                minx = tt - (int)((dy + 1) / 2);

                maxx = tt + (int)(dy / 2);
            }
            else
            {
                minx = tt - (int)(dy / 2);

                maxx = tt + (int)((dy + 1) / 2);
            }

            //Console.WriteLine("minx:" + minx);

            //Console.WriteLine("maxx:" + maxx);

            if (_targetPos < minx)
            {
                return dy + minx - _targetPos;
            }
            else if(_targetPos > maxx)
            {
                return dy + _targetPos - maxx;
            }
            else
            {
                return dy;
            }
        }
        else if(dy < 0)
        {
            int tt = _pos + dy * _width;

            int minx;

            int maxx;

            if(y1 % 2 == 0)
            {
                minx = tt + (int)(dy / 2);

                maxx = tt - (int)((dy - 1) / 2);
            }
            else
            {
                minx = tt + (int)((dy - 1) / 2);

                maxx = tt - (int)(dy / 2);
            }

            //Console.WriteLine("minx:" + minx);

            //Console.WriteLine("maxx:" + maxx);

            if (_targetPos < minx)
            {
                return -dy + minx - _targetPos;
            }
            else if (_targetPos > maxx)
            {
                return -dy + _targetPos - maxx;
            }
            else
            {
                return -dy;
            }
        }
        else
        {
            return Math.Abs(_pos - _targetPos);
        }
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

    public static void AccumulateDicData<T>(Dictionary<T,int> _dic,T _key,int _data)
    {
        if(_data != 0)
        {
            if (_dic.ContainsKey(_key))
            {
                _dic[_key] += _data;

                if(_dic[_key] == 0)
                {
                    _dic.Remove(_key);
                }
            }
            else
            {
                _dic.Add(_key, _data);
            }
        }
    }
}


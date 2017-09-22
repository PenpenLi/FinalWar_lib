using System.Collections.Generic;
using System;

namespace FinalWar
{
    public static class BattleAStar
    {
        private struct AstarUnit
        {
            public int pos;
            public int q;
            public int h;
            public int parent;
        }

        private static List<AstarUnit> open = new List<AstarUnit>();

        private static Dictionary<int, AstarUnit> close = new Dictionary<int, AstarUnit>();

        public static List<int> Find(MapData _mapData, int _startPos, int _endPos, int _maxNum, Func<int, int> _getRandomValueCallBack)
        {
            open.Clear();

            close.Clear();

            AstarUnit start = new AstarUnit();

            start.pos = _startPos;

            open.Add(start);

            while (open.Count > 0)
            {
                AstarUnit nowUnit = open[0];

                if (nowUnit.q > _maxNum)
                {
                    List<int> result = new List<int>();

                    while (nowUnit.pos != _startPos)
                    {
                        result.Add(nowUnit.pos);

                        nowUnit = close[nowUnit.parent];
                    }

                    result.Reverse();

                    return result;
                }

                open.RemoveAt(0);

                close.Add(nowUnit.pos, nowUnit);

                List<int> tmpList = BattlePublicTools.GetNeighbourPos(_mapData, nowUnit.pos);

                while (tmpList.Count > 0)
                {
                    int index;

                    if (_getRandomValueCallBack != null)
                    {
                        index = _getRandomValueCallBack(tmpList.Count);
                    }
                    else
                    {
                        index = 0;
                    }

                    int pos = tmpList[index];

                    tmpList.RemoveAt(index);

                    AstarUnit closeUnit;

                    if (close.TryGetValue(pos, out closeUnit))
                    {
                        int newQ = nowUnit.q + 1;

                        if (newQ < closeUnit.q)
                        {
                            closeUnit.q = newQ;

                            closeUnit.parent = nowUnit.pos;

                            close[pos] = closeUnit;
                        }

                        continue;
                    }

                    int openIndex = -1;

                    for (int m = 0; m < open.Count; m++)
                    {
                        if (open[m].pos == pos)
                        {
                            openIndex = m;

                            break;
                        }
                    }

                    if (openIndex == -1)
                    {
                        if (pos == _endPos)
                        {
                            List<int> result = new List<int>();

                            result.Add(pos);

                            while (nowUnit.pos != _startPos)
                            {
                                result.Add(nowUnit.pos);

                                nowUnit = close[nowUnit.parent];
                            }

                            result.Reverse();

                            return result;
                        }
                        else
                        {
                            AstarUnit newUnit = new AstarUnit();

                            newUnit.pos = pos;

                            newUnit.q = nowUnit.q + 1;

                            newUnit.h = BattlePublicTools.GetDistance(_mapData.mapHeight, pos, _endPos);

                            newUnit.parent = nowUnit.pos;

                            InsertToOpen(newUnit, open);
                        }
                    }
                    else
                    {
                        AstarUnit oldUnit = open[openIndex];

                        int newQ = nowUnit.q + 1;

                        if (newQ < oldUnit.q)
                        {
                            oldUnit.q = newQ;

                            oldUnit.parent = nowUnit.pos;

                            open.RemoveAt(openIndex);

                            InsertToOpen(oldUnit, open);
                        }
                    }
                }
            }

            return null;
        }

        private static void InsertToOpen(AstarUnit _unit, List<AstarUnit> _list)
        {
            for (int m = 0; m < _list.Count; m++)
            {
                AstarUnit k = _list[m];

                int newValue = _unit.q + _unit.h;

                int oldValue = k.q + k.h;

                if (newValue < oldValue || (newValue == oldValue && _unit.h < k.h))
                {
                    _list.Insert(m, _unit);

                    return;
                }
            }

            _list.Add(_unit);
        }
    }
}

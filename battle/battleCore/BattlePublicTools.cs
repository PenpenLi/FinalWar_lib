using System.Collections.Generic;
using System;
using FinalWar;

public class BattlePublicTools
{
    public static List<int> GetNeighbourPos(MapData _mapData, int _pos)
    {
        List<int> result = new List<int>();

        int[] arr = _mapData.neighbourPosMap[_pos];

        for (int i = 0; i < 6; i++)
        {
            int pos = arr[i];

            if (pos != -1)
            {
                MapData.MapUnitType mapUnitType = _mapData.dic[pos];

                if (mapUnitType == MapData.MapUnitType.M_AREA || mapUnitType == MapData.MapUnitType.O_AREA)
                {
                    result.Add(arr[i]);
                }
            }
        }

        return result;
    }

    public static List<int> GetNeighbourPos2(MapData _mapData, int _pos)
    {
        List<int> result = new List<int>();

        int[] arr = _mapData.neighbourPosMap[_pos];

        for (int i = 0; i < 6; i++)
        {
            int pos = arr[i];

            if (pos != -1)
            {
                MapData.MapUnitType mapUnitType = _mapData.dic[pos];

                if (mapUnitType != MapData.MapUnitType.HILL)
                {
                    int[] arr2 = _mapData.neighbourPosMap[pos];

                    pos = arr2[i];

                    if (pos != -1)
                    {
                        mapUnitType = _mapData.dic[pos];

                        if (mapUnitType == MapData.MapUnitType.M_AREA || mapUnitType == MapData.MapUnitType.O_AREA)
                        {
                            result.Add(pos);
                        }
                    }
                }
            }
        }

        return result;
    }

    private static Dictionary<int, bool> checkedDic = new Dictionary<int, bool>();

    private static int[] indexArr = new int[3];

    public static List<int> GetNeighbourPos3(MapData _mapData, int _pos)
    {
        List<int> result = new List<int>();

        int[] arr = _mapData.neighbourPosMap[_pos];

        for (int i = 0; i < 6; i++)
        {
            int pos = arr[i];

            if (pos != -1)
            {
                MapData.MapUnitType mapUnitType = _mapData.dic[pos];

                if (mapUnitType != MapData.MapUnitType.HILL)
                {
                    indexArr[0] = i;

                    if (i == 5)
                    {
                        indexArr[1] = 4;
                        indexArr[2] = 0;
                    }
                    else if (i == 0)
                    {
                        indexArr[1] = 5;
                        indexArr[2] = 1;
                    }
                    else
                    {
                        indexArr[1] = i - 1;
                        indexArr[2] = i + 1;
                    }

                    int[] arr2 = _mapData.neighbourPosMap[pos];

                    for (int m = 0; m < 3; m++)
                    {
                        pos = arr2[indexArr[m]];

                        if (pos != -1 && !checkedDic.ContainsKey(pos))
                        {
                            checkedDic.Add(pos, false);

                            mapUnitType = _mapData.dic[pos];

                            if (mapUnitType == MapData.MapUnitType.M_AREA || mapUnitType == MapData.MapUnitType.O_AREA)
                            {
                                result.Add(pos);
                            }
                        }
                    }
                }
            }
        }

        checkedDic.Clear();

        return result;
    }

    public static int GetDistance(int _width, int _pos, int _targetPos)
    {
        int y0;

        int ty = _pos / (_width * 2 - 1);
        int tx = _pos % (_width * 2 - 1);

        if (tx < _width)
        {
            y0 = ty * 2;
        }
        else
        {
            y0 = ty * 2 + 1;
        }

        int y1;

        ty = _targetPos / (_width * 2 - 1);
        tx = _targetPos % (_width * 2 - 1);

        if (tx < _width)
        {
            y1 = ty * 2;
        }
        else
        {
            y1 = ty * 2 + 1;
        }

        int dy = y1 - y0;

        if (dy > 0)
        {
            int minx = _pos + dy * (_width - 1);

            int maxx = _pos + dy * _width;

            if (_targetPos < minx)
            {
                return dy + minx - _targetPos;
            }
            else if (_targetPos > maxx)
            {
                return dy + _targetPos - maxx;
            }
            else
            {
                return dy;
            }
        }
        else if (dy < 0)
        {
            int minx = _pos + dy * _width;

            int maxx = _pos + dy * (_width - 1);

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

    public static List<int> GetCanAttackHeroPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        int nowThreadLevel = 0;

        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            bool b = _battle.GetPosIsMine(pos);

            if (b != _hero.isMine)
            {
                Hero hero;

                if (_battle.heroMapDic.TryGetValue(pos, out hero))
                {
                    if (hero.sds.GetHeroType().GetThread() > nowThreadLevel)
                    {
                        nowThreadLevel = hero.sds.GetHeroType().GetThread();

                        if (result == null)
                        {
                            result = new List<int>();
                        }
                        else
                        {
                            result.Clear();
                        }

                        result.Add(pos);
                    }
                    else if (hero.sds.GetHeroType().GetThread() == nowThreadLevel)
                    {
                        result.Add(pos);
                    }
                }
            }
        }

        return result;
    }

    public static List<int> GetCanAttackPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        int nowThreadLevel = 0;

        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            bool b = _battle.GetPosIsMine(pos);

            if (b != _hero.isMine)
            {
                Hero hero;

                if (_battle.heroMapDic.TryGetValue(pos, out hero))
                {
                    if (hero.sds.GetHeroType().GetThread() > nowThreadLevel)
                    {
                        nowThreadLevel = hero.sds.GetHeroType().GetThread();

                        if (result == null)
                        {
                            result = new List<int>();
                        }
                        else
                        {
                            result.Clear();
                        }

                        result.Add(pos);
                    }
                    else if (hero.sds.GetHeroType().GetThread() == nowThreadLevel)
                    {
                        if (result == null)
                        {
                            result = new List<int>();
                        }

                        result.Add(pos);
                    }
                }
                else
                {
                    if (nowThreadLevel == 0)
                    {
                        if (result == null)
                        {
                            result = new List<int>();
                        }

                        result.Add(pos);
                    }
                }
            }
        }

        return result;
    }

    public static List<int> GetCanShootHeroPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        if (_hero.sds.GetSkill() != 0)
        {
            List<int> posList = GetNeighbourPos2(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = _battle.GetPosIsMine(pos);

                if (b != _hero.isMine && _battle.heroMapDic.ContainsKey(pos))
                {
                    if (result == null)
                    {
                        result = new List<int>();
                    }

                    result.Add(pos);
                }
            }
        }

        return result;
    }

    public static List<int> GetCanThrowHeroPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        List<int> posList = GetNeighbourPos3(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            bool b = _battle.GetPosIsMine(pos);

            if (b != _hero.isMine && _battle.heroMapDic.ContainsKey(pos))
            {
                if (result == null)
                {
                    result = new List<int>();
                }

                result.Add(pos);
            }
        }

        return result;
    }

    public static bool CheckHeroCanBeAttacked(Battle _battle, Hero _hero)
    {
        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            if (_battle.GetPosIsMine(pos) != _hero.isMine)
            {
                Hero hero;

                if (_battle.heroMapDic.TryGetValue(pos, out hero))
                {
                    List<int> tmpList = GetCanAttackHeroPos(_battle, hero);

                    if (tmpList.Contains(_hero.pos))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static bool CheckPosCanBeAttacked(Battle _battle, int _pos)
    {
        List<int> posList = GetNeighbourPos(_battle.mapData, _pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            if (_battle.GetPosIsMine(pos) != _battle.GetPosIsMine(_pos))
            {
                Hero hero;

                if (_battle.heroMapDic.TryGetValue(pos, out hero))
                {
                    List<int> tmpList = GetCanAttackPos(_battle, hero);

                    if (tmpList.Contains(_pos))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static List<int> GetCanSupportPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            if (_battle.GetPosIsMine(pos) == _hero.isMine)
            {
                if (result == null)
                {
                    result = new List<int>();
                }

                result.Add(pos);
            }
        }

        return result;
    }

    public static List<int> GetCanSupportHeroPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            if (_battle.GetPosIsMine(pos) == _hero.isMine)
            {
                if (_battle.heroMapDic.ContainsKey(pos))
                {
                    if (result == null)
                    {
                        result = new List<int>();
                    }

                    result.Add(pos);
                }
            }
        }

        return result;
    }

    public static List<int> GetCanSupportCanBeAttackedHeroPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            if (_battle.GetPosIsMine(pos) == _hero.isMine)
            {
                Hero hero;

                if (_battle.heroMapDic.TryGetValue(pos, out hero))
                {
                    if (CheckHeroCanBeAttacked(_battle, hero))
                    {
                        if (result == null)
                        {
                            result = new List<int>();
                        }

                        result.Add(pos);
                    }
                }
            }
        }

        return result;
    }

    public static List<int> GetCanSupportCanBeAttackedPos(Battle _battle, Hero _hero)
    {
        List<int> result = null;

        List<int> posList = GetNeighbourPos(_battle.mapData, _hero.pos);

        for (int i = 0; i < posList.Count; i++)
        {
            int pos = posList[i];

            if (_battle.GetPosIsMine(pos) == _hero.isMine)
            {
                if (CheckPosCanBeAttacked(_battle, pos))
                {
                    if (result == null)
                    {
                        result = new List<int>();
                    }

                    result.Add(pos);
                }
            }
        }

        return result;
    }
}


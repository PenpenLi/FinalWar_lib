using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public class MapData
    {
        public enum MapUnitType
        {
            M_AREA,
            O_AREA,
            RIVER,
            HILL
        }

        public int mapWidth;
        public int mapHeight;

        public int size;

        public int mBase = -1;
        public int oBase = -1;

        public int mScore;
        public int oScore;

        public Dictionary<int, MapUnitType> dic = new Dictionary<int, MapUnitType>();

        public Dictionary<int, int[]> neighbourPosMap = new Dictionary<int, int[]>();

        public MapData()
        {

        }

        public MapData(int _mapWidth, int _mapHeight)
        {
            mapWidth = _mapWidth;
            mapHeight = _mapHeight;

            size = mapWidth * mapHeight - mapWidth / 2;
        }

        public void SetData(BinaryWriter _bw)
        {
            _bw.Write(mapWidth);
            _bw.Write(mapHeight);

            _bw.Write(mBase);
            _bw.Write(oBase);

            _bw.Write(dic.Count);

            IEnumerator<KeyValuePair<int, MapUnitType>> enumerator = dic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, MapUnitType> pair2 = enumerator.Current;

                _bw.Write(pair2.Key);

                MapUnitType mapUnitType = pair2.Value;

                _bw.Write((int)mapUnitType);
            }
        }

        public void GetData(BinaryReader _br)
        {
            mapWidth = _br.ReadInt32();
            mapHeight = _br.ReadInt32();

            mBase = _br.ReadInt32();
            oBase = _br.ReadInt32();

            size = mapWidth * mapHeight - mapWidth / 2;

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                MapUnitType mapUnitType = (MapUnitType)_br.ReadInt32();

                dic.Add(pos, mapUnitType);
            }

            SetNeighbourPosMap();
        }

        public void SetNeighbourPosMap()
        {
            IEnumerator<KeyValuePair<int, MapUnitType>> enumerator = dic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current.Key;

                MapUnitType mapUnitType = enumerator.Current.Value;

                if (mapUnitType == MapUnitType.M_AREA)
                {
                    mScore++;
                }
                else if (mapUnitType == MapUnitType.O_AREA)
                {
                    oScore++;
                }

                int[] vec = GetNeighbourPosVec(pos);

                neighbourPosMap.Add(pos, vec);
            }
        }

        private int[] GetNeighbourPosVec(int _pos)
        {
            int[] vec = new int[6];

            if (_pos % (mapHeight * 2 - 1) != 0)
            {
                if (_pos > mapHeight - 1)
                {
                    int p = _pos - mapHeight;

                    if (dic.ContainsKey(p))
                    {
                        vec[5] = p;
                    }
                    else
                    {
                        vec[5] = -1;
                    }
                }
                else
                {
                    vec[5] = -1;
                }

                if (_pos < size - (mapWidth % 2 == 1 ? mapHeight : mapHeight - 1))
                {
                    int p = _pos + mapHeight - 1;

                    if (dic.ContainsKey(p))
                    {
                        vec[3] = p;
                    }
                    else
                    {
                        vec[3] = -1;
                    }
                }
                else
                {
                    vec[3] = -1;
                }

                if (_pos % (mapHeight * 2 - 1) != mapHeight)
                {
                    int p = _pos - 1;

                    if (dic.ContainsKey(p))
                    {
                        vec[4] = p;
                    }
                    else
                    {
                        vec[4] = -1;
                    }
                }
                else
                {
                    vec[4] = -1;
                }
            }
            else
            {
                vec[3] = -1;
                vec[4] = -1;
                vec[5] = -1;
            }

            if (_pos % (mapHeight * 2 - 1) != mapHeight - 1)
            {
                if (_pos > mapHeight - 1)
                {
                    int p = _pos - mapHeight + 1;

                    if (dic.ContainsKey(p))
                    {
                        vec[0] = p;
                    }
                    else
                    {
                        vec[0] = -1;
                    }
                }
                else
                {
                    vec[0] = -1;
                }

                if (_pos < size - mapHeight)
                {
                    int p = _pos + mapHeight;

                    if (dic.ContainsKey(p))
                    {
                        vec[2] = p;
                    }
                    else
                    {
                        vec[2] = -1;
                    }
                }
                else
                {
                    vec[2] = -1;
                }

                if (_pos % (mapHeight * 2 - 1) != mapHeight * 2 - 2)
                {
                    int p = _pos + 1;

                    if (dic.ContainsKey(p))
                    {
                        vec[1] = p;
                    }
                    else
                    {
                        vec[1] = -1;
                    }
                }
                else
                {
                    vec[1] = -1;
                }
            }
            else
            {
                vec[0] = -1;
                vec[1] = -1;
                vec[2] = -1;
            }

            return vec;
        }
    }
}
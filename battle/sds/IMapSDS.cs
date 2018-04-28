using System.Collections.Generic;

namespace FinalWar
{
    public interface IMapSDS
    {
        MapData GetMapData();
        KeyValuePair<int, int>[] GetHero();
    }
}
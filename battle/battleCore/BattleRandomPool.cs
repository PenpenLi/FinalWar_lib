using System;
using System.IO;

namespace FinalWar
{
    public static class BattleRandomPool
    {
        public const int num = 1024;

        private static readonly float[] randomPool = new float[num];

        public static void Load(BinaryReader _br)
        {
            for (int i = 0; i < num; i++)
            {
                randomPool[i] = _br.ReadSingle();
            }
        }

        public static void Save(BinaryWriter _bw)
        {
            Random random = new Random();

            for (int i = 0; i < num; i++)
            {
                _bw.Write((float)random.NextDouble());
            }
        }

        public static float Get(int _index)
        {
            return randomPool[_index];
        }
    }
}

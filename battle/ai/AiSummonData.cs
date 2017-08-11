using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal class AiSummonData
    {
        internal List<List<int>> summonPosList;

        private Func<int, int> getRandomValueCallBack;

        internal AiSummonData(Func<int, int> _getRandomValueCallBack)
        {
            getRandomValueCallBack = _getRandomValueCallBack;
        }
    }
}

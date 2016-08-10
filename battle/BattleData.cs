using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalWar
{
    public class BattleData
    {
        public Dictionary<int, BattleCellData> actionDic = new Dictionary<int, BattleCellData>();

        public Dictionary<int, int> moveDic = new Dictionary<int, int>();
    }
}

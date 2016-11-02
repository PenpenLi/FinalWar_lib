using System.Collections.Generic;

namespace FinalWar
{
    public class BattleCellData
    {
        public int pos;

        public Hero stander;

        public List<Hero> shooters = new List<Hero>();

        public List<Hero> attackers = new List<Hero>();

        public List<Hero> supporters = new List<Hero>();

        internal bool attackHasBeenProcessed = false;

        public BattleCellData(int _pos)
        {
            pos = _pos;
        }
    }
}

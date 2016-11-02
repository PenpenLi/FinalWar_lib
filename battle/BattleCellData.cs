using System.Collections.Generic;

namespace FinalWar
{
    public class BattleCellData
    {
        public Hero stander;

        public List<Hero> shooters = new List<Hero>();

        public List<Hero> attackers = new List<Hero>();

        public List<Hero> supporters = new List<Hero>();

        internal bool attackHasBeenProcessed = false;
    }
}

using System.Collections.Generic;
namespace FinalWar
{
    public struct BattleDeathVO
    {
        public List<int> deads;

        public BattleDeathVO(List<int> _deads)
        {
            deads = _deads;
        }
    }
}

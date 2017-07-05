using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleDelCardsVO
    {
        public bool isMine;
        public List<int> delCards;

        public BattleDelCardsVO(bool _isMine, List<int> _delCards)
        {
            isMine = _isMine;
            delCards = _delCards;
        }
    }
}

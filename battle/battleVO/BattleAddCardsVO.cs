using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleAddCardsVO 
    {
        public bool isMine;
        public List<int> addCards;

        public BattleAddCardsVO(bool _isMine, List<int> _addCards)
        {
            isMine = _isMine;
            addCards = _addCards;
        }
    }
}

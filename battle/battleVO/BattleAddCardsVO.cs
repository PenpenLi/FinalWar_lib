using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleAddCardsVO 
    {
        public bool isMine;
        public Dictionary<int, int> addCards;

        public BattleAddCardsVO(bool _isMine, Dictionary<int, int> _addCards)
        {
            isMine = _isMine;
            addCards = _addCards;
        }
    }
}

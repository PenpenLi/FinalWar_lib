using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleSupportVO
    {
        public int supporter;
        public int stander;
        public List<BattleHeroEffectVO> effectList;

        public BattleSupportVO(int _supporter, int _stander, List<BattleHeroEffectVO> _effectList)
        {
            supporter = _supporter;
            stander = _stander;
            effectList = _effectList;
        }
    }
}

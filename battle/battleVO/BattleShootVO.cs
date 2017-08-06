using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleShootVO
    {
        public int shooter;
        public int stander;
        public List<BattleHeroEffectVO> effectList;

        public BattleShootVO(int _shooter, int _stander, List<BattleHeroEffectVO> _effectList)
        {
            shooter = _shooter;
            stander = _stander;
            effectList = _effectList;
        }
    }
}

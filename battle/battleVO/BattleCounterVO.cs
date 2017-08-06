using System.Collections.Generic;

namespace FinalWar
{
    public struct BattleCounterVO
    {
        public int pos;
        public int attacker;
        public int defender;
        public int damage;
        public List<BattleHeroEffectVO> effectList;

        public BattleCounterVO(int _pos, int _attacker, int _defender, int _damage, List<BattleHeroEffectVO> _effectList)
        {
            pos = _pos;
            attacker = _attacker;
            defender = _defender;
            damage = _damage;
            effectList = _effectList;
        }
    }
}

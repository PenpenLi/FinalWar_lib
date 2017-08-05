namespace FinalWar
{
    public struct BattleHeroEffectVO
    {
        public int pos;
        public SkillEffect effect;
        public int data;

        public BattleHeroEffectVO(int _pos, SkillEffect _effect, int _data)
        {
            pos = _pos;
            effect = _effect;
            data = _data;
        }
    }
}

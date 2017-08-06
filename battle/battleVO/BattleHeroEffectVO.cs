namespace FinalWar
{
    public struct BattleHeroEffectVO
    {
        public int pos;
        public Effect effect;
        public int data;

        public BattleHeroEffectVO(int _pos, Effect _effect, int _data)
        {
            pos = _pos;
            effect = _effect;
            data = _data;
        }
    }
}

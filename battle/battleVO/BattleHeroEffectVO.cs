namespace FinalWar
{
    public struct BattleHeroEffectVO
    {
        public Effect effect;
        public int[] data;

        public BattleHeroEffectVO(Effect _effect, int[] _data)
        {
            effect = _effect;
            data = _data;
        }
    }
}

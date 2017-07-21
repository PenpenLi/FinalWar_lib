namespace FinalWar
{
    public struct BattleShootVO
    {
        public int shooter;
        public int stander;
        public SkillEffect effect;
        public int data;

        public BattleShootVO(int _shooter, int _stander, SkillEffect _effect, int _data)
        {
            shooter = _shooter;
            stander = _stander;
            effect = _effect;
            data = _data;
        }
    }
}

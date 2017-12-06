namespace FinalWar
{
    public struct BattleScoreChangeVO
    {
        public bool isMine;
        public int score;

        public BattleScoreChangeVO(bool _isMine, int _score)
        {
            isMine = _isMine;
            score = _score;
        }
    }
}

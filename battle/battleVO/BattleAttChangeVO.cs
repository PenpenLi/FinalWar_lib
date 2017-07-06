using System.Collections.Generic;

namespace FinalWar
{
    public enum Att
    {
        ATTACK,
        HP,
        SHIELD,
        SPEED
    }

    public struct BattleAttChangeVO
    {
        public int pos;
        public List<KeyValuePair<Att,int>> datas;

        public BattleAttChangeVO(int _pos, List<KeyValuePair<Att, int>> _datas)
        {
            pos = _pos;
            datas = _datas;
        }
    }
}

#if !CLIENT
using bt;
using System.Collections.Generic;

namespace FinalWar
{
    internal class GetCanThrowHeroPosNode : ConditionNode<Battle, Hero, AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            List<int> posList = HeroAi.GetCanThrowHeroPos(_t, _u);

            if (posList.Count > 0)
            {
                _v.posListDic.Add("canThrowHeroPos", posList);
            }

            return true;
        }
    }
}
#endif

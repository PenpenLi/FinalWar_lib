using bt;
using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal class CheckHeroCanReachOppBaseConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> list = BattlePublicTools.GetCanAttackPos(_t, _u);

            int target = _u.isMine ? _t.mapData.oBase : _t.mapData.mBase;

            if (list != null && list.Contains(target))
            {
                if (!_t.heroMapDic.ContainsKey(target))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

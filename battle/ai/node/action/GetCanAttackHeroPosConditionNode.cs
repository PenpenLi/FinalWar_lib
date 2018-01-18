﻿using System.Collections.Generic;
using bt;
using System;

namespace FinalWar
{
    internal class GetCanAttackHeroPosConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = BattlePublicTools.GetCanAttackHeroPos(_t, _u);

            if (posList != null)
            {
                _v.Add(GetType().Name, posList);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
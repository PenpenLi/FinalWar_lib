using System.Collections.Generic;
using bt;
using System;

namespace FinalWar
{
    internal class GetCanAttackHeroPosConditionNode : ConditionNode<Battle, Hero, AiActionData>
    {
        public override bool Enter(Func<int, int> _getRandomValueCallBack, Battle _t, Hero _u, AiActionData _v)
        {
            List<int> posList = BattlePublicTools.GetCanAttackHeroPos(_t, _u);

            return CheckResult(posList, _v);
        }

        protected bool CheckResult(List<int> _posList, AiActionData _v)
        {
            if (_posList != null)
            {
                for (int i = _posList.Count - 1; i > -1; i--)
                {
                    if (_v.summon.ContainsKey(_posList[i]))
                    {
                        _posList.RemoveAt(i);
                    }
                }

                if (_posList.Count > 0)
                {
                    _v.Add(GetType().Name, _posList);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
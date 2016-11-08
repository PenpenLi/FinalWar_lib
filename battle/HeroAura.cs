using System;
using System.Collections.Generic;
using superEvent;

namespace FinalWar
{
    internal class HeroAura
    {
        internal static void Init(Battle _battle, Hero _hero)
        {
            if (_hero.sds.GetAuras().Length > 0)
            {
                int[] eventIDs = new int[_hero.sds.GetAuras().Length];

                for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
                {
                    int auraID = _hero.sds.GetAuras()[i];

                    IAuraSDS auraSDS = Battle.GetAuraData(auraID);

                    SuperEventListenerV.EventCallBack<int> dele = delegate (SuperEvent e, ref int _value)
                    {
                        TriggerAura(_battle, _hero, auraSDS, e, ref _value);
                    };

                    eventIDs[i] = _battle.eventListenerV.AddListener(auraSDS.GetAuraEffect().ToString(), dele);
                }

                Action<SuperEvent> dieDele = delegate (SuperEvent e)
                {
                    for (int i = 0; i < eventIDs.Length; i++)
                    {
                        _battle.eventListenerV.RemoveListener(eventIDs[i]);
                    }

                    _battle.eventListener.RemoveListener(e.index);
                };

                _battle.eventListener.AddListener(HeroSkill.GetEventName(_hero.uid, SkillTime.DIE), dieDele);
            }
        }

        private static void TriggerAura(Battle _battle, Hero _hero, IAuraSDS _auraSDS, SuperEvent e, ref int _value)
        {
            Hero targetHero = e.datas[0] as Hero;

            switch (_auraSDS.GetAuraTarget())
            {
                case AuraTarget.SELF:

                    if (targetHero != _hero)
                    {
                        return;
                    }

                    break;

                case AuraTarget.ALLY:

                    if (targetHero.isMine != _hero.isMine)
                    {
                        return;
                    }

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (!posList.Contains(targetHero.pos))
                    {
                        return;
                    }

                    break;

                case AuraTarget.ENEMY:

                    if (targetHero.isMine == _hero.isMine)
                    {
                        return;
                    }

                    posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (!posList.Contains(targetHero.pos))
                    {
                        return;
                    }

                    break;
            }

            _value += _auraSDS.GetAuraDatas()[0];
        }
    }
}

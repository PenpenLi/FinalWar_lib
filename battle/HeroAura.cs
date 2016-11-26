using System;
using System.Collections.Generic;
using superEvent;

namespace FinalWar
{
    internal class HeroAura
    {
        internal static void Add(Battle _battle, Hero _hero)
        {
            if (_hero.sds.GetAuras().Length > 0)
            {
                int[] eventIDs = new int[_hero.sds.GetAuras().Length];

                for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
                {
                    int auraID = _hero.sds.GetAuras()[i];

                    IAuraSDS auraSDS = Battle.GetAuraData(auraID);

                    switch (auraSDS.GetAuraEffect())
                    {
                        case AuraEffect.FIX_ATTACK:
                        case AuraEffect.FIX_ABILITY:
                        case AuraEffect.FIX_SHOOT_DAMAGE:

                            SuperEventListenerV.EventCallBack<int> dele = delegate (SuperEvent e, ref int _value)
                            {
                                TriggerAura(_battle, _hero, auraSDS, e, ref _value);
                            };

                            eventIDs[i] = _battle.eventListenerV.AddListener(auraSDS.GetAuraEffect().ToString(), dele);

                            break;

                        case AuraEffect.FIX_RUSH_DAMAGE:
                        case AuraEffect.SILENT:

                            SuperEventListenerV.EventCallBack<bool> dele2 = delegate (SuperEvent e, ref bool _value)
                            {
                                TriggerAura(_battle, _hero, auraSDS, e, ref _value);
                            };

                            eventIDs[i] = _battle.eventListenerV.AddListener(auraSDS.GetAuraEffect().ToString(), dele2);

                            break;
                    }
                }

                int dieEventID = 0;

                int levelUpEventID = 0;

                Action<SuperEvent> removeDele = delegate (SuperEvent e)
                {
                    for (int i = 0; i < eventIDs.Length; i++)
                    {
                        _battle.eventListenerV.RemoveListener(eventIDs[i]);
                    }

                    _battle.eventListener.RemoveListener(dieEventID);

                    _battle.eventListener.RemoveListener(levelUpEventID);
                };

                dieEventID = _battle.eventListener.AddListener(HeroSkill.GetEventName(_hero.uid, SkillTime.DIE), removeDele);

                levelUpEventID = _battle.eventListener.AddListener(HeroSkill.GetEventName(_hero.uid, SkillTime.LEVELUP), removeDele);
            }
        }

        private static bool CheckAuraTakeEffect(Battle _battle, Hero _hero, Hero _targetHero, IAuraSDS _auraSDS)
        {
            switch (_auraSDS.GetAuraTarget())
            {
                case AuraTarget.SELF:

                    if (_targetHero != _hero)
                    {
                        return false;
                    }

                    break;

                case AuraTarget.ALLY:

                    if (_targetHero.isMine != _hero.isMine)
                    {
                        return false;
                    }

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (!posList.Contains(_targetHero.pos))
                    {
                        return false;
                    }

                    break;

                case AuraTarget.ENEMY:

                    if (_targetHero.isMine == _hero.isMine)
                    {
                        return false;
                    }

                    posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (!posList.Contains(_targetHero.pos))
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        private static void TriggerAura(Battle _battle, Hero _hero, IAuraSDS _auraSDS, SuperEvent e, ref int _value)
        {
            Hero targetHero = e.datas[0] as Hero;

            bool b = CheckAuraTakeEffect(_battle, _hero, targetHero, _auraSDS);

            if (!b)
            {
                return;
            }

            _value += _auraSDS.GetAuraDatas()[0];
        }

        private static void TriggerAura(Battle _battle, Hero _hero, IAuraSDS _auraSDS, SuperEvent e, ref bool _value)
        {
            Hero targetHero = e.datas[0] as Hero;

            bool b = CheckAuraTakeEffect(_battle, _hero, targetHero, _auraSDS);

            if (!b)
            {
                return;
            }

            _value = false;
        }
    }
}

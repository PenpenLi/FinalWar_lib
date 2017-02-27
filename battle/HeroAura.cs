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

                            SuperEventListenerV.SuperFunctionCallBackV<int> dele = delegate (int _index, ref int _value, object[] _datas)
                            {
                                TriggerAura(_battle, _hero, auraSDS, ref _value, _datas);
                            };

                            eventIDs[i] = _battle.eventListenerV.AddListener(auraSDS.GetAuraEffect().ToString(), dele);

                            break;

                        case AuraEffect.SILENT:
                        case AuraEffect.DISABLE_RECOVER_SHIELD:

                            SuperEventListenerV.SuperFunctionCallBackV<bool> dele2 = delegate (int _index, ref bool _value, object[] _datas)
                            {
                                TriggerAura(_battle, _hero, auraSDS, ref _value, _datas);
                            };

                            eventIDs[i] = _battle.eventListenerV.AddListener(auraSDS.GetAuraEffect().ToString(), dele2);

                            break;
                    }
                }

                int dieEventID = 0;

                int removeEventID = 0;

                SuperEventListener.SuperFunctionCallBack removeDele = delegate (int _index, object[] _datas)
                {
                    for (int i = 0; i < eventIDs.Length; i++)
                    {
                        _battle.eventListenerV.RemoveListener(eventIDs[i]);
                    }

                    _battle.eventListener.RemoveListener(dieEventID);

                    _battle.eventListener.RemoveListener(removeEventID);
                };

                dieEventID = _battle.eventListener.AddListener(HeroSkill.GetEventName(_hero.uid, SkillTime.DIE), removeDele, SuperEventListener.MAX_PRIORITY - 1);

                removeEventID = _battle.eventListener.AddListener(HeroSkill.GetEventName(_hero.uid, Battle.REMOVE_EVENT_NAME), removeDele, SuperEventListener.MAX_PRIORITY - 1);
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

                    LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

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

        private static void TriggerAura(Battle _battle, Hero _hero, IAuraSDS _auraSDS, ref int _value, object[] _datas)
        {
            Hero targetHero = _datas[0] as Hero;

            bool b = CheckAuraTakeEffect(_battle, _hero, targetHero, _auraSDS);

            if (b)
            {
                _value += _auraSDS.GetAuraDatas()[0];
            }
        }

        private static void TriggerAura(Battle _battle, Hero _hero, IAuraSDS _auraSDS, ref bool _value, object[] _datas)
        {
            Hero targetHero = _datas[0] as Hero;

            bool b = CheckAuraTakeEffect(_battle, _hero, targetHero, _auraSDS);

            if (b)
            {
                _value = false;
            }
        }
    }
}

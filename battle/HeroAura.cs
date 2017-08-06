using superEvent;
using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal static partial class HeroAura
    {
        internal const string REMOVE_AURA = "removeAura";
        internal const string FIX_ATTACK = "fixAttack";
        internal const string FIX_SPEED = "fixSpeed";
        internal const string FIX_CAN_PIERCE_SHIELD = "fixCanPierceShield";
        internal const string FIX_CAN_MOVE = "fixCanMove";
        internal const string ATTACK = "attack";
        internal const string ROUND_START = "roundStart";
        internal const string ROUND_OVER = "roundOver";

        internal static void Init(Battle _battle, SuperEventListener _eventListener, Hero _hero)
        {
            if (_hero.sds.GetAuras().Length == 0)
            {
                return;
            }

            int[] ids = new int[_hero.sds.GetAuras().Length + 1];

            for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
            {
                int id = _hero.sds.GetAuras()[i];

                IAuraSDS sds = Battle.GetAuraData(id);

                ids[i] = RegisterAura(_battle, _eventListener, _hero, sds);
            }

            SuperEventListener.SuperFunctionCallBack1<Hero> dele = delegate (int _index, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        _eventListener.RemoveListener(ids[i]);
                    }
                }
            };

            ids[ids.Length - 1] = _eventListener.AddListener(REMOVE_AURA, dele, SuperEventListener.MAX_PRIORITY - 1);
        }

        private static int RegisterAura(Battle _battle, SuperEventListener _eventListener, Hero _hero, IAuraSDS _sds)
        {
            int result;

            switch (_sds.GetAuraType())
            {
                case AuraType.FIX_BOOL:

                    SuperEventListener.SuperFunctionCallBackV1<bool, Hero> dele0 = delegate (int _index, ref bool _result, Hero _triggerHero)
                    {
                        AuraFixBool(_index, _battle, _hero, _triggerHero, _sds, ref _result);
                    };

                    result = _eventListener.AddListener(_sds.GetEventName(), dele0);

                    break;

                case AuraType.FIX_INT:

                    SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele1 = delegate (int _index, ref int _result, Hero _triggerHero)
                    {
                        AuraFixInt(_index, _battle, _hero, _triggerHero, _sds, ref _result);
                    };

                    result = _eventListener.AddListener(_sds.GetEventName(), dele1);

                    break;

                case AuraType.CAST_SKILL:

                    if (_sds.GetAuraTarget() == AuraTarget.TRIGGER)
                    {
                        SuperEventListener.SuperFunctionCallBackV2<List<BattleHeroEffectVO>, Hero, Hero> dele2 = delegate (int _index, ref List<BattleHeroEffectVO> _list, Hero _triggerHero, Hero _targetHero)
                        {
                            if (_triggerHero == _hero)
                            {
                                if (_list == null)
                                {
                                    _list = new List<BattleHeroEffectVO>();
                                }

                                AuraCastSkill(_index, _battle, _targetHero, _sds, _list);
                            }
                        };

                        result = _eventListener.AddListener(_sds.GetEventName(), dele2);
                    }
                    else
                    {
                        SuperEventListener.SuperFunctionCallBackV<List<BattleHeroEffectVO>> dele3 = delegate (int _index, ref List<BattleHeroEffectVO> _list)
                        {
                            if (_list == null)
                            {
                                _list = new List<BattleHeroEffectVO>();
                            }

                            AuraCastSkill(_index, _battle, _hero, _sds, _list);
                        };

                        result = _eventListener.AddListener(_sds.GetEventName(), dele3);
                    }

                    break;

                default:

                    throw new Exception("Unknown AuraType:" + _sds.GetAuraType().ToString());
            }

            return result;
        }

        private static void AuraFixBool(int _index, Battle _battle, Hero _hero, Hero _triggerHero, IAuraSDS _sds, ref bool _result)
        {
            switch (_sds.GetAuraTarget())
            {
                case AuraTarget.SELF:

                    if (_triggerHero == _hero)
                    {
                        _result = _sds.GetAuraData()[0] == 1;
                    }

                    break;

                case AuraTarget.ALLY:

                    if (_hero.isMine == _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapWidth, _hero.pos, _triggerHero.pos) == 1)
                    {
                        _result = _sds.GetAuraData()[0] == 1;
                    }

                    break;

                case AuraTarget.ENEMY:

                    if (_hero.isMine != _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapWidth, _hero.pos, _triggerHero.pos) == 1)
                    {
                        _result = _sds.GetAuraData()[0] == 1;
                    }

                    break;

                default:

                    throw new Exception("AuraFixBool error:" + _sds.GetAuraTarget());

            }
        }

        private static void AuraFixInt(int _index, Battle _battle, Hero _hero, Hero _triggerHero, IAuraSDS _sds, ref int _result)
        {
            switch (_sds.GetAuraTarget())
            {
                case AuraTarget.SELF:

                    if (_triggerHero == _hero)
                    {
                        _result += _sds.GetAuraData()[0];
                    }

                    break;

                case AuraTarget.ALLY:

                    if (_hero.isMine == _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapWidth, _hero.pos, _triggerHero.pos) == 1)
                    {
                        _result += _sds.GetAuraData()[0];
                    }

                    break;

                case AuraTarget.ENEMY:

                    if (_hero.isMine != _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapWidth, _hero.pos, _triggerHero.pos) == 1)
                    {
                        _result += _sds.GetAuraData()[0];
                    }

                    break;

                default:

                    throw new Exception("AuraFixInt error:" + _sds.GetAuraTarget());

            }
        }

        private static void AuraCastSkill(int _index, Battle _battle, Hero _hero, IAuraSDS _sds, List<BattleHeroEffectVO> _list)
        {
            switch (_sds.GetAuraTarget())
            {
                case AuraTarget.SELF:
                case AuraTarget.TRIGGER:

                    for (int i = 0; i < _sds.GetAuraData().Length; i++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_hero, _sds.GetAuraData()[i]);

                        _list.Add(vo);
                    }

                    break;

                case AuraTarget.ALLY:

                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        Hero targetHero;

                        if (_battle.heroMapDic.TryGetValue(pos, out targetHero))
                        {
                            if (targetHero.isMine == _hero.isMine)
                            {
                                for (int m = 0; m < _sds.GetAuraData().Length; m++)
                                {
                                    BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_hero, _sds.GetAuraData()[m]);

                                    _list.Add(vo);
                                }
                            }
                        }
                    }

                    break;

                case AuraTarget.ENEMY:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        Hero targetHero;

                        if (_battle.heroMapDic.TryGetValue(pos, out targetHero))
                        {
                            if (targetHero.isMine != _hero.isMine)
                            {
                                for (int m = 0; m < _sds.GetAuraData().Length; m++)
                                {
                                    BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_hero, _sds.GetAuraData()[m]);

                                    _list.Add(vo);
                                }
                            }
                        }
                    }

                    break;

                default:

                    throw new Exception("AuraCastSkill error! Unknown AuraTarget:" + _sds.GetAuraTarget());
            }
        }
    }
}

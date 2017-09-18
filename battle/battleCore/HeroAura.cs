using superEvent;
using System;
using System.Collections.Generic;

namespace FinalWar
{
    internal static class HeroAura
    {
        internal static void Init(Battle _battle, Hero _hero)
        {
            if (_hero.sds.GetAuras().Length == 0)
            {
                return;
            }

            int[] ids = new int[_hero.sds.GetAuras().Length + 2];

            for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
            {
                int id = _hero.sds.GetAuras()[i];

                IAuraSDS sds = Battle.GetAuraData(id);

                ids[i] = RegisterAura(_battle, _hero, sds);
            }

            SuperEventListener.SuperFunctionCallBackV2<List<Func<BattleTriggerAuraVO>>, Hero, Hero> dele = delegate (int _index, ref List<Func<BattleTriggerAuraVO>> _list, Hero _triggerHero, Hero _targetHero)
            {
                if (_triggerHero == _hero)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        _battle.eventListener.RemoveListener(ids[i]);
                    }
                }
            };

            ids[ids.Length - 2] = _battle.eventListener.AddListener(BattleConst.BE_SILENCE, dele, SuperEventListener.MAX_PRIORITY - 1);

            ids[ids.Length - 1] = _battle.eventListener.AddListener(BattleConst.DIE, dele, SuperEventListener.MAX_PRIORITY - 1);
        }

        private static int RegisterAura(Battle _battle, Hero _hero, IAuraSDS _sds)
        {
            int result;

            switch (_sds.GetAuraType())
            {
                case AuraType.FIX_BOOL:

                    SuperEventListener.SuperFunctionCallBackV2<bool, Hero, Hero> dele0 = delegate (int _index, ref bool _result, Hero _triggerHero, Hero _targetHero)
                    {
                        if (CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckAuraCondition(_battle, _hero, _triggerHero, _targetHero, _sds))
                        {
                            _result = _sds.GetAuraData()[0] == 1;
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele0);

                    break;

                case AuraType.FIX_INT:

                    SuperEventListener.SuperFunctionCallBackV2<int, Hero, Hero> dele1 = delegate (int _index, ref int _result, Hero _triggerHero, Hero _targetHero)
                    {
                        if (CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckAuraCondition(_battle, _hero, _triggerHero, _targetHero, _sds))
                        {
                            _result += _sds.GetAuraData()[0];
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele1);

                    break;

                case AuraType.CAST_SKILL:

                    SuperEventListener.SuperFunctionCallBackV2<List<Func<BattleTriggerAuraVO>>, Hero, Hero> dele2 = delegate (int _index, ref List<Func<BattleTriggerAuraVO>> _list, Hero _triggerHero, Hero _targetHero)
                    {
                        if (CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckAuraCondition(_battle, _hero, _triggerHero, _targetHero, _sds))
                        {
                            if (_list == null)
                            {
                                _list = new List<Func<BattleTriggerAuraVO>>();
                            }

                            Func<BattleTriggerAuraVO> func = delegate ()
                            {
                                return AuraCastSkill(_battle, _hero, _triggerHero, _targetHero, _sds);
                            };

                            _list.Add(func);
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele2);

                    break;

                default:

                    throw new Exception("Unknown AuraType:" + _sds.GetAuraType().ToString());
            }

            return result;
        }

        private static BattleTriggerAuraVO AuraCastSkill(Battle _battle, Hero _hero, Hero _triggerHero, Hero _targetHero, IAuraSDS _sds)
        {
            Dictionary<int, List<BattleHeroEffectVO>> dic = new Dictionary<int, List<BattleHeroEffectVO>>();

            switch (_sds.GetAuraTarget())
            {
                case AuraTarget.OWNER:

                    List<BattleHeroEffectVO> list = new List<BattleHeroEffectVO>();

                    for (int m = 0; m < _sds.GetAuraData().Length; m++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _hero, _sds.GetAuraData()[m]);

                        list.Add(vo);
                    }

                    dic.Add(_hero.pos, list);

                    break;

                case AuraTarget.OWNER_NEIGHBOUR_ALLY:

                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    List<Hero> targetHerolist = null;

                    if (_sds.GetAuraTargetNum() > 0)
                    {
                        targetHerolist = new List<Hero>();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        Hero targetHero;

                        if (_battle.heroMapDic.TryGetValue(pos, out targetHero))
                        {
                            if (targetHero.isMine == _hero.isMine)
                            {
                                if (_sds.GetAuraTargetNum() > 0)
                                {
                                    targetHerolist.Add(targetHero);
                                }
                                else
                                {
                                    list = new List<BattleHeroEffectVO>();

                                    for (int m = 0; m < _sds.GetAuraData().Length; m++)
                                    {
                                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetAuraData()[m]);

                                        list.Add(vo);
                                    }

                                    dic.Add(targetHero.pos, list);
                                }
                            }
                        }
                    }

                    if (_sds.GetAuraTargetNum() > 0)
                    {
                        while (targetHerolist.Count > _sds.GetAuraTargetNum())
                        {
                            int index = _battle.GetRandomValue(targetHerolist.Count);

                            targetHerolist.RemoveAt(index);
                        }

                        for (int i = 0; i < targetHerolist.Count; i++)
                        {
                            Hero targetHero = targetHerolist[i];

                            list = new List<BattleHeroEffectVO>();

                            for (int m = 0; m < _sds.GetAuraData().Length; m++)
                            {
                                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetAuraData()[m]);

                                list.Add(vo);
                            }

                            dic.Add(targetHero.pos, list);
                        }
                    }

                    break;

                case AuraTarget.OWNER_NEIGHBOUR_ENEMY:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    targetHerolist = null;

                    if (_sds.GetAuraTargetNum() > 0)
                    {
                        targetHerolist = new List<Hero>();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        Hero targetHero;

                        if (_battle.heroMapDic.TryGetValue(pos, out targetHero))
                        {
                            if (targetHero.isMine != _hero.isMine)
                            {
                                if (_sds.GetAuraTargetNum() > 0)
                                {
                                    targetHerolist.Add(targetHero);
                                }
                                else
                                {
                                    list = new List<BattleHeroEffectVO>();

                                    for (int m = 0; m < _sds.GetAuraData().Length; m++)
                                    {
                                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetAuraData()[m]);

                                        list.Add(vo);
                                    }

                                    dic.Add(targetHero.pos, list);
                                }
                            }
                        }
                    }

                    if (_sds.GetAuraTargetNum() > 0)
                    {
                        while (targetHerolist.Count > _sds.GetAuraTargetNum())
                        {
                            int index = _battle.GetRandomValue(targetHerolist.Count);

                            targetHerolist.RemoveAt(index);
                        }

                        for (int i = 0; i < targetHerolist.Count; i++)
                        {
                            Hero targetHero = targetHerolist[i];

                            list = new List<BattleHeroEffectVO>();

                            for (int m = 0; m < _sds.GetAuraData().Length; m++)
                            {
                                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetAuraData()[m]);

                                list.Add(vo);
                            }

                            dic.Add(targetHero.pos, list);
                        }
                    }

                    break;

                case AuraTarget.TRIGGER:

                    list = new List<BattleHeroEffectVO>();

                    for (int m = 0; m < _sds.GetAuraData().Length; m++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _triggerHero, _sds.GetAuraData()[m]);

                        list.Add(vo);
                    }

                    dic.Add(_triggerHero.pos, list);

                    break;

                case AuraTarget.TRIGGER_TARGET:

                    list = new List<BattleHeroEffectVO>();

                    for (int m = 0; m < _sds.GetAuraData().Length; m++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _targetHero, _sds.GetAuraData()[m]);

                        list.Add(vo);
                    }

                    dic.Add(_targetHero.pos, list);

                    break;

                default:

                    throw new Exception("AuraCastSkill error! Unknown AuraTarget:" + _sds.GetAuraTarget());
            }

            BattleTriggerAuraVO result = new BattleTriggerAuraVO(_hero.pos, dic);

            return result;
        }

        private static bool CheckAuraCondition(Battle _battle, Hero _hero, Hero _triggerHero, Hero _targetHero, IAuraSDS _sds)
        {
            if (_sds.GetAuraCondition() != AuraCondition.NULL)
            {
                Hero firstHero;

                AuraTarget firstHeroTarget = _sds.GetAuraConditionTarget()[0];

                switch (firstHeroTarget)
                {
                    case AuraTarget.OWNER:

                        firstHero = _hero;

                        break;

                    case AuraTarget.TRIGGER:

                        firstHero = _triggerHero;

                        break;

                    case AuraTarget.TRIGGER_TARGET:

                        firstHero = _targetHero;

                        break;

                    default:

                        throw new Exception("CheckAuraCondition error0:" + firstHeroTarget);
                }

                if (firstHero == null)
                {
                    return false;
                }

                Hero secondHero = null;

                if (_sds.GetAuraConditionTarget().Length == 2)
                {
                    AuraTarget secondHeroTarget = _sds.GetAuraConditionTarget()[1];

                    switch (secondHeroTarget)
                    {
                        case AuraTarget.OWNER:

                            secondHero = _hero;

                            break;

                        case AuraTarget.TRIGGER:

                            secondHero = _triggerHero;

                            break;

                        case AuraTarget.TRIGGER_TARGET:

                            secondHero = _targetHero;

                            break;

                        default:

                            throw new Exception("CheckAuraCondition error1:" + secondHeroTarget);
                    }

                    if (secondHero == null)
                    {
                        return false;
                    }
                }

                return CheckAuraConditionReal(_battle, firstHero, secondHero, _sds);
            }

            return true;
        }

        private static bool CheckAuraTrigger(Battle _battle, Hero _hero, Hero _triggerHero, IAuraSDS _sds)
        {
            switch (_sds.GetAuraTrigger())
            {
                case AuraTarget.NULL:

                    return true;

                case AuraTarget.OWNER:

                    if (_triggerHero == _hero)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case AuraTarget.OWNER_NEIGHBOUR_ALLY:

                    if (_triggerHero != null && _hero.isMine == _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapWidth, _hero.pos, _triggerHero.pos) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case AuraTarget.OWNER_NEIGHBOUR_ENEMY:

                    if (_triggerHero != null && _hero.isMine != _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapWidth, _hero.pos, _triggerHero.pos) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:

                    throw new Exception("CheckAuraTrigger error:" + _sds.GetAuraTrigger());
            }
        }

        private static bool CheckAuraConditionReal(Battle _battle, Hero _firstHero, Hero _secondHero, IAuraSDS _sds)
        {
            switch (_sds.GetAuraCondition())
            {
                case AuraCondition.INJURED:

                    if (_firstHero.nowHp == _firstHero.sds.GetHp())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.HEALTHY:

                    if (_firstHero.nowHp < _firstHero.sds.GetHp())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.LEVEL_HIGHER_THAN:

                    if (_firstHero.sds.GetCost() <= _secondHero.sds.GetCost())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.LEVEL_LOWER_THAN:

                    if (_firstHero.sds.GetCost() >= _secondHero.sds.GetCost())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.NEIGHBOUR_ALLY_MORE_THAN:

                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _firstHero.pos);

                    int num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (_battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine == _firstHero.isMine)
                            {
                                num++;
                            }
                        }
                    }

                    if (num <= _sds.GetAuraConditionData())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.NEIGHBOUR_ALLY_LESS_THAN:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _firstHero.pos);

                    num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (_battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine == _firstHero.isMine)
                            {
                                num++;
                            }
                        }
                    }

                    if (num >= _sds.GetAuraConditionData())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.NEIGHBOUR_ENEMY_MORE_THAN:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _firstHero.pos);

                    num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (_battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine != _firstHero.isMine)
                            {
                                num++;
                            }
                        }
                    }

                    if (num <= _sds.GetAuraConditionData())
                    {
                        return false;
                    }

                    break;

                case AuraCondition.NEIGHBOUR_ENEMY_LESS_THAN:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _firstHero.pos);

                    num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (_battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine != _firstHero.isMine)
                            {
                                num++;
                            }
                        }
                    }

                    if (num >= _sds.GetAuraConditionData())
                    {
                        return false;
                    }

                    break;

                default:

                    throw new Exception("Unknown AuraCondition:" + _sds.GetAuraCondition());
            }

            return true;
        }
    }
}

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

            switch (_sds.GetEffectType())
            {
                case AuraType.FIX_BOOL:

                    SuperEventListener.SuperFunctionCallBackV2<bool, Hero, Hero> dele0 = delegate (int _index, ref bool _result, Hero _triggerHero, Hero _targetHero)
                    {
                        if (CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckAuraCondition(_battle, _hero, _triggerHero, _targetHero, _sds))
                        {
                            _result = _sds.GetEffectData()[0] == 1;
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele0, _sds.GetEventPriority());

                    break;

                case AuraType.FIX_INT:

                    SuperEventListener.SuperFunctionCallBackV2<int, Hero, Hero> dele1 = delegate (int _index, ref int _result, Hero _triggerHero, Hero _targetHero)
                    {
                        if (CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckAuraCondition(_battle, _hero, _triggerHero, _targetHero, _sds))
                        {
                            _result += _sds.GetEffectData()[0];
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele1, _sds.GetEventPriority());

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

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele2, _sds.GetEventPriority());

                    break;

                default:

                    throw new Exception("Unknown AuraType:" + _sds.GetEffectType().ToString());
            }

            return result;
        }

        private static BattleTriggerAuraVO AuraCastSkill(Battle _battle, Hero _hero, Hero _triggerHero, Hero _targetHero, IAuraSDS _sds)
        {
            Dictionary<int, List<BattleHeroEffectVO>> dic = new Dictionary<int, List<BattleHeroEffectVO>>();

            switch (_sds.GetEffectTarget())
            {
                case AuraTarget.OWNER:

                    List<BattleHeroEffectVO> list = new List<BattleHeroEffectVO>();

                    for (int m = 0; m < _sds.GetEffectData().Length; m++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _hero, _sds.GetEffectData()[m]);

                        list.Add(vo);
                    }

                    dic.Add(_hero.pos, list);

                    break;

                case AuraTarget.OWNER_NEIGHBOUR_ALLY:

                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    List<Hero> targetHerolist = null;

                    if (_sds.GetEffectTargetNum() > 0)
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
                                if (_sds.GetEffectTargetNum() > 0)
                                {
                                    targetHerolist.Add(targetHero);
                                }
                                else
                                {
                                    list = new List<BattleHeroEffectVO>();

                                    for (int m = 0; m < _sds.GetEffectData().Length; m++)
                                    {
                                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetEffectData()[m]);

                                        list.Add(vo);
                                    }

                                    dic.Add(targetHero.pos, list);
                                }
                            }
                        }
                    }

                    if (_sds.GetEffectTargetNum() > 0)
                    {
                        while (targetHerolist.Count > _sds.GetEffectTargetNum())
                        {
                            int index = _battle.GetRandomValue(targetHerolist.Count);

                            targetHerolist.RemoveAt(index);
                        }

                        for (int i = 0; i < targetHerolist.Count; i++)
                        {
                            Hero targetHero = targetHerolist[i];

                            list = new List<BattleHeroEffectVO>();

                            for (int m = 0; m < _sds.GetEffectData().Length; m++)
                            {
                                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetEffectData()[m]);

                                list.Add(vo);
                            }

                            dic.Add(targetHero.pos, list);
                        }
                    }

                    break;

                case AuraTarget.OWNER_NEIGHBOUR_ENEMY:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    targetHerolist = null;

                    if (_sds.GetEffectTargetNum() > 0)
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
                                if (_sds.GetEffectTargetNum() > 0)
                                {
                                    targetHerolist.Add(targetHero);
                                }
                                else
                                {
                                    list = new List<BattleHeroEffectVO>();

                                    for (int m = 0; m < _sds.GetEffectData().Length; m++)
                                    {
                                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetEffectData()[m]);

                                        list.Add(vo);
                                    }

                                    dic.Add(targetHero.pos, list);
                                }
                            }
                        }
                    }

                    if (_sds.GetEffectTargetNum() > 0)
                    {
                        while (targetHerolist.Count > _sds.GetEffectTargetNum())
                        {
                            int index = _battle.GetRandomValue(targetHerolist.Count);

                            targetHerolist.RemoveAt(index);
                        }

                        for (int i = 0; i < targetHerolist.Count; i++)
                        {
                            Hero targetHero = targetHerolist[i];

                            list = new List<BattleHeroEffectVO>();

                            for (int m = 0; m < _sds.GetEffectData().Length; m++)
                            {
                                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetEffectData()[m]);

                                list.Add(vo);
                            }

                            dic.Add(targetHero.pos, list);
                        }
                    }

                    break;

                case AuraTarget.TRIGGER:

                    list = new List<BattleHeroEffectVO>();

                    for (int m = 0; m < _sds.GetEffectData().Length; m++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _triggerHero, _sds.GetEffectData()[m]);

                        list.Add(vo);
                    }

                    dic.Add(_triggerHero.pos, list);

                    break;

                case AuraTarget.TRIGGER_TARGET:

                    list = new List<BattleHeroEffectVO>();

                    for (int m = 0; m < _sds.GetEffectData().Length; m++)
                    {
                        BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, _targetHero, _sds.GetEffectData()[m]);

                        list.Add(vo);
                    }

                    dic.Add(_targetHero.pos, list);

                    break;

                case AuraTarget.OWNER_NEIGHBOUR:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    targetHerolist = null;

                    if (_sds.GetEffectTargetNum() > 0)
                    {
                        targetHerolist = new List<Hero>();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        Hero targetHero;

                        if (_battle.heroMapDic.TryGetValue(pos, out targetHero))
                        {
                            if (_sds.GetEffectTargetNum() > 0)
                            {
                                targetHerolist.Add(targetHero);
                            }
                            else
                            {
                                list = new List<BattleHeroEffectVO>();

                                for (int m = 0; m < _sds.GetEffectData().Length; m++)
                                {
                                    BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetEffectData()[m]);

                                    list.Add(vo);
                                }

                                dic.Add(targetHero.pos, list);
                            }
                        }
                    }

                    if (_sds.GetEffectTargetNum() > 0)
                    {
                        while (targetHerolist.Count > _sds.GetEffectTargetNum())
                        {
                            int index = _battle.GetRandomValue(targetHerolist.Count);

                            targetHerolist.RemoveAt(index);
                        }

                        for (int i = 0; i < targetHerolist.Count; i++)
                        {
                            Hero targetHero = targetHerolist[i];

                            list = new List<BattleHeroEffectVO>();

                            for (int m = 0; m < _sds.GetEffectData().Length; m++)
                            {
                                BattleHeroEffectVO vo = HeroEffect.HeroTakeEffect(_battle, targetHero, _sds.GetEffectData()[m]);

                                list.Add(vo);
                            }

                            dic.Add(targetHero.pos, list);
                        }
                    }

                    break;

                default:

                    throw new Exception("AuraCastSkill error! Unknown AuraTarget:" + _sds.GetEffectTarget());
            }

            BattleTriggerAuraVO result = new BattleTriggerAuraVO(_hero.pos, dic);

            return result;
        }

        private static bool CheckAuraCondition(Battle _battle, Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, IAuraSDS _sds)
        {
            if (_sds.GetConditionCompare() != AuraConditionCompare.NULL)
            {
                return CheckAuraConditionReal(_battle, _hero, _triggerHero, _triggerTargetHero, _sds);
            }
            else
            {
                return true;
            }
        }

        private static bool CheckAuraTrigger(Battle _battle, Hero _hero, Hero _triggerHero, IAuraSDS _sds)
        {
            switch (_sds.GetTriggerTarget())
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

                    if (_triggerHero != null && _hero.isMine == _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapHeight, _hero.pos, _triggerHero.pos) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case AuraTarget.OWNER_NEIGHBOUR_ENEMY:

                    if (_triggerHero != null && _hero.isMine != _triggerHero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapHeight, _hero.pos, _triggerHero.pos) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case AuraTarget.OWNER_NEIGHBOUR:

                    if (_triggerHero != null && BattlePublicTools.GetDistance(_battle.mapData.mapHeight, _hero.pos, _triggerHero.pos) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:

                    throw new Exception("CheckAuraTrigger error:" + _sds.GetTriggerTarget());
            }
        }

        private static bool CheckAuraConditionReal(Battle _battle, Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, IAuraSDS _sds)
        {
            int first;

            int second;

            AuraConditionType conditionType = _sds.GetConditionType()[0];

            if (conditionType == AuraConditionType.DATA)
            {
                first = _sds.GetConditionData()[0];
            }
            else
            {
                Hero hero = GetConditionHero(_hero, _triggerHero, _triggerTargetHero, _sds.GetConditionTarget()[0]);

                first = GetConditionData(_battle, hero, conditionType);
            }

            conditionType = _sds.GetConditionType()[1];

            if (conditionType == AuraConditionType.DATA)
            {
                second = _sds.GetConditionData()[1];
            }
            else
            {
                Hero hero = GetConditionHero(_hero, _triggerHero, _triggerTargetHero, _sds.GetConditionTarget()[1]);

                second = GetConditionData(_battle, hero, conditionType);
            }

            switch (_sds.GetConditionCompare())
            {
                case AuraConditionCompare.EQUAL:

                    return first == second;

                case AuraConditionCompare.BIGGER:

                    return first > second;

                default:

                    return first < second;
            }
        }

        private static Hero GetConditionHero(Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, AuraTarget _conditionTarget)
        {
            switch (_conditionTarget)
            {
                case AuraTarget.OWNER:

                    return _hero;

                case AuraTarget.TRIGGER:

                    return _triggerHero;

                case AuraTarget.TRIGGER_TARGET:

                    return _triggerTargetHero;

                default:

                    throw new Exception("Unknown auraConditionTarget:" + _conditionTarget);
            }
        }

        private static int GetConditionData(Battle _battle, Hero _hero, AuraConditionType _type)
        {
            switch (_type)
            {
                case AuraConditionType.LEVEL:

                    return _hero.sds.GetCost();

                case AuraConditionType.ATTACK:

                    return _hero.sds.GetAttack();

                case AuraConditionType.MAXHP:

                    return _hero.sds.GetHp();

                case AuraConditionType.NOWHP:

                    return _hero.nowHp;

                case AuraConditionType.NEIGHBOUR_ALLY_NUM:

                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    int num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (_battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine == _hero.isMine)
                            {
                                num++;
                            }
                        }
                    }

                    return num;

                case AuraConditionType.NEIGHBOUR_ENEMY_NUM:

                    tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (_battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine != _hero.isMine)
                            {
                                num++;
                            }
                        }
                    }

                    return num;

                default:

                    throw new Exception("Unknown AuraConditionType:" + _type);
            }
        }
    }
}

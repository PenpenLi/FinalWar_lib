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

            for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
            {
                int id = _hero.sds.GetAuras()[i];

                Init(_battle, _hero, id, true);
            }
        }

        internal static void Init(Battle _battle, Hero _hero, int _auraID, bool _isInBorn)
        {
            IAuraSDS sds = Battle.GetAuraData(_auraID);

            List<int> ids = new List<int>();

            int id = RegisterAura(_battle, _hero, sds, _isInBorn);

            ids.Add(id);

            SuperEventListener.SuperFunctionCallBackV2<List<Func<BattleTriggerAuraVO>>, Hero, Hero> dele = delegate (int _index, ref List<Func<BattleTriggerAuraVO>> _funcList, Hero _triggerHero, Hero _triggerTargetHero)
            {
                if (_triggerHero == _hero)
                {
                    for (int i = 0; i < ids.Count; i++)
                    {
                        _battle.eventListener.RemoveListener(ids[i]);
                    }
                }
            };

            id = _battle.eventListener.AddListener(BattleConst.DIE, dele, BattleConst.MAX_PRIORITY - 1);

            ids.Add(id);

            if (_isInBorn)
            {
                id = _battle.eventListener.AddListener(BattleConst.REMOVE_BORN_AURA, dele, BattleConst.MAX_PRIORITY - 1);

                ids.Add(id);
            }
            else
            {
                id = _battle.eventListener.AddListener(BattleConst.BE_CLEAN, dele, BattleConst.MAX_PRIORITY - 1);

                ids.Add(id);

                SuperEventListener.SuperFunctionCallBackV1<List<int>, Hero> dele2 = delegate (int _index, ref List<int> _list, Hero _triggerHero)
                {
                    if (_triggerHero == _hero)
                    {
                        if (_list == null)
                        {
                            _list = new List<int>();
                        }

                        _list.Add(sds.GetID());
                    }
                };

                id = _battle.eventListener.AddListener(BattleConst.GET_AURA_DESC, dele2);

                ids.Add(id);
            }

            for (int i = 0; i < sds.GetRemoveEventNames().Length; i++)
            {
                id = _battle.eventListener.AddListener(sds.GetRemoveEventNames()[i], dele, BattleConst.MAX_PRIORITY - 1);

                ids.Add(id);
            }
        }

        private static int RegisterAura(Battle _battle, Hero _hero, IAuraSDS _sds, bool _isInBorn)
        {
            int result;

            switch (_sds.GetEffectType())
            {
                case AuraType.FIX_INT:

                    SuperEventListener.SuperFunctionCallBackV2<int, Hero, Hero> dele1 = delegate (int _index, ref int _result, Hero _triggerHero, Hero _triggerTargetHero)
                    {
                        if (CheckAuraIsBeSilenced(_battle, _hero, _isInBorn) && CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckCondition(_battle, _hero, _triggerHero, _triggerTargetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                        {
                            Hero.HeroData heroData = (Hero.HeroData)(_sds.GetEffectData()[0]);

                            if (heroData == Hero.HeroData.DATA)
                            {
                                _result += _sds.GetEffectData()[1];

                            }
                            else
                            {
                                _result += _hero.GetData(heroData) * _sds.GetEffectData()[1];
                            }
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele1);

                    break;

                case AuraType.CAST_SKILL:

                    IEffectSDS effectSDS = Battle.GetEffectData(_sds.GetEffectData()[0]);

                    int priority = effectSDS.GetPriority();

                    SuperEventListener.SuperFunctionCallBackV2<List<Func<BattleTriggerAuraVO>>, Hero, Hero> dele2 = delegate (int _index, ref List<Func<BattleTriggerAuraVO>> _funcList, Hero _triggerHero, Hero _triggerTargetHero)
                    {
                        if (CheckAuraIsBeSilenced(_battle, _hero, _isInBorn) && CheckAuraTrigger(_battle, _hero, _triggerHero, _sds) && CheckCondition(_battle, _hero, _triggerHero, _triggerTargetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                        {
                            Func<BattleTriggerAuraVO> func = delegate ()
                            {
                                return AuraCastSkill(_battle, _hero, _triggerHero, _triggerTargetHero, _sds);
                            };

                            if (_funcList == null)
                            {
                                _funcList = new List<Func<BattleTriggerAuraVO>>();
                            }

                            _funcList.Add(func);
                        }
                    };

                    result = _battle.eventListener.AddListener(_sds.GetEventName(), dele2);

                    break;

                default:

                    throw new Exception("Unknown AuraType:" + _sds.GetEffectType().ToString());
            }

            return result;
        }

        private static BattleTriggerAuraVO AuraCastSkill(Battle _battle, Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, IAuraSDS _sds)
        {
            Dictionary<int, List<BattleHeroEffectVO>> dic = new Dictionary<int, List<BattleHeroEffectVO>>();

            AuraCastSkillReal(_battle, _hero, _triggerHero, _triggerTargetHero, _sds, dic);

            BattleTriggerAuraVO result = new BattleTriggerAuraVO(_hero.pos, dic);

            return result;
        }

        private static void AuraCastSkillReal(Battle _battle, Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, IAuraSDS _sds, Dictionary<int, List<BattleHeroEffectVO>> _dic)
        {
            IEffectSDS effectSDS = Battle.GetEffectData(_sds.GetEffectData()[0]);

            switch (_sds.GetEffectTarget())
            {
                case AuraTarget.OWNER:

                    List<BattleHeroEffectVO> vo = HeroEffect.HeroTakeEffect(_battle, _hero, effectSDS);

                    _dic.Add(_hero.pos, vo);

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
                            if (targetHero.isMine == _hero.isMine && CheckCondition(_battle, _hero, _triggerHero, targetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                            {
                                if (_sds.GetEffectTargetNum() > 0)
                                {
                                    targetHerolist.Add(targetHero);
                                }
                                else
                                {
                                    vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                                    _dic.Add(targetHero.pos, vo);
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

                            vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                            _dic.Add(targetHero.pos, vo);
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
                            if (targetHero.isMine != _hero.isMine && CheckCondition(_battle, _hero, _triggerHero, targetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                            {
                                if (_sds.GetEffectTargetNum() > 0)
                                {
                                    targetHerolist.Add(targetHero);
                                }
                                else
                                {
                                    vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                                    _dic.Add(targetHero.pos, vo);
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

                            vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                            _dic.Add(targetHero.pos, vo);
                        }
                    }

                    break;

                case AuraTarget.TRIGGER:

                    vo = HeroEffect.HeroTakeEffect(_battle, _triggerHero, effectSDS);

                    _dic.Add(_triggerHero.pos, vo);

                    break;

                case AuraTarget.TRIGGER_TARGET:

                    vo = HeroEffect.HeroTakeEffect(_battle, _triggerTargetHero, effectSDS);

                    _dic.Add(_triggerTargetHero.pos, vo);

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

                        if (_battle.heroMapDic.TryGetValue(pos, out targetHero) && CheckCondition(_battle, _hero, _triggerHero, targetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                        {
                            if (_sds.GetEffectTargetNum() > 0)
                            {
                                targetHerolist.Add(targetHero);
                            }
                            else
                            {
                                vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                                _dic.Add(targetHero.pos, vo);
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

                            vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                            _dic.Add(targetHero.pos, vo);
                        }
                    }

                    break;

                case AuraTarget.OWNER_ALLY:

                    targetHerolist = null;

                    if (_sds.GetEffectTargetNum() > 0)
                    {
                        targetHerolist = new List<Hero>();
                    }

                    IEnumerator<Hero> enumerator = _battle.heroMapDic.Values.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        Hero targetHero = enumerator.Current;

                        if (targetHero != _hero && targetHero.isMine == _hero.isMine && CheckCondition(_battle, _hero, _triggerHero, targetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                        {
                            if (_sds.GetEffectTargetNum() > 0)
                            {
                                targetHerolist.Add(targetHero);
                            }
                            else
                            {
                                vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                                _dic.Add(targetHero.pos, vo);
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

                            vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                            _dic.Add(targetHero.pos, vo);
                        }
                    }

                    break;

                case AuraTarget.OWNER_ENEMY:

                    targetHerolist = null;

                    if (_sds.GetEffectTargetNum() > 0)
                    {
                        targetHerolist = new List<Hero>();
                    }

                    enumerator = _battle.heroMapDic.Values.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        Hero targetHero = enumerator.Current;

                        if (targetHero.isMine != _hero.isMine && CheckCondition(_battle, _hero, _triggerHero, targetHero, _sds.GetConditionCompare(), _sds.GetConditionType(), _sds.GetConditionData()))
                        {
                            if (_sds.GetEffectTargetNum() > 0)
                            {
                                targetHerolist.Add(targetHero);
                            }
                            else
                            {
                                vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                                _dic.Add(targetHero.pos, vo);
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

                            vo = HeroEffect.HeroTakeEffect(_battle, targetHero, effectSDS);

                            _dic.Add(targetHero.pos, vo);
                        }
                    }

                    break;

                default:

                    throw new Exception("AuraCastSkill error! Unknown AuraTarget:" + _sds.GetEffectTarget());
            }
        }

        private static bool CheckAuraIsBeSilenced(Battle _battle, Hero _hero, bool _isInBorn)
        {
            if (_isInBorn)
            {
                bool canTrigger = true;

                _battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.TRIGGER_BORN_AURA, ref canTrigger, _hero, null);

                if (!canTrigger)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CheckCondition(Battle _battle, Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, AuraConditionCompare _conditionCompare, Hero.HeroData[] _conditionType, int[] _conditionData)
        {
            if (_conditionCompare != AuraConditionCompare.NULL)
            {
                return CheckConditionReal(_battle, _hero, _triggerHero, _triggerTargetHero, _conditionCompare, _conditionType, _conditionData);
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

                    if (_triggerHero != null && _triggerHero.isMine == _hero.isMine && _triggerHero != _hero && BattlePublicTools.GetDistance(_battle.mapData.mapHeight, _hero.pos, _triggerHero.pos) == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case AuraTarget.OWNER_NEIGHBOUR_ENEMY:

                    if (_triggerHero != null && _triggerHero.isMine != _hero.isMine && BattlePublicTools.GetDistance(_battle.mapData.mapHeight, _hero.pos, _triggerHero.pos) == 1)
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

                case AuraTarget.OWNER_ALLY:

                    if (_triggerHero != null && _triggerHero.isMine == _hero.isMine && _triggerHero != _hero)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case AuraTarget.OWNER_ENEMY:

                    if (_triggerHero != null && _triggerHero.isMine != _hero.isMine)
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

        private static bool CheckConditionReal(Battle _battle, Hero _hero, Hero _triggerHero, Hero _triggerTargetHero, AuraConditionCompare _conditionCompare, Hero.HeroData[] _conditionType, int[] _conditionData)
        {
            int first;

            int second;

            Hero.HeroData heroData = _conditionType[0];

            if (heroData == Hero.HeroData.DATA)
            {
                first = _conditionData[0];
            }
            else
            {
                Hero hero = GetConditionHero(_hero, _triggerHero, _triggerTargetHero, (AuraTarget)_conditionData[0]);

                if (hero == null)
                {
                    return false;
                }

                first = hero.GetData(heroData);
            }

            heroData = _conditionType[1];

            if (heroData == Hero.HeroData.DATA)
            {
                second = _conditionData[1];
            }
            else
            {
                Hero hero = GetConditionHero(_hero, _triggerHero, _triggerTargetHero, (AuraTarget)_conditionData[1]);

                if (hero == null)
                {
                    return false;
                }

                second = hero.GetData(heroData);
            }

            switch (_conditionCompare)
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
    }
}

using System;
using System.Collections.Generic;
using superEvent;

namespace FinalWar
{
    internal class HeroSkill
    {
        internal static string GetEventName(int _heroUid, SkillTime _skillTime)
        {
            return string.Format("{0}_{1}", _heroUid, _skillTime);
        }

        internal static string GetEventName(int _heroUid, string _skillTime)
        {
            return string.Format("{0}_{1}", _heroUid, _skillTime);
        }

        internal static void Add(Battle _battle, Hero _hero)
        {
            if (_hero.sds.GetSkills().Length > 0)
            {
                int[] eventIDs = new int[_hero.sds.GetSkills().Length];

                for (int i = 0; i < _hero.sds.GetSkills().Length; i++)
                {
                    int skillID = _hero.sds.GetSkills()[i];

                    ISkillSDS skillSDS = Battle.GetSkillData(skillID);

                    Action<SuperEvent> dele = delegate (SuperEvent e)
                    {
                        TriggerSkill(_battle, _hero, skillSDS, e);
                    };

                    eventIDs[i] = _battle.eventListener.AddListener(GetEventName(_hero.uid, skillSDS.GetSkillTime()), dele, skillSDS.GetPriority());
                }

                int dieEventID = 0;

                int removeEventID = 0;

                Action<SuperEvent> removeDele = delegate (SuperEvent e)
                {
                    for (int i = 0; i < eventIDs.Length; i++)
                    {
                        _battle.eventListener.RemoveListener(eventIDs[i]);
                    }

                    _battle.eventListener.RemoveListener(dieEventID);

                    _battle.eventListener.RemoveListener(removeEventID);
                };

                dieEventID = _battle.eventListener.AddListener(GetEventName(_hero.uid, SkillTime.DIE), removeDele, SuperEventListener.MAX_PRIORITY - 1);

                removeEventID = _battle.eventListener.AddListener(GetEventName(_hero.uid, Battle.REMOVE_EVENT_NAME), removeDele, SuperEventListener.MAX_PRIORITY - 1);
            }
        }

        private static void TriggerSkill(Battle _battle, Hero _hero, ISkillSDS _skillSDS, SuperEvent e)
        {
            bool castSkill = true;

            _battle.eventListenerV.DispatchEvent<bool>(AuraEffect.SILENT.ToString(), ref castSkill, _hero);

            if (!castSkill)
            {
                return;
            }

            switch (_skillSDS.GetSkillTime())
            {
                case SkillTime.SHOOT:

                case SkillTime.RUSH:

                case SkillTime.ATTACK:

                case SkillTime.HELP:

                case SkillTime.COUNTER:

                case SkillTime.SUPPORT:

                    ShootRushAttackHelpCounter(_battle, _hero, _skillSDS, e.datas[1] as List<Hero>, e.datas[2] as List<Hero>, e.datas[3] as Dictionary<Hero, int>, e.datas[4] as Dictionary<Hero, int>, e.datas[5] as Dictionary<Hero, int>, e.datas[6] as LinkedList<IBattleVO>);

                    break;

                case SkillTime.ROUNDSTART:

                case SkillTime.SUMMON:

                case SkillTime.RECOVER:

                case SkillTime.DIE:

                case SkillTime.CAPTURE:

                    RoundStartSummonRecoverDieCapture(_battle, _hero, _skillSDS, e.datas[0] as Dictionary<Hero, int>, e.datas[1] as Dictionary<Hero, int>, e.datas[2] as Dictionary<Hero, int>, e.datas[3] as LinkedList<IBattleVO>);

                    break;
            }
        }

        private static void RoundStartSummonRecoverDieCapture(Battle _battle, Hero _hero, ISkillSDS _skillSDS, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_battle, _skillSDS, new List<Hero>() { _hero }, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);

                    break;

                case SkillTarget.ALLY:

                    List<Hero> heros = null;

                    LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

                        if (_battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = _battle.heroMapDic[pos];

                            if (tmpHero.isMine == _hero.isMine)
                            {
                                if (heros == null)
                                {
                                    heros = new List<Hero>();
                                }

                                heros.Add(tmpHero);
                            }
                        }
                    }

                    if (heros != null)
                    {
                        if (_skillSDS.GetTargetNum() != -1)
                        {
                            while (heros.Count > _skillSDS.GetTargetNum())
                            {
                                int index = Battle.random.Next(heros.Count);

                                heros.RemoveAt(index);
                            }
                        }

                        SkillTakeEffect(_battle, _skillSDS, heros, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);
                    }

                    break;

                case SkillTarget.ENEMY:

                    heros = null;

                    posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    enumerator = posList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

                        if (_battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = _battle.heroMapDic[pos];

                            if (tmpHero.isMine != _hero.isMine)
                            {
                                if (heros == null)
                                {
                                    heros = new List<Hero>();
                                }

                                heros.Add(tmpHero);
                            }
                        }
                    }

                    if (heros != null)
                    {
                        if (_skillSDS.GetTargetNum() != -1)
                        {
                            while (heros.Count > _skillSDS.GetTargetNum())
                            {
                                int index = Battle.random.Next(heros.Count);

                                heros.RemoveAt(index);
                            }
                        }

                        SkillTakeEffect(_battle, _skillSDS, heros, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);
                    }

                    break;
            }
        }

        private static void ShootRushAttackHelpCounter(Battle _battle, Hero _hero, ISkillSDS _skillSDS, List<Hero> _myHeros, List<Hero> _oppHeros, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_battle, _skillSDS, new List<Hero>() { _hero }, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);

                    break;

                case SkillTarget.ALLY:

                    List<Hero> myHeros = new List<Hero>(_myHeros);

                    if (myHeros.Contains(_hero))
                    {
                        myHeros.Remove(_hero);
                    }

                    if (_skillSDS.GetTargetNum() != -1 && myHeros.Count > _skillSDS.GetTargetNum())
                    {
                        myHeros.RemoveRange(_skillSDS.GetTargetNum(), myHeros.Count - _skillSDS.GetTargetNum());
                    }

                    SkillTakeEffect(_battle, _skillSDS, myHeros, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);

                    break;

                case SkillTarget.ENEMY:

                    List<Hero> oppHeros = new List<Hero>(_oppHeros);

                    if (_skillSDS.GetTargetNum() != -1 && oppHeros.Count > _skillSDS.GetTargetNum())
                    {
                        oppHeros.RemoveRange(_skillSDS.GetTargetNum(), oppHeros.Count - _skillSDS.GetTargetNum());
                    }

                    SkillTakeEffect(_battle, _skillSDS, oppHeros, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);

                    break;
            }
        }

        private static void SkillTakeEffect(Battle _battle, ISkillSDS _skillSDS, List<Hero> _heros, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList)
        {
            switch (_skillSDS.GetSkillEffect())
            {
                case SkillEffect.DAMAGE:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        BattlePublicTools.AccumulateDicData(_damageDic, _heros[i], _skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.SHIELD_CHANGE:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        BattlePublicTools.AccumulateDicData(_shieldChangeDic, _heros[i], _skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.HP_CHANGE:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        BattlePublicTools.AccumulateDicData(_hpChangeDic, _heros[i], _skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.RECOVER_ALL_HP:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        Hero hero = _heros[i];

                        if (hero.sds.GetHp() > hero.nowHp)
                        {
                            BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, hero.sds.GetHp() - hero.nowHp);
                        }
                    }

                    break;

                case SkillEffect.FIX_ATTACK:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].SetAttackFix(_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.FIX_ABILITY:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].SetAbilityFix(_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.LEVEL_UP:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _battle.HeroLevelUp(_heros[i], _skillSDS.GetSkillDatas()[0], _voList);
                    }

                    break;

                case SkillEffect.DISABLE_RECOVER_SHIELD:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].DisableRecoverShield();
                    }

                    break;

                case SkillEffect.ADD_CARDS:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _battle.ServerAddCards(_heros[i].isMine, _skillSDS.GetSkillDatas()[0], _voList);
                    }

                    break;

                case SkillEffect.DEL_CARDS:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _battle.ServerDelCards(_heros[i].isMine, _skillSDS.GetSkillDatas()[0], _voList);
                    }

                    break;

                case SkillEffect.DISABLE_MOVE:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].DisableMove();
                    }

                    break;
            }
        }
    }
}

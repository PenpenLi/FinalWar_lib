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

        internal static void Init(Battle _battle,Hero _hero)
        {
            int[] eventIDs = new int[_hero.sds.GetSkills().Length];

            for (int i = 0; i < _hero.sds.GetSkills().Length; i++)
            {
                int skillID = _hero.sds.GetSkills()[i];

                ISkillSDS skillSDS = Battle.GetSkillData(skillID);

                Action<SuperEvent> dele = delegate(SuperEvent e)
                {
                    TriggerSkill(_battle, _hero, skillSDS, e);
                };

                eventIDs[i] = _battle.eventListener.AddListener(GetEventName(_hero.uid, skillSDS.GetSkillTime()), dele);
            }

            Action<SuperEvent> dieDele = delegate (SuperEvent e)
            {
                for (int i = 0; i < eventIDs.Length; i++)
                {
                    _battle.eventListener.RemoveListener(eventIDs[i]);
                }

                _battle.eventListener.RemoveListener(e.index);
            };

            _battle.eventListener.AddListener(GetEventName(_hero.uid, SkillTime.DIE), dieDele);
        }

        private static void TriggerSkill(Battle _battle, Hero _hero, ISkillSDS _skillSDS, SuperEvent e)
        {
            switch (_skillSDS.GetSkillTime())
            {
                case SkillTime.SHOOT:

                case SkillTime.RUSH:

                case SkillTime.ATTACK:
                    
                case SkillTime.COUNTER:

                    ShootRushAttackCounter(_battle, _hero, _skillSDS, (int)e.datas[0], e.datas[1] as List<Hero>, e.datas[2] as List<Hero>, e.datas[3] as Dictionary<Hero, int>, e.datas[4] as Dictionary<Hero, int>);

                    break;

                case SkillTime.ROUNDSTART:

                case SkillTime.SUMMON:

                case SkillTime.RECOVER:

                case SkillTime.DIE:

                    RoundStartSummonRecoverDie(_battle, _hero, _skillSDS, e.datas[0] as Dictionary<Hero, int>, e.datas[1] as Dictionary<Hero, int>);

                    break;
            }
        }

        private static void RoundStartSummonRecoverDie(Battle _battle, Hero _hero, ISkillSDS _skillSDS, Dictionary<Hero,int> _hpChangeDic, Dictionary<Hero,int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { _hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:

                    List<Hero> heros = null;

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, _hero.pos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

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
                        while (heros.Count > _skillSDS.GetTargetNum())
                        {
                            int index = (int)(Battle.random.NextDouble() * heros.Count);

                            heros.RemoveAt(index);
                        }

                        SkillTakeEffect(_skillSDS, heros, _hpChangeDic, _powerChangeDic);
                    }

                    break;

                case SkillTarget.ENEMY:

                    heros = null;

                    posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, _hero.pos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

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
                        while (heros.Count > _skillSDS.GetTargetNum())
                        {
                            int index = (int)(Battle.random.NextDouble() * heros.Count);

                            heros.RemoveAt(index);
                        }

                        SkillTakeEffect(_skillSDS, heros, _hpChangeDic, _powerChangeDic);
                    }

                    break;
            }
        }

        private static void ShootRushAttackCounter(Battle _battle, Hero _hero, ISkillSDS _skillSDS, int _pos, List<Hero> _myHeros, List<Hero> _oppHeros, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { _hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:
                    
                    List<Hero> myHeros = new List<Hero>(_myHeros);

                    if (myHeros.Count > _skillSDS.GetTargetNum())
                    {
                        myHeros.RemoveRange(_skillSDS.GetTargetNum(), myHeros.Count - _skillSDS.GetTargetNum());
                    }

                    SkillTakeEffect(_skillSDS, myHeros, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ENEMY:

                    List<Hero> oppHeros = new List<Hero>(_oppHeros);

                    if (oppHeros.Count > _skillSDS.GetTargetNum())
                    {
                        oppHeros.RemoveRange(_skillSDS.GetTargetNum(), oppHeros.Count - _skillSDS.GetTargetNum());
                    }

                    SkillTakeEffect(_skillSDS, oppHeros, _hpChangeDic, _powerChangeDic);

                    break;
            }
        }

        private static void SkillTakeEffect(ISkillSDS _skillSDS, List<Hero> _heros, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillEffect())
            {
                case SkillEffect.DAMAGE:
                case SkillEffect.DAMAGE_WITH_LEADER:
                case SkillEffect.RECOVER:

                    for(int i = 0; i < _heros.Count; i++)
                    {
                        BattlePublicTools.AccumulateDicData(_hpChangeDic, _heros[i], (int)_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.FIX_ATTACK:

                    for(int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].SetAttackFix(_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.FIX_SHOOT:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].SetShootFix(_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.FIX_COUNTER:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].SetCounterFix(_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.FIX_DEFENSE:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        _heros[i].SetDfenseFix(_skillSDS.GetSkillDatas()[0]);
                    }

                    break;

                case SkillEffect.POWERCHANGE:

                    for (int i = 0; i < _heros.Count; i++)
                    {
                        BattlePublicTools.AccumulateDicData(_powerChangeDic, _heros[i], (int)_skillSDS.GetSkillDatas()[0]);
                    }

                    break;
            }
        }
    }
}

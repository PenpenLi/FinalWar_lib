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

        private Battle battle;
        private Hero hero;

        private int[] eventIDs;

        internal HeroSkill(Battle _battle,Hero _hero)
        {
            battle = _battle;
            hero = _hero;

            eventIDs = new int[hero.sds.GetSkills().Length];

            for (int i = 0; i < hero.sds.GetSkills().Length; i++)
            {
                int skillID = hero.sds.GetSkills()[i];

                ISkillSDS skillSDS = Battle.skillDataDic[skillID];

                string eventName = GetEventName(hero.uid, skillSDS.GetSkillTime());

                Action<SuperEvent> dele = delegate(SuperEvent e)
                {
                    TriggerSkill(skillSDS, e);
                };

                eventIDs[i] = battle.eventListener.AddListener(eventName, dele);
            }
        }

        private void TriggerSkill(ISkillSDS _skillSDS, SuperEvent e)
        {
            switch (_skillSDS.GetSkillTime())
            {
                case SkillTime.SHOOT:

                    Shoot(_skillSDS, e.datas[0] as Hero, e.datas[1] as Dictionary<Hero, int>, e.datas[2] as Dictionary<Hero, int>);

                    break;

                case SkillTime.RUSH:

                    Rush(_skillSDS, e.datas[0] as List<Hero>, e.datas[1] as Hero, e.datas[2] as Dictionary<Hero, int>, e.datas[3] as Dictionary<Hero, int>);

                    break;

                case SkillTime.ATTACK:

                    Attack(_skillSDS, e.datas[0] as List<Hero>, e.datas[1] as List<Hero>, e.datas[2] as Dictionary<Hero, int>, e.datas[3] as Dictionary<Hero, int>);

                    break;

                case SkillTime.COUNTER:

                    Counter(_skillSDS, e.datas[0] as List<Hero>, e.datas[1] as List<Hero>, e.datas[2] as Dictionary<Hero, int>, e.datas[3] as Dictionary<Hero, int>);

                    break;

                case SkillTime.SUMMON:

                    Summon(_skillSDS, e.datas[0] as Dictionary<int, int>, e.datas[1] as Dictionary<Hero, int>, e.datas[2] as Dictionary<Hero, int>);

                    break;

                case SkillTime.RECOVER:

                    Recover(_skillSDS, e.datas[0] as Dictionary<Hero, int>, e.datas[1] as Dictionary<Hero, int>);

                    break;
            }
        }

        private void Summon(ISkillSDS _skillSDS, Dictionary<int,int> _summon,Dictionary<Hero,int> _hpChangeDic,Dictionary<Hero,int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:

                    List<Hero> heros = null;

                    List<int> posList = BattlePublicTools.GetNeighbourPos(battle.mapData.neighbourPosMap, hero.pos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        if (!_summon.ContainsValue(pos) && battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = battle.heroMapDic[pos];

                            if (tmpHero.isMine == hero.isMine)
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

                    posList = BattlePublicTools.GetNeighbourPos(battle.mapData.neighbourPosMap, hero.pos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        if (!_summon.ContainsValue(pos) && battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = battle.heroMapDic[pos];

                            if (tmpHero.isMine != hero.isMine)
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

        private void Recover(ISkillSDS _skillSDS, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:

                    List<Hero> heros = null;

                    List<int> posList = BattlePublicTools.GetNeighbourPos(battle.mapData.neighbourPosMap, hero.pos);

                    for(int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        if (battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = battle.heroMapDic[pos];

                            if(tmpHero.isMine == hero.isMine)
                            {
                                if(heros == null)
                                {
                                    heros = new List<Hero>();
                                }

                                heros.Add(tmpHero);
                            }
                        }
                    }

                    if(heros != null)
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

                    posList = BattlePublicTools.GetNeighbourPos(battle.mapData.neighbourPosMap, hero.pos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        if (battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = battle.heroMapDic[pos];

                            if (tmpHero.isMine != hero.isMine)
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

        private void Rush(ISkillSDS _skillSDS, List<Hero> _attackers, Hero _stander, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:

                    if (_attackers.Count > 1)
                    {
                        List<Hero> attackers = new List<Hero>(_attackers);

                        attackers.Remove(hero);

                        while (attackers.Count > _skillSDS.GetTargetNum())
                        {
                            int index = (int)(Battle.random.NextDouble() * attackers.Count);

                            attackers.RemoveAt(index);
                        }

                        SkillTakeEffect(_skillSDS, attackers, _hpChangeDic, _powerChangeDic);
                    }

                    break;

                case SkillTarget.ENEMY:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { _stander }, _hpChangeDic, _powerChangeDic);

                    break;
            }
        }

        private void Shoot(ISkillSDS _skillSDS, Hero _target, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ENEMY:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { _target }, _hpChangeDic, _powerChangeDic);

                    break;
            }
        }

        private void Attack(ISkillSDS _skillSDS, List<Hero> _attackers, List<Hero> _supporters, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:

                    if(_attackers.Count > 1)
                    {
                        List<Hero> attackers = new List<Hero>(_attackers);

                        attackers.Remove(hero);

                        while (attackers.Count > _skillSDS.GetTargetNum())
                        {
                            int index = (int)(Battle.random.NextDouble() * attackers.Count);

                            attackers.RemoveAt(index);
                        }

                        SkillTakeEffect(_skillSDS, attackers, _hpChangeDic, _powerChangeDic);
                    }

                    break;

                case SkillTarget.ENEMY:

                    List<Hero> supporters = new List<Hero>(_supporters);

                    while (supporters.Count > _skillSDS.GetTargetNum())
                    {
                        int index = (int)(Battle.random.NextDouble() * supporters.Count);

                        supporters.RemoveAt(index);
                    }

                    SkillTakeEffect(_skillSDS, supporters, _hpChangeDic, _powerChangeDic);

                    break;
            }
        }

        private void Counter(ISkillSDS _skillSDS, List<Hero> _attackers, List<Hero> _supporters, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {
            switch (_skillSDS.GetSkillTarget())
            {
                case SkillTarget.SELF:

                    SkillTakeEffect(_skillSDS, new List<Hero>() { hero }, _hpChangeDic, _powerChangeDic);

                    break;

                case SkillTarget.ALLY:

                    if (_supporters.Count > 1)
                    {
                        List<Hero> supporters = new List<Hero>(_supporters);

                        supporters.Remove(hero);

                        while (supporters.Count > _skillSDS.GetTargetNum())
                        {
                            int index = (int)(Battle.random.NextDouble() * supporters.Count);

                            supporters.RemoveAt(index);
                        }

                        SkillTakeEffect(_skillSDS, supporters, _hpChangeDic, _powerChangeDic);
                    }

                    break;

                case SkillTarget.ENEMY:

                    List<Hero> attackers = new List<Hero>(_attackers);

                    while (attackers.Count > _skillSDS.GetTargetNum())
                    {
                        int index = (int)(Battle.random.NextDouble() * attackers.Count);

                        attackers.RemoveAt(index);
                    }

                    SkillTakeEffect(_skillSDS, attackers, _hpChangeDic, _powerChangeDic);

                    break;
            }
        }

        private void SkillTakeEffect(ISkillSDS _skillSDS, List<Hero> _heros, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
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

        internal void Destroy()
        {
            for(int i = 0; i < eventIDs.Length; i++)
            {
                battle.eventListener.RemoveListener(eventIDs[i]);
            }
        }
    }
}

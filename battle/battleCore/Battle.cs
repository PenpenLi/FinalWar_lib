﻿using System;
using System.Collections;
using System.Collections.Generic;
using superEvent;
using superRandom;
using tuple;

namespace FinalWar
{
    public class Battle
    {
        public enum BattleResult
        {
            NOT_OVER,
            M_WIN,
            O_WIN,
            DRAW,
        }

        internal static Func<int, IMapSDS> GetMapData;
        internal static Func<int, IHeroSDS> GetHeroData;
        internal static Func<int, IAuraSDS> GetAuraData;
        internal static Func<int, IEffectSDS> GetEffectData;

        public MapData mapData { get; private set; }

        private Dictionary<int, bool> mapBelongDic = new Dictionary<int, bool>();
        internal Dictionary<int, Hero> heroMapDic = new Dictionary<int, Hero>();

        public Queue<int> mCards = new Queue<int>();
        public Queue<int> oCards = new Queue<int>();

        public List<int> mHandCards = new List<int>();
        public List<int> oHandCards = new List<int>();

        private int deckCardsNum;

        private int addCardsNum;

        private int[] cardsArr;

        public int mScore { get; private set; }
        public int oScore { get; private set; }

        public int mMoney { get; private set; }
        public int oMoney { get; private set; }

        private Dictionary<int, int> summon = new Dictionary<int, int>();

        private List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

        private List<KeyValuePair<int, int>> fearAction = new List<KeyValuePair<int, int>>();

        public int roundNum { private set; get; }

        public int maxRoundNum { private set; get; }

        internal SuperEventListener eventListener = new SuperEventListener();

        private SuperRandom random = new SuperRandom();

        public static void Init<S, T, U, V>(Dictionary<int, S> _mapDataDic, Dictionary<int, T> _heroDataDic, Dictionary<int, U> _auraDataDic, Dictionary<int, V> _effectDataDic) where S : IMapSDS where T : IHeroSDS where U : IAuraSDS where V : IEffectSDS
        {
            GetMapData = delegate (int _id)
            {
                return _mapDataDic[_id];
            };

            GetHeroData = delegate (int _id)
            {
                return _heroDataDic[_id];
            };

            GetAuraData = delegate (int _id)
            {
                return _auraDataDic[_id];
            };

            GetEffectData = delegate (int _id)
            {
                return _effectDataDic[_id];
            };
        }

        internal int GetRandomValue(int _max)
        {
            return random.Get(_max);
        }

        internal void InitBattle(int _mapID, int _maxRoundNum, int _deckCardsNum, int _addCardsNum, int[] _mCards, int[] _oCards)
        {
            Reset();

            maxRoundNum = _maxRoundNum;

            deckCardsNum = _deckCardsNum;

            addCardsNum = _addCardsNum;

            cardsArr = new int[deckCardsNum * 2];

            IMapSDS mapSDS = GetMapData(_mapID);

            mapData = mapSDS.GetMapData();

            mScore = mapData.mScore;

            oScore = mapData.oScore;

            mMoney = oMoney = BattleConst.DEFAULT_MONEY;

            for (int i = 0; i < deckCardsNum && i < _mCards.Length; i++)
            {
                SetCard(i, _mCards[i]);

                mCards.Enqueue(i);
            }

            for (int i = 0; i < deckCardsNum && i < _oCards.Length; i++)
            {
                int index = deckCardsNum + i;

                SetCard(index, _oCards[i]);

                oCards.Enqueue(index);
            }

            for (int i = 0; i < BattleConst.DEFAULT_HAND_CARD_NUM; i++)
            {
                if (mCards.Count > 0)
                {
                    mHandCards.Add(mCards.Dequeue());
                }

                if (oCards.Count > 0)
                {
                    oHandCards.Add(oCards.Dequeue());
                }
            }

            for (int i = 0; i < mapSDS.GetHero().Length; i++)
            {
                KeyValuePair<int, int> pair = mapSDS.GetHero()[i];

                int pos = pair.Key;

                int id = pair.Value;

                bool isMine = GetPosIsMine(pos);

                IHeroSDS heroSDS = GetHeroData(id);

                Hero hero = new Hero(this, isMine, heroSDS, pos);

                heroMapDic.Add(pos, hero);
            }

            for (int i = 0; i < mapSDS.GetFearAction().Length; i++)
            {
                fearAction.Add(mapSDS.GetFearAction()[i]);
            }
        }

        internal IEnumerator StartBattle()
        {
            yield return new BattleStartVO();

            BattleData battleData = GetBattleData();

            ClearAction();

            ClearFearAction();

            Dictionary<int, int> tmpSummon = summon;

            summon = new Dictionary<int, int>();

            yield return DoSkill(battleData);

            yield return DoRoundStart(battleData);

            yield return DoAttack(battleData);

            yield return DoMove(battleData);

            yield return DoRoundOver();

            yield return DoRecover();

            yield return DoSummon(tmpSummon);

            yield return DoAddMoneyAndCards();

            yield return RoundOver();
        }

        private BattleResult RoundOver()
        {
            bool mWin = false;

            bool oWin = false;

            if (mapBelongDic.ContainsKey(mapData.mBase))
            {
                oWin = true;
            }

            if (mapBelongDic.ContainsKey(mapData.oBase))
            {
                mWin = true;
            }

            BattleResult battleResult;

            if (oWin && mWin)
            {
                battleResult = BattleResult.DRAW;
            }
            else if (oWin)
            {
                battleResult = BattleResult.O_WIN;
            }
            else if (mWin)
            {
                battleResult = BattleResult.M_WIN;
            }
            else
            {
                roundNum++;

                if (roundNum == maxRoundNum)
                {
                    if (mScore > oScore)
                    {
                        battleResult = BattleResult.M_WIN;
                    }
                    else if (mScore < oScore)
                    {
                        battleResult = BattleResult.O_WIN;
                    }
                    else
                    {
                        battleResult = BattleResult.DRAW;
                    }
                }
                else
                {
                    battleResult = BattleResult.NOT_OVER;
                }
            }

            return battleResult;
        }

        private void Reset()
        {
            eventListener.Clear();

            mapBelongDic.Clear();

            heroMapDic.Clear();

            mHandCards.Clear();

            oHandCards.Clear();

            mCards.Clear();

            oCards.Clear();

            ClearSummon();

            ClearAction();

            ClearFearAction();

            cardsArr = null;

            roundNum = 0;
        }

        private IEnumerator DoSummon(Dictionary<int, int> _summon)
        {
            IEnumerator<KeyValuePair<int, int>> enumerator = _summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int tmpCardUid = pair.Key;

                int pos = pair.Value;

                if (heroMapDic.ContainsKey(pos))
                {
                    throw new Exception("Summon error0");
                }

                Hero summonHero = SummonOneUnit(tmpCardUid, pos);

                heroMapDic.Add(pos, summonHero);

                yield return new BattleAddCardsVO(summonHero.isMine, null);

                yield return new BattleMoneyChangeVO(summonHero.isMine, -summonHero.sds.GetCost());

                yield return new BattleSummonVO(summonHero.isMine, tmpCardUid, summonHero.sds.GetID(), pos);
            }
        }

        private Hero SummonOneUnit(int _uid, int _pos)
        {
            bool isMine = GetPosIsMine(_pos);

            IHeroSDS sds;

            if (isMine)
            {
                int index = mHandCards.IndexOf(_uid);

                if (index != -1)
                {
                    int heroID = GetCard(_uid);

                    sds = GetHeroData(heroID);

                    if (mMoney < sds.GetCost())
                    {
                        throw new Exception("SummonOneUnit error0!");
                    }

                    mMoney -= sds.GetCost();

                    mHandCards.RemoveAt(index);
                }
                else
                {
                    throw new Exception("SummonOneUnit error1!");
                }
            }
            else
            {
                int index = oHandCards.IndexOf(_uid);

                if (index != -1)
                {
                    int heroID = GetCard(_uid);

                    sds = GetHeroData(heroID);

                    if (oMoney < sds.GetCost())
                    {
                        throw new Exception("SummonOneUnit error2!");
                    }

                    oMoney -= sds.GetCost();

                    oHandCards.RemoveAt(index);
                }
                else
                {
                    throw new Exception("SummonOneUnit error3!");
                }
            }

            Hero hero = new Hero(this, isMine, sds, _pos);

            return hero;
        }

        public BattleData GetBattleData()
        {
            IEnumerator<Hero> enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.sds.GetHeroType().GetCounterTimes() == 0)
                {
                    enumerator.Current.SetAction(Hero.HeroAction.NULL);
                }
                else
                {
                    enumerator.Current.SetAction(Hero.HeroAction.DEFENSE);
                }
            }

            BattleData battleData = new BattleData();

            for (int i = 0; i < fearAction.Count; i++)
            {
                KeyValuePair<int, int> pair = fearAction[i];

                int pos = pair.Key;

                int targetPos = pair.Value;

                if (pos != targetPos)
                {
                    GetOneUnitAction(pos, targetPos, battleData);
                }
            }

            IEnumerator<KeyValuePair<int, int>> enumerator2 = GetActionEnumerator();

            while (enumerator2.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator2.Current;

                int pos = pair.Key;

                int targetPos = pair.Value;

                GetOneUnitAction(pos, targetPos, battleData);
            }

            return battleData;
        }

        private void GetOneUnitAction(int _pos, int _targetPos, BattleData _battleData)
        {
            if (!heroMapDic.ContainsKey(_pos))
            {
                throw new Exception("action error0");
            }

            Hero hero = heroMapDic[_pos];

            bool targetPosIsMine = GetPosIsMine(_targetPos);

            List<int> arr = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            if (arr.Contains(_targetPos))
            {
                if (hero.isMine == targetPosIsMine)
                {
                    hero.SetAction(Hero.HeroAction.SUPPORT, _targetPos);

                    BattleCellData cellData;

                    if (!_battleData.actionDic.TryGetValue(_targetPos, out cellData))
                    {
                        cellData = new BattleCellData(_targetPos);

                        Hero tmpHero;

                        if (heroMapDic.TryGetValue(_targetPos, out tmpHero))
                        {
                            cellData.stander = tmpHero;
                        }

                        _battleData.actionDic.Add(_targetPos, cellData);
                    }

                    cellData.supporters.Add(hero);
                }
                else
                {
                    hero.SetAction(Hero.HeroAction.ATTACK, _targetPos);

                    BattleCellData cellData;

                    if (!_battleData.actionDic.TryGetValue(_targetPos, out cellData))
                    {
                        cellData = new BattleCellData(_targetPos);

                        Hero tmpHero;

                        if (heroMapDic.TryGetValue(_targetPos, out tmpHero))
                        {
                            cellData.stander = tmpHero;
                        }

                        _battleData.actionDic.Add(_targetPos, cellData);
                    }

                    cellData.attackers.Add(hero);
                }
            }
            else
            {
                hero.SetAction(Hero.HeroAction.SHOOT, _targetPos);

                BattleCellData cellData;

                if (!_battleData.actionDic.TryGetValue(_targetPos, out cellData))
                {
                    cellData = new BattleCellData(_targetPos);

                    Hero tmpHero;

                    if (heroMapDic.TryGetValue(_targetPos, out tmpHero))
                    {
                        cellData.stander = tmpHero;
                    }

                    _battleData.actionDic.Add(_targetPos, cellData);
                }

                cellData.shooters.Add(hero);
            }
        }

        private IEnumerator DoSkill(BattleData _battleData)
        {
            bool hasSkill = false;

            IEnumerator<BattleCellData> enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                if (cellData.stander != null)
                {
                    LinkedList<Tuple<int, Hero, Func<List<BattleHeroEffectVO>>>> linkedList = null;

                    if (cellData.shooters.Count > 0)
                    {
                        hasSkill = true;

                        if (linkedList == null)
                        {
                            linkedList = new LinkedList<Tuple<int, Hero, Func<List<BattleHeroEffectVO>>>>();
                        }

                        for (int i = 0; i < cellData.shooters.Count; i++)
                        {
                            Hero hero = cellData.shooters[i];

                            hero.SetAction(Hero.HeroAction.NULL);

                            HeroSkill.CastSkill(this, hero, cellData.stander, hero.sds.GetShootSkills(), linkedList);
                        }

                        cellData.shooters.Clear();
                    }

                    if (cellData.supporters.Count > 0)
                    {
                        hasSkill = true;

                        if (linkedList == null)
                        {
                            linkedList = new LinkedList<Tuple<int, Hero, Func<List<BattleHeroEffectVO>>>>();
                        }

                        for (int i = 0; i < cellData.supporters.Count; i++)
                        {
                            Hero hero = cellData.supporters[i];

                            if (hero.sds.GetSupportSkills().Length > 0)
                            {
                                HeroSkill.CastSkill(this, hero, cellData.stander, hero.sds.GetSupportSkills(), linkedList);
                            }
                        }
                    }

                    if (linkedList != null)
                    {
                        Dictionary<Hero, List<BattleHeroEffectVO>> result = new Dictionary<Hero, List<BattleHeroEffectVO>>();

                        IEnumerator<Tuple<int, Hero, Func<List<BattleHeroEffectVO>>>> enumerator3 = linkedList.GetEnumerator();

                        while (enumerator3.MoveNext())
                        {
                            Tuple<int, Hero, Func<List<BattleHeroEffectVO>>> tuple = enumerator3.Current;

                            Hero hero = tuple.second;

                            Func<List<BattleHeroEffectVO>> func = tuple.third;

                            List<BattleHeroEffectVO> list;

                            if (!result.TryGetValue(hero, out list))
                            {
                                list = new List<BattleHeroEffectVO>();

                                result.Add(hero, list);
                            }

                            List<BattleHeroEffectVO> vo = func();

                            list.AddRange(vo);
                        }

                        IEnumerator<KeyValuePair<Hero, List<BattleHeroEffectVO>>> enumerator4 = result.GetEnumerator();

                        while (enumerator4.MoveNext())
                        {
                            Hero hero = enumerator4.Current.Key;

                            if (hero.isMine == cellData.stander.isMine)
                            {
                                yield return new BattleSupportVO(hero.pos, cellData.pos, enumerator4.Current.Value);
                            }
                            else
                            {
                                yield return new BattleShootVO(hero.pos, cellData.pos, enumerator4.Current.Value);
                            }
                        }
                    }
                }
            }

            if (hasSkill)
            {
                IEnumerator<Hero> enumerator2 = heroMapDic.Values.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.ProcessDamage();
                }

                yield return RemoveDieHero(_battleData);
            }
        }

        private IEnumerator DoRush(BattleData _battleData)
        {
            while (true)
            {
                bool hasRush = false;

                LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

                IEnumerator<BattleCellData> enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        Hero stander = cellData.stander;

                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            if (attacker.action == Hero.HeroAction.ATTACK)
                            {
                                if (stander.action == Hero.HeroAction.ATTACK && stander.actionTarget == attacker.pos)
                                {
                                    BattleCellData cellData2 = _battleData.actionDic[attacker.pos];

                                    if (cellData2.supporters.Count == 0)
                                    {
                                        continue;
                                    }
                                }

                                if (!hasRush)
                                {
                                    hasRush = true;

                                    yield return new BattlePrepareRushVO();
                                }

                                attacker.DoAttack();

                                attacker.Attack(stander, ref funcList);

                                stander.BeAttacked();

                                yield return new BattleRushVO(attacker.pos, stander.pos);

                                break;
                            }
                        }
                    }
                }

                if (hasRush)
                {
                    if (funcList != null)
                    {
                        yield return InvokeFuncList(funcList);
                    }

                    IEnumerator<Hero> enumerator2 = heroMapDic.Values.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.ProcessDamage();
                    }

                    yield return RemoveDieHero(_battleData);
                }
                else
                {
                    yield return new BattleRushOverVO();

                    break;
                }
            }
        }

        private IEnumerator RemoveDieHero(BattleData _battleData)
        {
            List<int> dieList = null;

            IEnumerator<Hero> enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (!enumerator.Current.IsAlive())
                {
                    if (dieList == null)
                    {
                        dieList = new List<int>();
                    }

                    dieList.Add(enumerator.Current.pos);
                }
            }

            if (dieList != null)
            {
                LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

                for (int i = 0; i < dieList.Count; i++)
                {
                    int pos = dieList[i];

                    Hero hero = heroMapDic[pos];

                    heroMapDic.Remove(pos);

                    if (_battleData != null)
                    {
                        RemoveHeroAction(_battleData, hero);
                    }

                    hero.Die(ref funcList);
                }

                if (funcList != null)
                {
                    yield return InvokeFuncList(funcList);
                }

                yield return new BattleDeathVO(dieList);

                if (funcList != null)
                {
                    enumerator = heroMapDic.Values.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ProcessDamage();
                    }

                    yield return RemoveDieHero(_battleData);
                }
            }
        }

        private IEnumerator DoRoundStart(BattleData _battleData)
        {
            LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

            IEnumerator<Hero> enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.RoundStart(ref funcList);
            }

            if (funcList != null)
            {
                yield return InvokeFuncList(funcList);

                enumerator = heroMapDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    enumerator.Current.ProcessDamage();
                }

                yield return RemoveDieHero(_battleData);
            }

            yield return new BattleRoundStartVO();
        }

        private IEnumerator DoAttack(BattleData _battleData)
        {
            while (true)
            {
                yield return DoRush(_battleData);

                bool hasAttack = false;

                Dictionary<int, bool> checkedPosDic = null;

                LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

                IEnumerator<BattleCellData> enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (checkedPosDic != null && checkedPosDic.ContainsKey(cellData.pos))
                    {
                        continue;
                    }

                    if (cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                    {
                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            if (attacker.action == Hero.HeroAction.ATTACK)
                            {
                                if (!hasAttack)
                                {
                                    hasAttack = true;
                                }

                                attacker.DoAttack();

                                Hero defender;

                                int defenderSpeed;

                                AttackType attackType;

                                if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                                {
                                    defender = cellData.stander;

                                    defenderSpeed = defender.GetDefenseSpeed(attacker);

                                    defender.DoCounter();

                                    attackType = AttackType.A_D;
                                }
                                else if (cellData.supporters.Count > 0)
                                {
                                    defender = cellData.supporters[0];

                                    defenderSpeed = defender.GetSupportSpeed(attacker);

                                    attackType = AttackType.A_S;
                                }
                                else
                                {
                                    defender = cellData.stander;

                                    defenderSpeed = defender.GetAttackSpeed(attacker);

                                    defender.DoAttack();

                                    if (checkedPosDic == null)
                                    {
                                        checkedPosDic = new Dictionary<int, bool>();
                                    }

                                    checkedPosDic.Add(attacker.pos, true);

                                    attackType = AttackType.A_A;
                                }

                                int attackerSpeed = attacker.GetAttackSpeed(defender);

                                yield return new BattlePrepareAttackVO(cellData.pos, attackType, attacker.pos, attackerSpeed, defender.pos, defenderSpeed);

                                int speedDiff = attackerSpeed - defenderSpeed;

                                if (Math.Abs(speedDiff) < BattleConst.SPEED_GAP_1)
                                {
                                    if (speedDiff < BattleConst.SPEED_GAP_0)
                                    {
                                        attacker.Attack(defender, ref funcList);

                                        defender.Attack(attacker, ref funcList);

                                        attacker.BeAttacked();

                                        defender.BeAttacked();

                                        yield return new BattleAttackBothVO(cellData.pos, attacker.pos, defender.pos);
                                    }
                                    else if (speedDiff > 0)
                                    {
                                        attacker.Attack(defender, ref funcList);

                                        defender.BeAttacked();

                                        yield return new BattleAttackAndCounterVO(cellData.pos, attacker.pos, defender.pos);

                                        defender.ProcessDamage();

                                        if (defender.IsAlive())
                                        {
                                            defender.Attack(attacker, ref funcList);

                                            attacker.BeAttacked();

                                            yield return new BattleAttackAndCounterVO(cellData.pos, defender.pos, attacker.pos);
                                        }
                                    }
                                    else
                                    {
                                        defender.Attack(attacker, ref funcList);

                                        attacker.BeAttacked();

                                        yield return new BattleAttackAndCounterVO(cellData.pos, defender.pos, attacker.pos);

                                        attacker.ProcessDamage();

                                        if (attacker.IsAlive())
                                        {
                                            attacker.Attack(defender, ref funcList);

                                            defender.BeAttacked();

                                            yield return new BattleAttackAndCounterVO(cellData.pos, attacker.pos, defender.pos);
                                        }
                                    }
                                }
                                else if (speedDiff > 0)
                                {
                                    attacker.Attack(defender, ref funcList);

                                    defender.BeAttacked();

                                    yield return new BattleAttackAndCounterVO(cellData.pos, attacker.pos, defender.pos);
                                }
                                else
                                {
                                    defender.Attack(attacker, ref funcList);

                                    attacker.BeAttacked();

                                    yield return new BattleAttackAndCounterVO(cellData.pos, defender.pos, attacker.pos);
                                }

                                yield return new BattleAttackOverVO(cellData.pos, attackType, attacker.pos, defender.pos);

                                break;
                            }
                        }
                    }
                }

                if (hasAttack)
                {
                    if (funcList != null)
                    {
                        yield return InvokeFuncList(funcList);
                    }

                    IEnumerator<Hero> enumerator2 = heroMapDic.Values.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.ProcessDamage();
                    }

                    yield return RemoveDieHero(_battleData);
                }
                else
                {
                    break;
                }
            }
        }

        private IEnumerator DoMove(BattleData _battleData)
        {
            Dictionary<Hero, int> moveDic = new Dictionary<Hero, int>();

            IEnumerator<Hero> enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if (!moveDic.ContainsKey(hero))
                {
                    HeroTryMove(_battleData, moveDic, hero, hero);
                }
            }

            Dictionary<int, int> dic = null;

            List<Hero> captureList = null;

            IEnumerator<KeyValuePair<Hero, int>> enumerator2 = moveDic.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                int pos = enumerator2.Current.Value;

                if (pos != -1)
                {
                    Hero hero = enumerator2.Current.Key;

                    if (dic == null)
                    {
                        dic = new Dictionary<int, int>();
                    }

                    dic.Add(hero.pos, pos);

                    heroMapDic.Remove(hero.pos);

                    hero.PosChange(pos);

                    if (GetPosIsMine(pos) != hero.isMine)
                    {
                        if (captureList == null)
                        {
                            captureList = new List<Hero>();
                        }

                        captureList.Add(hero);

                        if (mapBelongDic.ContainsKey(pos))
                        {
                            mapBelongDic.Remove(pos);
                        }
                        else
                        {
                            mapBelongDic.Add(pos, true);
                        }

                        if (hero.isMine)
                        {
                            mScore++;
                            oScore--;
                        }
                        else
                        {
                            mScore--;
                            oScore++;
                        }
                    }
                }
            }

            enumerator2 = moveDic.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                int pos = enumerator2.Current.Value;

                if (pos != -1)
                {
                    Hero hero = enumerator2.Current.Key;

                    heroMapDic.Add(pos, hero);
                }
            }

            if (dic != null)
            {
                yield return new BattleMoveVO(dic);

                if (captureList != null)
                {
                    yield return new BattleScoreChangeVO();

                    LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

                    for (int i = 0; i < captureList.Count; i++)
                    {
                        Hero hero = captureList[i];

                        hero.CaptureArea(ref funcList);
                    }

                    if (funcList != null)
                    {
                        yield return InvokeFuncList(funcList);

                        enumerator = heroMapDic.Values.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            enumerator.Current.ProcessDamage();
                        }

                        yield return RemoveDieHero(_battleData);
                    }
                }
            }
        }

        private void HeroTryMove(BattleData _battleData, Dictionary<Hero, int> _moveDic, Hero _hero, Hero _firstHero)
        {
            if (_hero.actionTarget == -1)
            {
                _moveDic.Add(_hero, -1);
            }
            else
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                if (GetCellMoveInHero(cellData) == _hero)
                {
                    if (cellData.stander == null)
                    {
                        _moveDic.Add(_hero, _hero.actionTarget);
                    }
                    else
                    {
                        int tmpPos;

                        if (_moveDic.TryGetValue(cellData.stander, out tmpPos))
                        {
                            if (tmpPos == -1)
                            {
                                _moveDic.Add(_hero, -1);
                            }
                            else
                            {
                                _moveDic.Add(_hero, _hero.actionTarget);
                            }
                        }
                        else
                        {
                            if (cellData.stander == _firstHero)
                            {
                                if (_firstHero.actionTarget == _hero.pos)
                                {
                                    if (_firstHero.isMine == _hero.isMine)
                                    {
                                        _moveDic.Add(_hero, _hero.actionTarget);
                                    }
                                    else
                                    {
                                        _moveDic.Add(_hero, -1);
                                    }
                                }
                                else
                                {
                                    _moveDic.Add(_hero, _hero.actionTarget);
                                }
                            }
                            else
                            {
                                HeroTryMove(_battleData, _moveDic, cellData.stander, _firstHero);

                                if (_moveDic[cellData.stander] != -1)
                                {
                                    _moveDic.Add(_hero, _hero.actionTarget);
                                }
                                else
                                {
                                    _moveDic.Add(_hero, -1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    _moveDic.Add(_hero, -1);
                }
            }
        }

        private Hero GetCellMoveInHero(BattleCellData _cellData)
        {
            Hero hero = null;

            for (int i = 0; i < _cellData.supporters.Count; i++)
            {
                Hero tmpHero = _cellData.supporters[i];

                if (tmpHero.GetCanMove())
                {
                    hero = tmpHero;

                    break;
                }
            }

            if (hero == null)
            {
                for (int i = 0; i < _cellData.attackers.Count; i++)
                {
                    Hero tmpHero = _cellData.attackers[i];

                    if (tmpHero.GetCanMove())
                    {
                        hero = tmpHero;

                        break;
                    }
                }
            }

            return hero;
        }

        private IEnumerator DoRoundOver()
        {
            LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

            IEnumerator<Hero> enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.RoundOver(ref funcList);
            }

            if (funcList != null)
            {
                yield return InvokeFuncList(funcList);

                enumerator = heroMapDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    enumerator.Current.ProcessDamage();
                }

                yield return RemoveDieHero(null);
            }

            yield return new BattleRoundOverVO();
        }

        private IEnumerator DoRecover()
        {
            IEnumerator<Hero> enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.Recover();
            }

            yield return new BattleRecoverVO();
        }

        private void RemoveHeroAction(BattleData _battleData, Hero _hero)
        {
            BattleCellData cellData;

            if (_battleData.actionDic.TryGetValue(_hero.pos, out cellData))
            {
                cellData.stander = null;
            }

            if (_hero.action == Hero.HeroAction.ATTACK || _hero.action == Hero.HeroAction.ATTACK_OVER)
            {
                cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SHOOT)
            {
                cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.shooters.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SUPPORT)
            {
                cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.supporters.Remove(_hero);
            }

            _hero.SetAction(Hero.HeroAction.NULL);
        }

        private IEnumerator DoAddMoneyAndCards()
        {
            yield return AddMoneyAndCards(true);

            yield return AddMoneyAndCards(false);
        }

        internal void MoneyChangeReal(bool _isMine, int _num)
        {
            if (_isMine)
            {
                mMoney += _num;

                if (mMoney > BattleConst.MAX_MONEY)
                {
                    mMoney = BattleConst.MAX_MONEY;
                }
                else if (mMoney < 0)
                {
                    mMoney = 0;
                }
            }
            else
            {
                oMoney += _num;

                if (oMoney > BattleConst.MAX_MONEY)
                {
                    oMoney = BattleConst.MAX_MONEY;
                }
                else if (oMoney < 0)
                {
                    oMoney = 0;
                }
            }
        }

        public bool GetPosIsMine(int _pos)
        {
            MapData.MapUnitType mapUnitType = mapData.dic[_pos];

            if (mapUnitType == MapData.MapUnitType.M_AREA)
            {
                return !mapBelongDic.ContainsKey(_pos);
            }
            else if (mapUnitType == MapData.MapUnitType.O_AREA)
            {
                return mapBelongDic.ContainsKey(_pos);
            }
            else
            {
                throw new Exception("GetPosIsMine error!");
            }
        }

        private IEnumerator AddMoneyAndCards(bool _isMine)
        {
            Queue<int> cards;

            List<int> handCardsList;

            if (_isMine)
            {
                cards = mCards;

                handCardsList = mHandCards;
            }
            else
            {
                cards = oCards;

                handCardsList = oHandCards;
            }

            if (cards.Count > 0)
            {
                List<int> addList = new List<int>();

                for (int i = 0; i < addCardsNum && cards.Count > 0; i++)
                {
                    int uid = cards.Dequeue();

                    addList.Add(uid);

                    if (handCardsList.Count < BattleConst.MAX_HAND_CARD_NUM)
                    {
                        handCardsList.Add(uid);
                    }

                    MoneyChangeReal(_isMine, BattleConst.ADD_MONEY);
                }

                yield return new BattleAddCardsVO(_isMine, addList);

                yield return new BattleMoneyChangeVO(_isMine, _isMine ? mMoney : oMoney);
            }
        }

        private IEnumerator InvokeFuncList(LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            IEnumerator<KeyValuePair<int, Func<BattleTriggerAuraVO>>> enumerator = _funcList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.Value();
            }
        }




















        public IEnumerator<KeyValuePair<int, int>> GetSummonEnumerator()
        {
            return summon.GetEnumerator();
        }

        public int GetSummonNum()
        {
            return summon.Count;
        }

        public bool GetSummonContainsKey(int _uid)
        {
            return summon.ContainsKey(_uid);
        }

        public bool GetSummonContainsValue(int _pos)
        {
            return summon.ContainsValue(_pos);
        }

        internal bool AddSummon(bool _isMine, int _uid, int _pos)
        {
            List<int> handCards;

            int money;

            if (_isMine)
            {
                handCards = mHandCards;

                money = mMoney;
            }
            else
            {
                handCards = oHandCards;

                money = oMoney;
            }

            if (handCards.Contains(_uid))
            {
                int nowMoney = GetNowMoney(_isMine);

                IHeroSDS sds = GetHeroData(GetCard(_uid));

                if (sds.GetCost() <= nowMoney && !GetSummonContainsKey(_uid) && CheckPosCanSummon(_isMine, _pos))
                {
                    AddSummon(_uid, _pos);

                    return true;
                }
            }

            return false;
        }

        public bool CheckPosCanSummon(bool _isMine, int _pos)
        {
            MapData.MapUnitType mapUnitType;

            if (mapData.dic.TryGetValue(_pos, out mapUnitType))
            {
                if (mapUnitType != MapData.MapUnitType.M_AREA && mapUnitType != MapData.MapUnitType.O_AREA)
                {
                    return false;
                }

                if (GetPosIsMine(_pos) != _isMine)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            for (int i = 0; i < action.Count; i++)
            {
                KeyValuePair<int, int> pair = action[i];

                if (pair.Value == _pos)
                {
                    return false;
                }
            }

            if (!heroMapDic.ContainsKey(_pos) && !GetSummonContainsValue(_pos))
            {
                bool isMine = GetPosIsMine(_pos);

                List<int> list = BattlePublicTools.GetNeighbourPos(mapData, _pos);

                for (int i = 0; i < list.Count; i++)
                {
                    int pos = list[i];

                    if (GetPosIsMine(pos) != isMine)
                    {
                        Hero hero;

                        if (heroMapDic.TryGetValue(pos, out hero))
                        {
                            bool isFear = false;

                            for (int m = 0; m < fearAction.Count; m++)
                            {
                                KeyValuePair<int, int> pair = fearAction[m];

                                if (pair.Key == pos)
                                {
                                    isFear = true;

                                    if (pair.Value == _pos)
                                    {
                                        return false;
                                    }

                                    break;
                                }
                            }

                            if (!isFear)
                            {
                                List<int> tmpList = BattlePublicTools.GetCanAttackPos(this, hero);

                                if (tmpList != null && tmpList.Contains(_pos))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        internal void AddSummon(int _uid, int _pos)
        {
            summon.Add(_uid, _pos);
        }

        public int GetNowMoney(bool _isMine)
        {
            int money = _isMine ? mMoney : oMoney;

            IEnumerator<KeyValuePair<int, int>> enumerator = GetSummonEnumerator();

            while (enumerator.MoveNext())
            {
                int uid = enumerator.Current.Key;

                if (_isMine == uid < deckCardsNum)
                {
                    IHeroSDS sds = GetHeroData(GetCard(uid));

                    money -= sds.GetCost();
                }
            }

            return money;
        }

        internal void DelSummon(int _uid)
        {
            summon.Remove(_uid);
        }

        internal void ClearSummon()
        {
            summon.Clear();
        }

        public IEnumerator<KeyValuePair<int, int>> GetActionEnumerator()
        {
            return action.GetEnumerator();
        }

        public int GetActionNum()
        {
            return action.Count;
        }

        public int GetActionContainsKey(int _pos)
        {
            for (int i = 0; i < action.Count; i++)
            {
                KeyValuePair<int, int> pair = action[i];

                if (pair.Key == _pos)
                {
                    return pair.Value;
                }
            }

            return -1;
        }

        internal bool AddAction(bool _isMine, int _pos, int _targetPos)
        {
            Hero hero;

            if (!heroMapDic.TryGetValue(_pos, out hero))
            {
                return false;
            }

            if (hero.isMine != _isMine)
            {
                return false;
            }

            if (!hero.GetCanAction())
            {
                return false;
            }

            MapData.MapUnitType mapUnitType;

            if (mapData.dic.TryGetValue(_targetPos, out mapUnitType))
            {
                if (mapUnitType != MapData.MapUnitType.M_AREA && mapUnitType != MapData.MapUnitType.O_AREA)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (GetActionContainsKey(_pos) != -1)
            {
                return false;
            }

            bool targetPosIsMine = GetPosIsMine(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if (targetPosIsMine == hero.isMine)
                {
                    if (GetSummonContainsValue(_targetPos))
                    {
                        return false;
                    }
                }
                else
                {
                    int nowThreadLevel = 0;

                    Hero targetHero2;

                    if (heroMapDic.TryGetValue(_targetPos, out targetHero2))
                    {
                        nowThreadLevel = targetHero2.sds.GetHeroType().GetThread();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        if (pos != _targetPos)
                        {
                            Hero targetHero;

                            if (heroMapDic.TryGetValue(pos, out targetHero))
                            {
                                if (targetHero.isMine != hero.isMine)
                                {
                                    if (targetHero.sds.GetHeroType().GetThread() > nowThreadLevel)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }

                AddAction(_pos, _targetPos);

                return true;
            }
            else
            {
                if (targetPosIsMine != hero.isMine && hero.sds.GetShootSkills().Length > 0 && heroMapDic.ContainsKey(_targetPos))
                {
                    List<int> tmpList2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);

                    if (tmpList2.Contains(_targetPos))
                    {
                        AddAction(_pos, _targetPos);

                        return true;
                    }
                }

                return false;
            }
        }

        internal void AddAction(int _pos, int _targetPos)
        {
            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
        }

        internal void DelAction(int _pos)
        {
            for (int i = action.Count - 1; i > -1; i--)
            {
                KeyValuePair<int, int> pair = action[i];

                if (pair.Key == _pos)
                {
                    action.RemoveAt(i);

                    break;
                }
            }
        }

        internal void ClearAction()
        {
            action.Clear();
        }

        public IEnumerator<KeyValuePair<int, int>> GetFearActionEnumerator()
        {
            return fearAction.GetEnumerator();
        }

        internal bool GetFearActionContainsKey(int _pos)
        {
            for (int i = 0; i < fearAction.Count; i++)
            {
                if (fearAction[i].Key == _pos)
                {
                    return true;
                }
            }

            return false;
        }

        internal void AddFearAction(int _pos, int _targetPos)
        {
            int index = GetRandomValue(fearAction.Count);

            fearAction.Insert(index, new KeyValuePair<int, int>(_pos, _targetPos));
        }

        internal void ClearFearAction()
        {
            fearAction.Clear();
        }

        public Hero GetHero(int _pos)
        {
            return heroMapDic[_pos];
        }

        public IEnumerator<Hero> GetHeroEnumerator()
        {
            return heroMapDic.Values.GetEnumerator();
        }

        public bool GetHeroMapContainsKey(int _pos)
        {
            return heroMapDic.ContainsKey(_pos);
        }

        internal void SetRandomSeed(int _seed)
        {
            random.SetSeed(_seed);
        }

        internal void SetCard(int _uid, int _id)
        {
            cardsArr[_uid] = _id;
        }

        public int GetCard(int _uid)
        {
            return cardsArr[_uid];
        }
    }
}

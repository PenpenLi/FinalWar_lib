using System;
using System.Collections;
using System.Collections.Generic;
using superEvent;

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
        internal static Func<int, ISkillSDS> GetSkillData;
        internal static Func<int, IAuraSDS> GetAuraData;
        internal static Func<int, IEffectSDS> GetEffectData;

        public MapData mapData { get; private set; }

        private Dictionary<int, bool> mapBelongDic = new Dictionary<int, bool>();
        public Dictionary<int, Hero> heroMapDic = new Dictionary<int, Hero>();

        private Queue<int> mCards = new Queue<int>();
        private Queue<int> oCards = new Queue<int>();

        public List<int> mHandCards = new List<int>();
        public List<int> oHandCards = new List<int>();

        private int[] cardsArr = new int[BattleConst.DECK_CARD_NUM * 2];

        public int mScore { get; private set; }
        public int oScore { get; private set; }

        public int mMoney { get; private set; }
        public int oMoney { get; private set; }

        private Dictionary<int, int> summon = new Dictionary<int, int>();

        private List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

        private List<KeyValuePair<int, int>> fearAction = new List<KeyValuePair<int, int>>();

        private int randomIndex;

        public int roundNum { private set; get; }

        internal SuperEventListener eventListener = new SuperEventListener();

        public static void Init<S, T, U, V, W>(Dictionary<int, S> _mapDataDic, Dictionary<int, T> _heroDataDic, Dictionary<int, U> _skillDataDic, Dictionary<int, V> _auraDataDic, Dictionary<int, W> _effectDataDic) where S : IMapSDS where T : IHeroSDS where U : ISkillSDS where V : IAuraSDS where W : IEffectSDS
        {
            GetMapData = delegate (int _id)
            {
                return _mapDataDic[_id];
            };

            GetHeroData = delegate (int _id)
            {
                return _heroDataDic[_id];
            };

            GetSkillData = delegate (int _id)
            {
                return _skillDataDic[_id];
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
            double randomValue = BattleRandomPool.Get(randomIndex);

            randomIndex++;

            if (randomIndex == BattleRandomPool.num)
            {
                randomIndex = 0;
            }

            return (int)(randomValue * _max);
        }

        internal void InitBattle(int _mapID, int[] _mCards, int[] _oCards)
        {
            IMapSDS mapSDS = GetMapData(_mapID);

            mapData = mapSDS.GetMapData();

            mScore = mapData.mScore;
            oScore = mapData.oScore;

            mMoney = oMoney = BattleConst.DEFAULT_MONEY;

            for (int i = 0; i < BattleConst.DECK_CARD_NUM && i < _mCards.Length; i++)
            {
                SetCard(i, _mCards[i]);

                mCards.Enqueue(i);
            }

            for (int i = 0; i < BattleConst.DECK_CARD_NUM && i < _oCards.Length; i++)
            {
                int index = BattleConst.DECK_CARD_NUM + i;

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

            for (int i = 0; i < mapSDS.GetHeroID().Length; i++)
            {
                int pos = mapSDS.GetHeroPos()[i];

                int id = mapSDS.GetHeroID()[i];

                bool isMine = GetPosIsMine(pos);

                IHeroSDS heroSDS = GetHeroData(id);

                Hero hero = new Hero(this, isMine, heroSDS, pos);

                heroMapDic.Add(pos, hero);
            }

            IEnumerator ie = DoRecover();

            while (ie.MoveNext())
            {

            }
        }

        internal IEnumerator StartBattle()
        {
            BattleData battleData = GetBattleData();

            ClearAction();

            ClearFearAction();

            yield return new BattleRefreshVO();

            yield return DoSkill(battleData);

            yield return DoRoundStart(battleData);

            yield return DoSummon(battleData);

            ClearSummon();

            yield return DoRush(battleData);

            yield return DoAttack(battleData);

            yield return DoRush(battleData);

            yield return DoMove(battleData);

            yield return DoRecover();

            yield return DoAddMoney();

            yield return DoAddCards();

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

            if (oWin && mWin)
            {
                BattleOver();

                return BattleResult.DRAW;
            }
            else if (oWin)
            {
                BattleOver();

                return BattleResult.O_WIN;
            }
            else if (mWin)
            {
                BattleOver();

                return BattleResult.M_WIN;
            }
            else
            {
                roundNum++;

                if (roundNum == BattleConst.MAX_ROUND_NUM)
                {
                    BattleOver();

                    return BattleResult.DRAW;
                }
                else
                {
                    return BattleResult.NOT_OVER;
                }
            }
        }

        internal void BattleOver()
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

            roundNum = 0;
        }

        private IEnumerator DoSummon(BattleData _battleData)
        {
            IEnumerator<KeyValuePair<int, int>> enumerator = GetSummonEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int tmpCardUid = pair.Key;

                int pos = pair.Value;

                if (heroMapDic.ContainsKey(pos))
                {
                    throw new Exception("Summon error0");
                }

                Hero summonHero = SummonOneUnit(tmpCardUid, pos, _battleData);

                AddHero(_battleData, summonHero);

                yield return new BattleSummonVO(summonHero.isMine, tmpCardUid, summonHero.sds.GetID(), pos);
            }
        }

        private Hero SummonOneUnit(int _uid, int _pos, BattleData _battleData)
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

        private void AddHero(BattleData _battleData, Hero _hero)
        {
            heroMapDic.Add(_hero.pos, _hero);

            BattleCellData cellData;

            if (_battleData.actionDic.TryGetValue(_hero.pos, out cellData))
            {
                cellData.stander = _hero;
            }
        }

        public BattleData GetBattleData()
        {
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.SetAction(Hero.HeroAction.DEFENSE);
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

                        Hero herox;

                        if (heroMapDic.TryGetValue(_targetPos, out herox))
                        {
                            cellData.stander = herox;
                        }

                        _battleData.actionDic.Add(_targetPos, cellData);
                    }

                    cellData.supporters.Add(hero);
                }
                else
                {
                    int nowThreadLevel = 0;

                    Hero herox;

                    if (heroMapDic.TryGetValue(_targetPos, out herox))
                    {
                        nowThreadLevel = herox.sds.GetHeroType().GetThread();
                    }

                    for (int i = 0; i < arr.Count; i++)
                    {
                        int tmpPos = arr[i];

                        if (tmpPos == _targetPos)
                        {
                            continue;
                        }

                        Hero tmpHero;

                        if (heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine != hero.isMine)
                            {
                                if (tmpHero.sds.GetHeroType().GetThread() > nowThreadLevel)
                                {
                                    throw new Exception("attack error1");
                                }
                            }
                        }
                    }

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
                if (hero.sds.GetSkill() != 0 && heroMapDic.ContainsKey(_targetPos))
                {
                    List<int> arr2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);

                    if (arr2.Contains(_targetPos))
                    {
                        ISkillSDS skillSDS = GetSkillData(hero.sds.GetSkill());

                        if (targetPosIsMine != hero.isMine)
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
                        else
                        {
                            throw new Exception("shoot error5");
                        }
                    }
                    else
                    {
                        throw new Exception("shoot error4");
                    }
                }
                else
                {
                    throw new Exception("shoot error0");
                }
            }
        }

        private IEnumerator DoSkill(BattleData _battleData)
        {
            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                if (cellData.shooters.Count > 0)
                {
                    for (int i = 0; i < cellData.shooters.Count; i++)
                    {
                        Hero hero = cellData.shooters[i];

                        hero.SetAction(Hero.HeroAction.NULL);

                        List<BattleHeroEffectVO> effectList = HeroSkill.CastSkill(this, hero, cellData.stander);

                        yield return new BattleShootVO(hero.pos, cellData.pos, effectList);
                    }

                    //对同一个目标的所有法术施放完毕后再进行结算  因为施放法术没有先后顺序  所以最后一起结算
                    cellData.stander.ProcessDamage();

                    cellData.shooters.Clear();
                }
            }

            yield return RemoveDieHero(_battleData);
        }

        private IEnumerator DoRoundStart(BattleData _battleData)
        {
            List<Func<BattleTriggerAuraVO>> list = null;

            eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.ROUND_START, ref list, null, null);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i]();
                }

                Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    enumerator.Current.ProcessDamage();
                }

                yield return RemoveDieHero(_battleData);

                yield return new BattleRoundStartVO();
            }
        }

        private IEnumerator DoRush(BattleData _battleData)
        {
            Dictionary<BattleCellData, int> damageDic = null;

            while (true)
            {
                bool hasRush = false;

                Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        if (!hasRush)
                        {
                            hasRush = true;

                            if (damageDic == null)
                            {
                                damageDic = new Dictionary<BattleCellData, int>();
                            }
                            else
                            {
                                damageDic.Clear();
                            }

                            yield return new BattlePrepareRushVO();
                        }

                        Hero hero = cellData.attackers[0];

                        int damage = hero.GetDamage();

                        damageDic.Add(cellData, damage);
                    }
                }

                if (hasRush)
                {
                    Dictionary<BattleCellData, int>.Enumerator enumerator3 = damageDic.GetEnumerator();

                    while (enumerator3.MoveNext())
                    {
                        yield return ProcessCellDataRush(enumerator3.Current.Key, enumerator3.Current.Value);
                    }

                    Dictionary<int, Hero>.ValueCollection.Enumerator enumerator2 = heroMapDic.Values.GetEnumerator();

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

        private BattleRushVO ProcessCellDataRush(BattleCellData _cellData, int _damage)
        {
            Hero stander = _cellData.stander;

            Hero attacker = _cellData.attackers[0];

            attacker.DoAttack();

            if (attacker.attackTimes == 0)
            {
                _cellData.attackOvers.Add(attacker);

                _cellData.attackers.RemoveAt(0);

                attacker.SetAction(Hero.HeroAction.ATTACK_OVER, attacker.actionTarget);
            }

            List<BattleHeroEffectVO> attackerEffectList = null;

            List<BattleHeroEffectVO> defenderEffectList = null;

            attacker.Attack(stander, _damage, ref attackerEffectList, ref defenderEffectList);

            return new BattleRushVO(attacker.pos, _cellData.pos, attackerEffectList, defenderEffectList);
        }

        private IEnumerator RemoveDieHero(BattleData _battleData)
        {
            List<int> dieList = null;

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

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
                for (int i = 0; i < dieList.Count; i++)
                {
                    Hero hero = heroMapDic[dieList[i]];

                    yield return RemoveHero(_battleData, hero);
                }

                yield return new BattleDeathVO(dieList);
            }
        }

        private IEnumerator DoAttack(BattleData _battleData)
        {
            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                while (cellData.attackers.Count > 0 && ((cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE && cellData.stander.IsAlive()) || cellData.supporters.Count > 0))
                {
                    Hero attacker = cellData.attackers[0];

                    attacker.DoAttack();

                    Hero defender;

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        defender = cellData.stander;
                    }
                    else
                    {
                        defender = cellData.supporters[0];
                    }

                    List<int> attackerSupporters = null;

                    int attackerSpeedBonus = 0;

                    BattleCellData tmpCellData;

                    if (_battleData.actionDic.TryGetValue(attacker.pos, out tmpCellData))
                    {
                        for (int m = 0; m < tmpCellData.supporters.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supporters[m];

                            if (tmpHero.sds.GetHeroType().GetSupportSpeedBonus() > 0)
                            {
                                attackerSpeedBonus += tmpHero.sds.GetHeroType().GetSupportSpeedBonus();

                                if (attackerSupporters == null)
                                {
                                    attackerSupporters = new List<int>();
                                }

                                attackerSupporters.Add(tmpHero.pos);
                            }
                        }

                        for (int m = 0; m < tmpCellData.supportOvers.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supportOvers[m];

                            if (tmpHero.sds.GetHeroType().GetSupportSpeedBonus() > 0)
                            {
                                attackerSpeedBonus += tmpHero.sds.GetHeroType().GetSupportSpeedBonus();

                                if (attackerSupporters == null)
                                {
                                    attackerSupporters = new List<int>();
                                }

                                attackerSupporters.Add(tmpHero.pos);
                            }
                        }
                    }

                    List<int> defenderSupporters = null;

                    int defenderSpeedBonus = 0;

                    if (_battleData.actionDic.TryGetValue(defender.pos, out tmpCellData))
                    {
                        for (int m = 0; m < tmpCellData.supporters.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supporters[m];

                            if (tmpHero.sds.GetHeroType().GetSupportSpeedBonus() > 0)
                            {
                                defenderSpeedBonus += tmpHero.sds.GetHeroType().GetSupportSpeedBonus();

                                if (defenderSupporters == null)
                                {
                                    defenderSupporters = new List<int>();
                                }

                                defenderSupporters.Add(tmpHero.pos);
                            }
                        }

                        for (int m = 0; m < tmpCellData.supportOvers.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supportOvers[m];

                            if (tmpHero.sds.GetHeroType().GetSupportSpeedBonus() > 0)
                            {
                                defenderSpeedBonus += tmpHero.sds.GetHeroType().GetSupportSpeedBonus();

                                if (defenderSupporters == null)
                                {
                                    defenderSupporters = new List<int>();
                                }

                                defenderSupporters.Add(tmpHero.pos);
                            }
                        }
                    }

                    int attackerSpeed = attacker.GetAttackSpeed(attackerSpeedBonus);

                    int defenderSpeed;

                    if (defender == cellData.stander)
                    {
                        defenderSpeed = defender.GetDefenseSpeed(defenderSpeedBonus);
                    }
                    else
                    {
                        defenderSpeed = defender.GetSupportSpeed(defenderSpeedBonus);
                    }

                    yield return new BattlePrepareAttackVO(cellData.pos, attacker.pos, attackerSupporters, attackerSpeed, defender.pos, defenderSupporters, defenderSpeed);

                    int speedDiff = attackerSpeed - defenderSpeed;

                    int attackDamage = 0;

                    int defenseDamage = 0;

                    if (speedDiff == 0)
                    {
                        attackDamage = attacker.GetDamage();

                        defenseDamage = defender.GetDamage();

                        List<BattleHeroEffectVO> attackerEffectList = null;

                        List<BattleHeroEffectVO> defenderEffectList = null;

                        attacker.Attack(defender, attackDamage, ref attackerEffectList, ref defenderEffectList);

                        defender.Attack(attacker, defenseDamage, ref defenderEffectList, ref attackerEffectList);

                        defender.ProcessDamage();

                        attacker.ProcessDamage();

                        yield return new BattleAttackAndCounterVO(cellData.pos, attacker.pos, defender.pos, attackDamage, defenseDamage, attackerEffectList, defenderEffectList);
                    }
                    else if (speedDiff >= 1)
                    {
                        attackDamage = attacker.GetDamage();

                        List<BattleHeroEffectVO> attackerEffectList = null;

                        List<BattleHeroEffectVO> defenderEffectList = null;

                        attacker.Attack(defender, attackDamage, ref attackerEffectList, ref defenderEffectList);

                        defender.ProcessDamage();

                        yield return new BattleAttackVO(cellData.pos, attacker.pos, defender.pos, attackDamage, attackerEffectList, defenderEffectList);

                        if (speedDiff == 1 && defender.IsAlive())
                        {
                            defenseDamage = defender.GetDamage();

                            attackerEffectList = null;

                            defenderEffectList = null;

                            defender.Attack(attacker, defenseDamage, ref attackerEffectList, ref defenderEffectList);

                            attacker.ProcessDamage();

                            yield return new BattleCounterVO(cellData.pos, defender.pos, attacker.pos, defenseDamage, attackerEffectList, defenderEffectList);
                        }
                    }
                    else
                    {
                        defenseDamage = defender.GetDamage();

                        List<BattleHeroEffectVO> attackerEffectList = null;

                        List<BattleHeroEffectVO> defenderEffectList = null;

                        defender.Attack(attacker, defenseDamage, ref attackerEffectList, ref defenderEffectList);

                        attacker.ProcessDamage();

                        yield return new BattleCounterVO(cellData.pos, defender.pos, attacker.pos, defenseDamage, attackerEffectList, defenderEffectList);

                        if (speedDiff == -1 && attacker.IsAlive())
                        {
                            attackDamage = attacker.GetDamage();

                            attackerEffectList = null;

                            defenderEffectList = null;

                            attacker.Attack(defender, attackDamage, ref attackerEffectList, ref defenderEffectList);

                            defender.ProcessDamage();

                            yield return new BattleAttackVO(cellData.pos, attacker.pos, defender.pos, attackDamage, attackerEffectList, defenderEffectList);
                        }
                    }

                    if (!defender.IsAlive())
                    {
                        if (defender.action == Hero.HeroAction.DEFENSE)
                        {
                            defender.SetAction(Hero.HeroAction.NULL);
                        }
                        else
                        {
                            //将该单位放入SUPPORT_OVER数组是因为要在help的时候去查询
                            defender.SetAction(Hero.HeroAction.SUPPORT_OVER, defender.actionTarget);

                            cellData.supportOvers.Add(defender);

                            cellData.supporters.RemoveAt(0);
                        }
                    }

                    if (!attacker.IsAlive() || attacker.attackTimes == 0)
                    {
                        attacker.SetAction(Hero.HeroAction.ATTACK_OVER, attacker.actionTarget);

                        cellData.attackOvers.Add(attacker);

                        cellData.attackers.RemoveAt(0);
                    }

                    yield return new BattleAttackOverVO(cellData.pos, attacker.pos, defender.pos);
                }
            }

            yield return RemoveDieHero(_battleData);
        }

        private IEnumerator DoMove(BattleData _battleData)
        {
            Dictionary<Hero, int> moveDic = new Dictionary<Hero, int>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

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

            Dictionary<Hero, int>.Enumerator enumerator2 = moveDic.GetEnumerator();

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
                    for (int i = 0; i < captureList.Count; i++)
                    {
                        Hero hero = captureList[i];

                        yield return CaptureArea(hero, hero.pos);
                    }

                    enumerator = heroMapDic.Values.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.ProcessDamage();
                    }

                    yield return RemoveDieHero(_battleData);

                    yield return new BattleRecoverVO();
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
                for (int i = 0; i < _cellData.attackOvers.Count; i++)
                {
                    Hero tmpHero = _cellData.attackOvers[i];

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
            }

            return hero;
        }

        private IEnumerator DoRecover()
        {
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.Recover();
            }

            List<Func<BattleTriggerAuraVO>> list = null;

            eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.ROUND_OVER, ref list, null, null);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i]();
                }

                enumerator = heroMapDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    enumerator.Current.ProcessDamage();
                }

                yield return RemoveDieHero(null);
            }

            yield return new BattleRecoverVO();
        }

        private IEnumerator RemoveHero(BattleData _battleData, Hero _hero)
        {
            yield return _hero.Die();

            heroMapDic.Remove(_hero.pos);

            if (_battleData != null)
            {
                RemoveHeroAction(_battleData, _hero);
            }
        }

        private void RemoveHeroAction(BattleData _battleData, Hero _hero)
        {
            BattleCellData cellData;

            if (_battleData.actionDic.TryGetValue(_hero.pos, out cellData))
            {
                cellData.stander = null;
            }

            if (_hero.action == Hero.HeroAction.ATTACK)
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
            else if (_hero.action == Hero.HeroAction.ATTACK_OVER)
            {
                cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackOvers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SUPPORT_OVER)
            {
                cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.supportOvers.Remove(_hero);
            }

            _hero.SetAction(Hero.HeroAction.NULL);
        }

        private IEnumerator CaptureArea(Hero _hero, int _nowPos)
        {
            if (mapBelongDic.ContainsKey(_nowPos))
            {
                mapBelongDic.Remove(_nowPos);
            }
            else
            {
                mapBelongDic.Add(_nowPos, true);
            }

            List<Func<BattleTriggerAuraVO>> list = null;

            eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.CAPTURE_MAP_AREA, ref list, _hero, null);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i]();
                }
            }
        }

        private IEnumerator DoAddMoney()
        {
            yield return MoneyChange(true, BattleConst.ADD_MONEY);

            yield return MoneyChange(false, BattleConst.ADD_MONEY);
        }

        internal IEnumerator MoneyChange(bool _isMine, int _num)
        {
            MoneyChangeReal(_isMine, _num);

            yield return new BattleMoneyChangeVO(_isMine, _isMine ? mMoney : oMoney);
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

        private IEnumerator DoAddCards()
        {
            yield return AddCards(true, BattleConst.ADD_CARD_NUM);

            yield return AddCards(false, BattleConst.ADD_CARD_NUM);
        }

        private IEnumerator AddCards(bool _isMine, int _num)
        {
            Queue<int> cards = _isMine ? mCards : oCards;

            if (cards.Count > 0)
            {
                if (_num > cards.Count)
                {
                    _num = cards.Count;
                }

                List<int> handCardsList = _isMine ? mHandCards : oHandCards;

                List<int> addList = new List<int>();

                for (int i = 0; i < _num && cards.Count > 0; i++)
                {
                    int uid = cards.Dequeue();

                    addList.Add(uid);

                    if (handCardsList.Count < BattleConst.MAX_HAND_CARD_NUM)
                    {
                        handCardsList.Add(uid);
                    }
                }

                yield return new BattleAddCardsVO(_isMine, addList);
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
                if (GetPosIsMine(_pos) == _isMine)
                {
                    if (!heroMapDic.ContainsKey(_pos))
                    {
                        if (!GetSummonContainsKey(_uid) && !GetSummonContainsValue(_pos))
                        {
                            int nowMoney = GetNowMoney(_isMine);

                            IHeroSDS sds = GetHeroData(GetCard(_uid));

                            if (sds.GetCost() <= nowMoney)
                            {
                                AddSummon(_uid, _pos);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
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

                if (_isMine == uid < BattleConst.DECK_CARD_NUM)
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

        public bool GetActionContainsKey(int _pos, out int _targetPos)
        {
            for (int i = 0; i < action.Count; i++)
            {
                KeyValuePair<int, int> pair = action[i];

                if (pair.Key == _pos)
                {
                    _targetPos = pair.Value;

                    return true;
                }
            }

            _targetPos = -1;

            return false;
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

            int targetPos;

            if (GetActionContainsKey(_pos, out targetPos))
            {
                return false;
            }

            bool targetPosIsMine = GetPosIsMine(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if (targetPosIsMine == hero.isMine)
                {
                    AddAction(_pos, _targetPos);
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

                    AddAction(_pos, _targetPos);
                }

                return true;
            }
            else
            {
                if (hero.sds.GetSkill() != 0 && heroMapDic.ContainsKey(_targetPos))
                {
                    List<int> tmpList2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);

                    if (tmpList2.Contains(_targetPos))
                    {
                        if (targetPosIsMine != hero.isMine)
                        {
                            AddAction(_pos, _targetPos);

                            return true;
                        }
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

        public List<KeyValuePair<int, int>>.Enumerator GetFearActionEnumerator()
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

        internal void SetRandomIndex(int _index)
        {
            randomIndex = _index;
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

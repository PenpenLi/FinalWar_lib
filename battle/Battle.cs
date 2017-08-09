using System;
using System.Collections;
using System.Collections.Generic;
using superEvent;
using System.Linq;

namespace FinalWar
{
    public partial class Battle
    {
        internal static Func<int, MapData> GetMapData;
        internal static Func<int, IHeroSDS> GetHeroData;
        internal static Func<int, ISkillSDS> GetSkillData;
        internal static Func<int, IAuraSDS> GetAuraData;
        internal static Func<int, IEffectSDS> GetEffectData;

        public int mapID { get; private set; }
        public MapData mapData { get; private set; }

        private Dictionary<int, bool> mapBelongDic = new Dictionary<int, bool>();
        public Dictionary<int, Hero> heroMapDic = new Dictionary<int, Hero>();

        private List<int> mCards = new List<int>();
        private List<int> oCards = new List<int>();

        public Dictionary<int, int> mHandCards = new Dictionary<int, int>();
        public Dictionary<int, int> oHandCards = new Dictionary<int, int>();

        public int mScore { get; private set; }
        public int oScore { get; private set; }

        public int mMoney { get; private set; }
        public int oMoney { get; private set; }

        public Dictionary<int, int> summon = new Dictionary<int, int>();

        public List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

        private int cardUid;

        public bool mOver { get; private set; }
        public bool oOver { get; private set; }

        private Queue<int> randomList = new Queue<int>();

        internal SuperEventListener eventListener = new SuperEventListener();

        public bool mWin { get; private set; }
        public bool oWin { get; private set; }

        private bool isVsAi;

        public static void Init<T, U, V, W>(Dictionary<int, MapData> _mapDataDic, Dictionary<int, T> _heroDataDic, Dictionary<int, U> _skillDataDic, Dictionary<int, V> _auraDataDic, Dictionary<int, W> _effectDataDic) where T : IHeroSDS where U : ISkillSDS where V : IAuraSDS where W : IEffectSDS
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
#if !CLIENT
            int result = random.Next(_max);

            randomList.Enqueue(result);
#else
            int result = randomList.Dequeue();
#endif
            return result;
        }

        private IEnumerator StartBattle()
        {
            BattleData battleData = GetBattleData();

            action.Clear();

            yield return DoSkill(battleData);

            yield return DoRoundStart(battleData);

            yield return DoSummon(battleData);

            summon.Clear();

            yield return DoRush(battleData);

            yield return DoAttack(battleData);

            yield return DoRush(battleData);

            yield return DoMove(battleData);

            yield return DoRecover(battleData);

            yield return DoAddMoney();

            yield return DoAddCards();

            RoundOver();
        }

        private void EndBattle()
        {
            if (mWin || oWin)
            {
                BattleOver();
            }
        }

        private void BattleOver()
        {
            eventListener.Clear();

            summon.Clear();

            action.Clear();

            mapBelongDic.Clear();

            heroMapDic.Clear();

            mHandCards.Clear();

            oHandCards.Clear();

#if !CLIENT
            serverBattleOverCallBack();
#endif
        }

        private IEnumerator DoSummon(BattleData _battleData)
        {
            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int tmpCardUid = pair.Key;

                int pos = pair.Value;

                if (heroMapDic.ContainsKey(pos))
                {
                    Log.Write("Summon error0");

                    continue;
                }

                Hero summonHero = SummonOneUnit(tmpCardUid, pos, _battleData);

                ServerAddHero(_battleData, summonHero);

                yield return new BattleSummonVO(summonHero.isMine, tmpCardUid, summonHero.sds.GetID(), pos);
            }
        }

        private Hero SummonOneUnit(int _uid, int _pos, BattleData _battleData)
        {
            bool isMine = GetPosIsMine(_pos);

            IHeroSDS sds;

            if (isMine)
            {
                int heroID = mHandCards[_uid];

                sds = GetHeroData(heroID);

                if (mMoney < sds.GetCost())
                {
                    return null;
                }
#if !CLIENT
                oHandCardsChangeDic.Add(_uid, heroID);
#endif
                mMoney -= sds.GetCost();

                mHandCards.Remove(_uid);
            }
            else
            {
                int heroID = oHandCards[_uid];

                sds = GetHeroData(heroID);

                if (oMoney < sds.GetCost())
                {
                    return null;
                }
#if !CLIENT
                mHandCardsChangeDic.Add(_uid, heroID);
#endif
                oMoney -= sds.GetCost();

                oHandCards.Remove(_uid);
            }

            Hero hero = new Hero(this, isMine, sds, _pos, false);

            return hero;
        }

        private void ServerAddHero(BattleData _battleData, Hero _hero)
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

            for (int i = 0; i < action.Count; i++)
            {
                int pos = action[i].Key;

                int targetPos = action[i].Value;

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

            if (!hero.GetCanAction())
            {
                throw new Exception("action error999");
            }

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

            eventListener.DispatchEvent(BattleConst.ROUND_START, ref list);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i]();
                }
            }

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.ProcessDamage();
            }

            yield return new BattleRoundStartVO();

            yield return RemoveDieHero(_battleData);
        }

        private IEnumerator DoRush(BattleData _battleData)
        {
            Dictionary<int, int> dic = null;

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

                            if (dic == null)
                            {
                                dic = new Dictionary<int, int>();
                            }
                            else
                            {
                                dic.Clear();
                            }

                            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator2 = heroMapDic.Values.GetEnumerator();

                            while (enumerator2.MoveNext())
                            {
                                dic.Add(enumerator2.Current.pos, enumerator2.Current.GetDamage());
                            }

                            yield return new BattlePrepareRushVO();
                        }

                        yield return ProcessCellDataRush(_battleData, cellData, dic);
                    }
                }

                if (hasRush)
                {
                    yield return RemoveDieHero(_battleData);
                }
                else
                {
                    yield return new BattleRushOverVO();

                    break;
                }
            }
        }

        private IEnumerator ProcessCellDataRush(BattleData _battleData, BattleCellData _cellData, Dictionary<int, int> _dic)
        {
            Hero stander = _cellData.stander;

            while (_cellData.attackers.Count > 0 && stander.IsAlive())
            {
                Hero attacker = _cellData.attackers[0];

                attacker.DoAttack();

                if (attacker.attackTimes == 0)
                {
                    _cellData.attackOvers.Add(attacker);

                    _cellData.attackers.RemoveAt(0);

                    attacker.SetAction(Hero.HeroAction.ATTACK_OVER, attacker.actionTarget);
                }

                int damage = _dic[attacker.pos];

                List<BattleHeroEffectVO> effectList = attacker.Attack(stander, damage);

                //因为攻击有先后顺序  所以每打完一次就结算一次
                stander.ProcessDamage();

                yield return new BattleRushVO(attacker.pos, _cellData.pos, effectList);
            }
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

                    RemoveHero(_battleData, hero);
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

                        List<BattleHeroEffectVO> defenderEffectList = attacker.Attack(defender, attackDamage);

                        List<BattleHeroEffectVO> attackerEffectList = defender.Attack(attacker, defenseDamage);

                        defender.ProcessDamage();

                        attacker.ProcessDamage();

                        yield return new BattleAttackAndCounterVO(cellData.pos, attacker.pos, defender.pos, attackDamage, defenseDamage, attackerEffectList, defenderEffectList);
                    }
                    else if (speedDiff >= 1)
                    {
                        attackDamage = attacker.GetDamage();

                        List<BattleHeroEffectVO> effectList = attacker.Attack(defender, attackDamage);

                        defender.ProcessDamage();

                        yield return new BattleAttackVO(cellData.pos, attacker.pos, defender.pos, attackDamage, effectList);

                        if (speedDiff == 1 && defender.IsAlive())
                        {
                            defenseDamage = defender.GetDamage();

                            effectList = defender.Attack(attacker, defenseDamage);

                            attacker.ProcessDamage();

                            yield return new BattleCounterVO(cellData.pos, defender.pos, attacker.pos, defenseDamage, effectList);
                        }
                    }
                    else
                    {
                        defenseDamage = defender.GetDamage();

                        List<BattleHeroEffectVO> effectList = defender.Attack(attacker, defenseDamage);

                        attacker.ProcessDamage();

                        yield return new BattleCounterVO(cellData.pos, defender.pos, attacker.pos, defenseDamage, effectList);

                        if (speedDiff == -1 && attacker.IsAlive())
                        {
                            attackDamage = attacker.GetDamage();

                            effectList = attacker.Attack(defender, attackDamage);

                            defender.ProcessDamage();

                            yield return new BattleAttackVO(cellData.pos, attacker.pos, defender.pos, attackDamage, effectList);
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
                        CaptureArea(hero, pos);
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

        private IEnumerator DoRecover(BattleData _battleData)
        {
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.Recover();
            }

            eventListener.DispatchEvent(BattleConst.ROUND_OVER);

            yield return new BattleRecoverVO();
        }

        private void RemoveHero(BattleData _battleData, Hero _hero)
        {
            _hero.Die();

            heroMapDic.Remove(_hero.pos);

            RemoveHeroAction(_battleData, _hero);

            BattleCellData cellData;

            if (_battleData.actionDic.TryGetValue(_hero.pos, out cellData))
            {
                cellData.stander = null;
            }
        }

        private void RemoveHeroAction(BattleData _battleData, Hero _hero)
        {
            if (_hero.action == Hero.HeroAction.ATTACK)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SHOOT)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.shooters.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SUPPORT)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.supporters.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.ATTACK_OVER)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackOvers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SUPPORT_OVER)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.supportOvers.Remove(_hero);
            }

            _hero.SetAction(Hero.HeroAction.NULL);
        }

        private void CaptureArea(Hero _hero, int _nowPos)
        {
            if (mapData.mBase == _nowPos)
            {
                oWin = true;
            }
            else if (mapData.oBase == _nowPos)
            {
                mWin = true;
            }

            if (mapBelongDic.ContainsKey(_nowPos))
            {
                mapBelongDic.Remove(_nowPos);
            }
            else
            {
                mapBelongDic.Add(_nowPos, true);
            }

            bool b = eventListener.DispatchEvent(BattleConst.CAPTURE_MAP_AREA, _hero);

            if (b)
            {
                _hero.ProcessDamage();
            }
        }

        private IEnumerator DoAddMoney()
        {
            yield return MoneyChange(true, BattleConst.ADD_MONEY);

            if (!isVsAi)
            {
                yield return MoneyChange(false, BattleConst.ADD_MONEY);
            }
            else
            {
                yield return MoneyChange(false, BattleConst.AI_ADD_MONEY);
            }
        }

        internal IEnumerator MoneyChange(bool _isMine, int _num)
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

            yield return new BattleMoneyChangeVO(_isMine, _isMine ? mMoney : oMoney);
        }

        private void RoundOver()
        {
            mOver = oOver = false;
        }

        private int GetCardUid()
        {
            int result = cardUid;

            cardUid++;

            return result;
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

        internal IEnumerator AddCards(bool _isMine, int _num)
        {
            List<int> cards = _isMine ? mCards : oCards;

            if (cards.Count > 0)
            {
                if (_num > cards.Count)
                {
                    _num = cards.Count;
                }

                Dictionary<int, int> handCardsDic = _isMine ? mHandCards : oHandCards;

                if (handCardsDic.Count + _num > BattleConst.MAX_HAND_CARD_NUM)
                {
                    int delNum = handCardsDic.Count + _num - BattleConst.MAX_HAND_CARD_NUM;

                    yield return DelCards(_isMine, delNum);
                }

                Dictionary<int, int> addDic = new Dictionary<int, int>();

                for (int i = 0; i < _num && cards.Count > 0; i++)
                {
                    int index = GetRandomValue(cards.Count);

                    int cardID = cards[index];

                    cards.RemoveAt(index);

                    int tmpCardUid = GetCardUid();

                    handCardsDic.Add(tmpCardUid, cardID);

                    addDic.Add(tmpCardUid, cardID);
                }

                yield return new BattleAddCardsVO(_isMine, addDic);
            }
        }

        internal IEnumerator DelCards(bool _isMine, int _num)
        {
            Dictionary<int, int> handCardsDic = _isMine ? mHandCards : oHandCards;

            List<int> delList = null;

            for (int i = 0; i < _num && handCardsDic.Count > 0; i++)
            {
                int index = GetRandomValue(handCardsDic.Count);

                KeyValuePair<int, int> pair = handCardsDic.ElementAt(index);

                int uid = pair.Key;
#if !CLIENT
                if (_isMine)
                {
                    oHandCardsChangeDic.Add(pair.Key, pair.Value);
                }
                else
                {
                    mHandCardsChangeDic.Add(pair.Key, pair.Value);
                }
#endif
                handCardsDic.Remove(uid);

                if (delList == null)
                {
                    delList = new List<int>();
                }

                delList.Add(uid);
            }

            if (delList != null)
            {
                yield return new BattleDelCardsVO(_isMine, delList);
            }
        }
    }
}

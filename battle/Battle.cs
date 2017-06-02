using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using superEvent;
using collectionTools;
using stepTools;

namespace FinalWar
{
    public class Battle
    {

#if !CLIENT
        public static readonly Random random = new Random();
#endif



        internal static Func<int, MapData> GetMapData;
        internal static Func<int, IHeroSDS> GetHeroData;
        internal static Func<int, ISkillSDS> GetSkillData;
        internal static Func<int, IAuraSDS> GetAuraData;

        public const int DECK_CARD_NUM = 20;
        public const int DEFAULT_HAND_CARD_NUM = 5;
        public const int MAX_HAND_CARD_NUM = 7;
        public const int ADD_CARD_NUM = 1;
        public const int DEFAULT_MONEY = 5;
        public const int AI_DEFAULT_MONEY = 7;
        public const int ADD_MONEY = 2;
        public const int AI_ADD_MONEY = 3;
        public const int MAX_MONEY = 10;

        public int mapID { get; private set; }
        public MapData mapData { get; private set; }

        private Dictionary<int, bool> mapBelongDic = new Dictionary<int, bool>();
        public Dictionary<int, Hero> heroMapDic = new Dictionary<int, Hero>();

        private List<KeyValuePair<int, int>> mCards = new List<KeyValuePair<int, int>>();
        private List<KeyValuePair<int, int>> oCards = new List<KeyValuePair<int, int>>();

        public List<KeyValuePair<int, int>> mHandCards = new List<KeyValuePair<int, int>>();
        public List<KeyValuePair<int, int>> oHandCards = new List<KeyValuePair<int, int>>();

        public int mScore { get; private set; }
        public int oScore { get; private set; }

        public int mMoney { get; private set; }
        public int oMoney { get; private set; }

        public Dictionary<int, int> summon = new Dictionary<int, int>();

        public List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

        private int cardUid;
        private int heroUid;

        public bool mOver { get; private set; }
        public bool oOver { get; private set; }

        private List<int> randomList = new List<int>();


#if CLIENT

        public bool clientIsMine { get; private set; }

        private Action<MemoryStream> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<StepTools<IBattleVO>> clientDoActionCallBack;
        private Action clientBattleOverCallBack;
#else

        private Dictionary<int, int> mCardsChangeDic = new Dictionary<int, int>();
        private Dictionary<int, int> oCardsChangeDic = new Dictionary<int, int>();

        private Dictionary<int, int> mHandCardsChangeDic = new Dictionary<int, int>();
        private Dictionary<int, int> oHandCardsChangeDic = new Dictionary<int, int>();

        private Action<bool, MemoryStream> serverSendDataCallBack;
        private Action serverBattleOverCallBack;

#endif

        internal SuperEventListener eventListener = new SuperEventListener();

        public bool mWin { get; private set; }
        public bool oWin { get; private set; }

        private bool isVsAi;

        public static void Init<T, U, V>(Dictionary<int, MapData> _mapDataDic, Dictionary<int, T> _heroDataDic, Dictionary<int, U> _skillDataDic, Dictionary<int, V> _auraDataDic) where T : IHeroSDS where U : ISkillSDS where V : IAuraSDS
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
        }

#if !CLIENT


        public void ServerSetCallBack(Action<bool, MemoryStream> _serverSendDataCallBack, Action _serverBattleOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;
            serverBattleOverCallBack = _serverBattleOverCallBack;
        }

        public void ServerStart(int _mapID, Dictionary<int, int> _heros, List<int> _mCards, List<int> _oCards, bool _isVsAi)
        {
            Log.Write("Battle Start!");

            isVsAi = _isVsAi;

            mapID = _mapID;

            mapData = GetMapData(mapID);

            mScore = mapData.mScore;
            oScore = mapData.oScore;

            mMoney = DEFAULT_MONEY;

            if (!isVsAi)
            {
                oMoney = DEFAULT_MONEY;
            }
            else
            {
                oMoney = AI_DEFAULT_MONEY;
            }

            mWin = oWin = false;

            mOver = oOver = false;

            cardUid = 1;
            heroUid = 1;

            for (int i = 0; i < _mCards.Count; i++)
            {
                mCards.Add(new KeyValuePair<int, int>(i, _mCards[i]));
            }

            for (int i = 0; i < _oCards.Count; i++)
            {
                oCards.Add(new KeyValuePair<int, int>(i, _oCards[i]));
            }

            for (int i = 0; i < DEFAULT_HAND_CARD_NUM; i++)
            {
                if (mCards.Count > 0)
                {
                    int index = random.Next(mCards.Count);

                    mHandCards.Add(new KeyValuePair<int, int>(GetCardUid(), mCards[index].Value));

                    mCards.RemoveAt(index);
                }

                if (oCards.Count > 0)
                {
                    int index = random.Next(oCards.Count);

                    oHandCards.Add(new KeyValuePair<int, int>(GetCardUid(), oCards[index].Value));

                    oCards.RemoveAt(index);
                }
            }

            if (_heros != null)
            {
                Dictionary<int, int>.Enumerator enumerator = _heros.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    KeyValuePair<int, int> pair = enumerator.Current;

                    int pos = pair.Key;

                    int id = pair.Value;

                    bool isMine = GetPosIsMine(pos);

                    IHeroSDS heroSDS = GetHeroData(id);

                    Hero hero = new Hero(isMine, heroSDS, pos, GetHeroUid());

                    heroMapDic.Add(pos, hero);
                }
            }

            ServerRefreshData(true);

            if (!isVsAi)
            {
                ServerRefreshData(false);
            }
        }

        public void ServerGetPackage(byte[] _bytes, bool _isMine)
        {
            using (MemoryStream ms = new MemoryStream(_bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte tag = br.ReadByte();

                    switch (tag)
                    {
                        case PackageTag.C2S_REFRESH:

                            ServerRefreshData(_isMine);

                            break;

                        case PackageTag.C2S_DOACTION:

                            ServerDoAction(_isMine, br);

                            break;

                        case PackageTag.C2S_QUIT:

                            ServerQuitBattle();

                            break;
                    }
                }
            }
        }

        public void ServerRefreshData(bool _isMine)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Log.Write("ServerRefreshData  isMine:" + _isMine);

                    bw.Write(PackageTag.S2C_REFRESH);

                    bw.Write(_isMine);

                    bw.Write(mScore);

                    bw.Write(oScore);

                    bw.Write(heroUid);

                    bw.Write(cardUid);

                    bw.Write(mapID);

                    bw.Write(mapBelongDic.Count);

                    Dictionary<int, bool>.KeyCollection.Enumerator enumerator2 = mapBelongDic.Keys.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        bw.Write(enumerator2.Current);
                    }

                    bw.Write(heroMapDic.Count);

                    Dictionary<int, Hero>.ValueCollection.Enumerator enumerator3 = heroMapDic.Values.GetEnumerator();

                    while (enumerator3.MoveNext())
                    {
                        Hero hero = enumerator3.Current;

                        bw.Write(hero.sds.GetID());

                        bw.Write(hero.isMine);

                        bw.Write(hero.pos);

                        bw.Write(hero.nowHp);

                        bw.Write(hero.nowShield);
                    }

                    List<KeyValuePair<int, int>> handCards;

                    List<KeyValuePair<int, int>> handCards2;

                    List<KeyValuePair<int, int>> cards;

                    List<KeyValuePair<int, int>> cards2;

                    if (_isMine)
                    {
                        handCards = mHandCards;

                        handCards2 = oHandCards;

                        cards = mCards;

                        cards2 = oCards;
                    }
                    else
                    {
                        handCards = oHandCards;

                        handCards2 = mHandCards;

                        cards = oCards;

                        cards2 = mCards;
                    }

                    bw.Write(handCards.Count);

                    for (int i = 0; i < handCards.Count; i++)
                    {
                        KeyValuePair<int, int> pair = handCards[i];

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(handCards2.Count);

                    for (int i = 0; i < handCards2.Count; i++)
                    {
                        KeyValuePair<int, int> pair = handCards2[i];

                        bw.Write(pair.Key);

                        bw.Write(0);
                    }

                    bw.Write(cards.Count);

                    for (int i = 0; i < cards.Count; i++)
                    {
                        KeyValuePair<int, int> pair = cards[i];

                        bw.Write(pair.Key);

                        bw.Write(0);
                    }

                    bw.Write(cards2.Count);

                    for (int i = 0; i < cards2.Count; i++)
                    {
                        KeyValuePair<int, int> pair = cards2[i];

                        bw.Write(pair.Key);

                        bw.Write(0);
                    }

                    bw.Write(mMoney);

                    bw.Write(oMoney);

                    bool isOver = _isMine ? mOver : oOver;

                    bw.Write(isOver);

                    if (isOver)
                    {
                        int num = 0;

                        List<KeyValuePair<int, int>> tmpList = new List<KeyValuePair<int, int>>();

                        Dictionary<int, int>.Enumerator enumerator4 = summon.GetEnumerator();

                        while (enumerator4.MoveNext())
                        {
                            int pos = enumerator4.Current.Value;

                            if (GetPosIsMine(pos) == _isMine)
                            {
                                num++;

                                tmpList.Add(enumerator4.Current);
                            }
                        }

                        bw.Write(num);

                        for (int i = 0; i < num; i++)
                        {
                            bw.Write(tmpList[i].Key);

                            bw.Write(tmpList[i].Value);
                        }

                        num = 0;

                        tmpList.Clear();

                        for (int i = 0; i < action.Count; i++)
                        {
                            int pos = action[i].Key;

                            if (GetPosIsMine(pos) == _isMine)
                            {
                                num++;

                                tmpList.Add(action[i]);
                            }
                        }

                        bw.Write(num);

                        for (int i = 0; i < num; i++)
                        {
                            bw.Write(tmpList[i].Key);

                            bw.Write(tmpList[i].Value);
                        }
                    }

                    serverSendDataCallBack(_isMine, ms);
                }
            }
        }


        private void ServerDoAction(bool _isMine, BinaryReader _br)
        {
            List<KeyValuePair<int, int>> cards;

            if (_isMine)
            {
                if (mOver)
                {
                    return;
                }
                else
                {
                    mOver = true;
                }

                cards = mHandCards;
            }
            else
            {
                if (oOver)
                {
                    return;
                }
                else
                {
                    oOver = true;
                }

                cards = oHandCards;
            }

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                if (GetPosIsMine(pos) == _isMine)
                {
                    for (int m = 0; m < cards.Count; m++)
                    {
                        if (uid == cards[m].Key)
                        {
                            summon.Add(uid, pos);

                            break;
                        }
                    }
                }
            }

            Dictionary<int, bool> tmpDic = new Dictionary<int, bool>();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                if (mapData.dic.ContainsKey(targetPos))
                {
                    MapData.MapUnitType mapUnitType = mapData.dic[targetPos];

                    if (mapUnitType == MapData.MapUnitType.M_AREA || mapUnitType == MapData.MapUnitType.O_AREA)
                    {
                        if (heroMapDic.ContainsKey(pos) && heroMapDic[pos].isMine == _isMine)
                        {
                            action.Add(new KeyValuePair<int, int>(pos, targetPos));

                            tmpDic.Add(pos, false);
                        }
                    }
                }
            }

            if (!isVsAi)
            {
                if (mOver && oOver)
                {
                    ServerStartBattle();
                }
            }
            else
            {
                HeroAi.Start(this, false, 0.2);

                StartBattle();
            }
        }

        private void ServerStartBattle()
        {
            if (!isVsAi)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        ServerWriteActionAndSummon(true, mBw);

                        ServerWriteActionAndSummon(false, oBw);

                        StepTools<IBattleVO> step = new StepTools<IBattleVO>(StartBattle());

                        step.Done();

                        ServerWriteCardsAndRandom(true, mBw);

                        ServerWriteCardsAndRandom(false, mBw);

                        serverSendDataCallBack(true, mMs);

                        serverSendDataCallBack(false, oMs);
                    }
                }
            }
            else
            {
                using (MemoryStream mMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs))
                    {
                        mBw.Write(PackageTag.S2C_DOACTION);

                        serverSendDataCallBack(true, mMs);
                    }
                }
            }

            mCardsChangeDic.Clear();

            oCardsChangeDic.Clear();

            mHandCardsChangeDic.Clear();

            oHandCardsChangeDic.Clear();

            randomList.Clear();

            EndBattle();
        }

        private void ServerWriteActionAndSummon(bool _isMine, BinaryWriter _bw)
        {
            _bw.Write(PackageTag.S2C_DOACTION);

            long pos0 = _bw.BaseStream.Position;

            _bw.Write(0);

            int num = 0;

            for (int i = 0; i < action.Count; i++)
            {
                KeyValuePair<int, int> pair = action[i];

                if (GetPosIsMine(pair.Key) == _isMine)
                {
                    num++;

                    _bw.Write(pair.Key);

                    _bw.Write(pair.Value);
                }
            }

            _bw.Write(0);

            long pos1 = _bw.BaseStream.Position;

            _bw.BaseStream.Position = pos0;

            _bw.Write(num);

            _bw.BaseStream.Position = pos1;

            num = 0;

            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                if (GetPosIsMine(pair.Key) == _isMine)
                {
                    num++;

                    _bw.Write(pair.Key);

                    _bw.Write(pair.Value);
                }
            }

            pos0 = _bw.BaseStream.Position;

            _bw.BaseStream.Position = pos1;

            _bw.Write(num);

            _bw.BaseStream.Position = pos0;
        }

        private void ServerWriteCardsAndRandom(bool _isMine, BinaryWriter _bw)
        {
            Dictionary<int, int> cards = _isMine ? mCardsChangeDic : oCardsChangeDic;

            _bw.Write(cards.Count);

            Dictionary<int, int>.Enumerator enumerator = cards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _bw.Write(enumerator.Current.Key);

                _bw.Write(enumerator.Current.Value);
            }

            cards = _isMine ? mHandCardsChangeDic : oHandCardsChangeDic;

            _bw.Write(cards.Count);

            enumerator = cards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _bw.Write(enumerator.Current.Key);

                _bw.Write(enumerator.Current.Value);
            }

            _bw.Write(randomList.Count);

            for (int i = 0; i < randomList.Count; i++)
            {
                _bw.Write(randomList[i]);
            }
        }


        private void ServerQuitBattle()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((short)PackageTag.S2C_QUIT);

                    serverSendDataCallBack(true, ms);

                    if (!isVsAi)
                    {
                        serverSendDataCallBack(false, ms);
                    }
                }
            }

            BattleOver();
        }



#endif

        private int GetRandomValue(int _max)
        {
#if !CLIENT
            int result = random.Next(_max);

            randomList.Add(result);

            return result;
#else

            int result = randomList[0];

            randomList.RemoveAt(0);

            return result;
#endif
        }

        private IEnumerator StartBattle()
        {
            BattleData battleData = GetBattleData();

            action.Clear();

            yield return DoSummon(battleData);

            summon.Clear();

            yield return DoRush(battleData);

            yield return DoAttack(battleData);

            yield return DoRush(battleData);

            yield return DoMove(battleData);

            yield return DoRecover(battleData);

            yield return DoAddMoney();

            yield return DoAddCards();
        }

        private void EndBattle()
        {
            if (!mWin && !oWin)
            {
                RecoverOver();
            }
            else
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

                yield return new BattleSummonVO(tmpCardUid, summonHero.sds.GetID(), pos);
            }
        }

        private Hero SummonOneUnit(int _uid, int _pos, BattleData _battleData)
        {
            bool isMine = GetPosIsMine(_pos);

            int heroID = -1;

            IHeroSDS sds;

            if (isMine)
            {
                int index = -1;

                for (int i = 0; i < mHandCards.Count; i++)
                {
                    KeyValuePair<int, int> pair = mHandCards[i];

                    if (pair.Key == _uid)
                    {
                        index = i;

                        heroID = pair.Value;

                        break;
                    }
                }

                sds = GetHeroData(heroID);

                if (mMoney < sds.GetCost())
                {
                    return null;
                }

#if !CLIENT
                oHandCardsChangeDic.Add(_uid, heroID);
#endif

                mMoney -= sds.GetCost();

                mHandCards.RemoveAt(index);
            }
            else
            {
                int index = -1;

                for (int i = 0; i < oHandCards.Count; i++)
                {
                    KeyValuePair<int, int> pair = oHandCards[i];

                    if (pair.Key == _uid)
                    {
                        index = i;

                        heroID = pair.Value;

                        break;
                    }
                }

                sds = GetHeroData(heroID);

                if (oMoney < sds.GetCost())
                {
                    return null;
                }

#if !CLIENT
                mHandCardsChangeDic.Add(_uid, heroID);
#endif

                oMoney -= sds.GetCost();

                oHandCards.RemoveAt(index);
            }

            Hero hero = new Hero(isMine, sds, _pos, GetHeroUid());

            return hero;
        }

        private void ServerAddHero(BattleData _battleData, Hero _hero)
        {
            heroMapDic.Add(_hero.pos, _hero);

            if (_battleData.actionDic.ContainsKey(_hero.pos))
            {
                _battleData.actionDic[_hero.pos].stander = _hero;
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

            if (!hero.sds.GetHeroType().GetCanDoAction())
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

                    if (_battleData.actionDic.ContainsKey(_targetPos))
                    {
                        cellData = _battleData.actionDic[_targetPos];
                    }
                    else
                    {
                        cellData = new BattleCellData(_targetPos);

                        if (heroMapDic.ContainsKey(_targetPos))
                        {
                            cellData.stander = heroMapDic[_targetPos];
                        }

                        _battleData.actionDic.Add(_targetPos, cellData);
                    }

                    cellData.supporters.Add(hero);
                }
                else
                {
                    BattleCellData cellData;

                    int nowThreadLevel = 0;

                    if (heroMapDic.ContainsKey(_targetPos))
                    {
                        Hero targetHero = heroMapDic[_targetPos];

                        nowThreadLevel = targetHero.sds.GetHeroType().GetThread();
                    }

                    for (int i = 0; i < arr.Count; i++)
                    {
                        int tmpPos = arr[i];

                        if (tmpPos == _targetPos)
                        {
                            continue;
                        }

                        if (heroMapDic.ContainsKey(tmpPos))
                        {
                            Hero tmpHero = heroMapDic[tmpPos];

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

                    if (_battleData.actionDic.ContainsKey(_targetPos))
                    {
                        cellData = _battleData.actionDic[_targetPos];
                    }
                    else
                    {
                        cellData = new BattleCellData(_targetPos);

                        if (heroMapDic.ContainsKey(_targetPos))
                        {
                            cellData.stander = heroMapDic[_targetPos];
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

                            if (_battleData.actionDic.ContainsKey(_targetPos))
                            {
                                cellData = _battleData.actionDic[_targetPos];
                            }
                            else
                            {
                                cellData = new BattleCellData(_targetPos);

                                if (heroMapDic.ContainsKey(_targetPos))
                                {
                                    cellData.stander = heroMapDic[_targetPos];
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

        private IEnumerator DoRush(BattleData _battleData)
        {
            while (true)
            {
                LinkedList<BattleCellData> processList = null;

                Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        if (processList == null)
                        {
                            processList = new LinkedList<BattleCellData>();
                        }

                        processList.AddLast(cellData);
                    }
                }

                if (processList != null)
                {
                    LinkedList<BattleCellData>.Enumerator enumerator2 = processList.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        yield return ProcessCellDataRush(_battleData, enumerator2.Current);
                    }

                    yield return RemoveDieHero(_battleData);
                }
                else
                {
                    break;
                }
            }
        }

        private IEnumerator ProcessCellDataRush(BattleData _battleData, BattleCellData _cellData)
        {
            Hero stander = _cellData.stander;

            List<int> attackers = new List<int>();

            List<List<int>> helpers = new List<List<int>>();

            int hpDamage = 0;

            while (_cellData.attackers.Count > 0)
            {
                Hero attacker = _cellData.attackers[0];

                _cellData.attackOvers.Add(attacker);

                _cellData.attackers.RemoveAt(0);

                attacker.SetAction(Hero.HeroAction.ATTACK_OVER, attacker.actionTarget);

                attackers.Add(attacker.pos);

                hpDamage += attacker.GetDamage();

                List<int> tmpList = new List<int>();

                helpers.Add(tmpList);

                if (_battleData.actionDic.ContainsKey(attacker.pos))
                {
                    BattleCellData tmpCellData = _battleData.actionDic[attacker.pos];

                    for (int m = 0; m < tmpCellData.supporters.Count; m++)
                    {
                        Hero tmpHero = tmpCellData.supporters[m];

                        if (tmpHero.sds.GetHeroType().GetCanLendDamageWhenSupport())
                        {
                            hpDamage += tmpHero.GetDamage();

                            tmpList.Add(tmpHero.pos);
                        }
                    }
                }
            }

            stander.BeHpDamage(hpDamage);

            BattleRushVO vo = new BattleRushVO(attackers, helpers, _cellData.pos, -hpDamage);

            yield return vo;
        }

        private IEnumerator RemoveDieHero(BattleData _battleData)
        {
            LinkedList<Hero> dieList = null;

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (!enumerator.Current.IsAlive())
                {
                    if (dieList == null)
                    {
                        dieList = new LinkedList<Hero>();
                    }

                    dieList.AddLast(enumerator.Current);
                }
            }

            if (dieList != null)
            {
                LinkedList<int> tmpList = new LinkedList<int>();

                LinkedList<Hero>.Enumerator enumerator2 = dieList.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    tmpList.AddLast(enumerator2.Current.pos);

                    ServerRemoveHero(_battleData, enumerator2.Current);
                }

                yield return new BattleDeathVO(tmpList);
            }
        }

        private IEnumerator DoAttack(BattleData _battleData)
        {
            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                while (cellData.attackers.Count > 0 && ((cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE) || cellData.supporters.Count > 0))
                {
                    Hero attacker = cellData.attackers[0];

                    List<int> voAttackers = new List<int>() { attacker.pos };

                    List<int> helpers = new List<int>();

                    attacker.SetAction(Hero.HeroAction.ATTACK_OVER, attacker.actionTarget);

                    cellData.attackOvers.Add(attacker);

                    cellData.attackers.RemoveAt(0);

                    int attackDamage = attacker.GetDamage();

                    if (_battleData.actionDic.ContainsKey(attacker.pos))
                    {
                        BattleCellData tmpCellData = _battleData.actionDic[attacker.pos];

                        for (int m = 0; m < tmpCellData.supporters.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supporters[m];

                            if (tmpHero.sds.GetHeroType().GetCanLendDamageWhenSupport())
                            {
                                attackDamage += tmpHero.GetDamage();

                                helpers.Add(tmpHero.pos);
                            }
                        }

                        for (int m = 0; m < tmpCellData.supportOvers.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supportOvers[m];

                            if (tmpHero.sds.GetHeroType().GetCanLendDamageWhenSupport())
                            {
                                attackDamage += tmpHero.GetDamage();

                                helpers.Add(tmpHero.pos);
                            }
                        }
                    }

                    List<List<int>> voHelpers = new List<List<int>>() { helpers };

                    List<int> voSupporters = new List<int>();

                    Hero defender;

                    int defenseDamage;

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        defender = cellData.stander;

                        if (attacker.sds.GetHeroType().GetWillBeDamageByDefense() && defender.sds.GetHeroType().GetCanDoDamageWhenDefense())
                        {
                            defenseDamage = defender.GetDamage();
                        }
                        else
                        {
                            defenseDamage = 0;
                        }
                    }
                    else
                    {
                        defender = cellData.supporters[0];

                        if (attacker.sds.GetHeroType().GetWillBeDamageBySupport() && defender.sds.GetHeroType().GetCanDoDamageWhenSupport())
                        {
                            defenseDamage = defender.GetDamage();
                        }
                        else
                        {
                            defenseDamage = 0;
                        }

                        voSupporters.Add(defender.pos);
                    }

                    int defenderShieldDamage;

                    int defenderHpDamage;

                    int attackerShieldDamage;

                    int attackerHpDamage;

                    defender.BeDamage(attackDamage, out defenderShieldDamage, out defenderHpDamage);

                    attacker.BeDamage(defenseDamage, out attackerShieldDamage, out attackerHpDamage);

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

                    BattleAttackVO vo;

                    if (defender.action == Hero.HeroAction.DEFENSE || defender.action == Hero.HeroAction.NULL)
                    {
                        vo = new BattleAttackVO(voAttackers, voHelpers, voSupporters, cellData.pos, new List<int>() { attackerShieldDamage }, new List<int>() { attackerHpDamage }, new List<int>(), new List<int>(), defenderShieldDamage, defenderHpDamage);
                    }
                    else
                    {
                        vo = new BattleAttackVO(voAttackers, voHelpers, voSupporters, cellData.pos, new List<int>() { attackerShieldDamage }, new List<int>() { attackerHpDamage }, new List<int>() { defenderShieldDamage }, new List<int>() { defenderHpDamage }, 0, 0);
                    }

                    yield return vo;
                }
            }

            yield return RemoveDieHero(_battleData);
        }

        private IEnumerator DoMove(BattleData _battleData)
        {
            LinkedList<int> tmpList = null;

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                BattleCellData cellData = pair.Value;

                if (cellData.stander == null && (cellData.supporters.Count > 0 || cellData.attackers.Count > 0 || cellData.attackOvers.Count > 0))
                {
                    if (tmpList == null)
                    {
                        tmpList = new LinkedList<int>();
                    }

                    tmpList.AddLast(pair.Key);
                }
            }

            if (tmpList != null)
            {
                Dictionary<int, int> tmpMoveDic = null;

                LinkedList<int>.Enumerator enumerator3 = tmpList.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    OneCellEmpty(_battleData, enumerator3.Current, ref tmpMoveDic);
                }

                if (tmpMoveDic != null)
                {
                    yield return new BattleMoveVO(tmpMoveDic);
                }
            }
        }

        internal void HeroLevelUp(Hero _hero, int _id, LinkedList<IBattleVO> _voList)
        {
            _hero.LevelUp(GetHeroData(_id));

            _voList.AddLast(new BattleLevelUpVO(_hero.pos, _id));
        }

        private IEnumerator DoRecover(BattleData _battleData)
        {
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.Recover();
            }
        }

        private void ServerRemoveHero(BattleData _battleData, Hero _hero)
        {
            heroMapDic.Remove(_hero.pos);

            RemoveHeroAction(_battleData, _hero);

            if (_battleData.actionDic.ContainsKey(_hero.pos))
            {
                _battleData.actionDic[_hero.pos].stander = null;
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

        private void OneCellEmpty(BattleData _battleData, int _pos, ref Dictionary<int, int> _tmpMoveDic)
        {
            int nowPos = _pos;

            while (true)
            {
                if (!_battleData.actionDic.ContainsKey(nowPos))
                {
                    return;
                }

                BattleCellData cellData = _battleData.actionDic[nowPos];

                Hero hero = null;

                for (int i = 0; i < cellData.supporters.Count; i++)
                {
                    Hero tmpHero = cellData.supporters[i];

                    if (tmpHero.canMove)
                    {
                        hero = tmpHero;

                        break;
                    }
                }

                if (hero == null)
                {
                    for (int i = 0; i < cellData.attackOvers.Count; i++)
                    {
                        Hero tmpHero = cellData.attackOvers[i];

                        if (tmpHero.canMove)
                        {
                            hero = tmpHero;

                            CaptureArea(hero, nowPos);

                            break;
                        }
                    }

                    if (hero == null)
                    {
                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero tmpHero = cellData.attackers[i];

                            if (tmpHero.canMove)
                            {
                                hero = tmpHero;

                                CaptureArea(hero, nowPos);

                                break;
                            }
                        }
                    }
                }

                if (hero != null)
                {
                    if (_tmpMoveDic == null)
                    {
                        _tmpMoveDic = new Dictionary<int, int>();
                    }

                    _tmpMoveDic.Add(hero.pos, nowPos);

                    heroMapDic.Remove(hero.pos);

                    heroMapDic.Add(nowPos, hero);

                    int tmpPos = hero.pos;

                    hero.PosChange(nowPos);

                    nowPos = tmpPos;
                }
                else
                {
                    return;
                }
            }
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
        }

        private IEnumerator DoAddMoney()
        {
            yield return ServerMoneyChange(true, ADD_MONEY);

            if (!isVsAi)
            {
                yield return ServerMoneyChange(false, ADD_MONEY);
            }
            else
            {
                yield return ServerMoneyChange(false, AI_ADD_MONEY);
            }
        }

        internal IEnumerator ServerMoneyChange(bool _isMine, int _num)
        {
            if (_isMine)
            {
                mMoney += _num;

                if (mMoney > MAX_MONEY)
                {
                    mMoney = MAX_MONEY;
                }
                else if (mMoney < 0)
                {
                    mMoney = 0;
                }
            }
            else
            {
                oMoney += _num;

                if (oMoney > MAX_MONEY)
                {
                    oMoney = MAX_MONEY;
                }
                else if (oMoney < 0)
                {
                    oMoney = 0;
                }
            }

            yield return new BattleMoneyChangeVO(_isMine, _isMine ? mMoney : oMoney);
        }



        private void RecoverOver()
        {
            mOver = oOver = false;
        }

        private int GetCardUid()
        {
            int result = cardUid;

            cardUid++;

            return result;
        }

        private int GetHeroUid()
        {
            int result = heroUid;

            heroUid++;

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
            yield return AddCards(true, ADD_CARD_NUM);

            yield return AddCards(false, ADD_CARD_NUM);
        }

        internal IEnumerator AddCards(bool _isMine, int _num)
        {
            List<KeyValuePair<int, int>> cards = _isMine ? mCards : oCards;

            if (cards.Count > 0)
            {
                if (_num > cards.Count)
                {
                    _num = cards.Count;
                }

                List<KeyValuePair<int, int>> handCardsDic = _isMine ? mHandCards : oHandCards;

                if (handCardsDic.Count + _num > MAX_HAND_CARD_NUM)
                {
                    int delNum = handCardsDic.Count + _num - MAX_HAND_CARD_NUM;

                    yield return DelCards(_isMine, delNum);
                }

                Dictionary<int, int> addDic = new Dictionary<int, int>();

                for (int i = 0; i < _num && cards.Count > 0; i++)
                {
                    int index = GetRandomValue(cards.Count);

                    KeyValuePair<int, int> pair = cards[index];

                    int id = pair.Value;

                    cards.RemoveAt(index);

#if !CLIENT
                    if (_isMine)
                    {
                        mCardsChangeDic.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        oCardsChangeDic.Add(pair.Key, pair.Value);
                    }
#endif

                    int tmpCardUid = GetCardUid();

                    handCardsDic.Add(new KeyValuePair<int, int>(tmpCardUid, id));

                    addDic.Add(tmpCardUid, id);
                }

                yield return new BattleAddCardsVO(_isMine, addDic);
            }
        }

        internal IEnumerator DelCards(bool _isMine, int _num)
        {
            List<KeyValuePair<int, int>> handCardsDic = _isMine ? mHandCards : oHandCards;

            LinkedList<int> delList = null;

            for (int i = 0; i < _num && handCardsDic.Count > 0; i++)
            {
                int index = GetRandomValue(handCardsDic.Count);

                KeyValuePair<int, int> pair = handCardsDic[index];

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

                handCardsDic.RemoveAt(index);

                if (delList == null)
                {
                    delList = new LinkedList<int>();
                }

                delList.AddLast(uid);
            }

            if (delList != null)
            {
                yield return new BattleDelCardsVO(_isMine, delList);
            }
        }
























#if CLIENT

        public void ClientSetCallBack(Action<MemoryStream> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<StepTools<IBattleVO>> _clientDoActionCallBack, Action _clientBattleOverCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
            clientBattleOverCallBack = _clientBattleOverCallBack;
        }

        public void ClientGetPackage(byte[] _bytes)
        {
            MemoryStream ms = new MemoryStream(_bytes);
            BinaryReader br = new BinaryReader(ms);

            byte tag = br.ReadByte();

            switch (tag)
            {
                case PackageTag.S2C_REFRESH:

                    ClientRefreshData(br);

                    br.Close();

                    ms.Dispose();

                    break;

                case PackageTag.S2C_DOACTION:

                    ClientDoAction(br);

                    break;

                case PackageTag.S2C_QUIT:

                    clientBattleOverCallBack();

                    break;
            }
        }

        private void ClientRefreshData(BinaryReader _br)
        {
            clientIsMine = _br.ReadBoolean();

            Log.Write("ClientRefreshData  isMine:" + clientIsMine);

            mScore = _br.ReadInt32();

            oScore = _br.ReadInt32();

            heroUid = _br.ReadInt32();

            cardUid = _br.ReadInt32();

            mapID = _br.ReadInt32();

            mapData = GetMapData(mapID);

            mapBelongDic = new Dictionary<int, bool>();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                mapBelongDic.Add(pos, true);
            }

            heroMapDic.Clear();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int id = _br.ReadInt32();

                bool heroIsMine = _br.ReadBoolean();

                int pos = _br.ReadInt32();

                int nowHp = _br.ReadInt32();

                int nowShield = _br.ReadInt32();

                ClientAddHero(heroIsMine, GetHeroData(id), pos, nowHp, nowShield);
            }

            mHandCards = new List<KeyValuePair<int, int>>();

            oHandCards = new List<KeyValuePair<int, int>>();

            mCards = new List<KeyValuePair<int, int>>();

            oCards = new List<KeyValuePair<int, int>>();

            List<KeyValuePair<int, int>> handCards;

            List<KeyValuePair<int, int>> handCards2;

            List<KeyValuePair<int, int>> cards;

            List<KeyValuePair<int, int>> cards2;

            if (clientIsMine)
            {
                handCards = mHandCards;

                handCards2 = oHandCards;

                cards = mCards;

                cards2 = oCards;
            }
            else
            {
                handCards = oHandCards;

                handCards2 = mHandCards;

                cards = oCards;

                cards2 = mCards;
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards.Add(new KeyValuePair<int, int>(uid, id));
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards2.Add(new KeyValuePair<int, int>(uid, id));
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                cards.Add(new KeyValuePair<int, int>(uid, id));
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                cards2.Add(new KeyValuePair<int, int>(uid, id));
            }

            mMoney = _br.ReadInt32();

            oMoney = _br.ReadInt32();

            bool isOver;

            if (clientIsMine)
            {
                isOver = mOver = _br.ReadBoolean();
            }
            else
            {
                isOver = oOver = _br.ReadBoolean();
            }

            summon.Clear();

            action.Clear();

            if (isOver)
            {
                num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int uid = _br.ReadInt32();

                    int pos = _br.ReadInt32();

                    summon.Add(uid, pos);
                }

                num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int pos = _br.ReadInt32();

                    int targetPos = _br.ReadInt32();

                    action.Add(new KeyValuePair<int, int>(pos, targetPos));
                }
            }

            clientRefreshDataCallBack();
        }

        public bool ClientRequestSummon(int _cardUid, int _pos)
        {
            if (summon.ContainsValue(_pos) || heroMapDic.ContainsKey(_pos) || GetPosIsMine(_pos) != clientIsMine)
            {
                return false;
            }

            List<KeyValuePair<int, int>> handCards = clientIsMine ? mHandCards : oHandCards;

            int cardID = -1;

            for (int i = 0; i < handCards.Count; i++)
            {
                KeyValuePair<int, int> pair = handCards[i];

                if (pair.Key == _cardUid)
                {
                    cardID = pair.Value;

                    break;
                }
            }

            IHeroSDS heroSDS = GetHeroData(cardID);

            if (ClientGetMoney() < heroSDS.GetCost())
            {
                return false;
            }

            summon.Add(_cardUid, _pos);

            return true;
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            summon.Remove(_cardUid);
        }

        public int ClientGetMoney()
        {
            int money = clientIsMine ? mMoney : oMoney;

            List<KeyValuePair<int, int>> cards = clientIsMine ? mHandCards : oHandCards;

            Dictionary<int, int>.KeyCollection.Enumerator enumerator = summon.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    KeyValuePair<int, int> pair = cards[i];

                    if (pair.Key == enumerator.Current)
                    {
                        int cardID = pair.Value;

                        IHeroSDS heroSDS = GetHeroData(cardID);

                        money -= heroSDS.GetCost();

                        break;
                    }
                }
            }

            return money;
        }

        public void ClientRequestQuitBattle()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((short)PackageTag.C2S_QUIT);

                    clientSendDataCallBack(ms);
                }
            }
        }

        public bool ClientRequestAction(int _pos, int _targetPos)
        {
            Hero hero = heroMapDic[_pos];

            if (!hero.sds.GetHeroType().GetCanDoAction())
            {
                return false;
            }

            bool targetPosIsMine = GetPosIsMine(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if (targetPosIsMine == hero.isMine)
                {
                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                }
                else
                {
                    int nowThreadLevel = 0;

                    if (heroMapDic.ContainsKey(_targetPos))
                    {
                        Hero targetHero = heroMapDic[_targetPos];

                        nowThreadLevel = targetHero.sds.GetHeroType().GetThread();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        if (pos != _targetPos)
                        {
                            if (heroMapDic.ContainsKey(pos))
                            {
                                Hero targetHero = heroMapDic[pos];

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

                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
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
                        ISkillSDS skillSDS = GetSkillData(hero.sds.GetSkill());

                        if (targetPosIsMine != hero.isMine)
                        {
                            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void ClientRequestUnaction(int _pos)
        {
            for (int i = 0; i < action.Count; i++)
            {
                if (action[i].Key == _pos)
                {
                    action.RemoveAt(i);

                    break;
                }
            }
        }

        public void ClientRequestDoAction()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_DOACTION);

                    bw.Write(summon.Count);

                    Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(action.Count);

                    for (int i = 0; i < action.Count; i++)
                    {
                        bw.Write(action[i].Key);

                        bw.Write(action[i].Value);
                    }

                    if (clientIsMine)
                    {
                        mOver = true;
                    }
                    else
                    {
                        oOver = true;
                    }

                    clientSendDataCallBack(ms);
                }
            }
        }

        public void ClientRequestRefreshData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_REFRESH);

                    clientSendDataCallBack(ms);
                }
            }
        }


        private Hero ClientAddHero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowShield)
        {
            Hero hero = new Hero(_isMine, _sds, _pos, _nowHp, _nowShield);

            heroMapDic.Add(_pos, hero);

            return hero;
        }


        private void ClientDoAction(BinaryReader _br)
        {
            ClientReadActionAndSummon(_br);

            ClientReadCardsAndRandom(_br);

            clientDoActionCallBack(new StepTools<IBattleVO>(StartBattle()));
        }

        public void ClientEndBattle()
        {
            EndBattle();
        }

        private void ClientReadActionAndSummon(BinaryReader _br)
        {
            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int target = _br.ReadInt32();

                action.Add(new KeyValuePair<int, int>(pos, target));
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                summon.Add(uid, pos);
            }
        }

        private void ClientReadCardsAndRandom(BinaryReader _br)
        {
            int num = _br.ReadInt32();

            List<KeyValuePair<int, int>> cards = clientIsMine ? mCards : oCards;

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                for (int m = 0; m < cards.Count; m++)
                {
                    KeyValuePair<int, int> pair = cards[m];

                    if (pair.Key == uid)
                    {
                        cards[m] = new KeyValuePair<int, int>(uid, id);

                        break;
                    }
                }
            }

            num = _br.ReadInt32();

            cards = clientIsMine ? mHandCards : oHandCards;

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                for (int m = 0; m < cards.Count; m++)
                {
                    KeyValuePair<int, int> pair = cards[m];

                    if (pair.Key == uid)
                    {
                        cards[m] = new KeyValuePair<int, int>(uid, id);

                        break;
                    }
                }
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int value = _br.ReadInt32();

                randomList.Add(value);
            }
        }

        public bool GetClientCanAction()
        {
            return !(clientIsMine ? mOver : oOver);
        }
#endif




        public List<int> GetCanAttackHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            int nowThreadLevel = 0;

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.sds.GetHeroType().GetThread() > nowThreadLevel)
                    {
                        nowThreadLevel = hero.sds.GetHeroType().GetThread();

                        result.Clear();

                        result.Add(pos);
                    }
                    else if (hero.sds.GetHeroType().GetThread() == nowThreadLevel)
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }


        public List<int> GetCanAttackPos(Hero _hero)
        {
            List<int> result = new List<int>();

            int nowThreadLevel = 0;

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine)
                {
                    if (heroMapDic.ContainsKey(pos))
                    {
                        Hero hero = heroMapDic[pos];

                        if (hero.sds.GetHeroType().GetThread() > nowThreadLevel)
                        {
                            nowThreadLevel = hero.sds.GetHeroType().GetThread();

                            result.Clear();

                            result.Add(pos);
                        }
                        else if (hero.sds.GetHeroType().GetThread() == nowThreadLevel)
                        {
                            result.Add(pos);
                        }
                    }
                    else
                    {
                        if (nowThreadLevel == 0)
                        {
                            result.Add(pos);
                        }
                    }
                }
            }

            return result;
        }

        public List<int> GetCanShootHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    return result;
                }
            }

            posList = BattlePublicTools.GetNeighbourPos2(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    result.Add(pos);
                }
            }

            return result;
        }

        public List<int> GetCanThrowHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    return result;
                }
            }

            posList = BattlePublicTools.GetNeighbourPos3(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    result.Add(pos);
                }
            }

            return result;
        }

        public bool CheckHeroCanBeAttack(Hero _hero)
        {
            int nowThreadLevel = _hero.sds.GetHeroType().GetThread();

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                if (heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.isMine != _hero.isMine)
                    {
                        List<int> tmpPosList = BattlePublicTools.GetNeighbourPos(mapData, pos);

                        bool canAttack = true;

                        for (int m = 0; m < tmpPosList.Count; m++)
                        {
                            int tmpPos = tmpPosList[m];

                            if (tmpPos == _hero.pos)
                            {
                                continue;
                            }

                            if (heroMapDic.ContainsKey(tmpPos))
                            {
                                Hero tmpHero = heroMapDic[tmpPos];

                                if (tmpHero.isMine == _hero.isMine)
                                {
                                    if (tmpHero.sds.GetHeroType().GetThread() > nowThreadLevel)
                                    {
                                        canAttack = false;

                                        break;
                                    }
                                }
                            }
                        }

                        if (canAttack)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckPosCanBeAttack(int _pos)
        {
            if (heroMapDic.ContainsKey(_pos))
            {
                Hero hero = heroMapDic[_pos];

                return CheckHeroCanBeAttack(hero);
            }

            int nowThreadLevel = 0;

            bool isMine = GetPosIsMine(_pos);

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != isMine)
                {
                    if (heroMapDic.ContainsKey(pos))
                    {
                        Hero hero = heroMapDic[pos];

                        List<int> tmpPosList = BattlePublicTools.GetNeighbourPos(mapData, pos);

                        bool canAttack = true;

                        for (int m = 0; m < tmpPosList.Count; m++)
                        {
                            int tmpPos = tmpPosList[m];

                            if (tmpPos == _pos)
                            {
                                continue;
                            }

                            b = GetPosIsMine(tmpPos);

                            if (b == isMine && heroMapDic.ContainsKey(tmpPos))
                            {
                                Hero tmpHero = heroMapDic[tmpPos];

                                if (tmpHero.sds.GetHeroType().GetThread() > nowThreadLevel)
                                {
                                    canAttack = false;

                                    break;
                                }
                            }
                        }

                        if (canAttack)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public List<int> GetCanSupportHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b == _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    if (CheckHeroCanBeAttack(heroMapDic[pos]))
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }

        public List<int> GetCanHelpHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b == _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    List<int> posList2 = BattlePublicTools.GetNeighbourPos(mapData, pos);

                    for (int m = 0; m < posList2.Count; m++)
                    {
                        int pos2 = posList2[m];

                        b = GetPosIsMine(pos2);

                        if (b != _hero.isMine && heroMapDic.ContainsKey(pos2))
                        {
                            result.Add(pos);

                            break;
                        }
                    }
                }
            }

            return result;
        }

        public List<int> GetCanSupportPos(Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b == _hero.isMine)
                {
                    if (CheckPosCanBeAttack(pos))
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }
    }
}

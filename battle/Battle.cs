using System;
using System.Collections.Generic;
using System.IO;
using superEvent;
using collectionTools;

namespace FinalWar
{
    public class Battle
    {
        public static readonly Random random = new Random();

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

        private List<int> mCards;
        private List<int> oCards;

        public Dictionary<int, int> mHandCards = new Dictionary<int, int>();
        public Dictionary<int, int> oHandCards = new Dictionary<int, int>();

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

        private Action<bool, MemoryStream> serverSendDataCallBack;
        private Action serverBattleOverCallBack;

        public bool clientIsMine { get; private set; }

        private Action<MemoryStream> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<IEnumerator<IBattleVO>> clientDoActionCallBack;
        private Action clientBattleOverCallBack;

        internal SuperEventListener eventListener = new SuperEventListener();
        internal SuperEventListenerV eventListenerV = new SuperEventListenerV();

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

        public void ServerSetCallBack(Action<bool, MemoryStream> _serverSendDataCallBack, Action _serverBattleOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;
            serverBattleOverCallBack = _serverBattleOverCallBack;
        }

        public void ClientSetCallBack(Action<MemoryStream> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<IEnumerator<IBattleVO>> _clientDoActionCallBack, Action _clientBattleOverCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
            clientBattleOverCallBack = _clientBattleOverCallBack;
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

            mCards = new List<int>(_mCards);
            oCards = new List<int>(_oCards);

            for (int i = 0; i < DEFAULT_HAND_CARD_NUM; i++)
            {
                if (mCards.Count > 0)
                {
                    int index = random.Next(mCards.Count);

                    mHandCards.Add(GetCardUid(), mCards[index]);

                    mCards.RemoveAt(index);
                }

                if (oCards.Count > 0)
                {
                    int index = random.Next(oCards.Count);

                    oHandCards.Add(GetCardUid(), oCards[index]);

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

                    Hero hero = new Hero(eventListenerV, isMine, heroSDS, pos, GetHeroUid());

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

                    Dictionary<int, int> handCards;

                    Dictionary<int, int> handCards2;

                    if (_isMine)
                    {
                        handCards = mHandCards;

                        handCards2 = oHandCards;
                    }
                    else
                    {
                        handCards = oHandCards;

                        handCards2 = mHandCards;
                    }

                    bw.Write(handCards.Count);

                    Dictionary<int, int>.Enumerator enumerator4 = handCards.GetEnumerator();

                    while (enumerator4.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator4.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(handCards2.Count);

                    enumerator4 = handCards2.GetEnumerator();

                    while (enumerator4.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator4.Current;

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

                        enumerator4 = summon.GetEnumerator();

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

            mHandCards = new Dictionary<int, int>();

            oHandCards = new Dictionary<int, int>();

            Dictionary<int, int> handCards;

            Dictionary<int, int> handCards2;

            if (clientIsMine)
            {
                handCards = mHandCards;

                handCards2 = oHandCards;
            }
            else
            {
                handCards = oHandCards;

                handCards2 = mHandCards;
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards.Add(uid, id);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards2.Add(uid, id);
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

            Dictionary<int, int> handCards = clientIsMine ? mHandCards : oHandCards;

            int cardID = handCards[_cardUid];

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

            Dictionary<int, int> cards = clientIsMine ? mHandCards : oHandCards;

            Dictionary<int, int>.KeyCollection.Enumerator enumerator = summon.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int cardID = cards[enumerator.Current];

                IHeroSDS heroSDS = GetHeroData(cardID);

                money -= heroSDS.GetCost();
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

            LinkedList<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

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

                    LinkedList<int>.Enumerator enumerator = tmpList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

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
                    LinkedList<int> tmpList2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);

                    if (tmpList2.Contains(_targetPos))
                    {
                        ISkillSDS skillSDS = GetSkillData(hero.sds.GetSkill());

                        if ((targetPosIsMine == hero.isMine) == skillSDS.GetSkillTargetAlly())
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

        private void ServerDoAction(bool _isMine, BinaryReader _br)
        {
            Dictionary<int, int> cards;

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

                if (cards.ContainsKey(uid))
                {
                    if (GetPosIsMine(pos) == _isMine)
                    {
                        summon.Add(uid, pos);
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

                ServerStartBattle();
            }
        }

        private Hero ClientAddHero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowShield)
        {
            Hero hero = new Hero(_isMine, _sds, _pos, _nowHp, _nowShield);

            heroMapDic.Add(_pos, hero);

            return hero;
        }

        private void ServerStartBattle()
        {
            //记录战前情况
            FileInfo fi = new FileInfo("e:/aaa.txt");

            if (fi.Exists)
            {
                fi.Delete();
            }

            using (FileStream fs = fi.Create())
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    string str = "";

                    Dictionary<int, Hero>.ValueCollection.Enumerator ee = heroMapDic.Values.GetEnumerator();

                    while (ee.MoveNext())
                    {
                        str += "isMine:" + ee.Current.isMine + "  id:" + ee.Current.sds.GetID() + "  pos:" + ee.Current.pos + "  hp:" + ee.Current.nowHp + "  shield:" + ee.Current.nowShield + "\n";
                    }

                    for (int i = 0; i < action.Count; i++)
                    {
                        str += action[i].Key + "--->" + action[i].Value + "\n";
                    }

                    bw.Write(str);
                }
            }
            //----

            LinkedList<IBattleVO> voList = new LinkedList<IBattleVO>();

            BattleData battleData = GetBattleData();

            action.Clear();

            ServerDoSummon(battleData, voList);

            summon.Clear();

            ServerDoRush(battleData, voList);

            ServerDoAttack(battleData, voList);

            ServerDoRush(battleData, voList);

            ServerDoMove(battleData, voList);

            ServerDoRecover(battleData, voList);

            ServerDoAddCardsAndMoney(voList);

            //eventListener.LogNum();

            //eventListenerV.LogNum();

            if (!isVsAi)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        mBw.Write(PackageTag.S2C_DOACTION);

                        oBw.Write(PackageTag.S2C_DOACTION);

                        BattleVOTools.WriteDataToStream(true, voList, mBw);

                        BattleVOTools.WriteDataToStream(false, voList, oBw);

                        mBw.Write(mWin);

                        mBw.Write(oWin);

                        oBw.Write(mWin);

                        oBw.Write(oWin);

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

                        BattleVOTools.WriteDataToStream(true, voList, mBw);

                        mBw.Write(mWin);

                        mBw.Write(oWin);

                        //下发比对数据
                        Log.Write("server num:" + heroMapDic.Count);

                        mBw.Write(heroMapDic.Count);

                        Dictionary<int, Hero>.ValueCollection.Enumerator enumeratorX = heroMapDic.Values.GetEnumerator();

                        while (enumeratorX.MoveNext())
                        {
                            mBw.Write(enumeratorX.Current.pos);

                            mBw.Write(enumeratorX.Current.nowHp);

                            mBw.Write(enumeratorX.Current.nowShield);

                            Log.Write("client  pos:" + enumeratorX.Current.pos + "  hp:" + enumeratorX.Current.nowHp + "  shield:" + enumeratorX.Current.nowShield);
                        }
                        //----

                        serverSendDataCallBack(true, mMs);
                    }
                }
            }

            if (!mWin && !oWin)
            {
                RecoverOver();
            }
            else
            {
                BattleOver();
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

        private void BattleOver()
        {
            eventListener.Clear();

            eventListenerV.Clear();

            summon.Clear();

            action.Clear();

            mapBelongDic.Clear();

            heroMapDic.Clear();

            mHandCards.Clear();

            oHandCards.Clear();

            serverBattleOverCallBack();
        }

        private void ServerDoSummon(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<int, Hero> summonDic = null;

            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int tmpCardUid = pair.Key;

                int pos = pair.Value;

                if (heroMapDic.ContainsKey(pos) || (summonDic != null && summonDic.ContainsKey(pos)))
                {
                    Log.Write("Summon error0");

                    continue;
                }

                Hero summonHero = SummonOneUnit(tmpCardUid, pos, _battleData);

                if (summonDic == null)
                {
                    summonDic = new Dictionary<int, Hero>();
                }

                summonDic.Add(pos, summonHero);

                _voList.AddLast(new BattleSummonVO(tmpCardUid, summonHero.sds.GetID(), pos));
            }

            if (summonDic != null)
            {
                Dictionary<int, Hero>.ValueCollection.Enumerator enumerator2 = summonDic.Values.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    ServerAddHero(_battleData, enumerator2.Current);
                }
            }
        }

        private Hero SummonOneUnit(int _uid, int _pos, BattleData _battleData)
        {
            bool isMine = GetPosIsMine(_pos);

            int heroID;

            IHeroSDS sds;

            if (isMine)
            {
                heroID = mHandCards[_uid];

                sds = GetHeroData(heroID);

                if (mMoney < sds.GetCost())
                {
                    return null;
                }

                mMoney -= sds.GetCost();

                mHandCards.Remove(_uid);
            }
            else
            {
                heroID = oHandCards[_uid];

                sds = GetHeroData(heroID);

                if (oMoney < sds.GetCost())
                {
                    return null;
                }

                oMoney -= sds.GetCost();

                oHandCards.Remove(_uid);
            }

            Hero hero = new Hero(eventListenerV, isMine, sds, _pos, GetHeroUid());

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

            LinkedList<int> arr = BattlePublicTools.GetNeighbourPos(mapData, _pos);

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

                    LinkedList<int>.Enumerator enumerator = arr.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int tmpPos = enumerator.Current;

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
                    LinkedList<int> arr2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);

                    if (arr2.Contains(_targetPos))
                    {
                        ISkillSDS skillSDS = GetSkillData(hero.sds.GetSkill());

                        if ((targetPosIsMine == hero.isMine) == skillSDS.GetSkillTargetAlly())
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

        private void ServerDoRush(BattleData _battleData, LinkedList<IBattleVO> _voList)
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
                        ProcessCellDataRush(_battleData, enumerator2.Current, _voList);
                    }

                    RemoveDieHero(_battleData, _voList);
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessCellDataRush(BattleData _battleData, BattleCellData _cellData, LinkedList<IBattleVO> _voList)
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

            _voList.AddLast(vo);
        }

        private void RemoveDieHero(BattleData _battleData, LinkedList<IBattleVO> _voList)
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

                _voList.AddLast(new BattleDeathVO(tmpList));
            }
        }

        private void ServerDoAttack(BattleData _battleData, LinkedList<IBattleVO> _voList)
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

                    defender.BeDamage(attackDamage);

                    attacker.BeDamage(defenseDamage);

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

                    _voList.AddLast(vo);
                }
            }

            RemoveDieHero(_battleData, _voList);
        }

        private void ServerDoMove(BattleData _battleData, LinkedList<IBattleVO> _voList)
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
                    _voList.AddLast(new BattleMoveVO(tmpMoveDic));
                }
            }
        }

        internal void HeroLevelUp(Hero _hero, int _id, LinkedList<IBattleVO> _voList)
        {
            _hero.LevelUp(GetHeroData(_id));

            _voList.AddLast(new BattleLevelUpVO(_hero.pos, _id));
        }

        private void ServerDoRecover(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.ServerRecover(_voList);
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

        private void ServerDoAddCardsAndMoney(LinkedList<IBattleVO> _voList)
        {
            ServerMoneyChange(true, ADD_MONEY, _voList);

            if (!isVsAi)
            {
                ServerMoneyChange(false, ADD_MONEY, _voList);
            }
            else
            {
                ServerMoneyChange(false, AI_ADD_MONEY, _voList);
            }

            ServerAddCards(true, ADD_CARD_NUM, _voList);

            ServerAddCards(false, ADD_CARD_NUM, _voList);
        }

        internal void ServerMoneyChange(bool _isMine, int _num, LinkedList<IBattleVO> _voList)
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

            _voList.AddLast(new BattleMoneyChangeVO(_isMine, _isMine ? mMoney : oMoney));
        }

        internal void ServerAddCards(bool _isMine, int _num, LinkedList<IBattleVO> _voList)
        {
            List<int> cards = _isMine ? mCards : oCards;

            if (cards.Count > 0)
            {
                if (_num > cards.Count)
                {
                    _num = cards.Count;
                }

                Dictionary<int, int> handCardsDic = _isMine ? mHandCards : oHandCards;

                if (handCardsDic.Count + _num > MAX_HAND_CARD_NUM)
                {
                    int delNum = handCardsDic.Count + _num - MAX_HAND_CARD_NUM;

                    ServerDelCards(_isMine, delNum, _voList);
                }

                Dictionary<int, int> addDic = new Dictionary<int, int>();

                for (int i = 0; i < _num && cards.Count > 0; i++)
                {
                    int index = random.Next(cards.Count);

                    int id = cards[index];

                    cards.RemoveAt(index);

                    int tmpCardUid = GetCardUid();

                    handCardsDic.Add(tmpCardUid, id);

                    addDic.Add(tmpCardUid, id);
                }

                _voList.AddLast(new BattleAddCardsVO(_isMine, addDic));
            }
        }

        internal void ServerDelCards(bool _isMine, int _num, LinkedList<IBattleVO> _voList)
        {
            Dictionary<int, int> handCardsDic = _isMine ? mHandCards : oHandCards;

            LinkedList<int> delList = null;

            for (int i = 0; i < _num && handCardsDic.Count > 0; i++)
            {
                int uid = CollectionTools.ChooseOneKeyFromDic(handCardsDic, random);

                handCardsDic.Remove(uid);

                if (delList == null)
                {
                    delList = new LinkedList<int>();
                }

                delList.AddLast(uid);
            }

            if (delList != null)
            {
                _voList.AddLast(new BattleDelCardsVO(_isMine, delList));
            }
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




















        private void ClientDoAction(BinaryReader _br)
        {
            summon.Clear();

            action.Clear();

            clientDoActionCallBack(ClientDoActionReal(_br));
        }

        private IEnumerator<IBattleVO> ClientDoActionReal(BinaryReader _br)
        {
            LinkedList<IBattleVO> voList = BattleVOTools.ReadDataFromStream(_br);

            LinkedList<IBattleVO>.Enumerator enumerator = voList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                IBattleVO vo = enumerator.Current;

                if (vo is BattleSummonVO)
                {
                    ClientDoSummon((BattleSummonVO)vo);
                }
                else if (vo is BattleMoveVO)
                {
                    ClientDoMove((BattleMoveVO)vo);
                }
                else if (vo is BattleRushVO)
                {
                    ClientDoRush((BattleRushVO)vo);
                }
                else if (vo is BattleShootVO)
                {
                    ClientDoShoot((BattleShootVO)vo);
                }
                else if (vo is BattleAttackVO)
                {
                    ClientDoAttack((BattleAttackVO)vo);
                }
                else if (vo is BattleDeathVO)
                {
                    ClientDoDie((BattleDeathVO)vo);
                }
                else if (vo is BattleChangeVO)
                {
                    ClientDoChange((BattleChangeVO)vo);
                }
                else if (vo is BattleAddCardsVO)
                {
                    ClientDoAddCards((BattleAddCardsVO)vo);
                }
                else if (vo is BattleDelCardsVO)
                {
                    ClientDoDelCards((BattleDelCardsVO)vo);
                }
                else if (vo is BattleMoneyChangeVO)
                {
                    ClientDoMoneyChange((BattleMoneyChangeVO)vo);
                }
                else if (vo is BattleLevelUpVO)
                {
                    ClientDoLevelUp((BattleLevelUpVO)vo);
                }
                else if (vo is BattleRecoverShieldVO)
                {
                    ClientDoRecoverShield((BattleRecoverShieldVO)vo);
                }

                yield return vo;
            }

            ClientDoRecover(_br);
        }

        private void ClientDoSummon(BattleSummonVO _vo)
        {
            bool isMine = GetPosIsMine(_vo.pos);

            IHeroSDS sds = GetHeroData(_vo.heroID);

            if (isMine)
            {
                mHandCards.Remove(_vo.cardUid);

                mMoney -= sds.GetCost();
            }
            else
            {
                oHandCards.Remove(_vo.cardUid);

                oMoney -= sds.GetCost();
            }

            ClientAddHero(isMine, sds, _vo.pos, sds.GetHp(), sds.GetShield());
        }

        private void ClientDoMove(BattleMoveVO _vo)
        {
            Dictionary<int, Hero> tmpDic = new Dictionary<int, Hero>();

            Dictionary<int, int>.Enumerator enumerator = _vo.moves.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                tmpDic.Add(pair.Value, heroMapDic[pair.Key]);

                heroMapDic.Remove(pair.Key);
            }

            Dictionary<int, Hero>.Enumerator enumerator2 = tmpDic.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                KeyValuePair<int, Hero> pair = enumerator2.Current;

                int nowPos = pair.Key;

                Hero hero = pair.Value;

                heroMapDic.Add(nowPos, hero);

                hero.PosChange(nowPos);

                bool isMine = GetPosIsMine(nowPos);

                if (isMine != hero.isMine)
                {
                    if (mapBelongDic.ContainsKey(nowPos))
                    {
                        mapBelongDic.Remove(nowPos);
                    }
                    else
                    {
                        mapBelongDic.Add(nowPos, true);
                    }
                }
            }
        }

        private void ClientDoRush(BattleRushVO _vo)
        {
            Hero hero = heroMapDic[_vo.stander];

            hero.ShieldChange(_vo.shieldDamage);

            hero.HpChange(_vo.hpDamage);
        }

        private void ClientDoShoot(BattleShootVO _vo)
        {
            Hero hero = heroMapDic[_vo.stander];

            hero.ShieldChange(_vo.shieldDamage);

            hero.HpChange(_vo.hpDamage);
        }

        private void ClientDoAttack(BattleAttackVO _vo)
        {
            for (int i = 0; i < _vo.attackers.Count; i++)
            {
                Hero hero = heroMapDic[_vo.attackers[i]];

                hero.ShieldChange(_vo.attackersShieldDamage[i]);

                hero.HpChange(_vo.attackersHpDamage[i]);

                Log.Write("attacker be damage  shield:" + _vo.attackersShieldDamage[i] + "  hp:" + _vo.attackersHpDamage[i]);
            }

            for (int i = 0; i < _vo.supporters.Count; i++)
            {
                Hero hero = heroMapDic[_vo.supporters[i]];

                hero.ShieldChange(_vo.supportersShieldDamage[i]);

                hero.HpChange(_vo.supportersHpDamage[i]);

                Log.Write("supporter be damage  shield:" + _vo.supportersShieldDamage[i] + "  hp:" + _vo.supportersHpDamage[i]);
            }

            if (heroMapDic.ContainsKey(_vo.defender))
            {
                Hero hero = heroMapDic[_vo.defender];

                hero.ShieldChange(_vo.defenderShieldDamage);

                hero.HpChange(_vo.defenderHpDamage);

                Log.Write("defender be damage  shield:" + _vo.defenderShieldDamage + "  hp:" + _vo.defenderHpDamage);
            }
        }

        private void ClientDoDie(BattleDeathVO _vo)
        {
            LinkedList<int>.Enumerator enumerator = _vo.deads.GetEnumerator();

            while (enumerator.MoveNext())
            {
                heroMapDic.Remove(enumerator.Current);
            }
        }

        private void ClientDoChange(BattleChangeVO _vo)
        {
            for (int i = 0; i < _vo.pos.Count; i++)
            {
                Hero hero = heroMapDic[_vo.pos[i]];

                hero.ShieldChange(_vo.shieldChange[i]);

                hero.HpChange(_vo.hpChange[i]);
            }
        }

        private void ClientDoAddCards(BattleAddCardsVO _vo)
        {
            Dictionary<int, int> handCards = _vo.isMine ? mHandCards : oHandCards;

            Dictionary<int, int>.Enumerator enumerator = _vo.addCards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                handCards.Add(pair.Key, pair.Value);
            }
        }

        private void ClientDoDelCards(BattleDelCardsVO _vo)
        {
            Dictionary<int, int> handCards = _vo.isMine ? mHandCards : oHandCards;

            LinkedList<int>.Enumerator enumerator = _vo.delCards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                handCards.Remove(enumerator.Current);
            }
        }

        private void ClientDoMoneyChange(BattleMoneyChangeVO _vo)
        {
            if (_vo.isMine)
            {
                mMoney = _vo.money;
            }
            else
            {
                oMoney = _vo.money;
            }
        }

        private void ClientDoLevelUp(BattleLevelUpVO _vo)
        {
            Hero hero = heroMapDic[_vo.pos];

            IHeroSDS sds = GetHeroData(_vo.id);

            hero.LevelUp(sds);
        }

        private void ClientDoRecoverShield(BattleRecoverShieldVO _vo)
        {
            Hero hero = heroMapDic[_vo.pos];

            hero.ClientRecover();
        }

        private void ClientDoRecover(BinaryReader _br)
        {
            Log.Write("ClientDoRecover!");

            mWin = _br.ReadBoolean();

            oWin = _br.ReadBoolean();

            if (clientIsMine)
            {
                mOver = false;
            }
            else
            {
                oOver = false;
            }

            clientRefreshDataCallBack();

            if (mWin)
            {
                mWin = false;
            }

            if (oWin)
            {
                oWin = false;
            }

            //比对下发的数据
            int numx = _br.ReadInt32();

            Log.Write("client num:" + numx);

            for (int i = 0; i < numx; i++)
            {
                int pos = _br.ReadInt32();
                int hp = _br.ReadInt32();
                int shield = _br.ReadInt32();

                Log.Write("client  pos:" + pos + "  hp:" + hp + "  shield:" + shield);

                if (heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.nowHp != hp)
                    {
                        throw new Exception("hp error  server:" + hp + "  client:" + hero.nowHp);
                    }

                    if (hero.nowShield != shield)
                    {
                        throw new Exception("shield error  server:" + shield + "  client:" + hero.nowShield);
                    }
                }
                else
                {
                    throw new Exception("pos error  server:" + pos);
                }
            }
            //----
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

        public List<int> GetCanAttackHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            int nowThreadLevel = 0;

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    return result;
                }
            }

            posList = BattlePublicTools.GetNeighbourPos2(mapData, _hero.pos);

            enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    return result;
                }
            }

            posList = BattlePublicTools.GetNeighbourPos3(mapData, _hero.pos);

            enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

                if (heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.isMine != _hero.isMine)
                    {
                        LinkedList<int> tmpPosList = BattlePublicTools.GetNeighbourPos(mapData, pos);

                        LinkedList<int>.Enumerator enumerator2 = tmpPosList.GetEnumerator();

                        bool canAttack = true;

                        while (enumerator2.MoveNext())
                        {
                            int tmpPos = enumerator2.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

                bool b = GetPosIsMine(pos);

                if (b != isMine)
                {
                    if (heroMapDic.ContainsKey(pos))
                    {
                        Hero hero = heroMapDic[pos];

                        LinkedList<int> tmpPosList = BattlePublicTools.GetNeighbourPos(mapData, pos);

                        LinkedList<int>.Enumerator enumerator2 = tmpPosList.GetEnumerator();

                        bool canAttack = true;

                        while (enumerator2.MoveNext())
                        {
                            int tmpPos = enumerator2.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

                bool b = GetPosIsMine(pos);

                if (b == _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    LinkedList<int> posList2 = BattlePublicTools.GetNeighbourPos(mapData, pos);

                    LinkedList<int>.Enumerator enumerator2 = posList2.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        int pos2 = enumerator2.Current;

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

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

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

        public bool GetClientCanAction()
        {
            return !(clientIsMine ? mOver : oOver);
        }
    }
}

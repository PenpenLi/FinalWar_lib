using System;
using System.Collections.Generic;
using System.IO;
using superEvent;
using publicTools;

namespace FinalWar
{
    public class Battle
    {
        internal const string REMOVE_EVENT_NAME = "remove";

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
        public const int ADD_MONEY = 2;
        public const int AI_ADD_MONEY = 3;
        public const int MAX_MONEY = 10;

        public int mapID;
        public MapData mapData;

        private Dictionary<int, bool> mapBelongDic = new Dictionary<int, bool>();
        public Dictionary<int, Hero> heroMapDic = new Dictionary<int, Hero>();

        private List<int> mCards;
        private List<int> oCards;

        public Dictionary<int, int> mHandCards = new Dictionary<int, int>();
        public Dictionary<int, int> oHandCards = new Dictionary<int, int>();

        public int mScore;
        public int oScore;

        public int mMoney;
        public int oMoney;

        public Dictionary<int, int> summon = new Dictionary<int, int>();

        public List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

        public Dictionary<int, int> autoAction = new Dictionary<int, int>();

        private int cardUid;
        private int heroUid;

        public bool mOver;
        public bool oOver;

        private Action<bool, MemoryStream> serverSendDataCallBack;
        private Action serverBattleOverCallBack;

        public bool clientIsMine;

        private Action<MemoryStream> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<IEnumerator<IBattleVO>> clientDoActionCallBack;
        private Action clientBattleOverCallBack;

        internal SuperEventListener eventListener = new SuperEventListener();
        internal SuperEventListenerV eventListenerV = new SuperEventListenerV();

        public bool mWin = false;
        public bool oWin = false;

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

            mMoney = oMoney = DEFAULT_MONEY;

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

                    HeroSkill.Add(this, hero);

                    HeroAura.Add(this, hero);

                    heroMapDic.Add(pos, hero);
                }
            }

            ServerDoAutoAction();

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

                    bw.Write(autoAction.Count);

                    Dictionary<int, int>.Enumerator enumerator4 = autoAction.GetEnumerator();

                    while (enumerator4.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator4.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
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

                    enumerator4 = handCards.GetEnumerator();

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

            autoAction.Clear();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int target = _br.ReadInt32();

                autoAction.Add(pos, target);
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

            if (!hero.sds.GetCanControl())
            {
                if (_targetPos != autoAction[_pos])
                {
                    return false;
                }
                else
                {
                    if (_pos != _targetPos)
                    {
                        action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                    }

                    return true;
                }
            }

            bool b = GetPosIsMine(_targetPos);

            LinkedList<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if (b == hero.isMine)
                {
                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                }
                else
                {
                    if (heroMapDic.ContainsKey(_targetPos))
                    {
                        Hero targetHero = heroMapDic[_targetPos];

                        if (targetHero.sds.GetAbilityType() == AbilityType.Counter)
                        {
                            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                            return true;
                        }
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

                                if (targetHero.isMine != hero.isMine && targetHero.sds.GetAbilityType() == AbilityType.Counter)
                                {
                                    return false;
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
                if (b != hero.isMine && heroMapDic.ContainsKey(_targetPos))
                {
                    LinkedList<int> tmpList2;

                    if (hero.sds.GetAbilityType() == AbilityType.Shoot)
                    {
                        tmpList2 = BattlePublicTools.GetNeighbourPos2(mapData, _pos);
                    }
                    else if (hero.sds.GetAbilityType() == AbilityType.Throw || hero.sds.GetAbilityType() == AbilityType.Building)
                    {
                        tmpList2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);
                    }
                    else
                    {
                        return false;
                    }

                    LinkedList<int>.Enumerator enumerator = tmpList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

                        b = GetPosIsMine(pos);

                        if (b != hero.isMine && heroMapDic.ContainsKey(pos))
                        {
                            return false;
                        }
                    }

                    if (tmpList2.Contains(_targetPos))
                    {
                        action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                        return true;
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
                            if (autoAction.ContainsKey(pos) && targetPos != autoAction[pos])
                            {
                                continue;
                            }

                            action.Add(new KeyValuePair<int, int>(pos, targetPos));

                            tmpDic.Add(pos, false);
                        }
                    }
                }
            }

            Dictionary<int, int>.Enumerator enumerator = autoAction.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                if (GetPosIsMine(pair.Key) == _isMine)
                {
                    if (!tmpDic.ContainsKey(pair.Key))
                    {
                        if (pair.Key != pair.Value)
                        {
                            action.Add(pair);
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
            LinkedList<IBattleVO> voList = new LinkedList<IBattleVO>();

            BattleData battleData = GetBattleData();

            action.Clear();

            autoAction.Clear();

            ServerDoRoundStart(battleData, voList);

            ServerDoSummon(battleData, voList);

            summon.Clear();

            ServerDoShoot(battleData, voList);

            ServerDoRush(battleData, voList);

            ServerDoAttack(battleData, voList);

            ServerDoMove(battleData, voList);

            ServerDoRecover(battleData, voList);

            ServerDoAddCardsAndMoney(voList);

            if (!mWin && !oWin)
            {
                ServerDoAutoAction();
            }

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

                        mBw.Write(autoAction.Count);

                        oBw.Write(autoAction.Count);

                        Dictionary<int, int>.Enumerator enumerator = autoAction.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<int, int> pair = enumerator.Current;

                            mBw.Write(pair.Key);

                            mBw.Write(pair.Value);

                            oBw.Write(pair.Key);

                            oBw.Write(pair.Value);
                        }

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

                        mBw.Write(autoAction.Count);

                        Dictionary<int, int>.Enumerator enumerator = autoAction.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<int, int> pair = enumerator.Current;

                            mBw.Write(pair.Key);

                            mBw.Write(pair.Value);
                        }

                        mBw.Write(mWin);

                        mBw.Write(oWin);

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

            autoAction.Clear();

            mHandCards.Clear();

            oHandCards.Clear();

            serverBattleOverCallBack();
        }

        private void ServerDoSummon(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

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

                eventListener.DispatchEvent(HeroSkill.GetEventName(summonHero.uid, SkillTime.SUMMON), shieldChangeDic, hpChangeDic, damageDic, _voList);
            }

            if (summonDic != null)
            {
                Dictionary<int, Hero>.ValueCollection.Enumerator enumerator2 = summonDic.Values.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    ServerAddHero(_battleData, enumerator2.Current);
                }
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, false);
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

            HeroSkill.Add(this, hero);

            HeroAura.Add(this, hero);

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

                    bool needCheckOthers = true;

                    if (heroMapDic.ContainsKey(_targetPos))
                    {
                        Hero targetHero = heroMapDic[_targetPos];

                        if (targetHero.sds.GetAbilityType() == AbilityType.Counter)
                        {
                            needCheckOthers = false;
                        }
                    }

                    if (needCheckOthers)
                    {
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

                                if (tmpHero.isMine != hero.isMine && tmpHero.sds.GetAbilityType() == AbilityType.Counter)
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
                if (hero.isMine != targetPosIsMine && heroMapDic.ContainsKey(_targetPos))
                {
                    LinkedList<int> arr2;

                    if (hero.sds.GetAbilityType() == AbilityType.Shoot)
                    {
                        arr2 = BattlePublicTools.GetNeighbourPos2(mapData, _pos);
                    }
                    else if (hero.sds.GetAbilityType() == AbilityType.Throw || hero.sds.GetAbilityType() == AbilityType.Building)
                    {
                        arr2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);
                    }
                    else
                    {
                        throw new Exception("shoot error1");
                    }

                    LinkedList<int>.Enumerator enumerator = arr.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

                        targetPosIsMine = GetPosIsMine(pos);

                        if (targetPosIsMine != hero.isMine && heroMapDic.ContainsKey(pos))
                        {
                            throw new Exception("shoot error5");
                        }
                    }

                    if (arr2.Contains(_targetPos))
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
                        throw new Exception("shoot error4");
                    }
                }
                else
                {
                    throw new Exception("shoot error0");
                }
            }
        }

        private void ServerDoRoundStart(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                eventListener.DispatchEvent(HeroSkill.GetEventName(enumerator.Current.uid, SkillTime.ROUNDSTART), shieldChangeDic, hpChangeDic, damageDic, _voList);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, false);
        }

        private void ServerDoShoot(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                if (cellData.stander != null && cellData.shooters.Count > 0)
                {
                    for (int i = 0; i < cellData.shooters.Count; i++)
                    {
                        Hero shooter = cellData.shooters[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(shooter.uid, SkillTime.SHOOT), cellData.pos, new List<Hero>() { shooter }, new List<Hero>() { cellData.stander }, shieldChangeDic, hpChangeDic, damageDic, _voList);
                    }
                }
            }

            enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                ProcessCellDataShoot(_battleData, cellData, shieldChangeDic, hpChangeDic, damageDic, _voList);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, true);
        }

        private void ProcessCellDataShoot(BattleData _battleData, BattleCellData _cellData, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList)
        {
            if (_cellData.stander != null && _cellData.shooters.Count > 0)
            {
                Hero stander = _cellData.stander;

                List<int> shooters = new List<int>();

                int shootDamage = 0;

                int throwDamage = 0;

                for (int i = 0; i < _cellData.shooters.Count; i++)
                {
                    Hero shooter = _cellData.shooters[i];

                    shooter.SetAction(Hero.HeroAction.NULL);

                    shooters.Add(shooter.pos);

                    if (shooter.sds.GetAbilityType() == AbilityType.Throw)
                    {
                        throwDamage += shooter.GetShootDamage();
                    }
                    else
                    {
                        shootDamage += shooter.GetShootDamage();
                    }
                }

                //射击不穿甲
                if (shootDamage > 0)
                {
                    BattlePublicTools.AccumulateDicData(_damageDic, stander, -shootDamage);
                }

                if (throwDamage > 0)
                {
                    BattlePublicTools.AccumulateDicData(_shieldChangeDic, stander, -throwDamage);
                }

                int shield = stander.nowShield;

                if (_shieldChangeDic.ContainsKey(stander))
                {
                    shield += _shieldChangeDic[stander];

                    if (shield < 0)
                    {
                        shield = 0;
                    }
                }

                if (shield >= shootDamage)
                {
                    shield -= shootDamage;

                    shootDamage = 0;
                }
                else
                {
                    shootDamage -= shield;

                    shield = 0;
                }

                int shieldDamage = shield - stander.nowShield;

                int hp = stander.nowHp;

                if (_hpChangeDic.ContainsKey(stander))
                {
                    hp += _hpChangeDic[stander];

                    if (hp < 0)
                    {
                        hp = 0;
                    }
                }

                if (hp >= shootDamage)
                {
                    hp -= shootDamage;
                }
                else
                {
                    hp = 0;
                }

                int hpDamage = hp - stander.nowHp;
                //----

                //射击穿甲
                //if (damage > 0)
                //{
                //    BattlePublicTools.AccumulateDicData(_hpChangeDic, stander, -damage);
                //}

                //int shieldDamage;

                //if (_shieldChangeDic.ContainsKey(stander))
                //{
                //    shieldDamage = _shieldChangeDic[stander];
                //}
                //else
                //{
                //    shieldDamage = 0;
                //}

                //int hpDamage;

                //if (_hpChangeDic.ContainsKey(stander))
                //{
                //    hpDamage = _hpChangeDic[stander];
                //}
                //else
                //{
                //    hpDamage = 0;
                //}
                //----

                BattleShootVO vo = new BattleShootVO(shooters, _cellData.pos, shieldDamage, hpDamage);

                _voList.AddLast(vo);

                _cellData.shooters.Clear();
            }
        }

        private void ServerDoRush(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            while (true)
            {
                bool quit = true;

                Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

                Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

                Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

                Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (!cellData.attackHasBeenProcessed && cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        List<Hero> attackers = new List<Hero>(cellData.attackers);

                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            eventListener.DispatchEvent(HeroSkill.GetEventName(attacker.uid, SkillTime.ATTACK), cellData.pos, attackers, new List<Hero>() { cellData.stander }, shieldChangeDic, hpChangeDic, damageDic, _voList);

                            eventListener.DispatchEvent(HeroSkill.GetEventName(attacker.uid, SkillTime.RUSH), cellData.pos, attackers, new List<Hero>() { cellData.stander }, shieldChangeDic, hpChangeDic, damageDic, _voList);
                        }
                    }
                }

                enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    ProcessCellDataRush(_battleData, cellData, shieldChangeDic, hpChangeDic, damageDic, _voList, ref quit);
                }

                ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, true);

                if (quit)
                {
                    break;
                }
            }
        }

        private void ProcessCellDataRush(BattleData _battleData, BattleCellData _cellData, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList, ref bool _quit)
        {
            if (!_cellData.attackHasBeenProcessed && _cellData.stander != null && _cellData.attackers.Count > 0 && _cellData.stander.action != Hero.HeroAction.DEFENSE && _cellData.supporters.Count == 0)
            {
                _cellData.attackHasBeenProcessed = true;

                _quit = false;

                Hero stander = _cellData.stander;

                List<int> attackers = new List<int>();

                List<List<int>> helpers = new List<List<int>>();

                int damage = 0;

                for (int i = 0; i < _cellData.attackers.Count; i++)
                {
                    Hero attacker = _cellData.attackers[i];

                    attackers.Add(attacker.pos);

                    damage += attacker.GetAttackDamage();

                    List<int> tmpList = new List<int>();

                    helpers.Add(tmpList);

                    if (_battleData.actionDic.ContainsKey(attacker.pos))
                    {
                        BattleCellData tmpCellData = _battleData.actionDic[attacker.pos];

                        for (int m = 0; m < tmpCellData.supporters.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supporters[m];

                            damage += tmpHero.GetHelpDamage();

                            tmpList.Add(tmpHero.pos);
                        }
                    }
                }

                //突袭穿甲
                if (damage > 0)
                {
                    BattlePublicTools.AccumulateDicData(_hpChangeDic, stander, -damage);
                }

                int shieldDamage;

                if (_shieldChangeDic.ContainsKey(stander))
                {
                    shieldDamage = _shieldChangeDic[stander];
                }
                else
                {
                    shieldDamage = 0;
                }

                int hpDamage;

                if (_hpChangeDic.ContainsKey(stander))
                {
                    hpDamage = _hpChangeDic[stander];

                    if (stander.nowHp + hpDamage < 0)
                    {
                        hpDamage = -stander.nowHp;
                    }
                }
                else
                {
                    hpDamage = 0;
                }
                //----

                //突袭造成双倍伤害
                //bool doubleDamage = true;

                //eventListenerV.DispatchEvent(AuraEffect.HALF_RUSH_DAMAGE.ToString(), ref doubleDamage, stander);

                //if (doubleDamage)
                //{
                //    damage *= 2;
                //}

                //if (damage > 0)
                //{
                //    BattlePublicTools.AccumulateDicData(_damageDic, stander, -damage);
                //}

                //int shield = stander.nowShield;

                //if (_shieldChangeDic.ContainsKey(stander))
                //{
                //    shield += _shieldChangeDic[stander];

                //    if (shield < 0)
                //    {
                //        shield = 0;
                //    }
                //}

                //if (shield >= damage)
                //{
                //    shield -= damage;

                //    damage = 0;
                //}
                //else
                //{
                //    damage -= shield;

                //    shield = 0;
                //}

                //int shieldDamage = shield - stander.nowShield;

                //int hp = stander.nowHp;

                //if (_hpChangeDic.ContainsKey(stander))
                //{
                //    hp += _hpChangeDic[stander];

                //    if (hp < 0)
                //    {
                //        hp = 0;
                //    }
                //}

                //if (hp >= damage)
                //{
                //    hp -= damage;
                //}
                //else
                //{
                //    hp = 0;
                //}

                //int hpDamage = hp - stander.nowHp;

                BattleRushVO vo = new BattleRushVO(attackers, helpers, _cellData.pos, shieldDamage, hpDamage);

                _voList.AddLast(vo);
            }
        }

        private void ServerDoAttack(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                if (!cellData.attackHasBeenProcessed && cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                {
                    List<Hero> supporters = new List<Hero>(cellData.supporters);

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        supporters.Add(cellData.stander);
                    }

                    List<Hero> attackers = new List<Hero>(cellData.attackers);

                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.ATTACK), cellData.pos, attackers, supporters, shieldChangeDic, hpChangeDic, damageDic, _voList);
                    }

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        eventListener.DispatchEvent(HeroSkill.GetEventName(cellData.stander.uid, SkillTime.COUNTER), cellData.pos, supporters, attackers, shieldChangeDic, hpChangeDic, damageDic, _voList);
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.SUPPORT), cellData.pos, supporters, attackers, shieldChangeDic, hpChangeDic, damageDic, _voList);
                    }
                }
            }

            enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                ProcessCellDataAttack(_battleData, cellData, shieldChangeDic, hpChangeDic, damageDic, _voList);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, true);
        }

        private void ProcessCellDataAttack(BattleData _battleData, BattleCellData _cellData, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList)
        {
            if (!_cellData.attackHasBeenProcessed && _cellData.attackers.Count > 0 && (_cellData.stander != null || _cellData.supporters.Count > 0))
            {
                _cellData.attackHasBeenProcessed = true;

                List<int> attackers = new List<int>();

                List<List<int>> helpers = new List<List<int>>();

                List<int> supporters = new List<int>();

                List<int> attackersShieldDamage = new List<int>();

                List<int> attackersHpDamage = new List<int>();

                List<int> supportersShieldDamage = new List<int>();

                List<int> supportersHpDamage = new List<int>();

                int defenderShieldDamage = 0;

                int defenderHpDamage = 0;

                int defenseDamage;

                int attackDamage = 0;

                if (_cellData.stander != null && _cellData.stander.action == Hero.HeroAction.DEFENSE)
                {
                    defenseDamage = _cellData.stander.GetCounterDamage();
                }
                else
                {
                    defenseDamage = 0;
                }

                for (int i = 0; i < _cellData.supporters.Count; i++)
                {
                    Hero hero = _cellData.supporters[i];

                    supporters.Add(hero.pos);

                    defenseDamage += hero.GetSupportDamage();
                }

                for (int i = 0; i < _cellData.attackers.Count; i++)
                {
                    Hero hero = _cellData.attackers[i];

                    attackers.Add(hero.pos);

                    attackDamage += hero.GetAttackDamage();

                    List<int> tmpList = new List<int>();

                    helpers.Add(tmpList);

                    if (_battleData.actionDic.ContainsKey(hero.pos))
                    {
                        BattleCellData tmpCellData = _battleData.actionDic[hero.pos];

                        for (int m = 0; m < tmpCellData.supporters.Count; m++)
                        {
                            Hero tmpHero = tmpCellData.supporters[m];

                            tmpList.Add(tmpHero.pos);

                            attackDamage += tmpHero.GetHelpDamage();
                        }
                    }
                }

                for (int i = 0; i < _cellData.attackers.Count; i++)
                {
                    Hero hero = _cellData.attackers[i];

                    int tmpShield = hero.nowShield;

                    if (_shieldChangeDic.ContainsKey(hero))
                    {
                        tmpShield += _shieldChangeDic[hero];

                        if (tmpShield < 0)
                        {
                            tmpShield = 0;
                        }
                    }

                    if (defenseDamage > 0)
                    {
                        if (tmpShield >= defenseDamage)
                        {
                            tmpShield -= defenseDamage;

                            BattlePublicTools.AccumulateDicData(_shieldChangeDic, hero, -defenseDamage);

                            defenseDamage = 0;
                        }
                        else
                        {
                            BattlePublicTools.AccumulateDicData(_shieldChangeDic, hero, -tmpShield);

                            defenseDamage -= tmpShield;

                            tmpShield = 0;
                        }
                    }

                    int shieldDamage = tmpShield - hero.nowShield;

                    attackersShieldDamage.Add(shieldDamage);
                }

                for (int i = 0; i < _cellData.attackers.Count; i++)
                {
                    Hero hero = _cellData.attackers[i];

                    int tmpHp = hero.nowHp;

                    if (_hpChangeDic.ContainsKey(hero))
                    {
                        tmpHp += _hpChangeDic[hero];

                        if (tmpHp < 0)
                        {
                            tmpHp = 0;
                        }
                    }

                    if (defenseDamage > 0)
                    {
                        if (tmpHp >= defenseDamage)
                        {
                            tmpHp -= defenseDamage;

                            BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, -defenseDamage);

                            defenseDamage = 0;
                        }
                        else
                        {
                            BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, -tmpHp);

                            defenseDamage -= tmpHp;

                            tmpHp = 0;
                        }
                    }

                    int hpDamage = tmpHp - hero.nowHp;

                    attackersHpDamage.Add(hpDamage);
                }

                if (_cellData.stander != null && _cellData.stander.action == Hero.HeroAction.DEFENSE)
                {
                    Hero hero = _cellData.stander;

                    int tmpShield = hero.nowShield;

                    if (_shieldChangeDic.ContainsKey(hero))
                    {
                        tmpShield += _shieldChangeDic[hero];

                        if (tmpShield < 0)
                        {
                            tmpShield = 0;
                        }
                    }

                    if (tmpShield >= attackDamage)
                    {
                        tmpShield -= attackDamage;

                        BattlePublicTools.AccumulateDicData(_shieldChangeDic, hero, -attackDamage);

                        attackDamage = 0;
                    }
                    else
                    {
                        BattlePublicTools.AccumulateDicData(_shieldChangeDic, hero, -tmpShield);

                        attackDamage -= tmpShield;

                        tmpShield = 0;
                    }

                    defenderShieldDamage = tmpShield - hero.nowShield;
                }

                for (int i = 0; i < _cellData.supporters.Count; i++)
                {
                    Hero hero = _cellData.supporters[i];

                    int tmpShield = hero.nowShield;

                    if (_shieldChangeDic.ContainsKey(hero))
                    {
                        tmpShield += _shieldChangeDic[hero];

                        if (tmpShield < 0)
                        {
                            tmpShield = 0;
                        }
                    }

                    if (attackDamage > 0)
                    {
                        if (tmpShield >= attackDamage)
                        {
                            tmpShield -= attackDamage;

                            BattlePublicTools.AccumulateDicData(_shieldChangeDic, hero, -attackDamage);

                            attackDamage = 0;
                        }
                        else
                        {
                            BattlePublicTools.AccumulateDicData(_shieldChangeDic, hero, -tmpShield);

                            attackDamage -= tmpShield;

                            tmpShield = 0;
                        }
                    }

                    int shieldDamage = tmpShield - hero.nowShield;

                    supportersShieldDamage.Add(shieldDamage);
                }

                if (_cellData.stander != null && _cellData.stander.action == Hero.HeroAction.DEFENSE)
                {
                    Hero hero = _cellData.stander;

                    int tmpHp = hero.nowHp;

                    if (_hpChangeDic.ContainsKey(hero))
                    {
                        tmpHp += _hpChangeDic[hero];

                        if (tmpHp < 0)
                        {
                            tmpHp = 0;
                        }
                    }

                    if (tmpHp >= attackDamage)
                    {
                        tmpHp -= attackDamage;

                        BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, -attackDamage);

                        attackDamage = 0;
                    }
                    else
                    {
                        BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, -tmpHp);

                        attackDamage -= tmpHp;

                        tmpHp = 0;
                    }

                    defenderHpDamage = tmpHp - hero.nowHp;
                }

                for (int i = 0; i < _cellData.supporters.Count; i++)
                {
                    Hero hero = _cellData.supporters[i];

                    int tmpHp = hero.nowHp;

                    if (_hpChangeDic.ContainsKey(hero))
                    {
                        tmpHp += _hpChangeDic[hero];

                        if (tmpHp < 0)
                        {
                            tmpHp = 0;
                        }
                    }

                    if (attackDamage > 0)
                    {
                        if (tmpHp >= attackDamage)
                        {
                            tmpHp -= attackDamage;

                            BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, -attackDamage);

                            attackDamage = 0;
                        }
                        else
                        {
                            BattlePublicTools.AccumulateDicData(_hpChangeDic, hero, -tmpHp);

                            attackDamage -= tmpHp;

                            tmpHp = 0;
                        }
                    }

                    int hpDamage = tmpHp - hero.nowHp;

                    supportersHpDamage.Add(hpDamage);
                }

                if (attackDamage > 0 && _cellData.stander != null && _cellData.stander.action != Hero.HeroAction.DEFENSE)
                {
                    Hero hero = _cellData.stander;

                    BattlePublicTools.AccumulateDicData(_damageDic, hero, -attackDamage);

                    if (hero.action == Hero.HeroAction.ATTACK || hero.action == Hero.HeroAction.SUPPORT)
                    {
                        ProcessCellDataAttack(_battleData, _battleData.actionDic[hero.actionTarget], _shieldChangeDic, _hpChangeDic, _damageDic, _voList);
                    }

                    int recShield = hero.nowShield;

                    if (_shieldChangeDic.ContainsKey(hero))
                    {
                        recShield += _shieldChangeDic[hero];

                        if (recShield < 0)
                        {
                            recShield = 0;
                        }
                    }

                    if (recShield >= attackDamage)
                    {
                        defenderShieldDamage = -attackDamage;

                        attackDamage = 0;
                    }
                    else
                    {
                        defenderShieldDamage = -recShield;

                        attackDamage -= recShield;
                    }

                    int recHp = hero.nowHp;

                    if (_hpChangeDic.ContainsKey(hero))
                    {
                        recHp += _hpChangeDic[hero];

                        if (recHp < 0)
                        {
                            recHp = 0;
                        }
                    }

                    if (recHp >= attackDamage)
                    {
                        defenderHpDamage = -attackDamage;
                    }
                    else
                    {
                        defenderHpDamage = -recHp;
                    }
                }

                BattleAttackVO vo = new BattleAttackVO(attackers, helpers, supporters, _cellData.pos, attackersShieldDamage, attackersHpDamage, supportersShieldDamage, supportersHpDamage, defenderShieldDamage, defenderHpDamage);

                _voList.AddLast(vo);
            }
        }

        private void ServerDoMove(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            LinkedList<int> tmpList = null;

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                BattleCellData cellData = pair.Value;

                if (cellData.stander == null && (cellData.supporters.Count > 0 || cellData.attackers.Count > 0))
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

                LinkedList<Hero> captureList = null;

                LinkedList<int>.Enumerator enumerator3 = tmpList.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    OneCellEmpty(_battleData, enumerator3.Current, ref tmpMoveDic, ref captureList);
                }

                if (tmpMoveDic != null)
                {
                    _voList.AddLast(new BattleMoveVO(tmpMoveDic));
                }

                if (captureList != null)
                {
                    Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

                    Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

                    Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

                    LinkedList<Hero>.Enumerator enumerator2 = captureList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        Hero hero = enumerator2.Current;

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.CAPTURE), shieldChangeDic, hpChangeDic, damageDic, _voList);
                    }

                    ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, false);
                }
            }
        }

        internal void HeroLevelUp(Hero _hero, int _id, LinkedList<IBattleVO> _voList)
        {
            eventListener.DispatchEvent(HeroSkill.GetEventName(_hero.uid, REMOVE_EVENT_NAME));

            _hero.LevelUp(GetHeroData(_id));

            HeroSkill.Add(this, _hero);

            HeroAura.Add(this, _hero);

            _voList.AddLast(new BattleLevelUpVO(_hero.pos, _id));
        }

        private void ServerDoRecover(BattleData _battleData, LinkedList<IBattleVO> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                eventListener.DispatchEvent(HeroSkill.GetEventName(enumerator.Current.uid, SkillTime.RECOVER), shieldChangeDic, hpChangeDic, damageDic, _voList);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList, false);

            enumerator = heroMapDic.Values.GetEnumerator();

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

        private void ServerDoHeroDie(Hero _hero, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList)
        {
            string eventName = HeroSkill.GetEventName(_hero.uid, SkillTime.DIE);

            eventListener.DispatchEvent(eventName, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);
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

            _hero.SetAction(Hero.HeroAction.NULL);
        }

        private void OneCellEmpty(BattleData _battleData, int _pos, ref Dictionary<int, int> _tmpMoveDic, ref LinkedList<Hero> _captureList)
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

                    if (tmpHero.sds.GetAbilityType() != AbilityType.Building && tmpHero.canMove)
                    {
                        hero = tmpHero;

                        break;
                    }
                }

                if (hero == null)
                {
                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero tmpHero = cellData.attackers[i];

                        if (tmpHero.sds.GetAbilityType() != AbilityType.Building && tmpHero.canMove)
                        {
                            hero = tmpHero;

                            if (_captureList == null)
                            {
                                _captureList = new LinkedList<Hero>();
                            }

                            _captureList.AddLast(hero);

                            if (mapData.mBase == nowPos)
                            {
                                oWin = true;
                            }
                            else if (mapData.oBase == nowPos)
                            {
                                mWin = true;
                            }

                            if (mapBelongDic.ContainsKey(nowPos))
                            {
                                mapBelongDic.Remove(nowPos);
                            }
                            else
                            {
                                mapBelongDic.Add(nowPos, true);
                            }

                            break;
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

        private void ServerDoAutoAction()
        {
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if (!hero.sds.GetCanControl())
                {
                    int targetPos = GetHeroAutoAction(hero);

                    autoAction.Add(hero.pos, targetPos);
                }
            }
        }

        private int GetHeroAutoAction(Hero _hero)
        {
            List<int> posList;

            switch (_hero.sds.GetAbilityType())
            {
                case AbilityType.Shoot:

                    posList = GetCanShootPos(_hero);

                    if (posList.Count > 0)
                    {
                        int index = random.Next(posList.Count);

                        return posList[index];
                    }

                    break;

                case AbilityType.Throw:
                case AbilityType.Building:

                    posList = GetCanThrowPos(_hero);

                    if (posList.Count > 0)
                    {
                        int index = random.Next(posList.Count);

                        return posList[index];
                    }

                    break;

                case AbilityType.Counter:

                    if (CheckPosCanBeAttack(_hero.pos))
                    {
                        return _hero.pos;
                    }

                    break;

                case AbilityType.Support:

                    if (!CheckPosCanBeAttack(_hero.pos))
                    {
                        posList = GetCanSupportHeroPos(_hero);

                        if (posList.Count > 0)
                        {
                            int index = random.Next(posList.Count);

                            return posList[index];
                        }
                    }

                    break;

                case AbilityType.Help:

                    if (!CheckPosCanBeAttack(_hero.pos))
                    {
                        posList = GetCanHelpHeroPos(_hero);

                        if (posList.Count > 0)
                        {
                            int index = random.Next(posList.Count);

                            return posList[index];
                        }
                    }

                    break;
            }

            //攻击英雄
            posList = GetCanAttackerHeroPos(_hero);

            if (posList.Count > 0)
            {
                int index = random.Next(posList.Count);

                return posList[index];
            }

            //援护英雄
            posList = GetCanSupportHeroPos(_hero);

            if (posList.Count > 0)
            {
                int index = random.Next(posList.Count);

                return posList[index];
            }

            //无援护目标时向前进
            if (_hero.sds.GetAbilityType() != AbilityType.Building)
            {
                int targetPos;

                if (_hero.isMine)
                {
                    targetPos = mapData.moveMap[_hero.pos].Key;
                }
                else
                {
                    targetPos = mapData.moveMap[_hero.pos].Value;
                }

                return targetPos;
            }

            return _hero.pos;
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
                int uid = PublicTools.ChooseOneKeyFromDic(handCardsDic, random);

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

        private void ProcessChangeDic(BattleData _battleData, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _damageDic, LinkedList<IBattleVO> _voList, bool _isBattle)
        {
            while (_shieldChangeDic.Count > 0 || _hpChangeDic.Count > 0 || _damageDic.Count > 0)
            {
                LinkedList<int> diePos = null;

                LinkedList<Hero> dieHero = null;

                Dictionary<Hero, KeyValuePair<int, int>> recordDic = null;

                if (!_isBattle)
                {
                    recordDic = new Dictionary<Hero, KeyValuePair<int, int>>();
                }

                Dictionary<Hero, int>.Enumerator enumerator3 = _shieldChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    if (!_isBattle)
                    {
                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }

                    hero.ShieldChange(pair.Value);
                }

                _shieldChangeDic.Clear();

                enumerator3 = _hpChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    if (!_isBattle && !recordDic.ContainsKey(hero))
                    {
                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }

                    bool die = hero.HpChange(pair.Value);

                    if (die)
                    {
                        if (diePos == null)
                        {
                            diePos = new LinkedList<int>();

                            dieHero = new LinkedList<Hero>();
                        }

                        diePos.AddLast(hero.pos);

                        dieHero.AddLast(hero);

                        ServerRemoveHero(_battleData, hero);
                    }
                }

                _hpChangeDic.Clear();

                enumerator3 = _damageDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    if (!_isBattle && !recordDic.ContainsKey(hero))
                    {
                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }

                    if (hero.nowShield >= -pair.Value)
                    {
                        hero.ShieldChange(pair.Value);
                    }
                    else
                    {
                        int damage = hero.nowShield + pair.Value;

                        hero.ShieldChange(-hero.nowShield);

                        if (hero.nowHp > 0)
                        {
                            bool die = hero.HpChange(damage);

                            if (die)
                            {
                                if (diePos == null)
                                {
                                    diePos = new LinkedList<int>();

                                    dieHero = new LinkedList<Hero>();
                                }

                                diePos.AddLast(hero.pos);

                                dieHero.AddLast(hero);

                                ServerRemoveHero(_battleData, hero);
                            }
                        }
                    }
                }

                _damageDic.Clear();

                if (!_isBattle)
                {
                    List<int> pos = new List<int>();

                    List<int> shieldChange = new List<int>();

                    List<int> hpChange = new List<int>();

                    Dictionary<Hero, KeyValuePair<int, int>>.Enumerator enumerator = recordDic.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<Hero, KeyValuePair<int, int>> pair = enumerator.Current;

                        Hero hero = pair.Key;

                        KeyValuePair<int, int> data = pair.Value;

                        pos.Add(hero.pos);

                        shieldChange.Add(hero.nowShield - data.Key);

                        hpChange.Add(hero.nowHp - data.Value);
                    }

                    _voList.AddLast(new BattleChangeVO(pos, shieldChange, hpChange));
                }
                else
                {
                    _isBattle = false;
                }

                if (diePos != null)
                {
                    _voList.AddLast(new BattleDeathVO(diePos));

                    LinkedList<Hero>.Enumerator enumerator = dieHero.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        ServerDoHeroDie(enumerator.Current, _shieldChangeDic, _hpChangeDic, _damageDic, _voList);
                    }
                }
            }
        }



















        private void ClientDoAction(BinaryReader _br)
        {
            summon.Clear();

            action.Clear();

            autoAction.Clear();

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
            }

            for (int i = 0; i < _vo.supporters.Count; i++)
            {
                Hero hero = heroMapDic[_vo.supporters[i]];

                hero.ShieldChange(_vo.supportersShieldDamage[i]);

                hero.HpChange(_vo.supportersHpDamage[i]);
            }

            if (_vo.defenderShieldDamage < 0)
            {
                Hero hero = heroMapDic[_vo.defender];

                hero.ShieldChange(_vo.defenderShieldDamage);
            }

            if (_vo.defenderHpDamage < 0)
            {
                Hero hero = heroMapDic[_vo.defender];

                hero.HpChange(_vo.defenderHpDamage);
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
            autoAction.Clear();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                autoAction.Add(pos, targetPos);
            }

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

        public List<int> GetCanAttackerHeroPos(Hero _hero)
        {
            List<int> result = new List<int>();

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            bool getThreat = false;

            LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int pos = enumerator.Current;

                bool b = GetPosIsMine(pos);

                if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.sds.GetAbilityType() == AbilityType.Counter)
                    {
                        if (!getThreat)
                        {
                            getThreat = true;

                            if (result.Count > 0)
                            {
                                result.Clear();
                            }
                        }

                        result.Add(pos);
                    }
                    else
                    {
                        if (!getThreat)
                        {
                            result.Add(pos);
                        }
                    }
                }
            }

            return result;
        }


        public List<int> GetCanAttackPos(Hero _hero)
        {
            List<int> result = new List<int>();

            LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

            bool getThreat = false;

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

                        if (hero.sds.GetAbilityType() == AbilityType.Counter)
                        {
                            if (!getThreat)
                            {
                                getThreat = true;

                                if (result.Count > 0)
                                {
                                    result.Clear();
                                }
                            }

                            result.Add(pos);

                            continue;
                        }
                    }

                    if (!getThreat)
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }

        public List<int> GetCanShootPos(Hero _hero)
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

        public List<int> GetCanThrowPos(Hero _hero)
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

        public bool CheckPosCanBeAttack(int _pos)
        {
            bool isThreat = false;

            if (heroMapDic.ContainsKey(_pos))
            {
                Hero hero = heroMapDic[_pos];

                if (hero.sds.GetAbilityType() == AbilityType.Counter)
                {
                    isThreat = true;
                }
            }

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

                        if (isThreat)
                        {
                            return true;
                        }

                        bool canAttack = true;

                        LinkedList<int> tmpPosList = BattlePublicTools.GetNeighbourPos(mapData, pos);

                        LinkedList<int>.Enumerator enumerator2 = tmpPosList.GetEnumerator();

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

                                if (tmpHero.sds.GetAbilityType() == AbilityType.Counter)
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
                    if (CheckPosCanBeAttack(pos))
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

                    if (hero.sds.GetAbilityType() == AbilityType.Counter)
                    {
                        continue;
                    }

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

        //public List<int> GetMustSupportHeroPos(Hero _hero)
        //{
        //    List<int> result = new List<int>();

        //    LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

        //    LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

        //    while (enumerator.MoveNext())
        //    {
        //        int pos = enumerator.Current;

        //        bool b = GetPosIsMine(pos);

        //        if (b == _hero.isMine && heroMapDic.ContainsKey(pos))
        //        {
        //            if (CheckHeroMustBeAttack(heroMapDic[pos]))
        //            {
        //                result.Add(pos);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public bool CheckHeroMustBeAttack(Hero _hero)
        //{
        //    LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(mapData, _hero.pos);

        //    LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

        //    while (enumerator.MoveNext())
        //    {
        //        int pos = enumerator.Current;

        //        bool b = GetPosIsMine(pos);

        //        if (b != _hero.isMine && heroMapDic.ContainsKey(pos))
        //        {
        //            Hero hero = heroMapDic[pos];

        //            if (!hero.sds.GetCanControl())
        //            {
        //                bool mushBeAttack = true;

        //                LinkedList<int> posList2 = BattlePublicTools.GetNeighbourPos(mapData, pos);

        //                LinkedList<int>.Enumerator enumerator2 = posList2.GetEnumerator();

        //                while (enumerator2.MoveNext())
        //                {
        //                    int pos2 = enumerator2.Current;

        //                    if (pos2 == _hero.pos)
        //                    {
        //                        continue;
        //                    }

        //                    b = GetPosIsMine(pos2);

        //                    if (b == _hero.isMine && heroMapDic.ContainsKey(pos2))
        //                    {
        //                        hero = heroMapDic[pos2];

        //                        if (hero.sds.GetAbilityType() == AbilityType.Counter || _hero.sds.GetAbilityType() != AbilityType.Counter)
        //                        {
        //                            mushBeAttack = false;

        //                            break;
        //                        }
        //                    }
        //                }

        //                if (mushBeAttack)
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}

        public bool GetClientCanAction()
        {
            return !(clientIsMine ? mOver : oOver);
        }
    }
}

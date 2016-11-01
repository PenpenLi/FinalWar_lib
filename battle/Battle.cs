using System;
using System.Collections.Generic;
using System.IO;
using superEvent;

namespace FinalWar
{
    public class Battle
    {
        internal static readonly Random random = new Random();

        internal static Func<int, MapData> GetMapData;
        internal static Func<int, IHeroSDS> GetHeroData;
        internal static Func<int, ISkillSDS> GetSkillData;
        internal static Func<int, IAuraSDS> GetAuraData;

        private const int DEFAULT_HAND_CARD_NUM = 5;
        private const int MAX_HAND_CARD_NUM = 10;
        private const int DEFAULT_MONEY = 5;
        private const int ADD_MONEY = 3;
        private const int MAX_MONEY = 10;

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
        private Action<IEnumerator<ValueType>> clientDoActionCallBack;
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

        public void ClientSetCallBack(Action<MemoryStream> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<IEnumerator<ValueType>> _clientDoActionCallBack, Action _clientBattleOverCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
            clientBattleOverCallBack = _clientBattleOverCallBack;
        }

        public void ServerStart(int _mapID, List<int> _mCards, List<int> _oCards, bool _isVsAi)
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
                if(mCards.Count > 0)
                {
                    int index = (int)(random.NextDouble() * mCards.Count);

                    mHandCards.Add(GetCardUid(), mCards[index]);

                    mCards.RemoveAt(index);
                }

                if(oCards.Count > 0)
                {
                    int index = (int)(random.NextDouble() * oCards.Count);

                    oHandCards.Add(GetCardUid(), oCards[index]);

                    oCards.RemoveAt(index);
                }
            }

            ServerRefreshData(true);

            if (!isVsAi)
            {
                ServerRefreshData(false);
            }
        }

        public void ServerGetPackage(byte[] _bytes,bool _isMine)
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
                    }

                    bw.Write(autoAction.Count);

                    Dictionary<int, int>.Enumerator enumerator4 = autoAction.GetEnumerator();

                    while (enumerator4.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator4.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    Dictionary<int, int> handCards = _isMine ? mHandCards : oHandCards;

                    bw.Write(handCards.Count);

                    enumerator4 = handCards.GetEnumerator();

                    while (enumerator4.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator4.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bool isOver;

                    if (_isMine)
                    {
                        bw.Write(mMoney);

                        isOver = mOver;
                    }
                    else
                    {
                        bw.Write(oMoney);

                        isOver = oOver;
                    }

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

                        for(int i = 0; i < num; i++)
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

                        for(int i = 0; i < num; i++)
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

            for(int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                mapBelongDic.Add(pos, true);
            }

            heroMapDic.Clear();

            num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int id = _br.ReadInt32();

                bool heroIsMine = _br.ReadBoolean();

                int pos = _br.ReadInt32();

                int nowHp = _br.ReadInt32();

                ClientAddHero(heroIsMine, GetHeroData(id), pos, nowHp);
            }

            autoAction.Clear();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int target = _br.ReadInt32();

                autoAction.Add(pos, target);
            }

            Dictionary<int, int> handCards;

            if (clientIsMine)
            {
                mHandCards = new Dictionary<int, int>();

                handCards = mHandCards;
            }
            else
            {
                oHandCards = new Dictionary<int, int>();

                handCards = oHandCards;
            }

            num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards.Add(uid, id);
            }

            bool isOver;

            if (clientIsMine)
            {
                mMoney = _br.ReadInt32();

                isOver = mOver = _br.ReadBoolean();
            }
            else
            {
                oMoney = _br.ReadInt32();

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

        public void ClientRequestSummon(int _cardUid, int _pos)
        {
            summon.Add(_cardUid, _pos);
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            summon.Remove(_cardUid);
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

            bool b = GetPosIsMine(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if(b == hero.isMine)
                {
                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                    return true;
                }
                else
                {
                    if (heroMapDic.ContainsKey(_targetPos))
                    {
                        Hero targetHero = heroMapDic[_targetPos];

                        if (targetHero.sds.GetThreat())
                        {
                            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                            return true;
                        }
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        if (pos != _targetPos)
                        {
                            if (heroMapDic.ContainsKey(pos))
                            {
                                Hero targetHero = heroMapDic[pos];

                                if (targetHero.isMine != hero.isMine && targetHero.sds.GetThreat())
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                    return true;
                }
            }
            else
            {
                if(b != hero.isMine && heroMapDic.ContainsKey(_targetPos))
                {
                    List<int> tmpList2 = BattlePublicTools.GetNeighbourPos2(mapData.neighbourPosMap, _pos);

                    if (tmpList2.Contains(_targetPos))
                    {
                        for (int i = 0; i < tmpList.Count; i++)
                        {
                            int pos = tmpList[i];

                            b = GetPosIsMine(pos);

                            if (b != hero.isMine && heroMapDic.ContainsKey(pos))
                            {
                                return false;
                            }
                        }

                        action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                        return true;
                    }
                }
            }

            return false;
        }

        public void ClientRequestUnaction(int _pos)
        {
            for(int i = 0; i < action.Count; i++)
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

                    for(int i = 0; i < action.Count; i++)
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

            for(int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                if (cards.ContainsKey(uid) && GetPosIsMine(pos) == _isMine)
                {
                    summon.Add(uid, pos);
                }
            }

            Dictionary<int, bool> tmpDic = new Dictionary<int, bool>();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                if (heroMapDic.ContainsKey(pos) && heroMapDic[pos].isMine == _isMine)
                {
                    if (autoAction.ContainsKey(pos))
                    {
                        if(targetPos != autoAction[pos])
                        {
                            continue;
                        }
                    }

                    action.Add(new KeyValuePair<int, int>(pos, targetPos));

                    tmpDic.Add(pos, false);
                }
            }

            Dictionary<int, int>.Enumerator enumerator = autoAction.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                if(GetPosIsMine(pair.Key) == _isMine)
                {
                    if (!tmpDic.ContainsKey(pair.Key))
                    {
                        if(pair.Key != pair.Value)
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

        private Hero AddHero(bool _isMine, IHeroSDS _sds, int _pos, int _uid)
        {
            return new Hero(this, _isMine, _sds, _pos, _uid);
        }

        private Hero ClientAddHero(bool _isMine, IHeroSDS _sds, int _pos)
        {
            Hero hero = new Hero(_isMine, _sds, _pos);

            heroMapDic.Add(_pos, hero);

            return hero;
        }

        private Hero ClientAddHero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp)
        {
            Hero hero = new Hero(_isMine, _sds, _pos, _nowHp);

            heroMapDic.Add(_pos, hero);

            return hero;
        }

        private void ServerStartBattle()
        {
            List<ValueType> voList = new List<ValueType>();
            
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

            if(!mWin && !oWin)
            {
                ServerDoAutoAction();
            }

            //eventListener.LogNum();

            //eventListenerV.LogNum();

            byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.S2C_DOACTION);

                    BattleVOTools.WriteDataToStream(voList, bw);

                    bw.Write(autoAction.Count);

                    Dictionary<int, int>.Enumerator enumerator = autoAction.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(mWin);

                    bw.Write(oWin);

                    bytes = ms.ToArray();
                }
            }

            if (!isVsAi)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        mBw.Write(bytes);

                        oBw.Write(bytes);

                        RecoverCards(mBw, oBw);

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
                        mBw.Write(bytes);

                        RecoverCards(mBw, null);

                        serverSendDataCallBack(true, mMs);
                    }
                }
            }

            if(!mWin && !oWin)
            {
                RecoverMoney();

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

        private void ServerDoSummon(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            List<Hero> summonList = new List<Hero>();

            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int tmpCardUid = pair.Key;

                int pos = pair.Value;

                bool isMine = GetPosIsMine(pos);

                Hero summonHero = SummonOneUnit(tmpCardUid, pos, isMine, _battleData);

                summonList.Add(summonHero);

                _voList.Add(new BattleSummonVO(tmpCardUid, summonHero.sds.GetID(), pos));

                eventListener.DispatchEvent(HeroSkill.GetEventName(summonHero.uid, SkillTime.SUMMON), shieldChangeDic, hpChangeDic, damageDic);
            }

            for(int i = 0; i < summonList.Count; i++)
            {
                ServerAddHero(_battleData, summonList[i]);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList);
        }

        private Hero SummonOneUnit(int _uid, int _pos, bool _isMine, BattleData _battleData)
        {
            int heroID;

            if (_isMine)
            {
                heroID = mHandCards[_uid];
            }
            else
            {
                heroID = oHandCards[_uid];
            }

            IHeroSDS sds = GetHeroData(heroID);

            if (_isMine)
            {
                mMoney -= sds.GetCost();

                mHandCards.Remove(_uid);
            }
            else
            {
                oMoney -= sds.GetCost();

                oHandCards.Remove(_uid);
            }

            Hero hero = AddHero(_isMine, sds, _pos, GetHeroUid());

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

            List<int> arr = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

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
                        cellData = new BattleCellData();

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

                        if (targetHero.sds.GetThreat())
                        {
                            needCheckOthers = false;
                        }
                    }

                    if (needCheckOthers)
                    {
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

                                if (tmpHero.isMine != hero.isMine && tmpHero.sds.GetThreat())
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
                        cellData = new BattleCellData();

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
                List<int> arr2 = BattlePublicTools.GetNeighbourPos2(mapData.neighbourPosMap, _pos);

                if (arr2.Contains(_targetPos))
                {
                    if (hero.isMine == targetPosIsMine)
                    {
                        throw new Exception("shoot error0");
                    }
                    else
                    {
                        if (heroMapDic.ContainsKey(_targetPos))
                        {
                            for(int i = 0; i < arr.Count; i++)
                            {
                                int pos = arr[i];

                                targetPosIsMine = GetPosIsMine(pos);

                                if(targetPosIsMine != hero.isMine && heroMapDic.ContainsKey(pos))
                                {
                                    throw new Exception("shoot error2");
                                }
                            }

                            hero.SetAction(Hero.HeroAction.SHOOT, _targetPos);

                            BattleCellData cellData;

                            if (_battleData.actionDic.ContainsKey(_targetPos))
                            {
                                cellData = _battleData.actionDic[_targetPos];
                            }
                            else
                            {
                                cellData = new BattleCellData();

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
                            throw new Exception("shoot error1");
                        }
                    }
                }
                else
                {
                    throw new Exception("shoot error2");
                }
            }
        }

        private void ServerDoRoundStart(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                eventListener.DispatchEvent(HeroSkill.GetEventName(enumerator.Current.uid, SkillTime.ROUNDSTART), shieldChangeDic, hpChangeDic, damageDic);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList);
        }

        private void ServerDoShoot(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                BattleCellData cellData = pair.Value;

                if (cellData.stander != null && cellData.shooters.Count > 0)
                {
                    for (int i = 0; i < cellData.shooters.Count; i++)
                    {
                        Hero shooter = cellData.shooters[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(shooter.uid, SkillTime.SHOOT), pair.Key, new List<Hero>() { shooter }, new List<Hero>() { cellData.stander }, shieldChangeDic, hpChangeDic, damageDic);
                    }
                }
            }

            enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current.Value;

                if (cellData.stander != null && cellData.shooters.Count > 0)
                {
                    List<int> shooters = new List<int>();

                    int stander = cellData.stander.pos;

                    int damage = 0;

                    for (int i = 0; i < cellData.shooters.Count; i++)
                    {
                        Hero shooter = cellData.shooters[i];

                        shooter.SetAction(Hero.HeroAction.NULL);

                        shooters.Add(shooter.pos);

                        damage += shooter.GetShootDamage();
                    }

                    if(damage > 0)
                    {
                        BattlePublicTools.AccumulateDicData(damageDic, cellData.stander, -damage);
                    }

                    //if (hpChangeDic.ContainsKey(cellData.stander))
                    //{
                    //    damage = hpChangeDic[cellData.stander];

                    //    if (damage < 0)
                    //    {
                    //        _voList.Add(new BattleShootVO(shooters, stander, damage));
                    //    }
                    //    else
                    //    {
                    //        _voList.Add(new BattleShootVO(shooters, stander, 0));
                    //    }
                    //}
                    //else
                    //{
                    //    _voList.Add(new BattleShootVO(shooters, stander, 0));
                    //}

                    cellData.shooters.Clear();
                }
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList);
        }

        private void ServerDoRush(BattleData _battleData, List<ValueType> _voList)
        {
            while (true)
            {
                bool quit = true;

                Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

                Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

                Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

                Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                    BattleCellData cellData = pair.Value;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        List<Hero> attackers = new List<Hero>(cellData.attackers);

                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            eventListener.DispatchEvent(HeroSkill.GetEventName(attacker.uid, SkillTime.ATTACK), pair.Key, attackers, new List<Hero>() { cellData.stander }, shieldChangeDic, hpChangeDic, damageDic);

                            eventListener.DispatchEvent(HeroSkill.GetEventName(attacker.uid, SkillTime.RUSH), pair.Key, attackers, new List<Hero>() { cellData.stander }, shieldChangeDic, hpChangeDic, damageDic);
                        }
                    }
                }

                enumerator = _battleData.actionDic.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current.Value;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        quit = false;

                        List<int> attackers = new List<int>();

                        int stander = cellData.stander.pos;

                        int damage = 0;

                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            attacker.SetAction(Hero.HeroAction.ATTACKOVER);

                            attackers.Add(attacker.pos);

                            damage += attacker.GetAttackDamage() * 2;
                        }

                        List<Hero> tmpList = cellData.attackers;

                        cellData.attackers = cellData.attackOvers;

                        cellData.attackOvers = tmpList;

                        if(damage > 0)
                        {
                            BattlePublicTools.AccumulateDicData(damageDic, cellData.stander, -damage);
                        }

                        //if (hpChangeDic.ContainsKey(cellData.stander))
                        //{
                        //    damage = hpChangeDic[cellData.stander];

                        //    if (damage < 0)
                        //    {
                        //        _voList.Add(new BattleRushVO(attackers, stander, damage));
                        //    }
                        //    else
                        //    {
                        //        _voList.Add(new BattleRushVO(attackers, stander, 0));
                        //    }
                        //}
                        //else
                        //{
                        //    _voList.Add(new BattleRushVO(attackers, stander, 0));
                        //}
                    }
                }

                ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList);

                if (quit)
                {
                    break;
                }
            }
        }

        private void ServerDoAttack(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic2 = new Dictionary<Hero, int>();

            Dictionary<Hero, int> powerChangeDic = new Dictionary<Hero, int>();

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                BattleCellData cellData = pair.Value;

                if (cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                {
                    List<Hero> supporters = new List<Hero>(cellData.supporters);

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        supporters.Add(cellData.stander);
                    }

                    List<Hero> attackers = new List<Hero>(cellData.attackers);

                    for(int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.ATTACK), pair.Key, attackers, supporters, hpChangeDic, powerChangeDic);
                    }

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        eventListener.DispatchEvent(HeroSkill.GetEventName(cellData.stander.uid, SkillTime.COUNTER), pair.Key, supporters, attackers, hpChangeDic, powerChangeDic);
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.COUNTER), pair.Key, supporters, attackers, hpChangeDic, powerChangeDic);
                    }
                }
            }

            enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                BattleCellData cellData = pair.Value;

                if (cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                {
                    List<int> attackers = new List<int>();

                    List<int> supporters = new List<int>();

                    int defenderDamage = 0;

                    List<int> attackersDamage = new List<int>();

                    List<int> supportersDamage = new List<int>();

                    int defenseDamage;

                    int attackDamage = 0;

                    int attackerNum = cellData.attackers.Count;

                    int defenderNum;

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        defenseDamage = cellData.stander.GetCounterDamage();

                        defenderNum = cellData.supporters.Count + 1;
                    }
                    else
                    {
                        defenseDamage = 0;

                        defenderNum = cellData.supporters.Count;
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        supporters.Add(hero.pos);

                        defenseDamage += hero.GetCounterDamage();
                    }

                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        attackers.Add(hero.pos);

                        attackDamage += hero.GetAttackDamage();
                    }

                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        int tmpDamage;

                        if (defenseDamage > 0)
                        {
                            tmpDamage = hero.BeDamage(ref defenseDamage, hpChangeDic);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, hero, -tmpDamage);
                        }

                        if (hpChangeDic.ContainsKey(hero))
                        {
                            tmpDamage = hpChangeDic[hero];

                            if(tmpDamage < 0)
                            {
                                attackersDamage.Add(tmpDamage);
                            }
                            else
                            {
                                attackersDamage.Add(0);
                            }
                        }
                        else
                        {
                            attackersDamage.Add(0);
                        }

                        int powerChange = hero.Attack(attackerNum, defenderNum);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, hero, powerChange);
                    }

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        if (attackDamage > 0)
                        {
                            defenderDamage = cellData.stander.BeDamage(ref attackDamage, hpChangeDic);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, cellData.stander, -defenderDamage);
                        }

                        if (hpChangeDic.ContainsKey(cellData.stander))
                        {
                            defenderDamage = hpChangeDic[cellData.stander];

                            if (defenderDamage > 0)
                            {
                                defenderDamage = 0;
                            }
                        }
                        else
                        {
                            defenderDamage = 0;
                        }

                        int powerChange = cellData.stander.BeAttack(attackerNum, defenderNum);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, cellData.stander, powerChange);
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        int tmpDamage;

                        if (attackDamage > 0)
                        {
                            tmpDamage = hero.BeDamage(ref attackDamage, hpChangeDic);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, hero, -tmpDamage);
                        }

                        if (hpChangeDic.ContainsKey(hero))
                        {
                            tmpDamage = hpChangeDic[hero];

                            if (tmpDamage < 0)
                            {
                                supportersDamage.Add(tmpDamage);
                            }
                            else
                            {
                                supportersDamage.Add(0);
                            }
                        }
                        else
                        {
                            supportersDamage.Add(0);
                        }

                        int powerChange = hero.BeAttack(attackerNum, defenderNum);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, hero, powerChange);
                    }

                    if (cellData.stander != null && cellData.stander.action != Hero.HeroAction.DEFENSE)
                    {
                        if (attackDamage > 0)
                        {
                            defenderDamage = -cellData.stander.BeDamage(attackDamage);

                            hpChangeDic2.Add(cellData.stander, defenderDamage);
                        }
                    }

                    _voList.Add(new BattleAttackVO(attackers, supporters, pair.Key, attackersDamage, supportersDamage, defenderDamage));
                }
            }

            Dictionary<Hero, int>.Enumerator enumerator2 = hpChangeDic2.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                KeyValuePair<Hero, int> pair = enumerator2.Current;

                if (hpChangeDic.ContainsKey(pair.Key))
                {
                    if(hpChangeDic[pair.Key] < 0)
                    {
                        hpChangeDic[pair.Key] += pair.Value;
                    }
                    else
                    {
                        hpChangeDic[pair.Key] = pair.Value;
                    }
                }
                else
                {
                    hpChangeDic.Add(pair.Key, pair.Value);
                }
            }

            ProcessHpChangeDic(_battleData, hpChangeDic, powerChangeDic, _voList, true);

            ProcessPowerChangeDic(_battleData, powerChangeDic, _voList);
        }

        private void ServerDoMove(BattleData _battleData, List<ValueType> _voList)
        {
            List<int> tmpList = null;

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, BattleCellData> pair = enumerator.Current;

                BattleCellData cellData = pair.Value;

                if (cellData.stander == null && (cellData.supporters.Count > 0 || cellData.attackOvers.Count > 0 || cellData.attackers.Count > 0))
                {
                    if (tmpList == null)
                    {
                        tmpList = new List<int>();
                    }

                    tmpList.Add(pair.Key);
                }
            }

            if (tmpList != null)
            {
                Dictionary<int, int> tmpMoveDic = new Dictionary<int, int>();

                for (int i = 0; i < tmpList.Count; i++)
                {
                    OneCellEmpty(_battleData, tmpList[i], tmpMoveDic);
                }

                _voList.Add(new BattleMoveVO(tmpMoveDic));
            }
        }

        private void ServerDoRecover(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> shieldChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> damageDic = new Dictionary<Hero, int>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                eventListener.DispatchEvent(HeroSkill.GetEventName(enumerator.Current.uid, SkillTime.RECOVER), shieldChangeDic, hpChangeDic, damageDic);
            }

            ProcessChangeDic(_battleData, shieldChangeDic, hpChangeDic, damageDic, _voList);
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

        private void ServerDoHeroDie(Hero _hero, Dictionary<Hero, int> _hpChangeDic)
        {
            string eventName = HeroSkill.GetEventName(_hero.uid, SkillTime.DIE);

            eventListener.DispatchEvent(eventName, _hpChangeDic);
        }

        private void RemoveHeroAction(BattleData _battleData, Hero _hero)
        {
            if (_hero.action == Hero.HeroAction.ATTACK)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.ATTACKOVER)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackOvers.Remove(_hero);
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

        private void OneCellEmpty(BattleData _battleData, int _pos, Dictionary<int, int> _tmpMoveDic)
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

                bool changeMapBelong = false;

                if (cellData.supporters.Count > 0)
                {
                    hero = cellData.supporters[0];
                }
                else if (cellData.attackOvers.Count > 0)
                {
                    hero = cellData.attackOvers[0];

                    changeMapBelong = true;
                }
                else if (cellData.attackers.Count > 0)
                {
                    hero = cellData.attackers[0];

                    changeMapBelong = true;
                }

                if (hero != null)
                {
                    if (changeMapBelong)
                    {
                        if(mapData.mBase == nowPos)
                        {
                            oWin = true;
                        }
                        else if(mapData.oBase == nowPos)
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

                    if(targetPos != -1)
                    {
                        autoAction.Add(hero.pos, targetPos);
                    }
                }
            }
        }

        private int GetHeroAutoAction(Hero _hero)
        {
            //优先攻击英雄
            List<int> posList = GetCanAttackerHeroPos(_hero.pos);

            if (posList.Count > 0)
            {
                int index = (int)(random.NextDouble() * posList.Count);

                return posList[index];
            }

            //然后射击英雄
            posList = GetCanShootPos(_hero.pos);

            if (posList.Count > 0)
            {
                int index = (int)(random.NextDouble() * posList.Count);

                return posList[index];
            }
            
            //如果自己可能会被攻击  则防御
            if (CheckPosCanBeAttack(_hero.pos))
            {
                return _hero.pos;
            }

            //援护英雄
            posList = GetCanSupportHeroPos(_hero.pos);
                
            if (posList.Count > 0)
            {
                int index = (int)(random.NextDouble() * posList.Count);

                return posList[index];
            }
            //无援护目标时向前进
            else
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

                if (GetPosIsMine(targetPos) == _hero.isMine)
                {
                    return targetPos;
                }
                else
                {
                    return targetPos;
                }
            }
        }

        private void RecoverCards(BinaryWriter _mBw, BinaryWriter _oBw)
        {
            if (!mWin && !oWin && mCards.Count > 0)
            {
                int index = (int)(random.NextDouble() * mCards.Count);

                int id = mCards[index];

                mCards.RemoveAt(index);

                if (mHandCards.Count < MAX_HAND_CARD_NUM)
                {
                    int tmpCardUid = GetCardUid();

                    mHandCards.Add(tmpCardUid, id);

                    _mBw.Write(true);

                    _mBw.Write(tmpCardUid);

                    _mBw.Write(id);
                }
                else
                {
                    _mBw.Write(false);
                }
            }
            else
            {
                _mBw.Write(false);
            }

            if (!mWin && !oWin && oCards.Count > 0)
            {
                int index = (int)(random.NextDouble() * oCards.Count);

                int id = oCards[index];

                oCards.RemoveAt(index);

                if (oHandCards.Count < MAX_HAND_CARD_NUM)
                {
                    int tmpCardUid = GetCardUid();

                    oHandCards.Add(tmpCardUid, id);

                    if (!isVsAi)
                    {
                        _oBw.Write(true);

                        _oBw.Write(tmpCardUid);

                        _oBw.Write(id);
                    }
                }
                else
                {
                    if (!isVsAi)
                    {
                        _oBw.Write(false);
                    }
                }
            }
            else
            {
                if (!isVsAi)
                {
                    _oBw.Write(false);
                }
            }
        }

        private void RecoverMoney()
        {
            mMoney += ADD_MONEY;

            if (mMoney > MAX_MONEY)
            {
                mMoney = MAX_MONEY;
            }

            oMoney += ADD_MONEY;

            if (oMoney > MAX_MONEY)
            {
                oMoney = MAX_MONEY;
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

        private void ProcessChangeDic(BattleData _battleData, Dictionary<Hero, int> _shieldChangeDic, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero,int> _damageDic, List<ValueType> _voList)
        {
            while (_shieldChangeDic.Count > 0 || _hpChangeDic.Count > 0 || _damageDic.Count > 0)
            {
                List<int> diePos = null;

                Dictionary<Hero, KeyValuePair<int, int>> recordDic = null;

                Dictionary<Hero, int>.Enumerator enumerator3 = _shieldChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    if(recordDic == null)
                    {
                        recordDic = new Dictionary<Hero, KeyValuePair<int, int>>();
                    }

                    recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));

                    hero.ShieldChange(pair.Value);
                }

                _shieldChangeDic.Clear();

                enumerator3 = _hpChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;
                    
                    Hero hero = pair.Key;

                    if (recordDic == null)
                    {
                        recordDic = new Dictionary<Hero, KeyValuePair<int, int>>();

                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }
                    else if (!recordDic.ContainsKey(hero))
                    {
                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }

                    bool die = hero.HpChange(pair.Value);

                    if (die)
                    {
                        if (diePos == null)
                        {
                            diePos = new List<int>();
                        }

                        diePos.Add(hero.pos);

                        ServerRemoveHero(_battleData, hero);
                    }
                }

                _hpChangeDic.Clear();

                enumerator3 = _damageDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    if (recordDic == null)
                    {
                        recordDic = new Dictionary<Hero, KeyValuePair<int, int>>();

                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }
                    else if (!recordDic.ContainsKey(hero))
                    {
                        recordDic.Add(hero, new KeyValuePair<int, int>(hero.nowShield, hero.nowHp));
                    }

                    if(hero.nowShield >= pair.Value)
                    {
                        hero.ShieldChange(-pair.Value);
                    }
                    else
                    {
                        int damage = pair.Value - hero.nowShield;

                        hero.ShieldChange(-hero.nowShield);

                        if (hero.nowHp > 0)
                        {
                            bool die = hero.HpChange(-damage);

                            if (die)
                            {
                                if (diePos == null)
                                {
                                    diePos = new List<int>();
                                }

                                diePos.Add(hero.pos);

                                ServerRemoveHero(_battleData, hero);
                            }
                        }
                    }
                }

                _damageDic.Clear();

                //if (posList != null)
                //{
                //    _voList.Add(new BattleHpChangeVO(posList, hpChangeList));
                //}

                if (diePos != null)
                {
                    _voList.Add(new BattleDeathVO(diePos));

                    for (int i = 0; i < diePos.Count; i++)
                    {
                        ServerDoHeroDie(heroMapDic[diePos[i]], _hpChangeDic);
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

        private IEnumerator<ValueType> ClientDoActionReal(BinaryReader _br)
        {
            List<ValueType> voList = BattleVOTools.ReadDataFromStream(_br);

            for (int i = 0; i < voList.Count; i++)
            {
                ValueType vo = voList[i];

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
                else if (vo is BattlePowerChangeVO)
                {
                    ClientDoPowerChange((BattlePowerChangeVO)vo);
                }
                else if (vo is BattleHpChangeVO)
                {
                    ClientDoHpChange((BattleHpChangeVO)vo);
                }

                yield return vo;
            }

            ClientDoRecover(_br);
        }

        private void ClientDoSummon(BattleSummonVO _vo)
        {
            bool isMine = GetPosIsMine(_vo.pos);

            IHeroSDS sds = GetHeroData(_vo.heroID);

            if (isMine == clientIsMine)
            {
                if (clientIsMine)
                {
                    mHandCards.Remove(_vo.cardUid);

                    mMoney -= sds.GetCost();
                }
                else
                {
                    oHandCards.Remove(_vo.cardUid);

                    oMoney -= sds.GetCost();
                }
            }

            ClientAddHero(isMine, sds, _vo.pos);
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

            hero.ClientHpChange(_vo.damage);
        }

        private void ClientDoShoot(BattleShootVO _vo)
        {
            Hero hero = heroMapDic[_vo.stander];

            hero.ClientHpChange(_vo.damage);
        }

        private void ClientDoAttack(BattleAttackVO _vo)
        {
            for(int i = 0; i < _vo.attackers.Count; i++)
            {
                Hero hero = heroMapDic[_vo.attackers[i]];

                hero.ClientHpChange(_vo.attackersDamage[i]);
            }

            for (int i = 0; i < _vo.supporters.Count; i++)
            {
                Hero hero = heroMapDic[_vo.supporters[i]];

                hero.ClientHpChange(_vo.supportersDamage[i]);
            }

            if (_vo.defenderDamage < 0)
            {
                Hero hero = heroMapDic[_vo.defender];

                hero.ClientHpChange(_vo.defenderDamage);
            }
        }

        private void ClientDoDie(BattleDeathVO _vo)
        {
            for(int i = 0; i < _vo.deads.Count; i++)
            {
                heroMapDic.Remove(_vo.deads[i]);
            }
        }

        private void ClientDoPowerChange(BattlePowerChangeVO _vo)
        {
            for(int i = 0; i < _vo.pos.Count; i++)
            {
                Hero hero = heroMapDic[_vo.pos[i]];

                hero.PowerChange(_vo.powerChange[i]);
            }
        }

        private void ClientDoHpChange(BattleHpChangeVO _vo)
        {
            for(int i = 0; i < _vo.pos.Count; i++)
            {
                Hero hero = heroMapDic[_vo.pos[i]];

                hero.ClientHpChange(_vo.hpChange[i]);
            }
        }

        private void ClientDoRecover(BinaryReader _br)
        {
            autoAction.Clear();

            int num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                autoAction.Add(pos, targetPos);
            }

            mWin = _br.ReadBoolean();

            oWin = _br.ReadBoolean();

            bool addCard = _br.ReadBoolean();

            if (addCard)
            {
                Dictionary<int, int> tmpCards = clientIsMine ? mHandCards : oHandCards;

                int tmpCardUid = _br.ReadInt32();

                int id = _br.ReadInt32();

                tmpCards.Add(tmpCardUid, id);
            }

            RecoverMoney();

            if (clientIsMine)
            {
                mOver = false;
            }
            else
            {
                oOver = false;
            }

            clientRefreshDataCallBack();

            if(mWin)
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
            return mapData.dic[_pos] != mapBelongDic.ContainsKey(_pos);
        }

        public List<int> GetCanAttackerHeroPos(int _pos)
        {
            List<int> result = new List<int>();

            bool isMine = GetPosIsMine(_pos);

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            bool getThreat = false;

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != isMine && heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.sds.GetThreat())
                    {
                        if (!getThreat)
                        { 
                            getThreat = true;

                            if(result.Count > 0)
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


        public List<int> GetCanAttackPos(int _pos)
        {
            List<int> result = new List<int>();

            bool isMine = GetPosIsMine(_pos);

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            bool getThreat = false;

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if(b != isMine)
                {
                    if (heroMapDic.ContainsKey(pos))
                    {
                        Hero hero = heroMapDic[pos];

                        if (hero.sds.GetThreat())
                        {
                            if (!getThreat)
                            {
                                getThreat = true;

                                if(result.Count > 0)
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

        public List<int> GetCanShootPos(int _pos)
        {
            List<int> result = new List<int>();

            bool isMine = GetPosIsMine(_pos);

            List<int> posList2 = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            for(int i = 0; i < posList2.Count; i++)
            {
                int pos = posList2[i];

                bool b = GetPosIsMine(pos);

                if(b != isMine && heroMapDic.ContainsKey(pos))
                {
                    return result;
                }
            }

            List<int> posList = BattlePublicTools.GetNeighbourPos2(mapData.neighbourPosMap, _pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != isMine && heroMapDic.ContainsKey(pos))
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

                if (hero.sds.GetThreat())
                {
                    isThreat = true;
                }
            }

            bool isMine = GetPosIsMine(_pos);

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b != isMine)
                {
                    if (heroMapDic.ContainsKey(pos))
                    {
                        Hero hero = heroMapDic[pos];

                        if (hero.CheckCanDoAction(Hero.HeroAction.ATTACK))
                        {
                            if (isThreat)
                            {
                                return true;
                            }

                            bool canAttack = true;

                            List<int> tmpPosList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, pos);

                            for(int m = 0; m < tmpPosList.Count; m++)
                            {
                                int tmpPos = tmpPosList[m];

                                if(tmpPos == _pos)
                                {
                                    continue;
                                }

                                b = GetPosIsMine(tmpPos);

                                if(b == isMine && heroMapDic.ContainsKey(tmpPos))
                                {
                                    Hero tmpHero = heroMapDic[tmpPos];

                                    if (tmpHero.sds.GetThreat())
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
            }

            return false;
        }

        public List<int> GetCanSupportHeroPos(int _pos)
        {
            List<int> result = new List<int>();

            bool isMine = GetPosIsMine(_pos);

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b == isMine && heroMapDic.ContainsKey(pos))
                {
                    if (CheckPosCanBeAttack(pos))
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }

        public List<int> GetCanSupportPos(int _pos)
        {
            List<int> result = new List<int>();

            bool isMine = GetPosIsMine(_pos);

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = GetPosIsMine(pos);

                if (b == isMine)
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

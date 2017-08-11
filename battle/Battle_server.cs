#if !CLIENT
using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;

namespace FinalWar
{
    public partial class Battle
    {
        private static readonly Random random = new Random();

        private Dictionary<int, int> mHandCardsChangeDic = new Dictionary<int, int>();
        private Dictionary<int, int> oHandCardsChangeDic = new Dictionary<int, int>();

        private Action<bool, MemoryStream> serverSendDataCallBack;
        private Action serverBattleOverCallBack;


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

            mMoney = BattleConst.DEFAULT_MONEY;

            if (!isVsAi)
            {
                oMoney = BattleConst.DEFAULT_MONEY;
            }
            else
            {
                oMoney = BattleConst.AI_DEFAULT_MONEY;
            }

            mWin = oWin = false;

            mOver = oOver = false;

            cardUid = 1;

            mCards = new List<int>(_mCards);

            oCards = new List<int>(_oCards);

            for (int i = 0; i < BattleConst.DEFAULT_HAND_CARD_NUM; i++)
            {
                if (mCards.Count > 0)
                {
                    int index = random.Next(mCards.Count);

                    int cardID = mCards[index];

                    mHandCards.Add(GetCardUid(), cardID);

                    mCards.RemoveAt(index);
                }

                if (oCards.Count > 0)
                {
                    int index = random.Next(oCards.Count);

                    int cardID = oCards[index];

                    oHandCards.Add(GetCardUid(), cardID);

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

                    Hero hero = new Hero(this, isMine, heroSDS, pos, true);

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

                    bw.Write(isVsAi);

                    bw.Write(_isMine);

                    bw.Write(mScore);

                    bw.Write(oScore);

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

                        hero.WriteToStream(bw);
                    }

                    Dictionary<int, int> handCards;

                    Dictionary<int, int> handCards2;

                    List<int> cards;

                    List<int> cards2;

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

                    Dictionary<int, int>.Enumerator enumerator = handCards.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(handCards2.Count);

                    enumerator = handCards2.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(0);
                    }

                    bw.Write(cards.Count);

                    for (int i = 0; i < cards.Count; i++)
                    {
                        bw.Write(cards[i]);
                    }

                    bw.Write(cards2.Count);

                    for (int i = 0; i < cards2.Count; i++)
                    {
                        bw.Write(0);
                    }

                    bw.Write(mMoney);

                    bw.Write(oMoney);

                    bool isOver = _isMine ? mOver : oOver;

                    bw.Write(isOver);

                    if (isOver)
                    {
                        long tmpPos = bw.BaseStream.Position;

                        bw.Write(0);

                        int num = 0;

                        Dictionary<int, int>.Enumerator enumerator4 = summon.GetEnumerator();

                        while (enumerator4.MoveNext())
                        {
                            int pos = enumerator4.Current.Value;

                            if (GetPosIsMine(pos) == _isMine)
                            {
                                num++;

                                bw.Write(enumerator4.Current.Key);

                                bw.Write(enumerator4.Current.Value);
                            }
                        }

                        long tmpPos2 = bw.BaseStream.Position;

                        bw.BaseStream.Position = tmpPos;

                        bw.Write(num);

                        bw.BaseStream.Position = tmpPos2;

                        bw.Write(0);

                        num = 0;

                        for (int i = 0; i < action.Count; i++)
                        {
                            KeyValuePair<int, int> pair = action[i];

                            if (GetPosIsMine(pair.Key) == _isMine)
                            {
                                num++;

                                bw.Write(pair.Key);

                                bw.Write(pair.Value);
                            }
                        }

                        tmpPos = bw.BaseStream.Position;

                        bw.BaseStream.Position = tmpPos2;

                        bw.Write(num);

                        bw.BaseStream.Position = tmpPos;
                    }

                    serverSendDataCallBack(_isMine, ms);
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

                if (GetPosIsMine(pos) == _isMine)
                {
                    if (cards.ContainsKey(uid))
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

                MapData.MapUnitType mapUnitType;

                if (mapData.dic.TryGetValue(targetPos, out mapUnitType))
                {
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
                HeroAi.Start(this, false);

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

                        SuperEnumerator<ValueType> step = new SuperEnumerator<ValueType>(StartBattle());

                        step.Done();

                        ServerWriteCardsAndRandom(true, mBw);

                        ServerWriteCardsAndRandom(false, oBw);

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

                if (GetPosIsMine(pair.Key) != _isMine)
                {
                    num++;

                    _bw.Write(pair.Key);

                    _bw.Write(pair.Value);
                }
            }

            long pos1 = _bw.BaseStream.Position;

            _bw.BaseStream.Position = pos0;

            _bw.Write(num);

            _bw.BaseStream.Position = pos1;

            _bw.Write(0);

            num = 0;

            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                if (GetPosIsMine(pair.Value) != _isMine)
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
            Dictionary<int, int> cards = _isMine ? mHandCardsChangeDic : oHandCardsChangeDic;

            _bw.Write(cards.Count);

            Dictionary<int, int>.Enumerator enumerator = cards.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _bw.Write(enumerator.Current.Key);

                _bw.Write(enumerator.Current.Value);
            }

            _bw.Write(randomList.Count);

            Queue<double>.Enumerator enumerator2 = randomList.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                _bw.Write(enumerator2.Current);
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
    }
}
#endif

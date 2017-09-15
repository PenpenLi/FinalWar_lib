using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;

namespace FinalWar
{
    public class Battle_server
    {
        private enum CardState
        {
            N,
            M,
            O,
            A
        }

        private static readonly Random random = new Random();

        private static int GetRandomValue(int _max)
        {
            return random.Next(_max);
        }

        private Battle battle;

        private bool mOver;
        private bool oOver;

        //-----------------record data
        private int roundNum;

        private int mapID;

        private Dictionary<int, KeyValuePair<int, bool>>[] summon = new Dictionary<int, KeyValuePair<int, bool>>[BattleConst.MAX_ROUND_NUM];

        private Dictionary<int, KeyValuePair<int, bool>>[] action = new Dictionary<int, KeyValuePair<int, bool>>[BattleConst.MAX_ROUND_NUM];

        private int[] randomIndexList = new int[BattleConst.MAX_ROUND_NUM];

        private int[] mCards;

        private int[] oCards;

        private bool isVsAi;
        //-----------------

        private CardState[] cardStateArr = new CardState[BattleConst.DECK_CARD_NUM * 2];

        private Action<bool, bool, MemoryStream> serverSendDataCallBack;

        private Action<Battle.BattleResult> serverBattleOverCallBack;

        private int battleResult = -1;

        public Battle_server(bool _isBattle)
        {
            if (_isBattle)
            {
                battle = new Battle();
            }

            for (int i = 0; i < BattleConst.MAX_ROUND_NUM; i++)
            {
                summon[i] = new Dictionary<int, KeyValuePair<int, bool>>();

                action[i] = new Dictionary<int, KeyValuePair<int, bool>>();
            }
        }

        public void ServerSetCallBack(Action<bool, bool, MemoryStream> _serverSendDataCallBack, Action<Battle.BattleResult> _serverBattleOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;

            serverBattleOverCallBack = _serverBattleOverCallBack;

            if (battle != null)
            {
                battle.InitBattleEndCallBack(BattleOver);
            }
        }

        public void ServerStart(int _mapID, IList<int> _mCards, IList<int> _oCards, bool _isVsAi)
        {
            Log.Write("Battle Start!");

            mapID = _mapID;

            isVsAi = _isVsAi;

            InitCards(_mCards, _oCards, out mCards, out oCards);

            InitCardState();

            if (battle != null)
            {
                battle.InitBattle(_mapID, mCards, oCards);
            }
        }

        private void InitCards(IList<int> _mCards, IList<int> _oCards, out int[] _mCardsResult, out int[] _oCardsResult)
        {
            List<int> mTmpCards = new List<int>(_mCards);

            if (mTmpCards.Count > BattleConst.DECK_CARD_NUM)
            {
                _mCardsResult = new int[BattleConst.DECK_CARD_NUM];
            }
            else
            {
                _mCardsResult = new int[mTmpCards.Count];
            }

            for (int i = 0; i < _mCardsResult.Length; i++)
            {
                int index = GetRandomValue(mTmpCards.Count);

                int id = mTmpCards[index];

                mTmpCards.RemoveAt(index);

                _mCardsResult[i] = id;
            }

            List<int> oTmpCards = new List<int>(_oCards);

            if (oTmpCards.Count > BattleConst.DECK_CARD_NUM)
            {
                _oCardsResult = new int[BattleConst.DECK_CARD_NUM];
            }
            else
            {
                _oCardsResult = new int[oTmpCards.Count];
            }

            for (int i = 0; i < _oCardsResult.Length; i++)
            {
                int index = GetRandomValue(oTmpCards.Count);

                int id = oTmpCards[index];

                oTmpCards.RemoveAt(index);

                _oCardsResult[i] = id;
            }
        }

        private void InitCardState()
        {
            for (int i = 0; i < mCards.Length && i < BattleConst.DEFAULT_HAND_CARD_NUM; i++)
            {
                cardStateArr[i] = CardState.M;
            }

            for (int i = 0; i < oCards.Length && i < BattleConst.DEFAULT_HAND_CARD_NUM; i++)
            {
                cardStateArr[BattleConst.DECK_CARD_NUM + i] = CardState.O;
            }
        }

        public void ServerGetPackage(BinaryReader _br, bool _isMine)
        {
            byte tag = _br.ReadByte();

            switch (tag)
            {
                case PackageTag.C2S_REFRESH:

                    ServerRefreshData(_isMine);

                    break;

                case PackageTag.C2S_DOACTION:

                    ServerDoAction(_isMine, _br);

                    break;

                case PackageTag.C2S_QUIT:

                    ServerQuitBattle(_isMine);

                    break;
            }
        }

        private void ServerRefreshData(bool _isMine)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Log.Write("ServerRefreshData  isMine:" + _isMine);

                    bool isOver;

                    CardState tmpCardState;

                    if (_isMine)
                    {
                        isOver = mOver;

                        tmpCardState = CardState.M;
                    }
                    else
                    {
                        isOver = oOver;

                        tmpCardState = CardState.O;
                    }

                    bw.Write(isVsAi);

                    bw.Write(_isMine);

                    bw.Write(mapID);

                    bw.Write(mCards.Length);

                    bw.Write(oCards.Length);

                    long pos = ms.Position;

                    bw.Write(0);

                    int num = 0;

                    for (int i = 0; i < BattleConst.DECK_CARD_NUM; i++)
                    {
                        int index = i;

                        CardState cardState = cardStateArr[index];

                        if (cardState == CardState.A || cardState == tmpCardState || (isVsAi && cardState != CardState.N))
                        {
                            bw.Write(index);

                            bw.Write(mCards[i]);

                            num++;
                        }

                        index = BattleConst.DECK_CARD_NUM + i;

                        cardState = cardStateArr[index];

                        if (cardState == CardState.A || cardState == tmpCardState || (isVsAi && cardState != CardState.N))
                        {
                            bw.Write(index);

                            bw.Write(oCards[i]);

                            num++;
                        }
                    }

                    long pos2 = ms.Position;

                    ms.Position = pos;

                    bw.Write(num);

                    ms.Position = pos2;

                    bw.Write(roundNum);

                    for (int i = 0; i < roundNum; i++)
                    {
                        WriteRoundDataToStream(bw, i);
                    }

                    bw.Write(isOver);

                    if (isOver)
                    {
                        WriteRoundDataToStream(bw, roundNum);
                    }

                    serverSendDataCallBack(_isMine, false, ms);
                }
            }
        }

        private void WriteRoundDataToStream(BinaryWriter _bw, int _roundNum)
        {
            Dictionary<int, KeyValuePair<int, bool>> tmpDic = summon[_roundNum];

            _bw.Write(tmpDic.Count);

            Dictionary<int, KeyValuePair<int, bool>>.Enumerator enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, KeyValuePair<int, bool>> pair = enumerator.Current;

                _bw.Write(pair.Key);

                _bw.Write(pair.Value.Key);
            }

            tmpDic = action[_roundNum];

            _bw.Write(tmpDic.Count);

            enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, KeyValuePair<int, bool>> pair = enumerator.Current;

                _bw.Write(pair.Key);

                _bw.Write(pair.Value.Key);
            }

            _bw.Write(randomIndexList[_roundNum]);
        }


        private void ServerDoAction(bool _isMine, BinaryReader _br)
        {
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
            }

            Dictionary<int, KeyValuePair<int, bool>> tmpDic = summon[roundNum];

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                tmpDic.Add(uid, new KeyValuePair<int, bool>(pos, _isMine));
            }

            tmpDic = action[roundNum];

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                tmpDic.Add(pos, new KeyValuePair<int, bool>(targetPos, _isMine));
            }

            serverSendDataCallBack(_isMine, false, new MemoryStream());

            if ((mOver && oOver) || isVsAi)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        ServerStartBattleSetCardState();

                        ServerStartBattleSetRandom();

                        ServerStartBattle(mBw, oBw);

                        if (battle != null)
                        {
                            ProcessBattle();

                            if (battleResult == -1)
                            {
                                roundNum++;

                                mOver = oOver = false;

                                serverSendDataCallBack(true, true, mMs);

                                serverSendDataCallBack(false, true, oMs);
                            }
                            else
                            {
                                ResetData();

                                Battle.BattleResult tmpResult = (Battle.BattleResult)battleResult;

                                battleResult = -1;

                                serverSendDataCallBack(true, true, mMs);

                                serverSendDataCallBack(false, true, oMs);

                                serverBattleOverCallBack(tmpResult);
                            }
                        }
                        else
                        {
                            roundNum++;

                            mOver = oOver = false;

                            serverSendDataCallBack(true, true, mMs);

                            serverSendDataCallBack(false, true, oMs);
                        }
                    }
                }
            }
        }

        private void ServerStartBattleSetCardState()
        {
            Dictionary<int, KeyValuePair<int, bool>> tmpDic = summon[roundNum];

            Dictionary<int, KeyValuePair<int, bool>>.KeyCollection.Enumerator enumerator = tmpDic.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int uid = enumerator.Current;

                cardStateArr[uid] = CardState.A;
            }

            for (int i = 0; i < BattleConst.ADD_CARD_NUM; i++)
            {
                int index = BattleConst.DEFAULT_HAND_CARD_NUM + roundNum * BattleConst.ADD_CARD_NUM + i;

                if (index < mCards.Length)
                {
                    int uid = index;

                    cardStateArr[uid] = CardState.M;
                }

                if (index < oCards.Length)
                {
                    int uid = BattleConst.DECK_CARD_NUM + index;

                    cardStateArr[uid] = CardState.O;
                }
            }
        }

        private void ServerStartBattleSetRandom()
        {
            int randomIndex = GetRandomValue(BattleRandomPool.num);

            randomIndexList[roundNum] = randomIndex;
        }

        private void ServerStartBattle(BinaryWriter _mBw, BinaryWriter _oBw)
        {
            _mBw.Write(PackageTag.S2C_DOACTION);

            _oBw.Write(PackageTag.S2C_DOACTION);

            WriteRoundDataToStream(_mBw, roundNum);

            WriteRoundDataToStream(_oBw, roundNum);

            long pos = _mBw.BaseStream.Position;

            _mBw.Write(0);

            _oBw.Write(0);

            int mNum = 0;

            int oNum = 0;

            Dictionary<int, KeyValuePair<int, bool>> tmpDic = summon[roundNum];

            Dictionary<int, KeyValuePair<int, bool>>.KeyCollection.Enumerator enumerator = tmpDic.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int uid = enumerator.Current;

                _mBw.Write(uid);

                _oBw.Write(uid);

                if (uid < BattleConst.DECK_CARD_NUM)
                {
                    _mBw.Write(mCards[uid]);

                    _oBw.Write(mCards[uid]);
                }
                else
                {
                    _mBw.Write(oCards[uid - BattleConst.DECK_CARD_NUM]);

                    _oBw.Write(oCards[uid - BattleConst.DECK_CARD_NUM]);
                }

                mNum++;

                oNum++;
            }

            for (int i = 0; i < BattleConst.ADD_CARD_NUM; i++)
            {
                int index = BattleConst.DEFAULT_HAND_CARD_NUM + roundNum * BattleConst.ADD_CARD_NUM + i;

                if (index < mCards.Length)
                {
                    int uid = index;

                    int id = mCards[index];

                    _mBw.Write(uid);

                    _mBw.Write(id);

                    mNum++;

                    if (isVsAi)
                    {
                        _oBw.Write(uid);

                        _oBw.Write(id);

                        oNum++;
                    }
                }

                if (index < oCards.Length)
                {
                    int uid = BattleConst.DECK_CARD_NUM + index;

                    int id = oCards[index];

                    _oBw.Write(uid);

                    _oBw.Write(id);

                    oNum++;

                    if (isVsAi)
                    {
                        _mBw.Write(uid);

                        _mBw.Write(id);

                        mNum++;
                    }
                }
            }

            _mBw.BaseStream.Position = pos;

            _mBw.Write(mNum);

            _oBw.BaseStream.Position = pos;

            _oBw.Write(oNum);
        }

        private void ProcessBattle()
        {
            Dictionary<int, KeyValuePair<int, bool>>.Enumerator enumerator2 = summon[roundNum].GetEnumerator();

            while (enumerator2.MoveNext())
            {
                bool b = battle.AddSummon(enumerator2.Current.Value.Value, enumerator2.Current.Key, enumerator2.Current.Value.Key);

                if (!b)
                {
                    throw new Exception("summon error!");
                }
            }

            enumerator2 = action[roundNum].GetEnumerator();

            while (enumerator2.MoveNext())
            {
                bool b = battle.AddAction(enumerator2.Current.Value.Value, enumerator2.Current.Key, enumerator2.Current.Value.Key);

                if (!b)
                {
                    throw new Exception("action error!");
                }
            }

            battle.SetRandomIndex(randomIndexList[roundNum]);

            BattleAi.Start(battle, false, battle.GetRandomValue);

            SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(battle.StartBattle());

            superEnumerator.Done();
        }

        private void ServerQuitBattle(bool _isMine)
        {
            serverSendDataCallBack(_isMine, false, new MemoryStream());

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.S2C_QUIT);

                    bw.Write(_isMine);

                    serverSendDataCallBack(true, true, ms);

                    serverSendDataCallBack(false, true, ms);
                }
            }

            if (battle != null)
            {
                battle.BattleOver();
            }

            BattleQuit(_isMine);
        }

        private void BattleQuit(bool _isMine)
        {
            ResetData();

            if (_isMine)
            {
                serverBattleOverCallBack(Battle.BattleResult.O_WIN);
            }
            else
            {
                serverBattleOverCallBack(Battle.BattleResult.M_WIN);
            }
        }

        private void BattleOver(Battle.BattleResult _result)
        {
            battleResult = (int)_result;
        }

        public void ResetData()
        {
            mOver = oOver = false;

            roundNum = 0;

            for (int i = 0; i < BattleConst.DECK_CARD_NUM * 2; i++)
            {
                cardStateArr[i] = CardState.N;
            }

            for (int i = 0; i < BattleConst.MAX_ROUND_NUM; i++)
            {
                action[i].Clear();

                summon[i].Clear();
            }
        }

        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(mapID);

                    bw.Write(roundNum);

                    for (int i = 0; i < roundNum; i++)
                    {
                        Dictionary<int, KeyValuePair<int, bool>> tmpDic = action[i];

                        bw.Write(tmpDic.Count);

                        Dictionary<int, KeyValuePair<int, bool>>.Enumerator enumerator = tmpDic.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            bw.Write(enumerator.Current.Key);

                            bw.Write(enumerator.Current.Value.Key);

                            bw.Write(enumerator.Current.Value.Value);
                        }

                        tmpDic = summon[i];

                        bw.Write(tmpDic.Count);

                        enumerator = tmpDic.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            bw.Write(enumerator.Current.Key);

                            bw.Write(enumerator.Current.Value.Key);

                            bw.Write(enumerator.Current.Value.Value);
                        }

                        bw.Write(randomIndexList[i]);
                    }

                    bw.Write(mCards.Length);

                    for (int i = 0; i < mCards.Length; i++)
                    {
                        bw.Write(mCards[i]);
                    }

                    bw.Write(oCards.Length);

                    for (int i = 0; i < oCards.Length; i++)
                    {
                        bw.Write(oCards[i]);
                    }

                    bw.Write(isVsAi);

                    return ms.ToArray();
                }
            }
        }

        public void FromBytes(byte[] _bytes)
        {
            ResetData();

            using (MemoryStream ms = new MemoryStream(_bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    mapID = br.ReadInt32();

                    int tmpRoundNum = br.ReadInt32();

                    for (int i = 0; i < tmpRoundNum; i++)
                    {
                        int num = br.ReadInt32();

                        for (int m = 0; m < num; m++)
                        {
                            int k = br.ReadInt32();

                            int v1 = br.ReadInt32();

                            bool v2 = br.ReadBoolean();

                            action[i].Add(k, new KeyValuePair<int, bool>(v1, v2));
                        }

                        num = br.ReadInt32();

                        for (int m = 0; m < num; m++)
                        {
                            int k = br.ReadInt32();

                            int v1 = br.ReadInt32();

                            bool v2 = br.ReadBoolean();

                            summon[i].Add(k, new KeyValuePair<int, bool>(v1, v2));
                        }

                        randomIndexList[i] = br.ReadInt32();
                    }

                    int num2 = br.ReadInt32();

                    mCards = new int[num2];

                    for (int i = 0; i < num2; i++)
                    {
                        mCards[i] = br.ReadInt32();
                    }

                    num2 = br.ReadInt32();

                    oCards = new int[num2];

                    for (int i = 0; i < num2; i++)
                    {
                        oCards[i] = br.ReadInt32();
                    }

                    isVsAi = br.ReadBoolean();

                    InitCardState();

                    for (int i = 0; i < tmpRoundNum; i++)
                    {
                        ServerStartBattleSetCardState();

                        roundNum++;
                    }
                }
            }
        }
    }
}
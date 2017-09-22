using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;

namespace FinalWar
{
    public class Battle_server
    {
        private struct PlayerAction
        {
            public bool isMine;
            public int key;
            public int value;

            public PlayerAction(bool _isMine, int _key, int _value)
            {
                isMine = _isMine;
                key = _key;
                value = _value;
            }
        }

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
        public int roundNum { private set; get; }

        private int mapID;

        private List<PlayerAction>[] summon = new List<PlayerAction>[BattleConst.MAX_ROUND_NUM];

        private List<PlayerAction>[] action = new List<PlayerAction>[BattleConst.MAX_ROUND_NUM];

        private int[] randomIndexList = new int[BattleConst.MAX_ROUND_NUM];

        private int[] mCards;

        private int[] oCards;

        private bool isVsAi;
        //-----------------

        private CardState[] cardStateArr = new CardState[BattleConst.DECK_CARD_NUM * 2];

        private Action<bool, bool, MemoryStream> serverSendDataCallBack;

        private Action<Battle.BattleResult> serverBattleOverCallBack;

        private bool isBattle;

        public Battle_server(bool _isBattle)
        {
            isBattle = _isBattle;

            battle = new Battle();

            for (int i = 0; i < BattleConst.MAX_ROUND_NUM; i++)
            {
                summon[i] = new List<PlayerAction>();

                action[i] = new List<PlayerAction>();
            }
        }

        public void ServerSetCallBack(Action<bool, bool, MemoryStream> _serverSendDataCallBack, Action<Battle.BattleResult> _serverBattleOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;

            serverBattleOverCallBack = _serverBattleOverCallBack;
        }

        public void ServerStart(int _mapID, IList<int> _mCards, IList<int> _oCards, bool _isVsAi)
        {
            Log.Write("Battle Start!");

            mapID = _mapID;

            isVsAi = _isVsAi;

            InitCards(_mCards, _oCards, out mCards, out oCards);

            InitCardState();

            if (isBattle)
            {
                battle.InitBattle(mapID, mCards, oCards);
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

        public bool ServerGetPackage(BinaryReader _br, bool _isMine, bool _reply)
        {
            byte tag = _br.ReadByte();

            switch (tag)
            {
                case PackageTag.C2S_REFRESH:

                    ServerRefreshData(_isMine);

                    return false;

                case PackageTag.C2S_DOACTION:

                    ServerDoAction(_isMine, _br, _reply);

                    return true;

                case PackageTag.C2S_QUIT:

                    ServerQuitBattle(_isMine, _reply);

                    return false;

                default:

                    throw new Exception("Unknow PackageTag:" + tag);
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
            List<PlayerAction> tmpDic = summon[_roundNum];

            _bw.Write(tmpDic.Count);

            List<PlayerAction>.Enumerator enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                PlayerAction action = enumerator.Current;

                _bw.Write(action.key);

                _bw.Write(action.value);
            }

            tmpDic = action[_roundNum];

            _bw.Write(tmpDic.Count);

            enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                PlayerAction action = enumerator.Current;

                _bw.Write(action.key);

                _bw.Write(action.value);
            }

            _bw.Write(randomIndexList[_roundNum]);
        }


        private void ServerDoAction(bool _isMine, BinaryReader _br, bool _reply)
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

            int tmpRoundNum = _br.ReadInt32();

            if (tmpRoundNum == roundNum)
            {
                List<PlayerAction> tmpDic = summon[roundNum];

                int num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int uid = _br.ReadInt32();

                    int pos = _br.ReadInt32();

                    tmpDic.Add(new PlayerAction(_isMine, uid, pos));
                }

                tmpDic = action[roundNum];

                num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int pos = _br.ReadInt32();

                    int targetPos = _br.ReadInt32();

                    tmpDic.Add(new PlayerAction(_isMine, pos, targetPos));
                }
            }

            if (_reply)
            {
                serverSendDataCallBack(_isMine, false, new MemoryStream());
            }

            if ((mOver && oOver) || isVsAi)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        ServerStartBattleSetCardState();

                        ServerStartBattleSetRandom();

                        ServerStartBattle(mBw, oBw);

                        if (isBattle)
                        {
                            Battle.BattleResult battleResult = ProcessBattle();

                            if (battleResult == Battle.BattleResult.NOT_OVER)
                            {
                                roundNum++;

                                mOver = oOver = false;

                                serverSendDataCallBack(true, true, mMs);

                                serverSendDataCallBack(false, true, oMs);
                            }
                            else
                            {
                                ResetData();

                                serverSendDataCallBack(true, true, mMs);

                                serverSendDataCallBack(false, true, oMs);

                                serverBattleOverCallBack(battleResult);
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
            List<PlayerAction> tmpDic = summon[roundNum];

            for (int i = 0; i < tmpDic.Count; i++)
            {
                int uid = tmpDic[i].key;

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

            List<PlayerAction> tmpDic = summon[roundNum];

            for (int i = 0; i < tmpDic.Count; i++)
            {
                int uid = tmpDic[i].key;

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

        private Battle.BattleResult ProcessBattle()
        {
            List<PlayerAction>.Enumerator enumerator2 = summon[roundNum].GetEnumerator();

            while (enumerator2.MoveNext())
            {
                bool b = battle.AddSummon(enumerator2.Current.isMine, enumerator2.Current.key, enumerator2.Current.value);

                if (!b)
                {
                    throw new Exception("summon error!");
                }
            }

            enumerator2 = action[roundNum].GetEnumerator();

            while (enumerator2.MoveNext())
            {
                bool b = battle.AddAction(enumerator2.Current.isMine, enumerator2.Current.key, enumerator2.Current.value);

                if (!b)
                {
                    throw new Exception("action error!");
                }
            }

            battle.SetRandomIndex(randomIndexList[roundNum]);

            if (isVsAi)
            {
                BattleAi.Start(battle, false, battle.GetRandomValue);
            }

            SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(battle.StartBattle());

            superEnumerator.Done();

            return (Battle.BattleResult)superEnumerator.Current;
        }

        public Battle.BattleResult VerifyBattle()
        {
            int tmpRoundNum = roundNum;

            roundNum = 0;

            Battle.BattleResult battleResult = Battle.BattleResult.NOT_OVER;

            battle.InitBattle(mapID, mCards, oCards);

            while (roundNum < tmpRoundNum)
            {
                battleResult = ProcessBattle();

                if (battleResult == Battle.BattleResult.NOT_OVER)
                {
                    roundNum++;
                }
                else
                {
                    break;
                }
            }

            battle.BattleOver();

            roundNum = tmpRoundNum;

            return battleResult;
        }

        private void ServerQuitBattle(bool _isMine, bool _reply)
        {
            if (_reply)
            {
                serverSendDataCallBack(_isMine, false, new MemoryStream());
            }

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

            if (isBattle)
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
                        List<PlayerAction> tmpDic = action[i];

                        bw.Write(tmpDic.Count);

                        List<PlayerAction>.Enumerator enumerator = tmpDic.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            bw.Write(enumerator.Current.isMine);

                            bw.Write(enumerator.Current.key);

                            bw.Write(enumerator.Current.value);
                        }

                        tmpDic = summon[i];

                        bw.Write(tmpDic.Count);

                        enumerator = tmpDic.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            bw.Write(enumerator.Current.isMine);

                            bw.Write(enumerator.Current.key);

                            bw.Write(enumerator.Current.value);
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
                            bool isMine = br.ReadBoolean();

                            int key = br.ReadInt32();

                            int value = br.ReadInt32();

                            action[i].Add(new PlayerAction(isMine, key, value));
                        }

                        num = br.ReadInt32();

                        for (int m = 0; m < num; m++)
                        {
                            bool isMine = br.ReadBoolean();

                            int key = br.ReadInt32();

                            int value = br.ReadInt32();

                            summon[i].Add(new PlayerAction(isMine, key, value));
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
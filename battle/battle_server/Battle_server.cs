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

        private class BattleRecordData
        {
            public int roundNum;

            public int mapID;

            public int maxRoundNum;

            public List<BattleRecordRoundData> data = new List<BattleRecordRoundData>();

            public int[] mCards;

            public int[] oCards;

            public bool isVsAi;

            public int randomSeed;
        }

        private class BattleRecordRoundData
        {
            public List<PlayerAction> summon = new List<PlayerAction>();

            public List<PlayerAction> action = new List<PlayerAction>();

            public int randomSeed;
        }

        private static readonly Random random = new Random();

        private Battle battle;

        private bool mOver;
        private bool oOver;

        private CardState[] cardStateArr = new CardState[BattleConst.DECK_CARD_NUM * 2];

        private Action<bool, bool, MemoryStream> serverSendDataCallBack;

        private Action<Battle.BattleResult> serverRoundOverCallBack;

        private bool processBattle;

        private BattleRecordData recordData;

        public Battle_server(bool _processBattle)
        {
            processBattle = _processBattle;

            battle = new Battle();
        }

        public void ServerSetCallBack(Action<bool, bool, MemoryStream> _serverSendDataCallBack, Action<Battle.BattleResult> _serverBattleOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;

            serverRoundOverCallBack = _serverBattleOverCallBack;
        }

        public void ServerStart(int _mapID, int _maxRoundNum, IList<int> _mCards, IList<int> _oCards, bool _isVsAi)
        {
            Log.Write("Battle Start!");

            recordData = new BattleRecordData();

            recordData.data.Add(new BattleRecordRoundData());

            recordData.mapID = _mapID;

            recordData.maxRoundNum = _maxRoundNum;

            recordData.isVsAi = _isVsAi;

            recordData.randomSeed = random.Next();

            InitCards(_mCards, _oCards, out recordData.mCards, out recordData.oCards);

            InitCardState(cardStateArr, recordData);

            if (processBattle)
            {
                battle.InitBattle(recordData.mapID, recordData.maxRoundNum, recordData.mCards, recordData.oCards, recordData.randomSeed);
            }
        }

        public bool ServerGetPackage(BinaryReader _br, bool _isMine)
        {
            byte tag = _br.ReadByte();

            switch (tag)
            {
                case PackageTag.C2S_REFRESH:

                    ServerRefreshData(_isMine);

                    return false;

                case PackageTag.C2S_DOACTION:

                    return ServerDoAction(_isMine, _br);

                case PackageTag.C2S_QUIT:

                    ServerQuitBattle(_isMine);

                    return false;

                case PackageTag.C2S_RESULT:

                    Battle.BattleResult battleResult = (Battle.BattleResult)_br.ReadByte();

                    Log.Write("server get result:" + battleResult);

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

                    bw.Write(_isMine);

                    bw.Write(processBattle);

                    bw.Write(recordData.isVsAi);

                    bw.Write(recordData.mapID);

                    bw.Write(recordData.maxRoundNum);

                    bw.Write(recordData.randomSeed);

                    bw.Write(recordData.mCards.Length);

                    bw.Write(recordData.oCards.Length);

                    long pos = ms.Position;

                    bw.Write(0);

                    int num = 0;

                    for (int i = 0; i < BattleConst.DECK_CARD_NUM; i++)
                    {
                        int index = i;

                        CardState cardState = cardStateArr[index];

                        if (cardState == CardState.A || cardState == tmpCardState || (recordData.isVsAi && cardState != CardState.N))
                        {
                            bw.Write(index);

                            bw.Write(recordData.mCards[i]);

                            num++;
                        }

                        index = BattleConst.DECK_CARD_NUM + i;

                        cardState = cardStateArr[index];

                        if (cardState == CardState.A || cardState == tmpCardState || (recordData.isVsAi && cardState != CardState.N))
                        {
                            bw.Write(index);

                            bw.Write(recordData.oCards[i]);

                            num++;
                        }
                    }

                    long pos2 = ms.Position;

                    ms.Position = pos;

                    bw.Write(num);

                    ms.Position = pos2;

                    bw.Write(recordData.roundNum);

                    for (int i = 0; i < recordData.roundNum; i++)
                    {
                        WriteRoundDataToStream(bw, recordData.data[i]);
                    }

                    bw.Write(isOver);

                    if (isOver)
                    {
                        WriteRoundDataToStream(bw, recordData.data[recordData.roundNum]);
                    }

                    serverSendDataCallBack(_isMine, false, ms);
                }
            }
        }

        private bool ServerDoAction(bool _isMine, BinaryReader _br)
        {
            serverSendDataCallBack(_isMine, false, new MemoryStream());

            if (_isMine)
            {
                if (mOver)
                {
                    return false;
                }
            }
            else
            {
                if (oOver)
                {
                    return false;
                }
            }

            int tmpRoundNum = _br.ReadInt32();

            if (tmpRoundNum == recordData.roundNum)
            {
                BattleRecordRoundData data = recordData.data[recordData.roundNum];

                int num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int uid = _br.ReadInt32();

                    int pos = _br.ReadInt32();

                    data.summon.Add(new PlayerAction(_isMine, uid, pos));
                }

                num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int pos = _br.ReadInt32();

                    int targetPos = _br.ReadInt32();

                    data.action.Add(new PlayerAction(_isMine, pos, targetPos));
                }

                ServerDoActionReal(_isMine);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void ServerDoActionReal(bool _isMine)
        {
            if (_isMine)
            {
                mOver = true;
            }
            else
            {
                oOver = true;
            }

            if ((mOver && oOver) || recordData.isVsAi)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        ServerSetCardState(cardStateArr, recordData, recordData.roundNum);

                        ServerStartBattleSetRandom(recordData, recordData.roundNum);

                        ServerStartBattle(mBw, oBw, recordData);

                        if (processBattle)
                        {
                            Battle.BattleResult battleResult = ProcessBattle(battle, recordData.data[recordData.roundNum], recordData.isVsAi);

                            recordData.roundNum++;

                            if (battleResult == Battle.BattleResult.NOT_OVER)
                            {
                                recordData.data.Add(new BattleRecordRoundData());

                                mOver = oOver = false;

                                serverSendDataCallBack(true, true, mMs);

                                serverSendDataCallBack(false, true, oMs);
                            }
                            else
                            {
                                ResetData();

                                serverSendDataCallBack(true, true, mMs);

                                serverSendDataCallBack(false, true, oMs);
                            }

                            serverRoundOverCallBack(battleResult);
                        }
                        else
                        {
                            recordData.roundNum++;

                            if (recordData.roundNum == recordData.maxRoundNum)
                            {
                                ResetData();
                            }
                            else
                            {
                                recordData.data.Add(new BattleRecordRoundData());

                                mOver = oOver = false;
                            }

                            serverSendDataCallBack(true, true, mMs);

                            serverSendDataCallBack(false, true, oMs);
                        }
                    }
                }
            }
        }

        private void ServerQuitBattle(bool _isMine)
        {
            serverSendDataCallBack(_isMine, false, new MemoryStream());

            ServerQuitBattleReal(_isMine);
        }

        public void ServerQuitBattleReal(bool _isMine)
        {
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

            ResetData();

            if (_isMine)
            {
                serverRoundOverCallBack(Battle.BattleResult.O_WIN);
            }
            else
            {
                serverRoundOverCallBack(Battle.BattleResult.M_WIN);
            }
        }

        public void ResetData()
        {
            mOver = oOver = false;

            for (int i = 0; i < BattleConst.DECK_CARD_NUM * 2; i++)
            {
                cardStateArr[i] = CardState.N;
            }

            //save recordData
        }

        public void FromBytes(byte[] _bytes)
        {
            ResetData();

            recordData = ReadRecordDataFromBytes(_bytes);

            InitCardState(cardStateArr, recordData);

            for (int i = 0; i < recordData.roundNum; i++)
            {
                ServerSetCardState(cardStateArr, recordData, i);
            }

            if (processBattle)
            {
                battle.InitBattle(recordData.mapID, recordData.maxRoundNum, recordData.mCards, recordData.oCards, recordData.randomSeed);

                for (int i = 0; i < recordData.roundNum; i++)
                {
                    ProcessBattle(battle, recordData.data[i], recordData.isVsAi);
                }
            }
        }

        public byte[] ToBytes()
        {
            return WriteRecordDataToBytes(recordData);
        }

        public Battle.BattleResult VerifyBattle()
        {
            return VerifyBattle(battle, recordData);
        }





        private static void InitCards(IList<int> _mCards, IList<int> _oCards, out int[] _mCardsResult, out int[] _oCardsResult)
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
                int index = random.Next(mTmpCards.Count);

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
                int index = random.Next(oTmpCards.Count);

                int id = oTmpCards[index];

                oTmpCards.RemoveAt(index);

                _oCardsResult[i] = id;
            }
        }

        private static void InitCardState(CardState[] _cardStateArr, BattleRecordData _recordData)
        {
            for (int i = 0; i < _recordData.mCards.Length && i < BattleConst.DEFAULT_HAND_CARD_NUM; i++)
            {
                _cardStateArr[i] = CardState.M;
            }

            for (int i = 0; i < _recordData.oCards.Length && i < BattleConst.DEFAULT_HAND_CARD_NUM; i++)
            {
                _cardStateArr[BattleConst.DECK_CARD_NUM + i] = CardState.O;
            }
        }

        private static void WriteRoundDataToStream(BinaryWriter _bw, BattleRecordRoundData _data)
        {
            _bw.Write(_data.summon.Count);

            IEnumerator<PlayerAction> enumerator = _data.summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                PlayerAction action = enumerator.Current;

                _bw.Write(action.key);

                _bw.Write(action.value);
            }

            _bw.Write(_data.action.Count);

            enumerator = _data.action.GetEnumerator();

            while (enumerator.MoveNext())
            {
                PlayerAction action = enumerator.Current;

                _bw.Write(action.key);

                _bw.Write(action.value);
            }

            _bw.Write(_data.randomSeed);
        }

        private static void ServerSetCardState(CardState[] _cardStateArr, BattleRecordData _recordData, int _roundNum)
        {
            BattleRecordRoundData data = _recordData.data[_roundNum];

            for (int i = 0; i < data.summon.Count; i++)
            {
                int uid = data.summon[i].key;

                _cardStateArr[uid] = CardState.A;
            }

            for (int i = 0; i < BattleConst.ADD_CARD_NUM; i++)
            {
                int index = BattleConst.DEFAULT_HAND_CARD_NUM + _roundNum * BattleConst.ADD_CARD_NUM + i;

                if (index < _recordData.mCards.Length)
                {
                    int uid = index;

                    _cardStateArr[uid] = CardState.M;
                }

                if (index < _recordData.oCards.Length)
                {
                    int uid = BattleConst.DECK_CARD_NUM + index;

                    _cardStateArr[uid] = CardState.O;
                }
            }
        }

        private static void ServerStartBattleSetRandom(BattleRecordData _recordData, int _roundNum)
        {
            _recordData.data[_roundNum].randomSeed = random.Next();
        }

        private static void ServerStartBattle(BinaryWriter _mBw, BinaryWriter _oBw, BattleRecordData _recordData)
        {
            _mBw.Write(PackageTag.S2C_DOACTION);

            _oBw.Write(PackageTag.S2C_DOACTION);

            BattleRecordRoundData data = _recordData.data[_recordData.roundNum];

            WriteRoundDataToStream(_mBw, data);

            WriteRoundDataToStream(_oBw, data);

            long pos = _mBw.BaseStream.Position;

            _mBw.Write(0);

            _oBw.Write(0);

            int mNum = 0;

            int oNum = 0;

            for (int i = 0; i < data.summon.Count; i++)
            {
                int uid = data.summon[i].key;

                _mBw.Write(uid);

                _oBw.Write(uid);

                if (uid < BattleConst.DECK_CARD_NUM)
                {
                    _mBw.Write(_recordData.mCards[uid]);

                    _oBw.Write(_recordData.mCards[uid]);
                }
                else
                {
                    _mBw.Write(_recordData.oCards[uid - BattleConst.DECK_CARD_NUM]);

                    _oBw.Write(_recordData.oCards[uid - BattleConst.DECK_CARD_NUM]);
                }

                mNum++;

                oNum++;
            }

            for (int i = 0; i < BattleConst.ADD_CARD_NUM; i++)
            {
                int index = BattleConst.DEFAULT_HAND_CARD_NUM + _recordData.roundNum * BattleConst.ADD_CARD_NUM + i;

                if (index < _recordData.mCards.Length)
                {
                    int uid = index;

                    int id = _recordData.mCards[index];

                    _mBw.Write(uid);

                    _mBw.Write(id);

                    mNum++;

                    if (_recordData.isVsAi)
                    {
                        _oBw.Write(uid);

                        _oBw.Write(id);

                        oNum++;
                    }
                }

                if (index < _recordData.oCards.Length)
                {
                    int uid = BattleConst.DECK_CARD_NUM + index;

                    int id = _recordData.oCards[index];

                    _oBw.Write(uid);

                    _oBw.Write(id);

                    oNum++;

                    if (_recordData.isVsAi)
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

        private static Battle.BattleResult ProcessBattle(Battle _battle, BattleRecordRoundData _data, bool _isVsAi)
        {
            IEnumerator<PlayerAction> enumerator2 = _data.summon.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                bool b = _battle.AddSummon(enumerator2.Current.isMine, enumerator2.Current.key, enumerator2.Current.value);

                if (!b)
                {
                    throw new Exception("summon error!");
                }
            }

            enumerator2 = _data.action.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                bool b = _battle.AddAction(enumerator2.Current.isMine, enumerator2.Current.key, enumerator2.Current.value);

                if (!b)
                {
                    throw new Exception("action error!");
                }
            }

            _battle.SetRandomSeed(_data.randomSeed);

            if (_isVsAi)
            {
                BattleAi.Start(_battle, false, _battle.GetRandomValue);
            }

            SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(_battle.StartBattle());

            superEnumerator.Done();

            return (Battle.BattleResult)superEnumerator.Current;
        }

        private static Battle.BattleResult VerifyBattle(Battle _battle, BattleRecordData _recordData)
        {
            Battle.BattleResult battleResult = Battle.BattleResult.NOT_OVER;

            _battle.InitBattle(_recordData.mapID, _recordData.maxRoundNum, _recordData.mCards, _recordData.oCards, _recordData.randomSeed);

            for (int i = 0; i < _recordData.roundNum; i++)
            {
                battleResult = ProcessBattle(_battle, _recordData.data[i], _recordData.isVsAi);

                if (battleResult != Battle.BattleResult.NOT_OVER)
                {
                    break;
                }
            }

            return battleResult;
        }

        private static byte[] WriteRecordDataToBytes(BattleRecordData _recordData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(_recordData.isVsAi);

                    bw.Write(_recordData.mapID);

                    bw.Write(_recordData.maxRoundNum);

                    bw.Write(_recordData.roundNum);

                    bw.Write(_recordData.randomSeed);

                    for (int i = 0; i < _recordData.roundNum; i++)
                    {
                        BattleRecordRoundData data = _recordData.data[i];

                        bw.Write(data.action.Count);

                        IEnumerator<PlayerAction> enumerator = data.action.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            bw.Write(enumerator.Current.isMine);

                            bw.Write(enumerator.Current.key);

                            bw.Write(enumerator.Current.value);
                        }

                        bw.Write(data.summon.Count);

                        enumerator = data.summon.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            bw.Write(enumerator.Current.isMine);

                            bw.Write(enumerator.Current.key);

                            bw.Write(enumerator.Current.value);
                        }

                        bw.Write(data.randomSeed);
                    }

                    bw.Write(_recordData.mCards.Length);

                    for (int i = 0; i < _recordData.mCards.Length; i++)
                    {
                        bw.Write(_recordData.mCards[i]);
                    }

                    bw.Write(_recordData.oCards.Length);

                    for (int i = 0; i < _recordData.oCards.Length; i++)
                    {
                        bw.Write(_recordData.oCards[i]);
                    }

                    return ms.ToArray();
                }
            }
        }

        private static BattleRecordData ReadRecordDataFromBytes(byte[] _bytes)
        {
            BattleRecordData recordData = new BattleRecordData();

            using (MemoryStream ms = new MemoryStream(_bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    recordData.isVsAi = br.ReadBoolean();

                    recordData.mapID = br.ReadInt32();

                    recordData.maxRoundNum = br.ReadInt32();

                    recordData.data.Add(new BattleRecordRoundData());

                    recordData.roundNum = br.ReadInt32();

                    recordData.randomSeed = br.ReadInt32();

                    for (int i = 0; i < recordData.roundNum; i++)
                    {
                        recordData.data.Add(new BattleRecordRoundData());

                        BattleRecordRoundData data = recordData.data[i];

                        int num = br.ReadInt32();

                        for (int m = 0; m < num; m++)
                        {
                            bool isMine = br.ReadBoolean();

                            int key = br.ReadInt32();

                            int value = br.ReadInt32();

                            data.action.Add(new PlayerAction(isMine, key, value));
                        }

                        num = br.ReadInt32();

                        for (int m = 0; m < num; m++)
                        {
                            bool isMine = br.ReadBoolean();

                            int key = br.ReadInt32();

                            int value = br.ReadInt32();

                            data.summon.Add(new PlayerAction(isMine, key, value));
                        }

                        data.randomSeed = br.ReadInt32();
                    }

                    int num2 = br.ReadInt32();

                    recordData.mCards = new int[num2];

                    for (int i = 0; i < num2; i++)
                    {
                        recordData.mCards[i] = br.ReadInt32();
                    }

                    num2 = br.ReadInt32();

                    recordData.oCards = new int[num2];

                    for (int i = 0; i < num2; i++)
                    {
                        recordData.oCards[i] = br.ReadInt32();
                    }

                    return recordData;
                }
            }
        }
    }
}
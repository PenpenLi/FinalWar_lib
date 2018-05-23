using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;
using System.Collections;
using tuple;

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

        private class BattleRecordData
        {
            public int roundNum;

            public List<BattleRecordRoundData> data = new List<BattleRecordRoundData>();

            public int battleInitDataID;

            public int[] mCards;

            public int[] oCards;

            public bool isVsAi;
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

        private Action<bool, bool, MemoryStream> serverSendDataCallBack;

        private Action<Battle.BattleResult> serverRoundOverCallBack;

        private bool processBattle;

        private BattleRecordData recordData;

        public Battle_server(bool _processBattle)
        {
            processBattle = _processBattle;

            battle = new Battle();
        }

        public void ServerSetCallBack(Action<bool, bool, MemoryStream> _serverSendDataCallBack, Action<Battle.BattleResult> _serverRoundOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;

            serverRoundOverCallBack = _serverRoundOverCallBack;
        }

        public void ServerStart(int _battleInitDataID, IList<int> _mCards, IList<int> _oCards, bool _isVsAi)
        {
            Log.Write("Battle Start!");

            recordData = new BattleRecordData();

            recordData.data.Add(new BattleRecordRoundData());

            recordData.battleInitDataID = _battleInitDataID;

            recordData.isVsAi = _isVsAi;

            InitCards(recordData, _mCards, _oCards);

            if (processBattle || recordData.isVsAi)
            {
                battle.InitBattle(recordData.battleInitDataID, recordData.mCards, recordData.oCards);
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
                    IBattleInitDataSDS battleInitData = Battle.GetBattleInitData(recordData.battleInitDataID);

                    Log.Write("ServerRefreshData  isMine:" + _isMine);

                    bool isOver;

                    int[] cards;

                    IPlayerInitDataSDS playerInitData;

                    if (_isMine)
                    {
                        isOver = mOver;

                        cards = recordData.mCards;

                        playerInitData = battleInitData.GetMPlayerInitData();
                    }
                    else
                    {
                        isOver = oOver;

                        cards = recordData.oCards;

                        playerInitData = battleInitData.GetOPlayerInitData();
                    }

                    bw.Write(_isMine);

                    bw.Write(processBattle || recordData.isVsAi);

                    bw.Write(recordData.battleInitDataID);

                    bw.Write(recordData.mCards.Length);

                    bw.Write(recordData.oCards.Length);

                    List<int> mList = new List<int>();

                    List<int> oList = new List<int>();

                    int cardIndex = 0;

                    for (int i = 0; i < playerInitData.GetDefaultHandCardsNum(); i++)
                    {
                        if (cardIndex < cards.Length)
                        {
                            if (_isMine)
                            {
                                mList.Add(cardIndex);
                            }
                            else
                            {
                                oList.Add(cardIndex);
                            }

                            cardIndex++;
                        }
                    }

                    for (int i = 0; i < recordData.roundNum; i++)
                    {
                        BattleRecordRoundData roundData = recordData.data[i];

                        for (int m = 0; m < roundData.summon.Count; m++)
                        {
                            PlayerAction summon = roundData.summon[m];

                            if (summon.isMine != _isMine)
                            {
                                if (_isMine)
                                {
                                    oList.Add(summon.value);
                                }
                                else
                                {
                                    mList.Add(summon.value);
                                }
                            }
                            else
                            {
                                if (cardIndex < cards.Length)
                                {
                                    if (_isMine)
                                    {
                                        mList.Add(cardIndex);
                                    }
                                    else
                                    {
                                        oList.Add(cardIndex);
                                    }

                                    cardIndex++;
                                }
                            }
                        }
                    }

                    bw.Write(mList.Count);

                    for (int i = 0; i < mList.Count; i++)
                    {
                        int index = mList[i];

                        bw.Write(index);

                        bw.Write(recordData.mCards[index]);
                    }

                    bw.Write(oList.Count);

                    for (int i = 0; i < oList.Count; i++)
                    {
                        int index = oList[i];

                        bw.Write(index);

                        bw.Write(recordData.oCards[index]);
                    }

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
                    int pos = _br.ReadInt32();

                    int uid = _br.ReadInt32();

                    data.summon.Add(new PlayerAction(_isMine, pos, uid));
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
                        ServerStartBattleSetRandom(recordData, recordData.roundNum);

                        if (processBattle || recordData.isVsAi)
                        {
                            BattleRecordRoundData data = recordData.data[recordData.roundNum];

                            if (recordData.isVsAi)
                            {
                                Dictionary<int, int> action = new Dictionary<int, int>();

                                Dictionary<int, int> summon = new Dictionary<int, int>();

                                BattleAi.Start(battle, false, random.Next, action, summon);

                                IEnumerator<KeyValuePair<int, int>> enumerator = action.GetEnumerator();

                                while (enumerator.MoveNext())
                                {
                                    data.action.Add(new PlayerAction(false, enumerator.Current.Key, enumerator.Current.Value));
                                }

                                enumerator = summon.GetEnumerator();

                                while (enumerator.MoveNext())
                                {
                                    data.summon.Add(new PlayerAction(false, enumerator.Current.Key, enumerator.Current.Value));
                                }
                            }

                            ServerStartBattle(mBw, oBw, recordData, recordData.roundNum);

                            Battle.BattleResult battleResult = ProcessBattle(battle, data);

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
                            ServerStartBattle(mBw, oBw, recordData, recordData.roundNum);

                            recordData.roundNum++;

                            IBattleInitDataSDS battleInitData = Battle.GetBattleInitData(recordData.battleInitDataID);

                            if (recordData.roundNum == battleInitData.GetMaxRoundNum())
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

            //save recordData
        }

        public void FromBytes(byte[] _bytes)
        {
            ResetData();

            recordData = ReadRecordDataFromBytes(_bytes);

            if (processBattle || recordData.isVsAi)
            {
                battle.InitBattle(recordData.battleInitDataID, recordData.mCards, recordData.oCards);

                for (int i = 0; i < recordData.roundNum; i++)
                {
                    ProcessBattle(battle, recordData.data[i]);
                }
            }
        }

        public IEnumerator FromBytesAndReplay(byte[] _bytes)
        {
            ResetData();

            recordData = ReadRecordDataFromBytes(_bytes);

            int roundNum = recordData.roundNum;

            recordData.roundNum = 0;

            return Replay(roundNum);
        }

        private IEnumerator Replay(int _roundNum)
        {
            for (int i = 0; i < _roundNum; i++)
            {
                using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
                {
                    using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                    {
                        BattleRecordRoundData data = recordData.data[i];

                        ServerStartBattle(mBw, oBw, recordData, i);

                        recordData.roundNum++;

                        serverSendDataCallBack(true, true, mMs);

                        serverSendDataCallBack(false, true, oMs);

                        yield return null;
                    }
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





        private static void InitCards(BattleRecordData _recordData, IList<int> _mCards, IList<int> _oCards)
        {
            IBattleInitDataSDS battleInitData = Battle.GetBattleInitData(_recordData.battleInitDataID);

            _recordData.mCards = new int[Math.Min(_mCards.Count, battleInitData.GetMPlayerInitData().GetDeckCardsNum())];

            int[] tmpArr = new int[_mCards.Count];

            for (int i = 0; i < _mCards.Count; i++)
            {
                tmpArr[i] = _mCards[i];
            }

            BattlePublicTools.Shuffle(tmpArr, random.Next);

            Array.Copy(tmpArr, _recordData.mCards, _recordData.mCards.Length);

            _recordData.oCards = new int[Math.Min(_oCards.Count, battleInitData.GetOPlayerInitData().GetDeckCardsNum())];

            tmpArr = new int[_oCards.Count];

            for (int i = 0; i < _oCards.Count; i++)
            {
                tmpArr[i] = _oCards[i];
            }

            BattlePublicTools.Shuffle(tmpArr, random.Next);

            Array.Copy(tmpArr, _recordData.oCards, _recordData.oCards.Length);
        }

        private static void WriteRoundDataToStream(BinaryWriter _bw, BattleRecordRoundData _data)
        {
            _bw.Write(_data.summon.Count);

            IEnumerator<PlayerAction> enumerator = _data.summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                PlayerAction action = enumerator.Current;

                _bw.Write(action.isMine);

                _bw.Write(action.key);

                _bw.Write(action.value);
            }

            _bw.Write(_data.action.Count);

            enumerator = _data.action.GetEnumerator();

            while (enumerator.MoveNext())
            {
                PlayerAction action = enumerator.Current;

                _bw.Write(action.isMine);

                _bw.Write(action.key);

                _bw.Write(action.value);
            }

            _bw.Write(_data.randomSeed);
        }

        private static void ServerStartBattleSetRandom(BattleRecordData _recordData, int _roundNum)
        {
            _recordData.data[_roundNum].randomSeed = random.Next();
        }

        private static void ServerStartBattle(BinaryWriter _mBw, BinaryWriter _oBw, BattleRecordData _recordData, int _roundNum)
        {
            IBattleInitDataSDS battleInitData = Battle.GetBattleInitData(_recordData.battleInitDataID);

            int mCardIndex = Math.Min(battleInitData.GetMPlayerInitData().GetDefaultHandCardsNum(), _recordData.mCards.Length);

            int oCardIndex = Math.Min(battleInitData.GetOPlayerInitData().GetDefaultHandCardsNum(), _recordData.oCards.Length);

            for (int i = 0; i < _roundNum; i++)
            {
                List<PlayerAction> summon = _recordData.data[i].summon;

                for (int m = 0; m < summon.Count; m++)
                {
                    PlayerAction playerAction = summon[m];

                    if (playerAction.isMine)
                    {
                        if (mCardIndex < _recordData.mCards.Length)
                        {
                            mCardIndex++;
                        }
                    }
                    else
                    {
                        if (oCardIndex < _recordData.oCards.Length)
                        {
                            oCardIndex++;
                        }
                    }
                }
            }

            _mBw.Write(PackageTag.S2C_DOACTION);

            _oBw.Write(PackageTag.S2C_DOACTION);

            BattleRecordRoundData data = _recordData.data[_roundNum];

            List<Tuple<bool, int, int>> mList = null;

            List<Tuple<bool, int, int>> oList = null;

            for (int i = 0; i < data.summon.Count; i++)
            {
                PlayerAction action = data.summon[i];

                int uid = action.value;

                if (action.isMine)
                {
                    if (oList == null)
                    {
                        oList = new List<Tuple<bool, int, int>>();
                    }

                    oList.Add(new Tuple<bool, int, int>(true, uid, _recordData.mCards[uid]));

                    if (mCardIndex < _recordData.mCards.Length)
                    {
                        if (mList == null)
                        {
                            mList = new List<Tuple<bool, int, int>>();
                        }

                        mList.Add(new Tuple<bool, int, int>(true, mCardIndex, _recordData.mCards[mCardIndex]));

                        mCardIndex++;
                    }
                }
                else
                {
                    if (mList == null)
                    {
                        mList = new List<Tuple<bool, int, int>>();
                    }

                    mList.Add(new Tuple<bool, int, int>(false, uid, _recordData.oCards[uid]));

                    if (oCardIndex < _recordData.oCards.Length)
                    {
                        if (oList == null)
                        {
                            oList = new List<Tuple<bool, int, int>>();
                        }

                        oList.Add(new Tuple<bool, int, int>(false, oCardIndex, _recordData.oCards[oCardIndex]));

                        oCardIndex++;
                    }
                }
            }

            if (mList != null)
            {
                _mBw.Write(mList.Count);

                for (int i = 0; i < mList.Count; i++)
                {
                    Tuple<bool, int, int> t = mList[i];

                    _mBw.Write(t.first);

                    _mBw.Write(t.second);

                    _mBw.Write(t.third);
                }
            }
            else
            {
                _mBw.Write(0);
            }

            if (oList != null)
            {
                _oBw.Write(oList.Count);

                for (int i = 0; i < oList.Count; i++)
                {
                    Tuple<bool, int, int> t = oList[i];

                    _oBw.Write(t.first);

                    _oBw.Write(t.second);

                    _oBw.Write(t.third);
                }
            }
            else
            {
                _oBw.Write(0);
            }

            WriteRoundDataToStream(_mBw, data);

            WriteRoundDataToStream(_oBw, data);
        }

        private static Battle.BattleResult ProcessBattle(Battle _battle, BattleRecordRoundData _data)
        {
            IEnumerator<PlayerAction> enumerator2 = _data.action.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                int result = _battle.AddAction(enumerator2.Current.isMine, enumerator2.Current.key, enumerator2.Current.value);

                if (result != -1)
                {
                    throw new Exception("action error:" + result);
                }
            }

            enumerator2 = _data.summon.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                int result = _battle.AddSummon(enumerator2.Current.isMine, enumerator2.Current.key, enumerator2.Current.value);

                if (result != -1)
                {
                    throw new Exception("summon error:" + result);
                }
            }

            _battle.SetRandomSeed(_data.randomSeed);

            SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(_battle.StartBattle());

            superEnumerator.Done();

            return (Battle.BattleResult)superEnumerator.Current;
        }

        private static Battle.BattleResult VerifyBattle(Battle _battle, BattleRecordData _recordData)
        {
            Battle.BattleResult battleResult = Battle.BattleResult.NOT_OVER;

            _battle.InitBattle(_recordData.battleInitDataID, _recordData.mCards, _recordData.oCards);

            for (int i = 0; i < _recordData.roundNum; i++)
            {
                battleResult = ProcessBattle(_battle, _recordData.data[i]);

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

                    bw.Write(_recordData.battleInitDataID);

                    bw.Write(_recordData.roundNum);

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

                    recordData.battleInitDataID = br.ReadInt32();

                    recordData.data.Add(new BattleRecordRoundData());

                    recordData.roundNum = br.ReadInt32();

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
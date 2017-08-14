#if !CLIENT
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

        private Battle battle = new Battle();

        private bool mOver;
        private bool oOver;

        private int roundNum;

        //-----------------
        private int mapID;

        private Dictionary<int, int>[] summon = new Dictionary<int, int>[BattleConst.MAX_ROUND_NUM];

        private Dictionary<int, int>[] action = new Dictionary<int, int>[BattleConst.MAX_ROUND_NUM];

        private int[] randomIndexList = new int[BattleConst.MAX_ROUND_NUM];

        private int[] mCards;

        private int[] oCards;
        //-----------------

        private CardState[] cardStateArr = new CardState[BattleConst.DECK_CARD_NUM * 2];

        private Action<bool, MemoryStream> serverSendDataCallBack;

        private Action serverBattleOverCallBack;

        public Battle_server()
        {
            for (int i = 0; i < BattleConst.MAX_ROUND_NUM; i++)
            {
                summon[i] = new Dictionary<int, int>();

                action[i] = new Dictionary<int, int>();
            }
        }

        public void ServerSetCallBack(Action<bool, MemoryStream> _serverSendDataCallBack, Action _serverBattleOverCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;

            serverBattleOverCallBack = _serverBattleOverCallBack;
        }

        public void ServerStart(int _mapID, List<int> _mCards, List<int> _oCards)
        {
            Log.Write("Battle Start!");

            mapID = _mapID;

            InitCards(_mCards, _oCards, out mCards, out oCards);

            if (battle != null)
            {
                battle.InitBattle(_mapID, mCards, oCards);
            }

            ServerRefreshData(true);

            ServerRefreshData(false);
        }

        private void InitCards(List<int> _mCards, List<int> _oCards, out int[] _mCardsResult, out int[] _oCardsResult)
        {
            List<int> mCards = new List<int>(_mCards);

            if (mCards.Count > BattleConst.DECK_CARD_NUM)
            {
                _mCardsResult = new int[BattleConst.DECK_CARD_NUM];
            }
            else
            {
                _mCardsResult = new int[mCards.Count];
            }

            for (int i = 0; i < _mCardsResult.Length; i++)
            {
                int index = GetRandomValue(mCards.Count);

                int id = mCards[index];

                mCards.RemoveAt(index);

                _mCardsResult[i] = id;

                if (i < BattleConst.DEFAULT_HAND_CARD_NUM)
                {
                    cardStateArr[i] = CardState.M;
                }
                else
                {
                    cardStateArr[i] = CardState.N;
                }
            }

            List<int> oCards = new List<int>(_oCards);

            if (oCards.Count > BattleConst.DECK_CARD_NUM)
            {
                _oCardsResult = new int[BattleConst.DECK_CARD_NUM];
            }
            else
            {
                _oCardsResult = new int[oCards.Count];
            }

            for (int i = 0; i < _oCardsResult.Length; i++)
            {
                int index = GetRandomValue(oCards.Count);

                int id = oCards[index];

                oCards.RemoveAt(index);

                _oCardsResult[i] = id;

                if (i < BattleConst.DEFAULT_HAND_CARD_NUM)
                {
                    cardStateArr[BattleConst.DECK_CARD_NUM + i] = CardState.O;
                }
                else
                {
                    cardStateArr[BattleConst.DECK_CARD_NUM + i] = CardState.N;
                }
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

                    bw.Write(PackageTag.S2C_REFRESH);

                    bw.Write(_isMine);

                    bw.Write(mapID);

                    bw.Write(mCards.Length);

                    bw.Write(oCards.Length);

                    long pos = bw.BaseStream.Position;

                    bw.Write(0);

                    int num = 0;

                    for (int i = 0; i < BattleConst.DECK_CARD_NUM; i++)
                    {
                        int index = i;

                        CardState cardState = cardStateArr[index];

                        if (cardState == CardState.A || cardState == tmpCardState)
                        {
                            bw.Write(index);

                            bw.Write(mCards[i]);

                            num++;
                        }

                        index = BattleConst.DECK_CARD_NUM + i;

                        cardState = cardStateArr[index];

                        if (cardState == CardState.A || cardState == tmpCardState)
                        {
                            bw.Write(index);

                            bw.Write(oCards[i]);

                            num++;
                        }
                    }

                    long pos2 = bw.BaseStream.Position;

                    bw.BaseStream.Position = pos;

                    bw.Write(num);

                    bw.BaseStream.Position = pos2;

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
                }
            }
        }

        private void WriteRoundDataToStream(BinaryWriter _bw, int _roundNum)
        {
            Dictionary<int, int> tmpDic = summon[_roundNum];

            _bw.Write(tmpDic.Count);

            Dictionary<int, int>.Enumerator enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                _bw.Write(pair.Key);

                _bw.Write(pair.Value);
            }

            tmpDic = action[_roundNum];

            _bw.Write(tmpDic.Count);

            enumerator = tmpDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                _bw.Write(pair.Key);

                _bw.Write(pair.Value);
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

            Dictionary<int, int> tmpDic = summon[roundNum];

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                tmpDic.Add(uid, pos);
            }

            tmpDic = action[roundNum];

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                tmpDic.Add(pos, targetPos);
            }

            if (mOver && oOver)
            {
                ServerStartBattle();
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(PackageTag.S2C_WAIT);

                        serverSendDataCallBack(_isMine, ms);
                    }
                }
            }
        }

        private void ServerStartBattle()
        {
            int randomIndex = GetRandomValue(Battle.randomPool.Length);

            randomIndexList[roundNum] = randomIndex;

            using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
            {
                using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                {
                    mBw.Write(PackageTag.S2C_DOACTION);

                    oBw.Write(PackageTag.S2C_DOACTION);

                    WriteRoundDataToStream(mBw, roundNum);

                    WriteRoundDataToStream(oBw, roundNum);

                    Dictionary<int, int> tmpDic = summon[roundNum];

                    Dictionary<int, int>.Enumerator enumerator = tmpDic.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        cardStateArr[pair.Key] = CardState.A;

                        if (pair.Key < BattleConst.DECK_CARD_NUM)
                        {
                            mBw.Write(mCards[pair.Key]);

                            oBw.Write(mCards[pair.Key]);
                        }
                        else
                        {
                            mBw.Write(oCards[pair.Key - BattleConst.DECK_CARD_NUM]);

                            oBw.Write(oCards[pair.Key - BattleConst.DECK_CARD_NUM]);
                        }
                    }

                    for (int i = 0; i < BattleConst.ADD_CARD_NUM; i++)
                    {
                        int index = BattleConst.DEFAULT_HAND_CARD_NUM + roundNum * BattleConst.ADD_CARD_NUM + i;

                        cardStateArr[index] = CardState.M;

                        mBw.Write(mCards[index]);

                        cardStateArr[BattleConst.DECK_CARD_NUM + index] = CardState.O;

                        oBw.Write(oCards[index]);
                    }

                    serverSendDataCallBack(true, mMs);

                    serverSendDataCallBack(false, oMs);
                }
            }

            if (battle != null)
            {
                Dictionary<int, int>.Enumerator enumerator2 = summon[roundNum].GetEnumerator();

                Dictionary<int, int> tmpDic = battle.GetSummon();

                while (enumerator2.MoveNext())
                {
                    tmpDic.Add(enumerator2.Current.Key, enumerator2.Current.Value);
                }

                enumerator2 = action[roundNum].GetEnumerator();

                tmpDic = battle.GetAction();

                while (enumerator2.MoveNext())
                {
                    tmpDic.Add(enumerator2.Current.Key, enumerator2.Current.Value);
                }

                battle.SetRandomIndex(randomIndex);

                SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(battle.StartBattle());

                superEnumerator.Done();
            }

            roundNum++;

            mOver = oOver = false;
        }

        private void ServerQuitBattle()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.S2C_QUIT);

                    serverSendDataCallBack(true, ms);

                    serverSendDataCallBack(false, ms);
                }
            }

            if (battle != null)
            {
                battle.BattleOver();
            }

            BattleOver();
        }

        private void BattleOver()
        {
            mOver = oOver = false;

            roundNum = 0;

            for (int i = 0; i < BattleConst.DECK_CARD_NUM * 2; i++)
            {
                cardStateArr[i] = CardState.N;
            }

            serverBattleOverCallBack();
        }
    }
}
#endif

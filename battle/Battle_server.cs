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

        private Battle battle = new Battle();

        private bool mOver;
        private bool oOver;

        private int roundNum;



        private CardState[] cardStateArr = new CardState[BattleConst.DECK_CARD_NUM * 2];

        private int[] cardIDArr = new int[BattleConst.DECK_CARD_NUM * 2];

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

            mOver = oOver = false;

            battle.InitBattle(_mapID, _heros, _mCards, _oCards);

            ServerRefreshData(true);

            ServerRefreshData(false);
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

                    bw.Write(battle.mapID);

                    bw.Write(roundNum);

                    for (int i = 0; i < roundNum; i++)
                    {
                        WriteRoundDataToStream(bw, i);
                    }

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

                    bw.Write(isOver);

                    if (isOver)
                    {
                        WriteRoundDataToStream(bw, roundNum);
                    }

                    long pos = bw.BaseStream.Position;

                    bw.Write(0);

                    int num = 0;

                    for (int i = 0; i < BattleConst.DECK_CARD_NUM * 2; i++)
                    {
                        CardState cardState = cardStateArr[i];

                        if (cardState == CardState.A || cardState == tmpCardState)
                        {
                            bw.Write(i);

                            bw.Write(cardIDArr[i]);

                            num++;
                        }
                    }

                    bw.BaseStream.Position = pos;

                    bw.Write(num);
                }
            }
        }

        private void WriteRoundDataToStream(BinaryWriter _bw, int _roundNum)
        {
            Dictionary<int, int> summonDic = battle.GetSummon(_roundNum);

            _bw.Write(summonDic.Count);

            Dictionary<int, int>.Enumerator enumerator = summonDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                _bw.Write(pair.Key);

                _bw.Write(pair.Value);
            }

            List<KeyValuePair<int, int>> actionList = battle.GetAction(_roundNum);

            _bw.Write(actionList.Count);

            for (int m = 0; m < actionList.Count; m++)
            {
                KeyValuePair<int, int> pair = actionList[m];

                _bw.Write(pair.Key);

                _bw.Write(pair.Value);
            }

            _bw.Write(battle.GetRandomIndex(_roundNum));
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

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                Dictionary<int, int> tmpSummonDic = battle.GetSummon(roundNum);

                tmpSummonDic.Add(uid, pos);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                List<KeyValuePair<int, int>> tmpActionList = battle.GetAction(roundNum);

                tmpActionList.Add(new KeyValuePair<int, int>(pos, targetPos));
            }

            if (mOver && oOver)
            {
                ServerStartBattle();
            }
        }

        private void ServerStartBattle()
        {
            Dictionary<int, int> tmpDic = battle.GetSummon(roundNum);

            Dictionary<int, int>.KeyCollection.Enumerator enumerator = tmpDic.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int uid = enumerator.Current;

                cardStateArr[uid] = CardState.A;
            }



            using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
            {
                using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                {
                    WriteRoundDataToStream(mBw, roundNum);

                    WriteRoundDataToStream(oBw, roundNum);

                    serverSendDataCallBack(true, mMs);

                    serverSendDataCallBack(false, oMs);
                }
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

                    serverSendDataCallBack(false, ms);
                }
            }

            BattleOver();
        }
    }
}
#endif

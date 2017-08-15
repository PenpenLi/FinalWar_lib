#if CLIENT
using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;

namespace FinalWar
{
    public class Battle_client
    {
        private Battle battle = new Battle();

        public bool clientIsMine { get; private set; }

        private bool clientIsOver;

        private Action<MemoryStream> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<SuperEnumerator<ValueType>> clientDoActionCallBack;
        private Action clientBattleOverCallBack;

        public void ClientSetCallBack(Action<MemoryStream> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<SuperEnumerator<ValueType>> _clientDoActionCallBack, Action _clientBattleOverCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
            clientBattleOverCallBack = _clientBattleOverCallBack;
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

                case PackageTag.S2C_WAIT:

                    clientIsOver = true;

                    break;
            }
        }

        private void ClientRefreshData(BinaryReader _br)
        {
            battle.BattleOver();

            clientIsMine = _br.ReadBoolean();

            Log.Write("ClientRefreshData  isMine:" + clientIsMine);

            int mapID = _br.ReadInt32();

            int num = _br.ReadInt32();

            int[] mCards = new int[num];

            num = _br.ReadInt32();

            int[] oCards = new int[num];

            battle.InitBattle(mapID, mCards, oCards);

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                battle.SetCard(uid, id);
            }

            int roundNum = _br.ReadInt32();

            for (int i = 0; i < roundNum; i++)
            {
                ReadRoundDataFromStream(_br);

                SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(battle.StartBattle());

                superEnumerator.Done();
            }

            clientIsOver = _br.ReadBoolean();

            if (clientIsOver)
            {
                ReadRoundDataFromStream(_br);
            }

            clientRefreshDataCallBack();
        }

        private void ReadRoundDataFromStream(BinaryReader _br)
        {
            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                battle.AddSummon(clientIsMine, uid, pos);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                battle.AddAction(clientIsMine, pos, targetPos);
            }

            int randomIndex = _br.ReadInt32();

            battle.SetRandomIndex(randomIndex);
        }

        public bool ClientRequestSummon(int _cardUid, int _pos)
        {
            return battle.AddSummon(clientIsMine, _cardUid, _pos);
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            battle.DelSummon(_cardUid);
        }

        public int ClientGetMoney()
        {
            return battle.GetNowMoney(clientIsMine);
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
            return battle.AddAction(clientIsMine, _pos, _targetPos);
        }

        public void ClientRequestUnaction(int _pos)
        {
            battle.DelAction(_pos);
        }

        public void ClientRequestDoAction()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_DOACTION);

                    bw.Write(battle.GetSummonNum());

                    Dictionary<int, int>.Enumerator enumerator = battle.GetSummonEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(battle.GetActionNum());

                    enumerator = battle.GetActionEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
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

        private void ClientDoAction(BinaryReader _br)
        {
            ReadRoundDataFromStream(_br);

            int num = battle.GetSummonNum();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                battle.SetCard(uid, id);
            }

            for (int i = 0; i < BattleConst.ADD_CARD_NUM; i++)
            {
                int index = BattleConst.DEFAULT_HAND_CARD_NUM + battle.roundNum * BattleConst.ADD_CARD_NUM + i;

                if (!clientIsMine)
                {
                    index += BattleConst.DECK_CARD_NUM;
                }

                int id = _br.ReadInt32();

                battle.SetCard(index, id);
            }

            clientDoActionCallBack(new SuperEnumerator<ValueType>(battle.StartBattle()));
        }

        public void ClientEndBattle()
        {
            //EndBattle();
        }

        public bool GetClientCanAction()
        {
            return !clientIsOver;
        }
    }
}
#endif

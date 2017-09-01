using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;

namespace FinalWar
{
    public class Battle_client : Battle
    {
        public bool clientIsMine { get; private set; }

        private bool clientIsOver;

        private Action<MemoryStream, Action<BinaryReader>> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<SuperEnumerator<ValueType>> clientDoActionCallBack;
        private Action<BattleResult> clientBattleOverCallBack;

        private bool isVsAi;

        public void ClientSetCallBack(Action<MemoryStream, Action<BinaryReader>> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<SuperEnumerator<ValueType>> _clientDoActionCallBack, Action<BattleResult> _clientBattleOverCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
            clientBattleOverCallBack = _clientBattleOverCallBack;

            InitBattleEndCallBack(clientBattleOverCallBack);
        }

        public void ClientGetPackage(BinaryReader _br)
        {
            byte tag = _br.ReadByte();

            switch (tag)
            {
                case PackageTag.S2C_DOACTION:

                    ClientDoAction(_br);

                    break;

                case PackageTag.S2C_QUIT:

                    BattleOver();

                    clientBattleOverCallBack(BattleResult.QUIT);

                    break;
            }
        }

        private void ClientRefreshData(BinaryReader _br)
        {
            BattleOver();

            isVsAi = _br.ReadBoolean();

            clientIsMine = _br.ReadBoolean();

            Log.Write("ClientRefreshData  isMine:" + clientIsMine);

            int mapID = _br.ReadInt32();

            int num = _br.ReadInt32();

            int[] mCards = new int[num];

            num = _br.ReadInt32();

            int[] oCards = new int[num];

            InitBattle(mapID, mCards, oCards);

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                SetCard(uid, id);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                ReadRoundDataFromStream(_br);

                if (isVsAi)
                {
                    BattleAi.Start(this, false, GetRandomValue);
                }

                SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(StartBattle());

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

                AddSummon(uid, pos);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                AddAction(pos, targetPos);
            }

            int randomIndex = _br.ReadInt32();

            SetRandomIndex(randomIndex);
        }

        public bool ClientRequestSummon(int _cardUid, int _pos)
        {
            return AddSummon(clientIsMine, _cardUid, _pos);
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            DelSummon(_cardUid);
        }

        public int ClientGetMoney()
        {
            return GetNowMoney(clientIsMine);
        }

        public void ClientRequestQuitBattle()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((short)PackageTag.C2S_QUIT);

                    clientSendDataCallBack(ms, GetResponse);
                }
            }
        }

        public bool ClientRequestAction(int _pos, int _targetPos)
        {
            return AddAction(clientIsMine, _pos, _targetPos);
        }

        public void ClientRequestUnaction(int _pos)
        {
            DelAction(_pos);
        }

        public void ClientRequestDoAction()
        {
            clientIsOver = true;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_DOACTION);

                    bw.Write(GetSummonNum());

                    Dictionary<int, int>.Enumerator enumerator = GetSummonEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(GetActionNum());

                    enumerator = GetActionEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    clientSendDataCallBack(ms, GetResponse);
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

                    clientSendDataCallBack(ms, ClientRefreshData);
                }
            }
        }

        private void ClientDoAction(BinaryReader _br)
        {
            ClearSummon();

            ClearAction();

            ReadRoundDataFromStream(_br);

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                SetCard(uid, id);
            }

            if (isVsAi)
            {
                BattleAi.Start(this, false, GetRandomValue);
            }

            clientDoActionCallBack(new SuperEnumerator<ValueType>(StartBattle()));

            clientIsOver = false;
        }

        public bool GetClientCanAction()
        {
            return !clientIsOver;
        }

        private void GetResponse(BinaryReader _br)
        {

        }
    }
}
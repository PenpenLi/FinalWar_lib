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

        private bool serverProcessBattle;

        private Battle simulateBattle = new Battle();

        public void ClientSetCallBack(Action<MemoryStream, Action<BinaryReader>> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<SuperEnumerator<ValueType>> _clientDoActionCallBack, Action<BattleResult> _clientBattleOverCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
            clientBattleOverCallBack = _clientBattleOverCallBack;
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

                    BattleResult battleResult = _br.ReadBoolean() ? BattleResult.O_WIN : BattleResult.M_WIN;

                    clientBattleOverCallBack(battleResult);

                    break;
            }
        }

        private void ClientRefreshData(BinaryReader _br)
        {
            clientIsMine = _br.ReadBoolean();

            Log.Write("ClientRefreshData  isMine:" + clientIsMine);

            serverProcessBattle = _br.ReadBoolean();

            int battleInitDataID = _br.ReadInt32();

            int num = _br.ReadInt32();

            int[] mCards = new int[num];

            num = _br.ReadInt32();

            int[] oCards = new int[num];

            InitBattle(battleInitDataID, mCards, oCards);

            if (!serverProcessBattle)
            {
                simulateBattle.InitBattle(battleInitDataID, mCards, oCards);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                SetCard(true, uid, id);

                if (!serverProcessBattle)
                {
                    simulateBattle.SetCard(true, uid, id);
                }
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                SetCard(false, uid, id);

                if (!serverProcessBattle)
                {
                    simulateBattle.SetCard(false, uid, id);
                }
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                long pos = _br.BaseStream.Position;

                ReadRoundDataFromStream(_br, this);

                if (!serverProcessBattle)
                {
                    _br.BaseStream.Position = pos;

                    ReadRoundDataFromStream(_br, simulateBattle);
                }

                SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(StartBattle());

                superEnumerator.Done();

                if (!serverProcessBattle)
                {
                    superEnumerator = new SuperEnumerator<ValueType>(simulateBattle.StartBattle());

                    superEnumerator.Done();
                }
            }

            clientIsOver = _br.ReadBoolean();

            if (clientIsOver)
            {
                ReadRoundDataFromStream(_br, this);
            }

            clientRefreshDataCallBack();
        }

        private static void ReadRoundDataFromStream(BinaryReader _br, Battle _battle)
        {
            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                bool isMine = _br.ReadBoolean();

                int pos = _br.ReadInt32();

                int uid = _br.ReadInt32();

                int result = _battle.AddSummon(isMine, pos, uid);

                if (result != -1)
                {
                    throw new Exception("summon error:" + result);
                }
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                bool isMine = _br.ReadBoolean();

                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                int result = _battle.AddAction(isMine, pos, targetPos);

                if (result != -1)
                {
                    throw new Exception("action error:" + result);
                }
            }

            int randomSeed = _br.ReadInt32();

            _battle.SetRandomSeed(randomSeed);
        }

        public int ClientRequestSummon(int _pos, int _uid)
        {
            return AddSummon(clientIsMine, _pos, _uid);
        }

        public void ClientRequestUnsummon(int _pos)
        {
            DelSummon(_pos);
        }

        public void ClientRequestQuitBattle()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_QUIT);

                    clientSendDataCallBack(ms, GetResponse);
                }
            }
        }

        public int ClientRequestAction(int _pos, int _targetPos)
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

                    bw.Write(roundNum);

                    bw.Write(GetSummonNum());

                    IEnumerator<KeyValuePair<int, int>> enumerator = GetSummonEnumerator();

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
            clientIsOver = false;

            ClearSummon();

            ClearAction();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                bool isMine = _br.ReadBoolean();

                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                SetCard(isMine, uid, id);

                if (!serverProcessBattle)
                {
                    simulateBattle.SetCard(isMine, uid, id);
                }
            }

            long pos = _br.BaseStream.Position;

            ReadRoundDataFromStream(_br, this);

            if (!serverProcessBattle)
            {
                _br.BaseStream.Position = pos;

                ReadRoundDataFromStream(_br, simulateBattle);

                SuperEnumerator<ValueType> superEnumerator = new SuperEnumerator<ValueType>(simulateBattle.StartBattle());

                superEnumerator.Done();

                BattleResult battleResult = (BattleResult)superEnumerator.Current;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write(PackageTag.C2S_RESULT);

                        bw.Write((byte)battleResult);

                        clientSendDataCallBack(ms, GetResponse);
                    }
                }
            }

            clientDoActionCallBack(new SuperEnumerator<ValueType>(StartBattle()));
        }

        public bool GetClientCanAction()
        {
            return !clientIsOver;
        }

        private void GetResponse(BinaryReader _br)
        {

        }

        public int maxRoundNum
        {
            get
            {
                return GetMaxRoundNum();
            }
        }

        public int mAddMoney
        {
            get
            {
                return GetAddMoney(clientIsMine);
            }
        }

        public int oAddMoney
        {
            get
            {
                return GetAddMoney(!clientIsMine);
            }
        }

        public int mAddCardsNum
        {
            get
            {
                return GetAddCardsNum(clientIsMine);
            }
        }

        public int oAddCardsNum
        {
            get
            {
                return GetAddCardsNum(!clientIsMine);
            }
        }
    }
}
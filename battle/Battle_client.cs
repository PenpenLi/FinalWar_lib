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
            Dictionary<int, int> tmpDic = battle.GetSummon();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                tmpDic[uid] = pos;
            }

            tmpDic = battle.GetAction();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                tmpDic[pos] = targetPos;
            }

            int randomIndex = _br.ReadInt32();

            battle.SetRandomIndex(randomIndex);
        }

        public bool ClientRequestSummon(int _cardUid, int _pos)
        {
            Dictionary<int, int> tmpDic = battle.GetSummon();

            if (tmpDic.ContainsValue(_pos) || battle.heroMapDic.ContainsKey(_pos) || battle.GetPosIsMine(_pos) != clientIsMine)
            {
                return false;
            }

            int cardID = battle.GetCard(_cardUid);

            IHeroSDS heroSDS = Battle.GetHeroData(cardID);

            if (ClientGetMoney() < heroSDS.GetCost())
            {
                return false;
            }

            tmpDic.Add(_cardUid, _pos);

            return true;
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            battle.GetSummon().Remove(_cardUid);
        }

        public int ClientGetMoney()
        {
            int money = clientIsMine ? battle.mMoney : battle.oMoney;

            Dictionary<int, int>.KeyCollection.Enumerator enumerator = battle.GetSummon().Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int cardID = battle.GetCard(enumerator.Current);

                IHeroSDS heroSDS = Battle.GetHeroData(cardID);

                money -= heroSDS.GetCost();
            }

            return money;
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
            Hero hero = battle.heroMapDic[_pos];

            if (!hero.GetCanAction())
            {
                return false;
            }

            bool targetPosIsMine = battle.GetPosIsMine(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(battle.mapData, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if (targetPosIsMine == hero.isMine)
                {
                    battle.GetAction().Add(_pos, _targetPos);
                }
                else
                {
                    int nowThreadLevel = 0;

                    Hero targetHero2;

                    if (battle.heroMapDic.TryGetValue(_targetPos, out targetHero2))
                    {
                        nowThreadLevel = targetHero2.sds.GetHeroType().GetThread();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        if (pos != _targetPos)
                        {
                            Hero targetHero;

                            if (battle.heroMapDic.TryGetValue(pos, out targetHero))
                            {
                                if (targetHero.isMine != hero.isMine)
                                {
                                    if (targetHero.sds.GetHeroType().GetThread() > nowThreadLevel)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    battle.GetAction().Add(_pos, _targetPos);
                }

                return true;
            }
            else
            {
                if (hero.sds.GetSkill() != 0 && battle.heroMapDic.ContainsKey(_targetPos))
                {
                    List<int> tmpList2 = BattlePublicTools.GetNeighbourPos3(battle.mapData, _pos);

                    if (tmpList2.Contains(_targetPos))
                    {
                        if (targetPosIsMine != hero.isMine)
                        {
                            battle.GetAction().Add(_pos, _targetPos);

                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void ClientRequestUnaction(int _pos)
        {
            battle.GetAction().Remove(_pos);
        }

        public void ClientRequestDoAction()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_DOACTION);

                    Dictionary<int, int> tmpDic = battle.GetSummon();

                    bw.Write(tmpDic.Count);

                    Dictionary<int, int>.Enumerator enumerator = tmpDic.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    tmpDic = battle.GetAction();

                    bw.Write(tmpDic.Count);

                    enumerator = tmpDic.GetEnumerator();

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

            Dictionary<int, int> tmpDic = battle.GetSummon();

            Dictionary<int, int>.KeyCollection.Enumerator enumerator = tmpDic.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int uid = enumerator.Current;

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

#if CLIENT
using System;
using System.Collections.Generic;
using System.IO;
using superEnumerator;

namespace FinalWar
{
    public partial class Battle
    {
        public bool clientIsMine { get; private set; }

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
            }
        }

        private void ClientRefreshData(BinaryReader _br)
        {
            eventListener.Clear();

            isVsAi = _br.ReadBoolean();

            clientIsMine = _br.ReadBoolean();

            Log.Write("ClientRefreshData  isMine:" + clientIsMine);

            mScore = _br.ReadInt32();

            oScore = _br.ReadInt32();

            cardUid = _br.ReadInt32();

            mapID = _br.ReadInt32();

            mapData = GetMapData(mapID);

            mapBelongDic = new Dictionary<int, bool>();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                mapBelongDic.Add(pos, true);
            }

            heroMapDic.Clear();

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                Hero hero = new Hero(this);

                hero.ReadFromStream(_br);

                heroMapDic.Add(hero.pos, hero);
            }

            mHandCards.Clear();

            oHandCards.Clear();

            mCards.Clear();

            oCards.Clear();

            Dictionary<int, int> handCards;

            Dictionary<int, int> handCards2;

            List<int> cards;

            List<int> cards2;

            if (clientIsMine)
            {
                handCards = mHandCards;

                handCards2 = oHandCards;

                cards = mCards;

                cards2 = oCards;
            }
            else
            {
                handCards = oHandCards;

                handCards2 = mHandCards;

                cards = oCards;

                cards2 = mCards;
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards.Add(uid, id);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards2.Add(uid, id);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int id = _br.ReadInt32();

                cards.Add(id);
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int id = _br.ReadInt32();

                cards2.Add(id);
            }

            mMoney = _br.ReadInt32();

            oMoney = _br.ReadInt32();

            bool isOver;

            if (clientIsMine)
            {
                isOver = mOver = _br.ReadBoolean();
            }
            else
            {
                isOver = oOver = _br.ReadBoolean();
            }

            summon.Clear();

            action.Clear();

            if (isOver)
            {
                num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int uid = _br.ReadInt32();

                    int pos = _br.ReadInt32();

                    summon.Add(uid, pos);
                }

                num = _br.ReadInt32();

                for (int i = 0; i < num; i++)
                {
                    int pos = _br.ReadInt32();

                    int targetPos = _br.ReadInt32();

                    action.Add(new KeyValuePair<int, int>(pos, targetPos));
                }
            }

            clientRefreshDataCallBack();
        }

        public bool ClientRequestSummon(int _cardUid, int _pos)
        {
            if (summon.ContainsValue(_pos) || heroMapDic.ContainsKey(_pos) || GetPosIsMine(_pos) != clientIsMine)
            {
                return false;
            }

            Dictionary<int, int> handCards = clientIsMine ? mHandCards : oHandCards;

            int cardID = handCards[_cardUid];

            IHeroSDS heroSDS = GetHeroData(cardID);

            if (ClientGetMoney() < heroSDS.GetCost())
            {
                return false;
            }

            summon.Add(_cardUid, _pos);

            return true;
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            summon.Remove(_cardUid);
        }

        public int ClientGetMoney()
        {
            int money = clientIsMine ? mMoney : oMoney;

            Dictionary<int, int> cards = clientIsMine ? mHandCards : oHandCards;

            Dictionary<int, int>.KeyCollection.Enumerator enumerator = summon.Keys.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int cardID = cards[enumerator.Current];

                IHeroSDS heroSDS = GetHeroData(cardID);

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
            Hero hero = heroMapDic[_pos];

            if (!hero.GetCanAction())
            {
                return false;
            }

            bool targetPosIsMine = GetPosIsMine(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if (targetPosIsMine == hero.isMine)
                {
                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                }
                else
                {
                    int nowThreadLevel = 0;

                    Hero targetHero2;

                    if (heroMapDic.TryGetValue(_targetPos, out targetHero2))
                    {
                        nowThreadLevel = targetHero2.sds.GetHeroType().GetThread();
                    }

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int pos = tmpList[i];

                        if (pos != _targetPos)
                        {
                            Hero targetHero;

                            if (heroMapDic.TryGetValue(pos, out targetHero))
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

                    action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                }

                return true;
            }
            else
            {
                if (hero.sds.GetSkill() != 0 && heroMapDic.ContainsKey(_targetPos))
                {
                    List<int> tmpList2 = BattlePublicTools.GetNeighbourPos3(mapData, _pos);

                    if (tmpList2.Contains(_targetPos))
                    {
                        if (targetPosIsMine != hero.isMine)
                        {
                            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void ClientRequestUnaction(int _pos)
        {
            for (int i = 0; i < action.Count; i++)
            {
                if (action[i].Key == _pos)
                {
                    action.RemoveAt(i);

                    break;
                }
            }
        }

        public void ClientRequestDoAction()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.C2S_DOACTION);

                    bw.Write(summon.Count);

                    Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        KeyValuePair<int, int> pair = enumerator.Current;

                        bw.Write(pair.Key);

                        bw.Write(pair.Value);
                    }

                    bw.Write(action.Count);

                    for (int i = 0; i < action.Count; i++)
                    {
                        bw.Write(action[i].Key);

                        bw.Write(action[i].Value);
                    }

                    if (clientIsMine)
                    {
                        mOver = true;
                    }
                    else
                    {
                        oOver = true;
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
            ClientReadActionAndSummon(_br);

            ClientReadCardsAndRandom(_br);

            clientDoActionCallBack(new SuperEnumerator<ValueType>(StartBattle()));
        }

        public void ClientEndBattle()
        {
            EndBattle();
        }

        private void ClientReadActionAndSummon(BinaryReader _br)
        {
            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int target = _br.ReadInt32();

                action.Add(new KeyValuePair<int, int>(pos, target));
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                summon.Add(uid, pos);
            }
        }

        private void ClientReadCardsAndRandom(BinaryReader _br)
        {
            int num = _br.ReadInt32();

            Dictionary<int, int> cards = clientIsMine ? oHandCards : mHandCards;

            for (int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                cards[uid] = id;
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int value = _br.ReadInt32();

                randomList.Enqueue(value);
            }
        }

        public bool GetClientCanAction()
        {
            return !(clientIsMine ? mOver : oOver);
        }
    }
}
#endif

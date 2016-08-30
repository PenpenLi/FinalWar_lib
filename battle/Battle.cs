using System;
using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public class Battle
    {
        private static Dictionary<int, IHeroSDS> heroDataDic;
        private static Dictionary<int, MapData> mapDataDic;

        private const int DEFAULT_HAND_CARD_NUM = 5;
        private const int MAX_HAND_CARD_NUM = 10;
        private const int DEFAULT_MONEY = 5;
        private const int ADD_MONEY = 1;
        private const int MAX_MONEY = 10;

        public MapData mapData;

        public Dictionary<int, bool> mapBelongDic;
        public Dictionary<int, Hero> heroMapDic;

        private List<int> mCards;
        private List<int> oCards;

        public Dictionary<int,int> mHandCards;
        public Dictionary<int,int> oHandCards;

        public int mScore;
        public int oScore;

        public int mMoney;
        public int oMoney;

        public Dictionary<int, int> summon = new Dictionary<int, int>();

        public List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

        private int cardUid;

        public bool mOver;
        public bool oOver;

        private Random random;

        private Action<bool, MemoryStream> serverSendDataCallBack;

        public bool clientIsMine;

        private Action<MemoryStream> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<IEnumerator<ValueType>> clientDoActionCallBack;

        public static void Init(Dictionary<int, IHeroSDS> _heroDataDic, Dictionary<int, MapData> _mapDataDic)
        {
            heroDataDic = _heroDataDic;
            mapDataDic = _mapDataDic;
        }

        public void ServerSetCallBack(Action<bool, MemoryStream> _serverSendDataCallBack)
        {
            serverSendDataCallBack = _serverSendDataCallBack;
        }

        public void ClientSetCallBack(Action<MemoryStream> _clientSendDataCallBack, Action _clientRefreshDataCallBack, Action<IEnumerator<ValueType>> _clientDoActionCallBack)
        {
            clientSendDataCallBack = _clientSendDataCallBack;
            clientRefreshDataCallBack = _clientRefreshDataCallBack;
            clientDoActionCallBack = _clientDoActionCallBack;
        }

        public void ServerStart(int _mapID,List<int> _mCards,List<int> _oCards)
        {
            Log.Write("Battle Start!");

            random = new Random();

            mapData = mapDataDic[_mapID];

            heroMapDic = new Dictionary<int, Hero>();

            mapBelongDic = new Dictionary<int, bool>();

            mScore = mapData.score1;
            oScore = mapData.score2;

            mMoney = oMoney = DEFAULT_MONEY;

            cardUid = 1;

            mCards = _mCards;
            oCards = _oCards;

            mHandCards = new Dictionary<int, int>();
            oHandCards = new Dictionary<int, int>();

            for (int i = 0; i < DEFAULT_HAND_CARD_NUM; i++)
            {
                int index = (int)(random.NextDouble() * mCards.Count);

                mHandCards.Add(GetCardUid(), mCards[index]);

                mCards.RemoveAt(index);

                index = (int)(random.NextDouble() * oCards.Count);

                oHandCards.Add(GetCardUid(), oCards[index]);

                oCards.RemoveAt(index);
            }

            mOver = oOver = false;

            ServerRefreshData(true);

            ServerRefreshData(false);
        }

        public void ServerGetPackage(byte[] _bytes,bool _isMine)
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
                    }
                }
            }
        }

        private void ServerRefreshData(bool _isMine)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    Log.Write("ServerRefreshData  isMine:" + _isMine);

                    bw.Write(PackageTag.S2C_REFRESH);

                    bw.Write(_isMine);

                    bw.Write(mScore);

                    bw.Write(oScore);

                    bw.Write(mapData.id);

                    bw.Write(mapBelongDic.Count);

                    Dictionary<int, bool>.KeyCollection.Enumerator enumerator2 = mapBelongDic.Keys.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        bw.Write(enumerator2.Current);
                    }

                    bw.Write(heroMapDic.Count);

                    Dictionary<int, Hero>.ValueCollection.Enumerator enumerator3 = heroMapDic.Values.GetEnumerator();

                    while (enumerator3.MoveNext())
                    {
                        Hero hero = enumerator3.Current;

                        bw.Write(hero.sds.GetID());

                        bw.Write(hero.isMine);

                        bw.Write(hero.pos);

                        bw.Write(hero.nowHp);
                    }

                    Dictionary<int, int> handCards = _isMine ? mHandCards : oHandCards;

                    bw.Write(handCards.Count);

                    Dictionary<int, int>.Enumerator enumerator4 = handCards.GetEnumerator();

                    while (enumerator4.MoveNext())
                    {
                        bw.Write(enumerator4.Current.Key);

                        bw.Write(enumerator4.Current.Value);
                    }

                    bool isOver;

                    if (_isMine)
                    {
                        bw.Write(mMoney);

                        isOver = mOver;
                    }
                    else
                    {
                        bw.Write(oMoney);

                        isOver = oOver;
                    }

                    bw.Write(isOver);

                    if (isOver)
                    {
                        int num = 0;

                        List<KeyValuePair<int, int>> tmpList = new List<KeyValuePair<int, int>>();

                        enumerator4 = summon.GetEnumerator();

                        while (enumerator4.MoveNext())
                        {
                            int pos = enumerator4.Current.Key;

                            if((mapData.dic[pos] == _isMine) == !mapBelongDic.ContainsKey(pos))
                            {
                                num++;

                                tmpList.Add(enumerator4.Current);
                            }
                        }

                        bw.Write(num);

                        for(int i = 0; i < num; i++)
                        {
                            bw.Write(tmpList[i].Key);

                            bw.Write(tmpList[i].Value);
                        }

                        num = 0;

                        tmpList.Clear();

                        for (int i = 0; i < action.Count; i++)
                        {
                            int pos = action[i].Key;

                            if ((mapData.dic[pos] == _isMine) == !mapBelongDic.ContainsKey(pos))
                            {
                                num++;

                                tmpList.Add(action[i]);
                            }
                        }

                        bw.Write(num);

                        for(int i = 0; i < num; i++)
                        {
                            bw.Write(tmpList[i].Key);

                            bw.Write(tmpList[i].Value);
                        }
                    }

                    serverSendDataCallBack(_isMine, ms);
                }
            }
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
            }
        }

        private void ClientRefreshData(BinaryReader _br)
        {
            clientIsMine = _br.ReadBoolean();

            Log.Write("ClientRefreshData  isMine:" + clientIsMine);

            mScore = _br.ReadInt32();

            oScore = _br.ReadInt32();

            int mapID = _br.ReadInt32();

            mapData = mapDataDic[mapID];

            mapBelongDic = new Dictionary<int, bool>();

            int num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                mapBelongDic.Add(pos, true);
            }

            heroMapDic = new Dictionary<int, Hero>();

            num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int id = _br.ReadInt32();

                bool heroIsMine = _br.ReadBoolean();

                int pos = _br.ReadInt32();

                int nowHp = _br.ReadInt32();

                AddHero(heroIsMine, heroDataDic[id], pos, nowHp);
            }

            Dictionary<int, int> handCards;

            if (clientIsMine)
            {
                mHandCards = new Dictionary<int, int>();

                handCards = mHandCards;
            }
            else
            {
                oHandCards = new Dictionary<int, int>();

                handCards = oHandCards;
            }

            num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int id = _br.ReadInt32();

                handCards.Add(uid, id);
            }

            bool isOver;

            if (clientIsMine)
            {
                mMoney = _br.ReadInt32();

                isOver = mOver = _br.ReadBoolean();
            }
            else
            {
                oMoney = _br.ReadInt32();

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

        public void ClientRequestSummon(int _cardUid, int _pos)
        {
            summon.Add(_cardUid, _pos);
        }

        public void ClientRequestUnsummon(int _cardUid)
        {
            summon.Remove(_cardUid);
        }

        public void ClientRequestAction(int _pos, int _targetPos)
        {
            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));
        }

        public void ClientRequestUnaction(int _pos)
        {
            for(int i = 0; i < action.Count; i++)
            {
                if(action[i].Key == _pos)
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
                        bw.Write(enumerator.Current.Key);

                        bw.Write(enumerator.Current.Value);
                    }

                    bw.Write(action.Count);

                    for(int i = 0; i < action.Count; i++)
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

        private void ServerDoAction(bool _isMine, BinaryReader _br)
        {
            Dictionary<int, int> cards;

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

                cards = mHandCards;
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

                cards = oHandCards;
            }

            int num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                int uid = _br.ReadInt32();

                int pos = _br.ReadInt32();

                if (cards.ContainsKey(uid) && (mapData.dic[pos] == _isMine) == !mapBelongDic.ContainsKey(pos))
                {
                    summon.Add(uid, pos);
                }
            }

            num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                int pos = _br.ReadInt32();

                int targetPos = _br.ReadInt32();

                if (heroMapDic.ContainsKey(pos) && heroMapDic[pos].isMine == _isMine)
                {
                    action.Add(new KeyValuePair<int, int>(pos, targetPos));
                }
            }

            if(mOver && oOver)
            {
                ServerStartBattle();

                //ServerRefreshData(true);

                //ServerRefreshData(false);
            }
        }

        private Hero AddHero(bool _isMine, IHeroSDS _sds, int _pos)
        {
            Hero hero = new Hero(_isMine, _sds, _pos);

            heroMapDic.Add(_pos, hero);

            return hero;
        }

        private Hero AddHero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp)
        {
            Hero hero = new Hero(_isMine, _sds, _pos, _nowHp);

            heroMapDic.Add(_pos, hero);

            return hero;
        }

        private void ServerStartBattle()
        {
            List<ValueType> voList = new List<ValueType>();
            
            BattleData battleData = GetBattleData();

            action.Clear();

            DoSummonAction(battleData, voList);

            summon.Clear();

            DoMoveAction(battleData, voList);

            DoAttackAction(battleData, voList);

            DoMoveAfterAttack(battleData, voList);

            byte[] bytes;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(PackageTag.S2C_DOACTION);

                    BattleVOTools.WriteDataToStream(voList, bw);

                    bytes = ms.ToArray();
                }
            }

            using (MemoryStream mMs = new MemoryStream(), oMs = new MemoryStream())
            {
                using (BinaryWriter mBw = new BinaryWriter(mMs), oBw = new BinaryWriter(oMs))
                {
                    mBw.Write(bytes);

                    oBw.Write(bytes);

                    RecoverCards(mBw, oBw);

                    serverSendDataCallBack(true, mMs);

                    serverSendDataCallBack(false, oMs);
                }
            }

            RecoverMoney();

            RecoverOver();
        }

        private void DoSummonAction(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int tmpCardUid = enumerator.Current.Key;

                int pos = enumerator.Current.Value;

                int heroID = SummonOneUnit(tmpCardUid, pos, _battleData);

                _voList.Add(new BattleSummonVO(tmpCardUid, heroID, pos));
            }
        }

        private int SummonOneUnit(int _uid, int _pos, BattleData _battleData)
        {
            bool isMine = mapData.dic[_pos] == !mapBelongDic.ContainsKey(_pos);
            
            int heroID;

            if (isMine)
            {
                heroID = mHandCards[_uid];
            }
            else
            {
                heroID = oHandCards[_uid];
            }

            IHeroSDS sds = heroDataDic[heroID];

            if (isMine)
            {
                mMoney -= sds.GetCost();

                mHandCards.Remove(_uid);
            }
            else
            {
                oMoney -= sds.GetCost();

                oHandCards.Remove(_uid);
            }

            Hero hero = AddHero(isMine, sds, _pos);

            if (_battleData.actionDic.ContainsKey(_pos))
            {
                _battleData.actionDic[_pos].stander = hero;

                hero.action = Hero.HeroAction.NULL;
            }

            return heroID;
        }

        public BattleData GetBattleData()
        {
            BattleData battleData = new BattleData();

            List<KeyValuePair<int, int>> shtList = new List<KeyValuePair<int, int>>();

            List<KeyValuePair<int, int>> atkList = new List<KeyValuePair<int, int>>();

            List<KeyValuePair<int, int>> supList = new List<KeyValuePair<int, int>>();

            Dictionary<int, int> supDic = new Dictionary<int, int>();

            for (int i = 0; i < action.Count; i++)
            { 
                int pos = action[i].Key;
                int targetPos = action[i].Value;

                GetOneUnitAction(pos, targetPos, shtList, atkList, supList, supDic);
            }

            for (int i = 0; i < supList.Count; i++)
            { 
                int pos = supList[i].Key;

                if (!supDic.ContainsKey(pos))
                {
                    continue;
                }

                List<int> tmpList = new List<int>();

                while (true)
                {
                    tmpList.Add(pos);

                    int targetPos = supDic[pos];

                    if (supDic.ContainsKey(targetPos))
                    {
                        if (tmpList.Contains(targetPos))
                        {
                            //绕圈援助
                            break;
                        }
                        else
                        {
                            pos = targetPos;
                        }
                    }
                    else
                    {
                        if (!summon.ContainsValue(targetPos) && !battleData.moveDic.ContainsValue(targetPos) && (!heroMapDic.ContainsKey(targetPos) || battleData.moveDic.ContainsKey(targetPos)))
                        {
                            for (int m = tmpList.Count - 1; m > -1; m--)
                            {
                                int oldPos = tmpList[m];

                                int newPos = supDic[oldPos];

                                battleData.moveDic.Add(oldPos, newPos);
                            }
                        }

                        break;
                    }
                }

                for(int m = 0; m < tmpList.Count; m++)
                {
                    supDic.Remove(tmpList[m]);
                }
            }

            for(int i = supList.Count - 1; i > -1 ; i--)
            {
                if (battleData.moveDic.ContainsKey(supList[i].Key))
                {
                    supList.RemoveAt(i);
                }
            }

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                enumerator.Current.action = Hero.HeroAction.DEFENSE;
            }

            for(int i = 0; i < shtList.Count; i++)
            {
                KeyValuePair<int, int> pair = shtList[i];

                Hero hero = heroMapDic[pair.Key];

                hero.action = Hero.HeroAction.SHOOT;

                hero.actionTarget = pair.Value;

                BattleCellData cellData;

                if (battleData.actionDic.ContainsKey(pair.Value))
                {
                    cellData = battleData.actionDic[pair.Value];
                }
                else
                {
                    cellData = new BattleCellData();

                    if (heroMapDic.ContainsKey(pair.Value))
                    {
                        cellData.stander = heroMapDic[pair.Value];
                    }

                    battleData.actionDic.Add(pair.Value, cellData);
                }

                cellData.shooters.Add(hero);
            }

            for (int i = 0; i < atkList.Count; i++)
            {
                KeyValuePair<int, int> pair = atkList[i];

                Hero hero = heroMapDic[pair.Key];

                hero.action = Hero.HeroAction.ATTACK;

                hero.actionTarget = pair.Value;

                BattleCellData cellData;

                if (battleData.actionDic.ContainsKey(pair.Value))
                {
                    cellData = battleData.actionDic[pair.Value];
                }
                else
                {
                    cellData = new BattleCellData();

                    if (heroMapDic.ContainsKey(pair.Value))
                    {
                        cellData.stander = heroMapDic[pair.Value];
                    }

                    battleData.actionDic.Add(pair.Value, cellData);
                }

                cellData.attackers.Add(hero);
            }

            for (int i = 0; i < supList.Count; i++)
            {
                KeyValuePair<int, int> pair = supList[i];

                Hero hero = heroMapDic[pair.Key];

                hero.action = Hero.HeroAction.SUPPORT;

                hero.actionTarget = pair.Value;

                BattleCellData cellData;

                if (battleData.actionDic.ContainsKey(pair.Value))
                {
                    cellData = battleData.actionDic[pair.Value];
                }
                else
                {
                    cellData = new BattleCellData();

                    if (heroMapDic.ContainsKey(pair.Value))
                    {
                        cellData.stander = heroMapDic[pair.Value];
                    }

                    battleData.actionDic.Add(pair.Value, cellData);
                }

                cellData.supporters.Add(hero);
            }

            return battleData;
        }

        private void GetOneUnitAction(int _pos, int _targetPos, List<KeyValuePair<int, int>> _shtList, List<KeyValuePair<int, int>> _atkList, List<KeyValuePair<int, int>> _supList, Dictionary<int, int> _supDic)
        {
            bool posIsMine = mapData.dic[_pos] == !mapBelongDic.ContainsKey(_pos);

            bool targetPosIsMine = mapData.dic[_targetPos] == !mapBelongDic.ContainsKey(_targetPos);

            List<int> arr = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            if (arr.Contains(_targetPos))
            {
                if (posIsMine == targetPosIsMine)
                {
                    _supList.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                    _supDic.Add(_pos, _targetPos);
                }
                else
                {
                    _atkList.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                }
            }
            else
            {
                arr = BattlePublicTools.GetNeighbourPos2(mapData.neighbourPosMap, _pos);

                if (arr.Contains(_targetPos))
                {
                    if (posIsMine == targetPosIsMine)
                    {
                        throw new Exception("shoot error");
                    }
                    else
                    {
                        _shtList.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                    }
                }
                else
                {
                    throw new Exception("targetPos error");
                }
            }
        }

        private void DoMoveAction(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<int, Hero> tmpDic = new Dictionary<int, Hero>();

            Dictionary<int, int>.Enumerator enumerator = _battleData.moveDic.GetEnumerator();

            _voList.Add(new BattleMoveVO(_battleData.moveDic));

            while (enumerator.MoveNext())
            {
                tmpDic.Add(enumerator.Current.Value, heroMapDic[enumerator.Current.Key]);

                heroMapDic.Remove(enumerator.Current.Key);

                if (_battleData.actionDic.ContainsKey(enumerator.Current.Key))
                {
                    _battleData.actionDic[enumerator.Current.Key].stander = null;
                }
            }

            Dictionary<int, Hero>.Enumerator enumerator2 = tmpDic.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                heroMapDic.Add(enumerator2.Current.Key, enumerator2.Current.Value);

                enumerator2.Current.Value.pos = enumerator2.Current.Key;

                if (_battleData.actionDic.ContainsKey(enumerator2.Current.Key))
                {
                    _battleData.actionDic[enumerator2.Current.Key].stander = enumerator2.Current.Value;
                }
            }
        }

        private void DoAttackAction(BattleData _battleData, List<ValueType> _voList)
        {
            bool doRush = true;

            bool doShoot = true;

            List<int> diePos = new List<int>();

            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator;

            while (true)
            {
                if (doRush)
                {
                    enumerator = _battleData.actionDic.Values.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        BattleCellData cellData = enumerator.Current;

                        if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                        {
                            List<KeyValuePair<int, int>> attackers = new List<KeyValuePair<int, int>>();

                            int stander = cellData.stander.pos;

                            for (int i = 0; i < cellData.attackers.Count; i++)
                            {
                                Hero attacker = cellData.attackers[i];

                                cellData.attackOvers.Add(attacker);

                                attacker.action = Hero.HeroAction.ATTACKOVER;

                                if (cellData.stander.nowHp > 0)
                                {
                                    cellData.stander.nowHp -= attacker.sds.GetAttack();

                                    if (cellData.stander.nowHp < 1)
                                    {
                                        diePos.Add(cellData.stander.pos);
                                    }
                                }

                                attackers.Add(new KeyValuePair<int, int>(attacker.pos, attacker.sds.GetAttack()));
                            }

                            cellData.attackers.Clear();

                            _voList.Add(new BattleRushVO(attackers, stander));
                        }
                    }

                    if (diePos.Count > 0)
                    {
                        _voList.Add(new BattleDeathVO(new List<int>(diePos)));

                        DieHeros(_battleData, diePos);

                        diePos.Clear();

                        continue;
                    }
                }

                if (doShoot)
                {
                    doShoot = false;

                    enumerator = _battleData.actionDic.Values.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        BattleCellData cellData = enumerator.Current;

                        if (cellData.stander != null && cellData.shooters.Count > 0)
                        {
                            List<KeyValuePair<int, int>> shooters = new List<KeyValuePair<int, int>>();
                            int stander = cellData.stander.pos;

                            for (int i = 0; i < cellData.shooters.Count; i++)
                            {
                                Hero shooter = cellData.shooters[i];

                                shooter.action = Hero.HeroAction.NULL;

                                if (cellData.stander.nowHp > 0)
                                {
                                    cellData.stander.nowHp -= shooter.sds.GetShoot();

                                    if (cellData.stander.nowHp < 1)
                                    {
                                        diePos.Add(cellData.stander.pos);
                                    }
                                }

                                shooters.Add(new KeyValuePair<int, int>(shooter.pos,shooter.sds.GetShoot()));
                            }

                            cellData.shooters.Clear();

                            _voList.Add(new BattleShootVO(shooters, stander));
                        }
                    }

                    if (diePos.Count > 0)
                    {
                        _voList.Add(new BattleDeathVO(new List<int>(diePos)));

                        DieHeros(_battleData, diePos);

                        diePos.Clear();

                        continue;
                    }
                }

                doRush = false;

                bool hasAction = false;

                Dictionary<int, BattleCellData>.Enumerator enumerator2 = _battleData.actionDic.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    BattleCellData cellData = enumerator2.Current.Value;

                    if (cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                    {
                        Hero attacker = cellData.attackers[0];

                        cellData.attackers.RemoveAt(0);

                        cellData.attackOvers.Add(attacker);
                        
                        attacker.action = Hero.HeroAction.ATTACKOVER;

                        Hero target;

                        if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                        {
                            target = cellData.stander;

                            attacker.nowHp -= target.sds.GetDefense();

                            _voList.Add(new BattleAttackVO(attacker.pos, enumerator2.Current.Key, -1, attacker.sds.GetAttack(), target.sds.GetDefense()));
                        }
                        else
                        {
                            target = cellData.supporters[0];

                            attacker.nowHp -= target.sds.GetSupport();

                            _voList.Add(new BattleAttackVO(attacker.pos, enumerator2.Current.Key, target.pos, attacker.sds.GetAttack(), target.sds.GetSupport()));
                        }

                        target.nowHp -= attacker.sds.GetAttack();

                        if (target.nowHp < 1)
                        {
                            diePos.Add(target.pos);
                        }

                        if (attacker.nowHp < 1)
                        {
                            diePos.Add(attacker.pos);
                        }

                        if (cellData.attackers.Count > 0)
                        {
                            hasAction = true;
                        }
                    }
                }

                if (diePos.Count > 0)
                {
                    _voList.Add(new BattleDeathVO(new List<int>(diePos)));

                    DieHeros(_battleData, diePos);

                    diePos.Clear();

                    doRush = true;
                }

                if (!hasAction)
                {
                    break;
                }
            }
        }

        private void DoMoveAfterAttack(BattleData _battleData, List<ValueType> _voList)
        {
            List<int> tmpList = new List<int>();

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current.Value;

                if (cellData.stander == null && (cellData.supporters.Count > 0 || cellData.attackOvers.Count > 0 || cellData.attackers.Count > 0))
                {
                    tmpList.Add(enumerator.Current.Key);
                }
            }

            if(tmpList.Count > 0)
            {
                Dictionary<int, int> tmpMoveDic = new Dictionary<int, int>();

                for (int i = 0; i < tmpList.Count; i++)
                {
                    OneCellEmpty(_battleData, tmpList[i], tmpMoveDic);
                }

                _voList.Add(new BattleMoveVO(tmpMoveDic));
            }
        }

        private void DieHeros(BattleData _battleData, List<int> _diePos)
        {
            for (int i = 0; i < _diePos.Count; i++)
            {
                int nowPos = _diePos[i];

                Hero hero = heroMapDic[nowPos];

                if (hero.action == Hero.HeroAction.ATTACK)
                {
                    BattleCellData cellData = _battleData.actionDic[hero.actionTarget];

                    cellData.attackers.Remove(hero);
                }
                else if(hero.action == Hero.HeroAction.ATTACKOVER)
                {
                    BattleCellData cellData = _battleData.actionDic[hero.actionTarget];

                    cellData.attackOvers.Remove(hero);
                }
                else if (hero.action == Hero.HeroAction.SHOOT)
                {
                    BattleCellData cellData = _battleData.actionDic[hero.actionTarget];

                    cellData.shooters.Remove(hero);
                }
                else if (hero.action == Hero.HeroAction.SUPPORT)
                {
                    BattleCellData cellData = _battleData.actionDic[hero.actionTarget];

                    cellData.supporters.Remove(hero);
                }

                heroMapDic.Remove(hero.pos);

                if (_battleData.actionDic.ContainsKey(nowPos))
                {
                    _battleData.actionDic[nowPos].stander = null;
                }
            }
        }

        private void OneCellEmpty(BattleData _battleData, int _pos, Dictionary<int, int> _tmpMoveDic)
        {
            int nowPos = _pos;

            while (true)
            {
                if (!_battleData.actionDic.ContainsKey(nowPos))
                {
                    return;
                }

                BattleCellData cellData = _battleData.actionDic[nowPos];

                Hero hero = null;

                bool changeMapBelong = false;

                if (cellData.supporters.Count > 0)
                {
                    hero = cellData.supporters[0];
                }
                else if(cellData.attackOvers.Count > 0)
                {
                    hero = cellData.attackOvers[0];

                    changeMapBelong = true;
                }
                else if(cellData.attackers.Count > 0)
                {
                    hero = cellData.attackers[0];

                    changeMapBelong = true;
                }

                if(hero != null)
                {
                    if (changeMapBelong)
                    {
                        if (mapBelongDic.ContainsKey(nowPos))
                        {
                            mapBelongDic.Remove(nowPos);
                        }
                        else
                        {
                            mapBelongDic.Add(nowPos, true);
                        }
                    }

                    _tmpMoveDic.Add(hero.pos, nowPos);

                    heroMapDic.Remove(hero.pos);

                    heroMapDic.Add(nowPos, hero);

                    int tmpPos = hero.pos;

                    hero.pos = nowPos;

                    nowPos = tmpPos;
                }
                else
                {
                    return;
                }
            }
        }

        private void RecoverCards(BinaryWriter _mBw, BinaryWriter _oBw)
        {
            if(mCards.Count > 0)
            {
                int index = (int)(random.NextDouble() * mCards.Count);

                int id = mCards[index];

                mCards.RemoveAt(index);

                if(mHandCards.Count < MAX_HAND_CARD_NUM)
                {
                    int tmpCardUid = GetCardUid();

                    mHandCards.Add(tmpCardUid, id);

                    _mBw.Write(true);

                    _mBw.Write(tmpCardUid);

                    _mBw.Write(id);
                }
                else
                {
                    _mBw.Write(false);
                }
            }
            else
            {
                _mBw.Write(false);
            }

            if (oCards.Count > 0)
            {
                int index = (int)(random.NextDouble() * oCards.Count);

                int id = oCards[index];

                oCards.RemoveAt(index);

                if (oHandCards.Count < MAX_HAND_CARD_NUM)
                {
                    int tmpCardUid = GetCardUid();

                    oHandCards.Add(tmpCardUid, id);

                    _oBw.Write(true);

                    _oBw.Write(tmpCardUid);

                    _oBw.Write(id);
                }
                else
                {
                    _oBw.Write(false);
                }
            }
            else
            {
                _oBw.Write(false);
            }
        }

        private void RecoverMoney()
        {
            mMoney += ADD_MONEY;

            if(mMoney > MAX_MONEY)
            {
                mMoney = MAX_MONEY;
            }

            oMoney += ADD_MONEY;

            if(oMoney > MAX_MONEY)
            {
                oMoney = MAX_MONEY;
            }
        }

        private void RecoverOver()
        {
            mOver = oOver = false;
        }

        private int GetCardUid()
        {
            int result = cardUid;

            cardUid++;

            return result;
        }

        private void ClientDoAction(BinaryReader _br)
        {
            summon.Clear();

            action.Clear();

            clientDoActionCallBack(ClientDoActionReal(_br));
        }

        private IEnumerator<ValueType> ClientDoActionReal(BinaryReader _br)
        {
            List<ValueType> voList = BattleVOTools.ReadDataFromStream(_br);

            for (int i = 0; i < voList.Count; i++)
            {
                ValueType vo = voList[i];

                if(vo is BattleSummonVO)
                {
                    ClientDoSummon((BattleSummonVO)vo);
                }
                else if(vo is BattleMoveVO)
                {
                    ClientDoMove((BattleMoveVO)vo);
                }
                else if(vo is BattleRushVO)
                {
                    ClientDoRush((BattleRushVO)vo);
                }
                else if(vo is BattleShootVO)
                {
                    ClientDoShoot((BattleShootVO)vo);
                }
                else if(vo is BattleAttackVO)
                {
                    ClientDoAttack((BattleAttackVO)vo);
                }
                else if(vo is BattleDeathVO)
                {
                    ClientDoDie((BattleDeathVO)vo);
                }

                yield return vo;
            }

            ClientDoRecover(_br);
        }

        private void ClientDoSummon(BattleSummonVO _vo)
        {
            bool isMine = mapData.dic[_vo.pos] == !mapBelongDic.ContainsKey(_vo.pos);

            IHeroSDS sds = heroDataDic[_vo.heroID];

            if (isMine == clientIsMine)
            {
                if (clientIsMine)
                {
                    mHandCards.Remove(_vo.cardUid);

                    mMoney -= sds.GetCost();
                }
                else
                {
                    oHandCards.Remove(_vo.cardUid);

                    oMoney -= sds.GetCost();
                }
            }

            AddHero(isMine, sds, _vo.pos);
        }

        private void ClientDoMove(BattleMoveVO _vo)
        {
            Dictionary<int, Hero> tmpDic = new Dictionary<int, Hero>();

            Dictionary<int, int>.Enumerator enumerator = _vo.moves.GetEnumerator();

            while (enumerator.MoveNext())
            {
                tmpDic.Add(enumerator.Current.Value, heroMapDic[enumerator.Current.Key]);

                heroMapDic.Remove(enumerator.Current.Key);
            }

            Dictionary<int, Hero>.Enumerator enumerator2 = tmpDic.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                int nowPos = enumerator2.Current.Key;

                Hero hero = enumerator2.Current.Value;

                heroMapDic.Add(nowPos, hero);

                hero.pos = nowPos;

                bool isMine = mapData.dic[nowPos] == !mapBelongDic.ContainsKey(nowPos);

                if(isMine != hero.isMine)
                {
                    if (mapBelongDic.ContainsKey(nowPos))
                    {
                        mapBelongDic.Remove(nowPos);
                    }
                    else
                    {
                        mapBelongDic.Add(nowPos, true);
                    }
                }
            }
        }

        private void ClientDoRush(BattleRushVO _vo)
        {
            Hero hero = heroMapDic[_vo.stander];

            for(int i = 0; i < _vo.attackers.Count; i++)
            {
                hero.nowHp -= _vo.attackers[i].Value;
            }
        }

        private void ClientDoShoot(BattleShootVO _vo)
        {
            Hero hero = heroMapDic[_vo.stander];

            for (int i = 0; i < _vo.shooters.Count; i++)
            {
                hero.nowHp -= _vo.shooters[i].Value;
            }
        }

        private void ClientDoAttack(BattleAttackVO _vo)
        {
            Hero hero = heroMapDic[_vo.attacker];

            hero.nowHp -= _vo.damageSelf;

            if (heroMapDic.ContainsKey(_vo.supporter))
            {
                Hero support = heroMapDic[_vo.supporter];

                support.nowHp -= _vo.damage;
            }
            else
            {
                Hero defender = heroMapDic[_vo.defender];

                defender.nowHp -= _vo.damage;
            }
        }

        private void ClientDoDie(BattleDeathVO _vo)
        {
            for(int i = 0; i < _vo.deads.Count; i++)
            {
                heroMapDic.Remove(_vo.deads[i]);
            }
        }

        public void ClientDoRecover(BinaryReader _br)
        { 
            bool addCard = _br.ReadBoolean();

            if (addCard)
            {
                Dictionary<int, int> tmpCards = clientIsMine ? mHandCards : oHandCards;

                int tmpCardUid = _br.ReadInt32();

                int id = _br.ReadInt32();

                tmpCards.Add(tmpCardUid, id);
            }

            RecoverMoney();

            if (clientIsMine)
            {
                mOver = false;
            }
            else
            {
                oOver = false;
            }

            clientRefreshDataCallBack();
        }
    }
}

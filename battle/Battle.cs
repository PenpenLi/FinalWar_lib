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

        public void ServerRefreshData(bool _isMine)
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

                        bw.Write(hero.nowPower);
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

                            if ((mapData.dic[pos] == _isMine) == !mapBelongDic.ContainsKey(pos))
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

                int nowPower = _br.ReadInt32();

                AddHero(heroIsMine, heroDataDic[id], pos, nowHp, nowPower);
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

            if (mOver && oOver)
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

        private Hero AddHero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowPower)
        {
            Hero hero = new Hero(_isMine, _sds, _pos, _nowHp, _nowPower);

            heroMapDic.Add(_pos, hero);

            return hero;
        }

        private void ServerStartBattle()
        {
            List<ValueType> voList = new List<ValueType>();
            
            BattleData battleData = GetBattleData();

            action.Clear();

            DoShootAction(battleData, voList);

            DoSummonAction(battleData, voList);

            summon.Clear();

            DoRushAction(battleData, voList);

            DoAttackAction(battleData, voList);

            DoMoveAction(battleData, voList);

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
                        throw new Exception("shoot error0");
                    }
                    else
                    {
                        if (heroMapDic.ContainsKey(_pos))
                        {
                            _shtList.Add(new KeyValuePair<int, int>(_pos, _targetPos));
                        }
                        else
                        {
                            throw new Exception("shoot error1");
                        }
                    }
                }
                else
                {
                    throw new Exception("shoot error2");
                }
            }
        }

        private void DoShootAction(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> damageDic = null;

            Dictionary<Hero, int> powerChangeDic = null;

            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                if (cellData.stander != null && cellData.shooters.Count > 0)
                {
                    List<int> shooters = new List<int>();

                    int stander = cellData.stander.pos;

                    int damage = 0;

                    for (int i = 0; i < cellData.shooters.Count; i++)
                    {
                        Hero shooter = cellData.shooters[i];

                        shooter.action = Hero.HeroAction.NULL;

                        shooters.Add(shooter.pos);

                        int tmpDamage = shooter.GetShootDamage();

                        damage += tmpDamage;

                        int shooterPowerChange = shooter.Shoot(tmpDamage);

                        if (shooterPowerChange != 0)
                        {
                            BattlePublicTools.AccumulationDicData(ref powerChangeDic, shooter, shooterPowerChange);
                        }
                    }

                    cellData.shooters.Clear();

                    damage = cellData.stander.BeDamage(damage);

                    if(damage != 0)
                    {
                        BattlePublicTools.AccumulationDicData(ref damageDic, cellData.stander, damage);
                    }

                    int powerChange = cellData.stander.BeShoot(damage);

                    if(powerChange != 0)
                    {
                        BattlePublicTools.AccumulationDicData(ref powerChangeDic, cellData.stander, powerChange);
                    }

                    _voList.Add(new BattleShootVO(shooters, stander, damage));
                }
            }

            if (damageDic != null)
            {
                List<int> diePos = null;

                Dictionary<Hero, int>.Enumerator enumerator3 = damageDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    hero.HpChange(-pair.Value);

                    if (hero.nowHp < 1)
                    {
                        if (diePos == null)
                        {
                            diePos = new List<int>();
                        }

                        diePos.Add(hero.pos);

                        DieHero(_battleData, hero, ref powerChangeDic);
                    }
                }

                if (diePos != null)
                {
                    _voList.Add(new BattleDeathVO(diePos));
                }
            }

            if (powerChangeDic != null)
            {
                Dictionary<Hero, int>.Enumerator enumerator3 = powerChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    hero.PowerChange(pair.Value);
                }
            }
        }

        private void DoRushAction(BattleData _battleData, List<ValueType> _voList)
        {
            while (true)
            {
                bool quit = true;

                Dictionary<Hero, int> damageDic = null;

                Dictionary<Hero, int> powerChangeDic = null;

                Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        quit = false;

                        List<int> attackers = new List<int>();

                        int stander = cellData.stander.pos;

                        int damage = 0;

                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            attacker.action = Hero.HeroAction.ATTACKOVER;

                            attackers.Add(attacker.pos);

                            int tmpDamage = attacker.GetAttackDamage();

                            damage += tmpDamage;

                            int powerChange = attacker.Rush(tmpDamage);

                            if(powerChange != 0)
                            {
                                BattlePublicTools.AccumulationDicData(ref powerChangeDic, attacker, powerChange);
                            }
                        }

                        List<Hero> tmpList = cellData.attackers;

                        cellData.attackers = cellData.attackOvers;

                        cellData.attackOvers = tmpList;

                        damage = cellData.stander.BeDamage(damage);

                        if(damage != 0)
                        {
                            BattlePublicTools.AccumulationDicData(ref damageDic, cellData.stander, damage);
                        }

                        int standerPowerChange = cellData.stander.BeRush(damage);

                        if(standerPowerChange != 0)
                        {
                            BattlePublicTools.AccumulationDicData(ref powerChangeDic, cellData.stander, standerPowerChange);
                        }

                        _voList.Add(new BattleRushVO(attackers, stander, damage));
                    }
                }

                if (damageDic != null)
                {
                    List<int> diePos = null;

                    Dictionary<Hero, int>.Enumerator enumerator3 = damageDic.GetEnumerator();

                    while (enumerator3.MoveNext())
                    {
                        KeyValuePair<Hero, int> pair = enumerator3.Current;

                        Hero hero = pair.Key;

                        hero.HpChange(-pair.Value);

                        if (hero.nowHp < 1)
                        {
                            if (diePos == null)
                            {
                                diePos = new List<int>();
                            }

                            diePos.Add(hero.pos);

                            DieHero(_battleData, hero, ref powerChangeDic);
                        }
                    }

                    if (diePos != null)
                    {
                        _voList.Add(new BattleDeathVO(diePos));
                    }
                }

                if (powerChangeDic != null)
                {
                    Dictionary<Hero, int>.Enumerator enumerator3 = powerChangeDic.GetEnumerator();

                    while (enumerator3.MoveNext())
                    {
                        KeyValuePair<Hero, int> pair = enumerator3.Current;

                        Hero hero = pair.Key;

                        hero.PowerChange(pair.Value);
                    }
                }

                if (quit)
                {
                    break;
                }
            }
        }

        private void DoAttackAction(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> damageDic = null;

            Dictionary<Hero, int> powerChangeDic = null;

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current.Value;

                if (cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                {
                    List<int> attackers = new List<int>();

                    List<int> supporters = new List<int>();

                    int defenderDamage = 0;

                    List<int> attackersDamage = new List<int>();

                    List<int> supportersDamage = new List<int>();

                    int defenseDamage;

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        defenseDamage = cellData.stander.GetCounterDamage();
                    }
                    else
                    {
                        defenseDamage = 0;
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        supporters.Add(hero.pos);

                        defenseDamage += hero.GetCounterDamage();
                    }

                    int attackDamage = 0;

                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        attackers.Add(hero.pos);

                        attackDamage += hero.GetAttackDamage();

                        if(defenseDamage > 0)
                        {
                            int tmpDamage = hero.BeDamage(ref defenseDamage);

                            if(tmpDamage > 0)
                            {
                                BattlePublicTools.AccumulationDicData(ref damageDic, hero, tmpDamage);
                            }

                            attackersDamage.Add(tmpDamage);
                        }
                        else
                        {
                            attackersDamage.Add(0);
                        }
                    }

                    if(attackDamage > 0 && cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        defenderDamage = cellData.stander.BeDamage(ref attackDamage);

                        if(defenderDamage > 0)
                        {
                            BattlePublicTools.AccumulationDicData(ref damageDic, cellData.stander, defenderDamage);
                        }
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        if (attackDamage > 0)
                        {
                            int tmpDamage = hero.BeDamage(ref attackDamage);

                            if(tmpDamage > 0)
                            {
                                BattlePublicTools.AccumulationDicData(ref damageDic, hero, tmpDamage);
                            }

                            supportersDamage.Add(tmpDamage);
                        }
                        else
                        {
                            supportersDamage.Add(0);
                        }
                    }

                    if(attackDamage > 0 && cellData.stander != null && cellData.stander.action != Hero.HeroAction.DEFENSE)
                    {
                        defenderDamage = cellData.stander.BeDamage(ref attackDamage);

                        if(defenderDamage > 0)
                        {
                            BattlePublicTools.AccumulationDicData(ref damageDic, cellData.stander, defenderDamage);
                        }
                    }

                    _voList.Add(new BattleAttackVO(attackers, supporters, enumerator.Current.Key, attackersDamage, supportersDamage, defenderDamage));
                }
            }

            if (damageDic != null)
            {
                List<int> diePos = null;

                Dictionary<Hero, int>.Enumerator enumerator3 = damageDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    hero.HpChange(-pair.Value);

                    if (hero.nowHp < 1)
                    {
                        if (diePos == null)
                        {
                            diePos = new List<int>();
                        }

                        diePos.Add(hero.pos);

                        DieHero(_battleData, hero, ref powerChangeDic);
                    }
                }

                if (diePos != null)
                {
                    _voList.Add(new BattleDeathVO(diePos));
                }
            }
        }

        private void DoMoveAction(BattleData _battleData, List<ValueType> _voList)
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

            if (tmpList.Count > 0)
            {
                Dictionary<int, int> tmpMoveDic = new Dictionary<int, int>();

                for (int i = 0; i < tmpList.Count; i++)
                {
                    OneCellEmpty(_battleData, tmpList[i], tmpMoveDic);
                }

                _voList.Add(new BattleMoveVO(tmpMoveDic));
            }
        }

        private void DieHero(BattleData _battleData, Hero _hero, ref Dictionary<Hero, int> _powerChangeDic)
        {
            RemoveHeroAction(_battleData, _hero);

            heroMapDic.Remove(_hero.pos);

            if (_battleData.actionDic.ContainsKey(_hero.pos))
            {
                _battleData.actionDic[_hero.pos].stander = null;
            }


        }

        private void RemoveHeroAction(BattleData _battleData, Hero _hero)
        {
            if (_hero.action == Hero.HeroAction.ATTACK)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.ATTACKOVER)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.attackOvers.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SHOOT)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.shooters.Remove(_hero);
            }
            else if (_hero.action == Hero.HeroAction.SUPPORT)
            {
                BattleCellData cellData = _battleData.actionDic[_hero.actionTarget];

                cellData.supporters.Remove(_hero);
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
                else if (cellData.attackOvers.Count > 0)
                {
                    hero = cellData.attackOvers[0];

                    changeMapBelong = true;
                }
                else if (cellData.attackers.Count > 0)
                {
                    hero = cellData.attackers[0];

                    changeMapBelong = true;
                }

                if (hero != null)
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
            if (mCards.Count > 0)
            {
                int index = (int)(random.NextDouble() * mCards.Count);

                int id = mCards[index];

                mCards.RemoveAt(index);

                if (mHandCards.Count < MAX_HAND_CARD_NUM)
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

            if (mMoney > MAX_MONEY)
            {
                mMoney = MAX_MONEY;
            }

            oMoney += ADD_MONEY;

            if (oMoney > MAX_MONEY)
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

                if (vo is BattleSummonVO)
                {
                    ClientDoSummon((BattleSummonVO)vo);
                }
                else if (vo is BattleMoveVO)
                {
                    ClientDoMove((BattleMoveVO)vo);
                }
                else if (vo is BattleRushVO)
                {
                    ClientDoRush((BattleRushVO)vo);
                }
                else if (vo is BattleShootVO)
                {
                    ClientDoShoot((BattleShootVO)vo);
                }
                else if (vo is BattleAttackVO)
                {
                    ClientDoAttack((BattleAttackVO)vo);
                }
                else if (vo is BattleDeathVO)
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

                if (isMine != hero.isMine)
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

            hero.HpChange(-_vo.damage);
        }

        private void ClientDoShoot(BattleShootVO _vo)
        {
            Hero hero = heroMapDic[_vo.stander];

            hero.HpChange(-_vo.damage);
        }

        private void ClientDoAttack(BattleAttackVO _vo)
        {
            for(int i = 0; i < _vo.attackers.Count; i++)
            {
                Hero hero = heroMapDic[_vo.attackers[i]];

                hero.HpChange(-_vo.attackersDamage[i]);
            }

            for (int i = 0; i < _vo.supporters.Count; i++)
            {
                Hero hero = heroMapDic[_vo.supporters[i]];

                hero.HpChange(-_vo.supportersDamage[i]);
            }

            if(_vo.defenderDamage > 0)
            {
                Hero hero = heroMapDic[_vo.defender];

                hero.HpChange(-_vo.defenderDamage);
            }
        }

        private void ClientDoDie(BattleDeathVO _vo)
        {
            for(int i = 0; i < _vo.deads.Count; i++)
            {
                heroMapDic.Remove(_vo.deads[i]);
            }
        }

        private void ClientDoRecover(BinaryReader _br)
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

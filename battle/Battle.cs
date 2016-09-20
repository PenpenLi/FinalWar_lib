using System;
using System.Collections.Generic;
using System.IO;
using superEvent;

namespace FinalWar
{
    public class Battle
    {
        internal static readonly Random random = new Random();

        internal static Dictionary<int, MapData> mapDataDic;
        internal static Dictionary<int, IHeroSDS> heroDataDic;
        internal static Dictionary<int, ISkillSDS> skillDataDic;

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
        private int heroUid;

        public bool mOver;
        public bool oOver;

        private Action<bool, MemoryStream> serverSendDataCallBack;

        public bool clientIsMine;

        private Action<MemoryStream> clientSendDataCallBack;
        private Action clientRefreshDataCallBack;
        private Action<IEnumerator<ValueType>> clientDoActionCallBack;

        internal SuperEventListener eventListener = new SuperEventListener();

        public static void Init(Dictionary<int, MapData> _mapDataDic, Dictionary<int, IHeroSDS> _heroDataDic, Dictionary<int, ISkillSDS> _skillDataDic)
        {
            mapDataDic = _mapDataDic;
            heroDataDic = _heroDataDic;
            skillDataDic = _skillDataDic;
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

            mapData = mapDataDic[_mapID];

            heroMapDic = new Dictionary<int, Hero>();

            mapBelongDic = new Dictionary<int, bool>();

            mScore = mapData.score1;
            oScore = mapData.score2;

            mMoney = oMoney = DEFAULT_MONEY;

            cardUid = 1;
            heroUid = 1;

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

                            if ((mapData.dic[pos] == _isMine) != mapBelongDic.ContainsKey(pos))
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

                            if ((mapData.dic[pos] == _isMine) != mapBelongDic.ContainsKey(pos))
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

        public bool ClientRequestAction(int _pos, int _targetPos)
        {
            Hero hero = heroMapDic[_pos];

            bool b = mapData.dic[_targetPos] != mapBelongDic.ContainsKey(_targetPos);

            List<int> tmpList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _pos);

            if (tmpList.Contains(_targetPos))
            {
                if(b == hero.isMine)
                {
                    if (hero.CheckCanDoAction(Hero.HeroAction.SUPPORT))
                    {
                        action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                        return true;
                    }
                }
                else
                {
                    if (hero.CheckCanDoAction(Hero.HeroAction.ATTACK))
                    {
                        action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                        return true;
                    }
                }
            }
            else
            {
                if(b != hero.isMine && heroMapDic.ContainsKey(_targetPos))
                {
                    tmpList = BattlePublicTools.GetNeighbourPos2(mapData.neighbourPosMap, _pos);

                    if (tmpList.Contains(_targetPos))
                    {
                        if (hero.CheckCanDoAction(Hero.HeroAction.SHOOT))
                        {
                            action.Add(new KeyValuePair<int, int>(_pos, _targetPos));

                            return true;
                        }
                    }
                }
            }

            return false;
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

                if (cards.ContainsKey(uid) && (mapData.dic[pos] == _isMine) != mapBelongDic.ContainsKey(pos))
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
            }
        }

        private Hero AddHero(bool _isMine, IHeroSDS _sds, int _pos, int _uid)
        {
            Hero hero = new Hero(this, _isMine, _sds, _pos, _uid);

            heroMapDic.Add(_pos, hero);

            return hero;
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

            ServerDoShoot(battleData, voList);

            ServerDoSummon(battleData, voList);

            summon.Clear();

            ServerDoRush(battleData, voList);

            ServerDoAttack(battleData, voList);

            ServerDoMove(battleData, voList);

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

        private void ServerDoSummon(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> powerChangeDic = new Dictionary<Hero, int>();

            Dictionary<int, int>.Enumerator enumerator = summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                int tmpCardUid = enumerator.Current.Key;

                int pos = enumerator.Current.Value;

                bool isMine = mapData.dic[pos] != mapBelongDic.ContainsKey(pos);

                Hero summonHero = SummonOneUnit(tmpCardUid, pos, isMine, _battleData);

                _voList.Add(new BattleSummonVO(tmpCardUid, summonHero.sds.GetID(), pos));

                List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, pos);

                for (int i = 0; i < posList.Count; i++)
                {
                    pos = posList[i];

                    if (!summon.ContainsValue(pos) && heroMapDic.ContainsKey(pos))
                    {
                        Hero hero = heroMapDic[pos];

                        if (hero.isMine == isMine)
                        {
                            int powerChange = hero.SummonHero();

                            BattlePublicTools.AccumulateDicData(powerChangeDic, hero, powerChange);
                        }
                    }
                }

                eventListener.DispatchEvent(HeroSkill.GetEventName(summonHero.uid, SkillTime.SUMMON), hpChangeDic, powerChangeDic);
            }

            ProcessHpChangeDic(_battleData, hpChangeDic, powerChangeDic, _voList);

            ProcessPowerChangeDic(_battleData, powerChangeDic, _voList);
        }

        private Hero SummonOneUnit(int _uid, int _pos, bool _isMine, BattleData _battleData)
        {
            int heroID;

            if (_isMine)
            {
                heroID = mHandCards[_uid];
            }
            else
            {
                heroID = oHandCards[_uid];
            }

            IHeroSDS sds = heroDataDic[heroID];

            if (_isMine)
            {
                mMoney -= sds.GetCost();

                mHandCards.Remove(_uid);
            }
            else
            {
                oMoney -= sds.GetCost();

                oHandCards.Remove(_uid);
            }

            Hero hero = AddHero(_isMine, sds, _pos, GetHeroUid());

            if (_battleData.actionDic.ContainsKey(_pos))
            {
                _battleData.actionDic[_pos].stander = hero;
            }

            return hero;
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
                if(enumerator.Current.CheckCanDoAction(Hero.HeroAction.DEFENSE))
                {
                    enumerator.Current.SetAction(Hero.HeroAction.DEFENSE);
                }
                else
                {
                    enumerator.Current.SetAction(Hero.HeroAction.NULL);
                }
            }

            for(int i = 0; i < shtList.Count; i++)
            {
                KeyValuePair<int, int> pair = shtList[i];

                Hero hero = heroMapDic[pair.Key];

                if(!hero.CheckCanDoAction(Hero.HeroAction.SHOOT))
                {
                    continue;
                }

                hero.SetAction(Hero.HeroAction.SHOOT, pair.Value);

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

                if (!hero.CheckCanDoAction(Hero.HeroAction.ATTACK))
                {
                    continue;
                }

                hero.SetAction(Hero.HeroAction.ATTACK, pair.Value);

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

                if (!hero.CheckCanDoAction(Hero.HeroAction.SUPPORT))
                {
                    continue;
                }

                hero.SetAction(Hero.HeroAction.SUPPORT, pair.Value);

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
            bool posIsMine = mapData.dic[_pos] != mapBelongDic.ContainsKey(_pos);

            bool targetPosIsMine = mapData.dic[_targetPos] != mapBelongDic.ContainsKey(_targetPos);

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

        private void ServerDoShoot(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> powerChangeDic = new Dictionary<Hero, int>();

            Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current;

                if (cellData.stander != null && cellData.shooters.Count > 0)
                {
                    for (int i = 0; i < cellData.shooters.Count; i++)
                    {
                        Hero shooter = cellData.shooters[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(shooter.uid, SkillTime.SHOOT), cellData.stander, hpChangeDic, powerChangeDic);
                    }
                }
            }

            enumerator = _battleData.actionDic.Values.GetEnumerator();

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

                        shooter.SetAction(Hero.HeroAction.NULL);

                        shooters.Add(shooter.pos);

                        int tmpDamage = shooter.GetShootDamage();

                        damage += tmpDamage;

                        int shooterPowerChange = shooter.Shoot();

                        BattlePublicTools.AccumulateDicData(powerChangeDic, shooter, shooterPowerChange);
                    }

                    if(damage > 0)
                    {
                        damage = cellData.stander.BeDamageByShoot(damage);

                        BattlePublicTools.AccumulateDicData(hpChangeDic, cellData.stander, -damage);
                    }

                    int powerChange = cellData.stander.BeShoot(cellData.shooters.Count);

                    BattlePublicTools.AccumulateDicData(powerChangeDic, cellData.stander, powerChange);

                    _voList.Add(new BattleShootVO(shooters, stander, damage));

                    cellData.shooters.Clear();
                }
            }

            ProcessHpChangeDic(_battleData, hpChangeDic, powerChangeDic, _voList);

            ProcessPowerChangeDic(_battleData, powerChangeDic, _voList);
        }

        private void ServerDoRush(BattleData _battleData, List<ValueType> _voList)
        {
            while (true)
            {
                bool quit = true;

                Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

                Dictionary<Hero, int> powerChangeDic = new Dictionary<Hero, int>();

                Dictionary<int, BattleCellData>.ValueCollection.Enumerator enumerator = _battleData.actionDic.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BattleCellData cellData = enumerator.Current;

                    if (cellData.stander != null && cellData.attackers.Count > 0 && cellData.stander.action != Hero.HeroAction.DEFENSE && cellData.supporters.Count == 0)
                    {
                        List<Hero> attackers = new List<Hero>(cellData.attackers);

                        for (int i = 0; i < cellData.attackers.Count; i++)
                        {
                            Hero attacker = cellData.attackers[i];

                            eventListener.DispatchEvent(HeroSkill.GetEventName(attacker.uid, SkillTime.ATTACK), attackers, new List<Hero>() { cellData.stander }, hpChangeDic, powerChangeDic);

                            eventListener.DispatchEvent(HeroSkill.GetEventName(attacker.uid, SkillTime.RUSH), attackers, cellData.stander, hpChangeDic, powerChangeDic);
                        }
                    }
                }

                enumerator = _battleData.actionDic.Values.GetEnumerator();

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

                            attacker.SetAction(Hero.HeroAction.ATTACKOVER);

                            attackers.Add(attacker.pos);

                            int tmpDamage = attacker.GetAttackDamage();

                            damage += tmpDamage;

                            int powerChange = attacker.Rush();

                            BattlePublicTools.AccumulateDicData(powerChangeDic, attacker, powerChange);
                        }

                        List<Hero> tmpList = cellData.attackers;

                        cellData.attackers = cellData.attackOvers;

                        cellData.attackOvers = tmpList;

                        if(damage > 0)
                        {
                            damage = cellData.stander.BeDamage(damage);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, cellData.stander, -damage);
                        }

                        int standerPowerChange = cellData.stander.BeRush(cellData.attackOvers.Count);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, cellData.stander, standerPowerChange);

                        _voList.Add(new BattleRushVO(attackers, stander, damage));
                    }
                }

                ProcessHpChangeDic(_battleData, hpChangeDic, powerChangeDic, _voList);

                ProcessPowerChangeDic(_battleData, powerChangeDic, _voList);

                if (quit)
                {
                    break;
                }
            }
        }

        private void ServerDoAttack(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> powerChangeDic = new Dictionary<Hero, int>();

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current.Value;

                if (cellData.attackers.Count > 0 && (cellData.stander != null || cellData.supporters.Count > 0))
                {
                    List<Hero> supporters = new List<Hero>(cellData.supporters);

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        supporters.Add(cellData.stander);
                    }

                    List<Hero> attackers = new List<Hero>(cellData.attackers);

                    for(int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.ATTACK), attackers, supporters, hpChangeDic, powerChangeDic);
                    }

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        eventListener.DispatchEvent(HeroSkill.GetEventName(cellData.stander.uid, SkillTime.COUNTER), attackers, supporters, hpChangeDic, powerChangeDic);
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.COUNTER), attackers, supporters, hpChangeDic, powerChangeDic);
                    }
                }
            }

            enumerator = _battleData.actionDic.GetEnumerator();

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

                    int attackDamage = 0;

                    int attackerNum = cellData.attackers.Count;

                    int defenderNum;

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        defenseDamage = cellData.stander.GetCounterDamage();

                        defenderNum = cellData.supporters.Count + 1;
                    }
                    else
                    {
                        defenseDamage = 0;

                        defenderNum = cellData.supporters.Count;
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        supporters.Add(hero.pos);

                        defenseDamage += hero.GetCounterDamage();
                    }

                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        attackers.Add(hero.pos);

                        attackDamage += hero.GetAttackDamage();
                    }

                    for (int i = 0; i < cellData.attackers.Count; i++)
                    {
                        Hero hero = cellData.attackers[i];

                        if (defenseDamage > 0)
                        {
                            int tmpDamage = hero.BeDamage(ref defenseDamage, hpChangeDic);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, hero, -tmpDamage);

                            attackersDamage.Add(tmpDamage);
                        }
                        else
                        {
                            attackersDamage.Add(0);
                        }

                        int powerChange = hero.Attack(attackerNum, defenderNum);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, hero, powerChange);
                    }

                    if (cellData.stander != null && cellData.stander.action == Hero.HeroAction.DEFENSE)
                    {
                        if (attackDamage > 0)
                        {
                            defenderDamage = cellData.stander.BeDamage(ref attackDamage, hpChangeDic);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, cellData.stander, -defenderDamage);
                        }

                        int powerChange = cellData.stander.BeAttack(attackerNum, defenderNum);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, cellData.stander, powerChange);
                    }

                    for (int i = 0; i < cellData.supporters.Count; i++)
                    {
                        Hero hero = cellData.supporters[i];

                        if (attackDamage > 0)
                        {
                            int tmpDamage = hero.BeDamage(ref attackDamage, hpChangeDic);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, hero, -tmpDamage);

                            supportersDamage.Add(tmpDamage);
                        }
                        else
                        {
                            supportersDamage.Add(0);
                        }

                        int powerChange = hero.BeAttack(attackerNum, defenderNum);

                        BattlePublicTools.AccumulateDicData(powerChangeDic, hero, powerChange);
                    }

                    if (cellData.stander != null && cellData.stander.action != Hero.HeroAction.DEFENSE)
                    {
                        if (attackDamage > 0)
                        {
                            defenderDamage = cellData.stander.BeDamage(attackDamage);

                            BattlePublicTools.AccumulateDicData(hpChangeDic, cellData.stander, -defenderDamage);
                        }

                        //int powerChange = cellData.stander.BeAttack(attackerNum, defenderNum);

                        //BattlePublicTools.AccumulateDicData(powerChangeDic, cellData.stander, powerChange);
                    }

                    _voList.Add(new BattleAttackVO(attackers, supporters, enumerator.Current.Key, attackersDamage, supportersDamage, defenderDamage));
                }
            }

            ProcessHpChangeDic(_battleData, hpChangeDic, powerChangeDic, _voList);

            ProcessPowerChangeDic(_battleData, powerChangeDic, _voList);
        }

        private void ServerDoMove(BattleData _battleData, List<ValueType> _voList)
        {
            Dictionary<Hero, int> hpChangeDic = new Dictionary<Hero, int>();

            Dictionary<Hero, int> powerChangeDic = new Dictionary<Hero, int>();

            List<int> tmpList = null;

            Dictionary<int, BattleCellData>.Enumerator enumerator = _battleData.actionDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                BattleCellData cellData = enumerator.Current.Value;

                if (cellData.stander == null && (cellData.supporters.Count > 0 || cellData.attackOvers.Count > 0 || cellData.attackers.Count > 0))
                {
                    if (tmpList == null)
                    {
                        tmpList = new List<int>();
                    }

                    tmpList.Add(enumerator.Current.Key);
                }
            }

            if (tmpList != null)
            {
                Dictionary<int, int> tmpMoveDic = new Dictionary<int, int>();

                for (int i = 0; i < tmpList.Count; i++)
                {
                    OneCellEmpty(_battleData, tmpList[i], tmpMoveDic, powerChangeDic);
                }

                _voList.Add(new BattleMoveVO(tmpMoveDic));
            }

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator2 = heroMapDic.Values.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                Hero hero = enumerator2.Current;

                eventListener.DispatchEvent(HeroSkill.GetEventName(hero.uid, SkillTime.RECOVER), hpChangeDic, powerChangeDic);
            }

            enumerator2 = heroMapDic.Values.GetEnumerator();

            while (enumerator2.MoveNext())
            {
                Hero hero = enumerator2.Current;

                int powerChange = hero.RecoverPower();

                BattlePublicTools.AccumulateDicData(powerChangeDic, hero, powerChange);
            }

            ProcessHpChangeDic(_battleData, hpChangeDic, powerChangeDic, _voList);

            ProcessPowerChangeDic(null, powerChangeDic, _voList);
        }

        private void DieHero(BattleData _battleData, Hero _hero, Dictionary<Hero, int> _powerChangeDic)
        {
            if (_powerChangeDic != null && _powerChangeDic.ContainsKey(_hero))
            {
                _powerChangeDic.Remove(_hero);
            }

            RemoveHeroAction(_battleData, _hero);

            heroMapDic.Remove(_hero.pos);

            _hero.Die();

            if (_battleData.actionDic.ContainsKey(_hero.pos))
            {
                _battleData.actionDic[_hero.pos].stander = null;
            }

            List<int> posList = BattlePublicTools.GetNeighbourPos(mapData.neighbourPosMap, _hero.pos);

            for(int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                if (heroMapDic.ContainsKey(pos))
                {
                    Hero hero = heroMapDic[pos];

                    if (hero.nowHp > 0)
                    {
                        int powerChange = hero.OtherHeroDie(_hero.isMine);

                        BattlePublicTools.AccumulateDicData(_powerChangeDic, hero, powerChange);
                    }
                }
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

        private void OneCellEmpty(BattleData _battleData, int _pos, Dictionary<int, int> _tmpMoveDic, Dictionary<Hero,int> _powerChangeDic)
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

                        bool b = mapData.dic[nowPos] != mapBelongDic.ContainsKey(nowPos);

                        Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = heroMapDic.Values.GetEnumerator();

                        while (enumerator.MoveNext())
                        {
                            Hero tmpHero = enumerator.Current;

                            int powerChange = tmpHero.MapBelongChange(b);

                            BattlePublicTools.AccumulateDicData(_powerChangeDic, tmpHero, powerChange);
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

        private int GetHeroUid()
        {
            int result = heroUid;

            heroUid++;

            return result;
        }

        private void ProcessHpChangeDic(BattleData _battleData, Dictionary<Hero,int> _hpChangeDic, Dictionary<Hero,int> _powerChangeDic, List<ValueType> _voList)
        {
            if (_hpChangeDic.Count > 0)
            {
                List<int> diePos = null;

                Dictionary<Hero, int>.Enumerator enumerator3 = _hpChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    bool isDie = hero.HpChange(pair.Value);

                    if (isDie)
                    {
                        if (diePos == null)
                        {
                            diePos = new List<int>();
                        }

                        diePos.Add(hero.pos);

                        DieHero(_battleData, hero, _powerChangeDic);
                    }
                }

                if (diePos != null)
                {
                    _voList.Add(new BattleDeathVO(diePos));
                }
            }
        }

        private void ProcessPowerChangeDic(BattleData _battleData, Dictionary<Hero,int> _powerChangeDic, List<ValueType> _voList)
        {
            if (_powerChangeDic.Count > 0)
            {
                List<int> posList = new List<int>();

                List<int> powerChangeList = new List<int>();

                List<bool> isDizzList = new List<bool>();

                Dictionary<Hero, int>.Enumerator enumerator3 = _powerChangeDic.GetEnumerator();

                while (enumerator3.MoveNext())
                {
                    KeyValuePair<Hero, int> pair = enumerator3.Current;

                    Hero hero = pair.Key;

                    posList.Add(hero.pos);

                    powerChangeList.Add(pair.Value);

                    if (_battleData != null)
                    {
                        bool isDizz = hero.PowerChange(pair.Value);

                        if (isDizz)
                        {
                            RemoveHeroAction(_battleData, hero);
                        }

                        isDizzList.Add(isDizz);
                    }
                    else
                    {
                        hero.PowerChange(pair.Value);

                        isDizzList.Add(false);
                    }
                }

                _voList.Add(new BattlePowerChangeVO(posList, powerChangeList, isDizzList));
            }
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
                else if (vo is BattlePowerChangeVO)
                {
                    ClientDoPowerChange((BattlePowerChangeVO)vo);
                }

                yield return vo;
            }

            ClientDoRecover(_br);
        }

        private void ClientDoSummon(BattleSummonVO _vo)
        {
            bool isMine = mapData.dic[_vo.pos] != mapBelongDic.ContainsKey(_vo.pos);

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

                bool isMine = mapData.dic[nowPos] != mapBelongDic.ContainsKey(nowPos);

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

            if (_vo.defenderDamage > 0)
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

        private void ClientDoPowerChange(BattlePowerChangeVO _vo)
        {
            for(int i = 0; i < _vo.pos.Count; i++)
            {
                Hero hero = heroMapDic[_vo.pos[i]];

                hero.PowerChange(_vo.powerChange[i]);
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

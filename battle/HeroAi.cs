using System.Collections.Generic;
using publicTools;

namespace FinalWar
{
    public class HeroAi
    {
        private enum BeState
        {
            WILL_BE_ATTACK,
            WILL_BE_SHOOT,
            WILL_BE_SUPPORT,
        }

        private enum CanState
        {
            CAN_ATTACK,
            CAN_SHOOT,
            CAN_SUPPORT,
        }

        private class HeroState
        {
            public Dictionary<BeState, List<int>> beState = new Dictionary<BeState, List<int>>();
            public Dictionary<CanState, List<int>> canState = new Dictionary<CanState, List<int>>();

            public bool done = false;
        }

        public static void Start(Battle _battle, bool _isMine, double _wrongValue)
        {
            ClearAction(_battle, _isMine);

            DoAction(_battle, _isMine, _wrongValue);

            DoSummon(_battle, _isMine, _wrongValue);
        }

        private static void DoAction(Battle _battle, bool _isMine, double _wrongValue)
        {
            List<Hero> myHeroList = null;

            Dictionary<Hero, HeroState> myHeroDic = null;

            Dictionary<int, List<int>> willBeAttackPos = new Dictionary<int, List<int>>();

            Dictionary<int, bool> hasBeenSupportedPos = new Dictionary<int, bool>();

            //doAction
            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = _battle.heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if (hero.isMine == _isMine)
                {
                    if (myHeroDic == null)
                    {
                        myHeroList = new List<Hero>();

                        myHeroDic = new Dictionary<Hero, HeroState>();
                    }

                    myHeroDic.Add(hero, new HeroState());

                    myHeroList.Add(hero);
                }
                else
                {
                    if (hero.CheckCanDoAction(Hero.HeroAction.ATTACK))
                    {
                        List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, hero.pos);

                        List<int> neighbourOppThreatPos = new List<int>();

                        List<int> neighbourOppPos = new List<int>();

                        for (int i = 0; i < posList.Count; i++)
                        {
                            int pos = posList[i];

                            bool mapIsMine = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                            if (mapIsMine == _isMine)
                            {
                                neighbourOppPos.Add(pos);

                                if (_battle.heroMapDic.ContainsKey(pos))
                                {
                                    Hero tmpHero = _battle.heroMapDic[pos];

                                    if (tmpHero.sds.GetThreat())
                                    {
                                        neighbourOppThreatPos.Add(pos);
                                    }
                                }
                            }
                        }

                        if (neighbourOppThreatPos.Count > 0)
                        {
                            neighbourOppPos = neighbourOppThreatPos;
                        }

                        for (int i = 0; i < neighbourOppPos.Count; i++)
                        {
                            int pos = neighbourOppPos[i];

                            List<int> tmpList;

                            if (willBeAttackPos.ContainsKey(pos))
                            {
                                tmpList = willBeAttackPos[pos];
                            }
                            else
                            {
                                tmpList = new List<int>();

                                willBeAttackPos.Add(pos, tmpList);
                            }

                            tmpList.Add(hero.pos);
                        }
                    }
                }
            }

            if (myHeroDic != null)
            {
                Dictionary<Hero, HeroState>.Enumerator enumerator2 = myHeroDic.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    KeyValuePair<Hero, HeroState> pair = enumerator2.Current;

                    if (willBeAttackPos.ContainsKey(pair.Key.pos))
                    {
                        pair.Value.beState.Add(BeState.WILL_BE_ATTACK, willBeAttackPos[pair.Key.pos]);
                    }

                    List<int> neighbourOppThreatPos = new List<int>();

                    List<int> neighbourOppPos = new List<int>();

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, pair.Key.pos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        bool mapIsMine = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                        if (mapIsMine != _isMine)
                        {
                            neighbourOppPos.Add(pos);

                            if (_battle.heroMapDic.ContainsKey(pos))
                            {
                                Hero tmpHero = _battle.heroMapDic[pos];

                                if (tmpHero.sds.GetThreat())
                                {
                                    neighbourOppThreatPos.Add(pos);
                                }
                            }
                        }
                    }

                    if (pair.Key.CheckCanDoAction(Hero.HeroAction.ATTACK))
                    {
                        if (neighbourOppThreatPos.Count > 0)
                        {
                            pair.Value.canState.Add(CanState.CAN_ATTACK, neighbourOppThreatPos);
                        }
                        else if (neighbourOppPos.Count > 0)
                        {
                            pair.Value.canState.Add(CanState.CAN_ATTACK, neighbourOppPos);
                        }
                    }

                    posList = BattlePublicTools.GetNeighbourPos2(_battle.mapData.neighbourPosMap, pair.Key.pos);

                    List<int> shootPos = new List<int>();

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        bool mapIsMine = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                        if (mapIsMine != _isMine)
                        {
                            if (_battle.heroMapDic.ContainsKey(pos))
                            {
                                shootPos.Add(pos);

                                Hero tmpHero = _battle.heroMapDic[pos];

                                if (tmpHero.CheckCanDoAction(Hero.HeroAction.SHOOT))
                                {
                                    List<int> tmpList;

                                    if (!pair.Value.beState.ContainsKey(BeState.WILL_BE_SHOOT))
                                    {
                                        tmpList = new List<int>();

                                        pair.Value.beState.Add(BeState.WILL_BE_SHOOT, tmpList);
                                    }
                                    else
                                    {
                                        tmpList = pair.Value.beState[BeState.WILL_BE_SHOOT];
                                    }

                                    tmpList.Add(pos);
                                }

                                if (pair.Key.CheckCanDoAction(Hero.HeroAction.SHOOT))
                                {
                                    List<int> tmpList;

                                    if (pair.Value.canState.ContainsKey(CanState.CAN_SHOOT))
                                    {
                                        tmpList = pair.Value.canState[CanState.CAN_SHOOT];
                                    }
                                    else
                                    {
                                        tmpList = new List<int>();

                                        pair.Value.canState.Add(CanState.CAN_SHOOT, tmpList);
                                    }

                                    tmpList.Add(pos);
                                }
                            }
                        }
                    }
                }

                enumerator2 = myHeroDic.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    KeyValuePair<Hero, HeroState> pair = enumerator2.Current;

                    if (pair.Key.CheckCanDoAction(Hero.HeroAction.SUPPORT))
                    {
                        List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, pair.Key.pos);

                        for (int i = 0; i < posList.Count; i++)
                        {
                            int pos = posList[i];

                            if (willBeAttackPos.ContainsKey(pos))
                            {
                                List<int> tmpList;

                                if (pair.Value.canState.ContainsKey(CanState.CAN_SUPPORT))
                                {
                                    tmpList = pair.Value.canState[CanState.CAN_SUPPORT];
                                }
                                else
                                {
                                    tmpList = new List<int>();

                                    pair.Value.canState.Add(CanState.CAN_SUPPORT, tmpList);
                                }

                                tmpList.Add(pos);

                                if (_battle.heroMapDic.ContainsKey(pos))
                                {
                                    Hero tmpHero = _battle.heroMapDic[pos];

                                    HeroState state = myHeroDic[tmpHero];

                                    if (state.beState.ContainsKey(BeState.WILL_BE_SUPPORT))
                                    {
                                        tmpList = state.beState[BeState.WILL_BE_SUPPORT];
                                    }
                                    else
                                    {
                                        tmpList = new List<int>();

                                        state.beState.Add(BeState.WILL_BE_SUPPORT, tmpList);
                                    }

                                    tmpList.Add(pos);
                                }
                            }
                        }
                    }
                }

                //具体做动作
                PublicTools.ShuffleList(myHeroList, Battle.random);

                for (int i = 0; i < myHeroList.Count; i++)
                {
                    Hero hero = myHeroList[i];

                    HeroState state = myHeroDic[hero];

                    if (state.done)
                    {
                        continue;
                    }
                    else
                    {
                        state.done = true;
                    }

                    if (hero.CheckCanDoAction(Hero.HeroAction.SUPPORT))
                    {
                        if (state.canState.Count > 0)
                        {
                            if (!state.beState.ContainsKey(BeState.WILL_BE_ATTACK) || hasBeenSupportedPos.ContainsKey(hero.pos))
                            {
                                CheckDoAction(_battle, _isMine, _wrongValue, hero, state, hasBeenSupportedPos);
                            }
                            else
                            {
                                if (state.beState.ContainsKey(BeState.WILL_BE_SUPPORT))
                                {
                                    List<int> tmpList = state.beState[BeState.WILL_BE_SUPPORT];

                                    PublicTools.ShuffleList(tmpList, Battle.random);

                                    for (int m = 0; m < tmpList.Count; m++)
                                    {
                                        int pos = tmpList[m];

                                        Hero tmpHero = _battle.heroMapDic[pos];

                                        HeroState tmpState = myHeroDic[tmpHero];

                                        if (!tmpState.done)
                                        {
                                            if (Battle.random.NextDouble() < 0.5)
                                            {
                                                _battle.action.Add(new KeyValuePair<int, int>(pos, hero.pos));

                                                tmpState.done = true;
                                            }
                                        }
                                    }
                                }

                                if (Battle.random.NextDouble() < 0.5)
                                {
                                    CheckDoAction(_battle, _isMine, _wrongValue, hero, state, hasBeenSupportedPos);
                                }
                            }
                        }
                        else
                        {
                            int targetPos = _isMine ? _battle.mapData.base2 : _battle.mapData.base1;

                            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, hero.pos);

                            List<int> resultPos = new List<int>();

                            int minDis = int.MaxValue;

                            for (int m = 0; m < posList.Count; m++)
                            {
                                int pos = posList[m];

                                

                                int dis = BattlePublicTools.GetDistance(_battle.mapData.mapWidth, pos, targetPos);

                                if (dis < minDis)
                                {
                                    resultPos.Clear();

                                    resultPos.Add(pos);

                                    minDis = dis;
                                }
                                else if (dis == minDis)
                                {
                                    resultPos.Add(pos);
                                }
                            }

                            if (resultPos.Count > 0)
                            {
                                List<double> randomList = new List<double>();

                                for (int m = 0; m < resultPos.Count; m++)
                                {
                                    randomList.Add(1);
                                }

                                int index = PublicTools.Choose(randomList, Battle.random);

                                _battle.action.Add(new KeyValuePair<int, int>(hero.pos, resultPos[index]));
                            }
                        }
                    }
                }
            }
        }

        private static void DoSummon(Battle _battle, bool _isMine, double _wrongValue)
        { 
            //---summon
            int money;
            Dictionary<int, int> handCards;
            int oppBasePos;

            if (_isMine)
            {
                oppBasePos = _battle.mapData.base2;
                money = _battle.mMoney;
                handCards = _battle.mHandCards;
            }
            else
            {
                oppBasePos = _battle.mapData.base1;
                money = _battle.oMoney;
                handCards = _battle.oHandCards;
            }

            List<int> cards = new List<int>();
            List<double> randomList2 = new List<double>();

            Dictionary<int, int>.Enumerator enumerator4 = handCards.GetEnumerator();

            while (enumerator4.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator4.Current;

                int cardID = pair.Value;

                IHeroSDS heroSDS = Battle.heroDataDic[cardID];

                if(heroSDS.GetCost() <= money)
                {
                    cards.Add(pair.Key);

                    randomList2.Add(1);
                }
            }

            if(cards.Count > 0)
            {
                List<int> resultList = new List<int>();

                List<int> resultList2 = new List<int>();

                List<int> nowCheckPos = new List<int>() { oppBasePos };

                Dictionary<int, bool> checkedPos = new Dictionary<int, bool>();

                checkedPos.Add(oppBasePos, false);

                while (nowCheckPos.Count > 0)
                {
                    int nowPos = nowCheckPos[0];

                    nowCheckPos.RemoveAt(0);

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, nowPos);

                    for (int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        if (!checkedPos.ContainsKey(pos))
                        {
                            checkedPos.Add(pos, false);

                            bool b = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                            if (b == _isMine)
                            {
                                if (!_battle.heroMapDic.ContainsKey(pos))
                                {
                                    resultList.Add(pos);
                                }
                            }
                            else
                            {
                                nowCheckPos.Add(pos);
                            }
                        }
                    }
                }

                for (int i = 0; i < resultList.Count; i++)
                {
                    int nowPos = resultList[i];

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, nowPos);

                    for (int m = 0; m < posList.Count; m++)
                    {
                        int pos = posList[m];

                        if(!_battle.heroMapDic.ContainsKey(pos) && !resultList.Contains(pos) && !resultList2.Contains(pos))
                        {
                            bool b = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                            if (b == _isMine)
                            {
                                resultList2.Add(pos);
                            }
                        }
                    }
                }

                PublicTools.ShuffleList(cards, Battle.random);

                while (Battle.random.NextDouble() < 0.8 && cards.Count > 0 && (resultList.Count > 0 || resultList2.Count > 0))
                {
                    int cardUid = cards[0];

                    cards.RemoveAt(0);

                    randomList2.RemoveAt(0);

                    int cardID = handCards[cardUid];

                    IHeroSDS heroSDS = Battle.heroDataDic[cardID];

                    if(heroSDS.GetCost() <= money)
                    {
                        List<int> summonPosList;

                        if(resultList.Count > 0 && resultList2.Count > 0)
                        {
                            if (Battle.random.NextDouble() < 0.6)
                            {
                                summonPosList = resultList;
                            }
                            else
                            {
                                summonPosList = resultList2;
                            }
                        }
                        else if(resultList.Count > 0)
                        {
                            summonPosList = resultList;
                        }
                        else
                        {
                            summonPosList = resultList2;
                        }

                        int index = (int)(Battle.random.NextDouble() * summonPosList.Count);

                        int summonPos = summonPosList[index];

                        summonPosList.RemoveAt(index);

                        _battle.summon.Add(cardUid, summonPos);

                        money -= heroSDS.GetCost();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

         

        private static void CheckDoAction(Battle _battle, bool _isMine, double _wrongValue, Hero _hero, HeroState _state, Dictionary<int,bool> _hasBeenSupportedPos)
        {
            List<CanState> canList = new List<CanState>();

            List<double> randomList = new List<double>();

            if (_state.canState.ContainsKey(CanState.CAN_ATTACK))
            {
                canList.Add(CanState.CAN_ATTACK);

                randomList.Add(1);
            }

            if (_state.canState.ContainsKey(CanState.CAN_SHOOT))
            {
                canList.Add(CanState.CAN_SHOOT);

                randomList.Add(1);
            }

            if (_state.canState.ContainsKey(CanState.CAN_SUPPORT))
            {
                canList.Add(CanState.CAN_SUPPORT);

                randomList.Add(1);
            }

            randomList.Add(_wrongValue);

            int index = PublicTools.Choose(randomList, Battle.random);

            if (index < canList.Count)
            {
                CanState tmpState = canList[index];

                List<int> targetPosList = _state.canState[tmpState];

                randomList.Clear();

                for (int m = 0; m < targetPosList.Count; m++)
                {
                    randomList.Add(1);
                }

                index = PublicTools.Choose(randomList, Battle.random);

                int targetPos = targetPosList[index];

                _battle.action.Add(new KeyValuePair<int, int>(_hero.pos, targetPos));

                if (tmpState == CanState.CAN_SUPPORT)
                {
                    if (!_hasBeenSupportedPos.ContainsKey(targetPos))
                    {
                        _hasBeenSupportedPos.Add(targetPos, false);
                    }
                }
            }
        }

        private static void ClearAction(Battle _battle, bool _isMine)
        {
            List<int> delList = null;

            Dictionary<int, int>.Enumerator enumerator = _battle.summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int pos = pair.Value;

                bool b = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                if(b == _isMine)
                {
                    if(delList == null)
                    {
                        delList = new List<int>();
                    }

                    delList.Add(pair.Key);
                }
            }

            if(delList != null)
            {
                for (int i = 0; i < delList.Count; i++)
                {
                    _battle.summon.Remove(delList[i]);
                }
            }

            for(int i = _battle.action.Count - 1; i > -1; i--)
            {
                KeyValuePair<int, int> pair = _battle.action[i];

                int pos = pair.Key;

                bool b = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                if (b == _isMine)
                {
                    _battle.action.RemoveAt(i);
                }
            }
        }
    }
}

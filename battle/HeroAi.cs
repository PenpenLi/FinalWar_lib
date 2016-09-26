using System.Collections.Generic;
using publicTools;

namespace FinalWar
{
    internal class HeroAi
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

        internal static void Start(Battle _battle, bool _isMine, double _wrongValue)
        {
            ClearAction(_battle, _isMine);

            List<Hero> myHeroList= null;

            Dictionary<Hero, HeroState> myHeroDic = null;

            Dictionary<int, List<int>> willBeAttackPos = new Dictionary<int, List<int>>();

            Dictionary<int, bool> hasBeenSupportedPos = new Dictionary<int, bool>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = _battle.heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if(hero.isMine == _isMine)
                {
                    if(myHeroDic == null)
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

                            if(mapIsMine == _isMine)
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

                        if(neighbourOppThreatPos.Count > 0)
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
                            }
                        }
                    }

                    if (pair.Key.CheckCanDoAction(Hero.HeroAction.SHOOT))
                    {
                        pair.Value.canState.Add(CanState.CAN_SHOOT, shootPos);
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

                for(int i = 0; i < myHeroList.Count; i++)
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

                    if (state.canState.Count > 0)
                    {
                        if (!state.beState.ContainsKey(BeState.WILL_BE_ATTACK))
                        {
                            List<CanState> canList = new List<CanState>();

                            List<double> randomList = new List<double>();

                            if (state.canState.ContainsKey(CanState.CAN_ATTACK))
                            {
                                canList.Add(CanState.CAN_ATTACK);

                                randomList.Add(1);
                            }

                            if (state.canState.ContainsKey(CanState.CAN_SHOOT))
                            {
                                canList.Add(CanState.CAN_SHOOT);

                                randomList.Add(1);
                            }

                            if (state.canState.ContainsKey(CanState.CAN_SUPPORT))
                            {
                                canList.Add(CanState.CAN_SUPPORT);

                                randomList.Add(1);
                            }

                            randomList.Add(_wrongValue);

                            int index = PublicTools.Choose(randomList, Battle.random);

                            if (index < canList.Count)
                            {
                                CanState tmpState = canList[index];

                                List<int> targetPosList = state.canState[tmpState];

                                randomList.Clear();

                                for(int m = 0; m < targetPosList.Count; m++)
                                {
                                    randomList.Add(1);
                                }

                                index = PublicTools.Choose(randomList, Battle.random);

                                int targetPos = targetPosList[index];

                                _battle.action.Add(new KeyValuePair<int, int>(hero.pos, targetPos));

                                if(tmpState == CanState.CAN_SUPPORT)
                                {
                                    if (!hasBeenSupportedPos.ContainsKey(targetPos))
                                    {
                                        hasBeenSupportedPos.Add(targetPos, false);
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        private static void ClearAction(Battle _battle, bool _isMine)
        {
            List<int> delList = new List<int>();

            Dictionary<int, int>.Enumerator enumerator = _battle.summon.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int pos = pair.Value;

                bool b = _battle.mapData.dic[pos] != _battle.mapBelongDic.ContainsKey(pos);

                if(b == _isMine)
                {
                    delList.Add(pair.Key);
                }
            }

            for(int i = 0; i < delList.Count; i++)
            {
                _battle.summon.Remove(delList[i]);
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

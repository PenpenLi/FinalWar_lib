using System;
using System.Collections.Generic;

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
        }

        internal static void Start(Battle _battle, bool _isMine)
        {
            Dictionary<Hero, HeroState> myHeros = null;

            Dictionary<int, List<int>> willBeAttackPos = new Dictionary<int, List<int>>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = _battle.heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if(hero.isMine == _isMine)
                {
                    if(myHeros == null)
                    {
                        myHeros = new Dictionary<Hero, HeroState>();
                    }

                    myHeros.Add(hero, new HeroState());
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

            if(myHeros != null)
            {
                Dictionary<Hero, HeroState>.Enumerator enumerator2 = myHeros.GetEnumerator();

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

                enumerator2 = myHeros.GetEnumerator();

                while (enumerator2.MoveNext())
                {

                }
            }
        }
    }
}

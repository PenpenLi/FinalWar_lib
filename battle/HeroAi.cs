using System.Collections.Generic;
using publicTools;

namespace FinalWar
{
    public class HeroAi
    {
        public static void Start(Battle _battle, bool _isMine, double _wrongValue)
        {
            ClearAction(_battle, _isMine);

            DoAction(_battle, _isMine, _wrongValue);

            DoSummon(_battle, _isMine, _wrongValue);
        }

        private static void DoAction(Battle _battle, bool _isMine, double _wrongValue)
        {
            List<KeyValuePair<int, int>> action = new List<KeyValuePair<int, int>>();

            Dictionary<int, Hero>.ValueCollection.Enumerator enumerator = _battle.heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if (hero.isMine == _isMine)
                {
                    if (hero.sds.GetCanControl())
                    {
                        if (_battle.CheckPosCanBeAttack(hero.pos))
                        {
                            if (Battle.random.NextDouble() < 0.5)
                            {
                                continue;
                            }
                        }

                        List<int> result = null;

                        List<int> posList = _battle.GetCanAttackPos(hero);

                        if (posList.Count > 0)
                        {
                            result = posList;
                        }

                        posList = _battle.GetCanAttackerHeroPos(hero);

                        if (posList.Count > 0)
                        {
                            result.InsertRange(result.Count, posList);
                        }

                        if (hero.sds.GetAbilityType() == AbilityType.Shoot)
                        {
                            posList = _battle.GetCanShootPos(hero);

                            if (posList.Count > 0)
                            {
                                if (result != null)
                                {
                                    result.InsertRange(result.Count, posList);
                                }
                                else
                                {
                                    result = posList;
                                }
                            }
                        }

                        posList = _battle.GetCanSupportPos(hero);

                        if (posList.Count > 0)
                        {
                            if (result != null)
                            {
                                result.InsertRange(result.Count, posList);
                            }
                            else
                            {
                                result = posList;
                            }
                        }

                        posList = _battle.GetCanSupportHeroPos(hero);

                        if (posList.Count > 0)
                        {
                            result.InsertRange(result.Count, posList);
                        }

                        if (result != null)
                        {
                            int index = Battle.random.Next(result.Count);

                            action.Add(new KeyValuePair<int, int>(hero.pos, result[index]));
                        }
                        else
                        {
                            int targetPos;

                            if (hero.isMine)
                            {
                                targetPos = _battle.mapData.moveMap[hero.pos].Key;
                            }
                            else
                            {
                                targetPos = _battle.mapData.moveMap[hero.pos].Value;
                            }

                            action.Add(new KeyValuePair<int, int>(hero.pos, targetPos));
                        }
                    }
                    else
                    {
                        if (_battle.autoAction.ContainsKey(hero.pos))
                        {
                            int targetPos = _battle.autoAction[hero.pos];

                            if (targetPos != hero.pos)
                            {
                                action.Add(new KeyValuePair<int, int>(hero.pos, targetPos));
                            }
                        }
                    }
                }
            }

            PublicTools.ShuffleList(action, Battle.random);

            _battle.action.InsertRange(_battle.action.Count, action);
        }

        private static void DoSummon(Battle _battle, bool _isMine, double _wrongValue)
        {
            //---summon
            int money;
            Dictionary<int, int> handCards;
            int oppBasePos;

            if (_isMine)
            {
                oppBasePos = _battle.mapData.oBase;
                money = _battle.mMoney;
                handCards = _battle.mHandCards;
            }
            else
            {
                oppBasePos = _battle.mapData.mBase;
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

                IHeroSDS heroSDS = Battle.GetHeroData(cardID);

                if (heroSDS.GetCost() <= money)
                {
                    cards.Add(pair.Key);

                    randomList2.Add(1);
                }
            }

            if (cards.Count > 0)
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

                    LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, nowPos);

                    LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

                        if (!checkedPos.ContainsKey(pos))
                        {
                            checkedPos.Add(pos, false);

                            bool b = _battle.GetPosIsMine(pos);

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

                    LinkedList<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, nowPos);

                    LinkedList<int>.Enumerator enumerator = posList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int pos = enumerator.Current;

                        if (!_battle.heroMapDic.ContainsKey(pos) && !resultList.Contains(pos) && !resultList2.Contains(pos))
                        {
                            bool b = _battle.GetPosIsMine(pos);

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

                    IHeroSDS heroSDS = Battle.GetHeroData(cardID);

                    if (heroSDS.GetCost() <= money)
                    {
                        List<int> summonPosList;

                        if (resultList.Count > 0 && resultList2.Count > 0)
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
                        else if (resultList.Count > 0)
                        {
                            summonPosList = resultList;
                        }
                        else
                        {
                            summonPosList = resultList2;
                        }

                        int index = Battle.random.Next(summonPosList.Count);

                        int summonPos = summonPosList[index];

                        summonPosList.RemoveAt(index);

                        _battle.summon.Add(cardUid, summonPos);

                        money -= heroSDS.GetCost();
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

                bool b = _battle.GetPosIsMine(pos);

                if (b == _isMine)
                {
                    if (delList == null)
                    {
                        delList = new List<int>();
                    }

                    delList.Add(pair.Key);
                }
            }

            if (delList != null)
            {
                for (int i = 0; i < delList.Count; i++)
                {
                    _battle.summon.Remove(delList[i]);
                }
            }

            for (int i = _battle.action.Count - 1; i > -1; i--)
            {
                KeyValuePair<int, int> pair = _battle.action[i];

                int pos = pair.Key;

                bool b = _battle.GetPosIsMine(pos);

                if (b == _isMine)
                {
                    _battle.action.RemoveAt(i);
                }
            }
        }
    }
}

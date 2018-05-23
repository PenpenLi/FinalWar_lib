using System.Collections.Generic;
using bt;
using System;
using System.Linq;
using System.Reflection;

namespace FinalWar
{
    public static class BattleAi
    {
        private static BtRoot<Battle, Hero, AiActionData> actionBtRoot;

        private static BtRoot<Battle, bool, AiSummonData> summonBtRoot;

        private static AiActionData aiActionData;

        private static AiSummonData aiSummonData;

        public static void Init(string _actionStr, string _summonStr)
        {
            bt.Log.Init(Log.Write);

            actionBtRoot = BtTools.Create<Battle, Hero, AiActionData>(_actionStr, Assembly.GetExecutingAssembly().FullName);

            summonBtRoot = BtTools.Create<Battle, bool, AiSummonData>(_summonStr, Assembly.GetExecutingAssembly().FullName);

            aiActionData = new AiActionData();

            aiSummonData = new AiSummonData();
        }

        public static void Start(Battle _battle, bool _isMine, Func<int, int> _getRandomValueCallBack, Dictionary<int, int> _action, Dictionary<int, int> _summon)
        {
            ActionHero(_battle, _isMine, _getRandomValueCallBack, _action, _summon);

            SummonHero(_battle, _isMine, _getRandomValueCallBack, _action, _summon);
        }

        private static void ActionHero(Battle _battle, bool _isMine, Func<int, int> _getRandomValueCallBack, Dictionary<int, int> _action, Dictionary<int, int> _summon)
        {
            aiActionData.action = _action;

            aiActionData.summon = _summon;

            List<Hero> heroList = null;

            IEnumerator<Hero> enumerator = _battle.heroMapDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Hero hero = enumerator.Current;

                if (hero.isMine == _isMine)
                {
                    if (heroList == null)
                    {
                        heroList = new List<Hero>();
                    }

                    heroList.Add(hero);
                }
            }

            if (heroList != null)
            {
                while (heroList.Count > 0)
                {
                    int index = _getRandomValueCallBack(heroList.Count);

                    Hero hero = heroList[index];

                    heroList.RemoveAt(index);

                    actionBtRoot.Enter(_getRandomValueCallBack, _battle, hero, aiActionData);

                    aiActionData.dic.Clear();
                }
            }

            aiActionData.action = null;

            aiActionData.summon = null;
        }

        private static void SummonHero(Battle _battle, bool _isMine, Func<int, int> _getRandomValueCallBack, Dictionary<int, int> _action, Dictionary<int, int> _summon)
        {
            aiSummonData.action = _action;

            aiSummonData.summon = _summon;

            summonBtRoot.Enter(_getRandomValueCallBack, _battle, _isMine, aiSummonData);

            aiSummonData.action = null;

            aiSummonData.summon = null;
        }








        public static Dictionary<int, List<int>> GetSummonPosToEmemyAreaList(Battle _battle, bool _isMine, int _max, Dictionary<int, int> _summon, Dictionary<int, int> _action)
        {
            int startPos = _isMine ? _battle.mapData.oBase : _battle.mapData.mBase;

            Dictionary<int, int> close = new Dictionary<int, int>();

            Dictionary<int, int> open = new Dictionary<int, int>();

            open.Add(startPos, -1);

            while (open.Count > 0)
            {
                KeyValuePair<int, int> pair = open.ElementAt(0);

                int pos = pair.Key;

                int range = pair.Value;

                open.Remove(pos);

                close.Add(pos, range);

                List<int> list = BattlePublicTools.GetNeighbourPos(_battle.mapData, pos);

                for (int i = 0; i < list.Count; i++)
                {
                    int tmpPos = list[i];

                    int nowRange;

                    if (_battle.GetPosIsMine(tmpPos) == _isMine)
                    {
                        nowRange = range + 1;
                    }
                    else
                    {
                        nowRange = -1;
                    }

                    int oldRange;

                    if (open.TryGetValue(tmpPos, out oldRange))
                    {
                        if (oldRange > nowRange)
                        {
                            open[tmpPos] = nowRange;
                        }

                        continue;
                    }

                    if (close.TryGetValue(tmpPos, out oldRange))
                    {
                        if (oldRange > nowRange)
                        {
                            close[tmpPos] = nowRange;
                        }

                        continue;
                    }

                    open.Add(tmpPos, nowRange);
                }
            }

            Dictionary<int, List<int>> result = null;

            IEnumerator<KeyValuePair<int, int>> enumerator = close.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int pos = pair.Key;

                int range = pair.Value;

                if (range > -1 && range < _max && !_summon.ContainsKey(pos) && _battle.CheckPosCanSummon(_isMine, pos) == -1 && !_action.ContainsValue(pos))
                {
                    if (result == null)
                    {
                        result = new Dictionary<int, List<int>>();
                    }

                    List<int> tmpList;

                    if (!result.TryGetValue(range, out tmpList))
                    {
                        tmpList = new List<int>();

                        result.Add(range, tmpList);
                    }

                    tmpList.Add(pos);
                }
            }

            return result;
        }

        public static Dictionary<int, List<int>> GetSummonPosToEmemyHeroList(Battle _battle, bool _isMine, int _max, Dictionary<int, int> _summon, Dictionary<int, int> _action)
        {
            Dictionary<int, int> close = new Dictionary<int, int>();

            Dictionary<int, int> open = new Dictionary<int, int>();

            IEnumerator<KeyValuePair<int, Hero>> enumerator = _battle.heroMapDic.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value.isMine != _isMine)
                {
                    open.Add(enumerator.Current.Key, -1);
                }
            }

            while (open.Count > 0)
            {
                KeyValuePair<int, int> pair = open.ElementAt(0);

                int pos = pair.Key;

                int range = pair.Value;

                open.Remove(pos);

                close.Add(pos, range);

                List<int> list = BattlePublicTools.GetNeighbourPos(_battle.mapData, pos);

                for (int i = 0; i < list.Count; i++)
                {
                    int nowRange;

                    int tmpPos = list[i];

                    if (_battle.GetPosIsMine(tmpPos) == _isMine)
                    {
                        nowRange = range + 1;
                    }
                    else
                    {
                        nowRange = -1;
                    }

                    int oldRange;

                    if (open.TryGetValue(tmpPos, out oldRange))
                    {
                        if (oldRange > nowRange)
                        {
                            open[tmpPos] = nowRange;
                        }

                        continue;
                    }

                    if (close.TryGetValue(tmpPos, out oldRange))
                    {
                        if (oldRange > nowRange)
                        {
                            close[tmpPos] = nowRange;
                        }

                        continue;
                    }

                    open.Add(tmpPos, nowRange);
                }
            }

            Dictionary<int, List<int>> result = null;

            IEnumerator<KeyValuePair<int, int>> enumerator2 = close.GetEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator2.Current;

                int pos = pair.Key;

                int range = pair.Value;

                if (range > -1 && range < _max && !_summon.ContainsKey(pos) && _battle.CheckPosCanSummon(_isMine, pos) == -1 && !_action.ContainsValue(pos))
                {
                    if (result == null)
                    {
                        result = new Dictionary<int, List<int>>();
                    }

                    List<int> tmpList;

                    if (!result.TryGetValue(range, out tmpList))
                    {
                        tmpList = new List<int>();

                        result.Add(range, tmpList);
                    }

                    tmpList.Add(pos);
                }
            }

            return result;
        }
    }
}
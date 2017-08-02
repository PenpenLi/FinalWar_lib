#if !CLIENT

using System.Collections.Generic;

namespace FinalWar
{
    public static class HeroAi
    {
        public static void Start(Battle _battle, bool _isMine)
        {
            ClearAction(_battle, _isMine);

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









        public static List<int> GetCanAttackHeroPos(Battle _battle, Hero _hero)
        {
            List<int> result = null;

            int nowThreadLevel = 0;

            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = _battle.GetPosIsMine(pos);

                if (b != _hero.isMine)
                {
                    Hero hero;

                    if (_battle.heroMapDic.TryGetValue(pos, out hero))
                    {
                        if (hero.sds.GetHeroType().GetThread() > nowThreadLevel)
                        {
                            nowThreadLevel = hero.sds.GetHeroType().GetThread();

                            if (result == null)
                            {
                                result = new List<int>();
                            }
                            else
                            {
                                result.Clear();
                            }

                            result.Add(pos);
                        }
                        else if (hero.sds.GetHeroType().GetThread() == nowThreadLevel)
                        {
                            result.Add(pos);
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> GetCanAttackPos(Battle _battle, Hero _hero)
        {
            List<int> result = null;

            int nowThreadLevel = 0;

            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = _battle.GetPosIsMine(pos);

                if (b != _hero.isMine)
                {
                    Hero hero;

                    if (_battle.heroMapDic.TryGetValue(pos, out hero))
                    {
                        if (hero.sds.GetHeroType().GetThread() > nowThreadLevel)
                        {
                            nowThreadLevel = hero.sds.GetHeroType().GetThread();

                            if (result == null)
                            {
                                result = new List<int>();
                            }
                            else
                            {
                                result.Clear();
                            }

                            result.Add(pos);
                        }
                        else if (hero.sds.GetHeroType().GetThread() == nowThreadLevel)
                        {
                            if (result == null)
                            {
                                result = new List<int>();
                            }

                            result.Add(pos);
                        }
                    }
                    else
                    {
                        if (nowThreadLevel == 0)
                        {
                            if (result == null)
                            {
                                result = new List<int>();
                            }

                            result.Add(pos);
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> GetCanShootHeroPos(Battle _battle, Hero _hero)
        {
            List<int> result = new List<int>();

            if (_hero.sds.GetSkill() != 0)
            {
                List<int> posList = BattlePublicTools.GetNeighbourPos2(_battle.mapData, _hero.pos);

                for (int i = 0; i < posList.Count; i++)
                {
                    int pos = posList[i];

                    bool b = _battle.GetPosIsMine(pos);

                    if (b != _hero.isMine && _battle.heroMapDic.ContainsKey(pos))
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }

        public static List<int> GetCanThrowHeroPos(Battle _battle, Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos3(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                bool b = _battle.GetPosIsMine(pos);

                if (b != _hero.isMine && _battle.heroMapDic.ContainsKey(pos))
                {
                    result.Add(pos);
                }
            }

            return result;
        }

        public static bool CheckHeroCanBeAttack(Battle _battle, Hero _hero)
        {
            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                if (_battle.GetPosIsMine(pos) != _hero.isMine)
                {
                    Hero hero;

                    if (_battle.heroMapDic.TryGetValue(pos, out hero))
                    {
                        List<int> tmpList = GetCanAttackHeroPos(_battle, hero);

                        if (tmpList.Contains(_hero.pos))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool CheckPosCanBeAttack(Battle _battle, int _pos)
        {
            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                if (_battle.GetPosIsMine(pos) != _battle.GetPosIsMine(_pos))
                {
                    Hero hero;

                    if (_battle.heroMapDic.TryGetValue(pos, out hero))
                    {
                        List<int> tmpList = GetCanAttackPos(_battle, hero);

                        if (tmpList.Contains(_pos))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static List<int> GetCanSupportHeroPos(Battle _battle, Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                if (_battle.GetPosIsMine(pos) == _hero.isMine)
                {
                    Hero hero;

                    if (_battle.heroMapDic.TryGetValue(pos, out hero))
                    {
                        if (CheckHeroCanBeAttack(_battle, hero))
                        {
                            result.Add(pos);
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> GetCanSupportPos(Battle _battle, Hero _hero)
        {
            List<int> result = new List<int>();

            List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

            for (int i = 0; i < posList.Count; i++)
            {
                int pos = posList[i];

                if (_battle.GetPosIsMine(pos) == _hero.isMine)
                {
                    if (CheckPosCanBeAttack(_battle, pos))
                    {
                        result.Add(pos);
                    }
                }
            }

            return result;
        }
    }
}
#endif

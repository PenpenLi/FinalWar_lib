using System.Collections.Generic;
using bt;
using System.Xml;
using System;
using System.Linq;

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
            actionBtRoot = BtTools.Create(_actionStr, GetActionConditionNode, GetActionActionNode);

            summonBtRoot = BtTools.Create(_summonStr, GetSummonConditionNode, GetSummonActionNode);

            aiActionData = new AiActionData();

            aiSummonData = new AiSummonData();
        }

        public static void Start(Battle _battle, bool _isMine, Func<int, int> _getRandomValueCallBack)
        {
            ClearAction(_battle, _isMine);

            ActionHero(_battle, _isMine, _getRandomValueCallBack);

            SummonHero(_battle, _isMine, _getRandomValueCallBack);
        }

        private static void ActionHero(Battle _battle, bool _isMine, Func<int, int> _getRandomValueCallBack)
        {
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
        }

        private static void SummonHero(Battle _battle, bool _isMine, Func<int, int> _getRandomValueCallBack)
        {
            summonBtRoot.Enter(_getRandomValueCallBack, _battle, _isMine, aiSummonData);
        }

        private static void ClearAction(Battle _battle, bool _isMine)
        {
            List<int> delList = null;

            IEnumerator<KeyValuePair<int, int>> enumerator = _battle.GetSummonEnumerator();

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
                    _battle.DelSummon(delList[i]);
                }

                delList.Clear();
            }

            enumerator = _battle.GetActionEnumerator();

            while (enumerator.MoveNext())
            {
                KeyValuePair<int, int> pair = enumerator.Current;

                int pos = pair.Key;

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
                    _battle.DelAction(delList[i]);
                }
            }
        }

        private static ActionNode<Battle, Hero, AiActionData> GetActionActionNode(XmlNode _node)
        {
            XmlAttribute typeAtt = _node.Attributes["type"];

            if (typeAtt == null)
            {
                throw new Exception("ActionNode has not type attribute:" + _node.ToString());
            }

            ActionNode<Battle, Hero, AiActionData> actionNode;

            switch (typeAtt.InnerText)
            {
                case DefenseActionNode.key:

                    actionNode = new DefenseActionNode();

                    break;

                case ChooseTargetActionNode.key:

                    actionNode = new ChooseTargetActionNode(_node);

                    break;

                case MoveForwardActionNode.key:

                    actionNode = new MoveForwardActionNode(_node);

                    break;

                default:

                    throw new Exception("Unknown ActionNode:" + _node.ToString());
            }

            return actionNode;
        }

        private static ConditionNode<Battle, Hero, AiActionData> GetActionConditionNode(XmlNode _node)
        {
            XmlAttribute typeAtt = _node.Attributes["type"];

            if (typeAtt == null)
            {
                throw new Exception("ConditionNode has not type attribute:" + _node.ToString());
            }

            ConditionNode<Battle, Hero, AiActionData> conditionNode;

            string key = typeAtt.InnerText;

            switch (key)
            {
                case CheckHeroCanActionConditionNode.key:

                    conditionNode = new CheckHeroCanActionConditionNode();

                    break;

                case CheckHeroCanBeAttackConditionNode.key:

                    conditionNode = new CheckHeroCanBeAttackConditionNode();

                    break;

                case CheckHeroTypeConditionNode.key:

                    conditionNode = new CheckHeroTypeConditionNode(_node);

                    break;

                case GetCanAttackHeroPosConditionNode.key:

                    conditionNode = new GetCanAttackHeroPosConditionNode();

                    break;

                case GetCanAttackPosConditionNode.key:

                    conditionNode = new GetCanAttackPosConditionNode();

                    break;

                case GetCanShootHeroPosConditionConditionNode.key:

                    conditionNode = new GetCanShootHeroPosConditionConditionNode();

                    break;

                case GetCanSupportHeroPosConditionNode.key:

                    conditionNode = new GetCanSupportHeroPosConditionNode();

                    break;

                case GetCanSupportPosConditionNode.key:

                    conditionNode = new GetCanSupportPosConditionNode();

                    break;

                case CheckHeroCanShootConditionNode.key:

                    conditionNode = new CheckHeroCanShootConditionNode();

                    break;

                default:

                    throw new Exception("Unknown ConditionNode:" + _node.ToString());
            }

            return conditionNode;
        }



        private static ActionNode<Battle, bool, AiSummonData> GetSummonActionNode(XmlNode _node)
        {
            XmlAttribute typeAtt = _node.Attributes["type"];

            if (typeAtt == null)
            {
                throw new Exception("ActionNode has not type attribute:" + _node.ToString());
            }

            ActionNode<Battle, bool, AiSummonData> actionNode;

            switch (typeAtt.InnerText)
            {
                case SummonActionNode.key:

                    actionNode = new SummonActionNode(_node);

                    break;

                default:

                    throw new Exception("Unknown ActionNode:" + _node.ToString());
            }

            return actionNode;
        }

        private static ConditionNode<Battle, bool, AiSummonData> GetSummonConditionNode(XmlNode _node)
        {
            XmlAttribute typeAtt = _node.Attributes["type"];

            if (typeAtt == null)
            {
                throw new Exception("ConditionNode has not type attribute:" + _node.ToString());
            }

            ConditionNode<Battle, bool, AiSummonData> conditionNode;

            string key = typeAtt.InnerText;

            switch (key)
            {
                case GetSummonPosToEnemyAreaListConditionNode.key:

                    conditionNode = new GetSummonPosToEnemyAreaListConditionNode(_node);

                    break;

                case GetSummonHeroIdConditionNode.key:

                    conditionNode = new GetSummonHeroIdConditionNode();

                    break;

                case ChooseSummonPosConditionNode.key:

                    conditionNode = new ChooseSummonPosConditionNode(_node);

                    break;

                case CheckSummonHeroTypeConditionNode.key:

                    conditionNode = new CheckSummonHeroTypeConditionNode(_node);

                    break;

                case GetMoneyConditionNode.key:

                    conditionNode = new GetMoneyConditionNode();

                    break;

                case GetSummonPosToEnemyHeroListConditionNode.key:

                    conditionNode = new GetSummonPosToEnemyHeroListConditionNode(_node);

                    break;

                default:

                    throw new Exception("Unknown ConditionNode:" + _node.ToString());
            }

            return conditionNode;
        }






        public static Dictionary<int, List<int>> GetSummonPosToEmemyAreaList(Battle _battle, bool _isMine, int _max)
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

                if (range > -1 && range < _max && _battle.CheckPosCanSummon(_isMine, pos))
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

        public static Dictionary<int, List<int>> GetSummonPosToEmemyHeroList(Battle _battle, bool _isMine, int _max)
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

                if (range > -1 && range < _max && _battle.CheckPosCanSummon(_isMine, pos))
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
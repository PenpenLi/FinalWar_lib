using System.Collections.Generic;
using bt;
using System.Xml;
using System;

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

        private static BtRoot<Battle, Hero, AiData> Create()
        {
            return BtTools.Create("aaa", GetConditionNode, GetActionNode, null);
        }

        private static ActionNode<Battle, Hero, AiData> GetActionNode(XmlNode _node)
        {
            XmlAttribute typeAtt = _node.Attributes["type"];

            if (typeAtt == null)
            {
                throw new Exception("ActionNode has not type attribute:" + _node.ToString());
            }

            ActionNode<Battle, Hero, AiData> actionNode;

            switch (typeAtt.InnerText)
            {
                case ChooseTargetActionNode.key:

                    actionNode = new ChooseTargetActionNode(_node);

                    break;

                case DefenseActionNode.key:

                    actionNode = new DefenseActionNode();

                    break;

                default:

                    throw new Exception("Unknown ActionNode:" + _node.ToString());
            }

            return actionNode;
        }

        private static ConditionNode<Battle, Hero, AiData> GetConditionNode(XmlNode _node)
        {
            XmlAttribute typeAtt = _node.Attributes["type"];

            if (typeAtt == null)
            {
                throw new Exception("ConditionNode has not type attribute:" + _node.ToString());
            }

            ConditionNode<Battle, Hero, AiData> conditionNode;

            string key = typeAtt.InnerText;

            switch (key)
            {
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

                default:

                    throw new Exception("Unknown ConditionNode:" + _node.ToString());
            }

            return conditionNode;
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
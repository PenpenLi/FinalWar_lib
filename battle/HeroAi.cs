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
            public Dictionary<BeState, int> beState = new Dictionary<BeState, int>();
            public Dictionary<CanState, Dictionary<Hero, int>> canState = new Dictionary<CanState, Dictionary<Hero, int>>();
        }

        internal static void Start(Battle _battle, bool _isMine)
        {
            Dictionary<Hero, HeroState> myHeros = null;

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
            }

            if(myHeros != null)
            {
                Dictionary<Hero, HeroState>.Enumerator enumerator2 = myHeros.GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    KeyValuePair<Hero, HeroState> pair = enumerator2.Current;

                    List<int> posList = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, pair.Key.pos);

                    for(int i = 0; i < posList.Count; i++)
                    {
                        int pos = posList[i];

                        if (_battle.heroMapDic.ContainsKey(pos))
                        {
                            Hero tmpHero = _battle.heroMapDic[pos];

                            if(tmpHero.isMine != _isMine)
                            {
                                if (tmpHero.CheckCanDoAction(Hero.HeroAction.ATTACK))
                                {
                                    bool willBeAttacked = true;

                                    if (!pair.Key.sds.GetThreat())
                                    {
                                        List<int> posList2 = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, tmpHero.pos);

                                        for (int m = 0; m < posList2.Count; m++)
                                        {
                                            int pos2 = posList2[m];

                                            if (_battle.heroMapDic.ContainsKey(pos2))
                                            {
                                                Hero tmpHero2 = _battle.heroMapDic[pos2];

                                                if (tmpHero2.isMine == _isMine && tmpHero2.sds.GetThreat())
                                                {
                                                    willBeAttacked = false;

                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (willBeAttacked)
                                    {
                                        int data = tmpHero.sds.GetAiAttackAdd() + tmpHero.sds.GetAiAttackMul() * tmpHero.nowHp;

                                        BattlePublicTools.AccumulateDicData(pair.Value.beState, BeState.WILL_BE_ATTACK, data);
                                    }
                                }

                                if (pair.Key.CheckCanDoAction(Hero.HeroAction.ATTACK))
                                {
                                    bool canAttack = true;

                                    if (!tmpHero.sds.GetThreat())
                                    {
                                        for(int m = 0; m < posList.Count; m++)
                                        {
                                            int pos2 = posList[m];

                                            if (_battle.heroMapDic.ContainsKey(pos2))
                                            {
                                                Hero tmpHero2 = _battle.heroMapDic[pos2];

                                                if (tmpHero2.isMine != _isMine && tmpHero2.sds.GetThreat())
                                                {
                                                    canAttack = false;

                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (canAttack)
                                    {
                                        Dictionary<Hero, int> tmpDic;

                                        if (!pair.Value.canState.ContainsKey(CanState.CAN_ATTACK))
                                        {
                                            tmpDic = new Dictionary<Hero, int>();

                                            pair.Value.canState.Add(CanState.CAN_ATTACK, tmpDic);
                                        }
                                        else
                                        {
                                            tmpDic = pair.Value.canState[CanState.CAN_ATTACK];
                                        }

                                        tmpDic.Add(tmpHero, 0);
                                    }
                                }
                            }
                            else
                            {
                                if (tmpHero.CheckCanDoAction(Hero.HeroAction.SUPPORT))
                                {
                                    int data = tmpHero.sds.GetAiCounterAdd() + tmpHero.sds.GetAiCounterMul() * tmpHero.nowHp;

                                    BattlePublicTools.AccumulateDicData(pair.Value.beState, BeState.WILL_BE_SUPPORT, data);
                                }

                                if (pair.Key.CheckCanDoAction(Hero.HeroAction.SUPPORT))
                                {
                                    Dictionary<Hero, int> tmpDic;

                                    if (!pair.Value.canState.ContainsKey(CanState.CAN_SUPPORT))
                                    {
                                        tmpDic = new Dictionary<Hero, int>();

                                        pair.Value.canState.Add(CanState.CAN_SUPPORT, tmpDic);
                                    }
                                    else
                                    {
                                        tmpDic = pair.Value.canState[CanState.CAN_SUPPORT];
                                    }

                                    tmpDic.Add(tmpHero, 0);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

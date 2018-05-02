using System.Collections.Generic;
using System;

namespace FinalWar
{
    public class Hero
    {
        public enum HeroData
        {
            DATA,
            NOWHP,
            MAXHP,
            LEVEL,
            ATTACK,
            NEIGHBOUR_ALLY_NUM,
            NEIGHBOUR_ENEMY_NUM,
            NEIGHBOUR_NUM,
            NOWSHIELD,
            MAXSHIELD,
            BE_ATTACKED_TIMES,
            SCORE_DIFF,
            LOSE_HP,
            LOSE_SHIELD,
            ISALIVE,
        }

        internal enum HeroAction
        {
            ATTACK,
            ATTACK_OVER,
            SHOOT,
            SUPPORT,
            DEFENSE,
            NULL
        }

        public bool isMine { get; private set; }

        public IHeroSDS sds { get; private set; }

        public int pos { get; private set; }

        internal HeroAction action { get; private set; }

        internal int actionTarget { get; private set; }

        public int nowHp { get; private set; }

        public int nowShield { get; private set; }

        private Battle battle;

        private int attackTimes = 0;

        private int counterTimes = 0;

        private int shieldChange = 0;

        private int hpChange = 0;

        private int damage = 0;

        private bool beKilled = false;

        private int beAttackedTimes = 0;

        internal Hero(Battle _battle, bool _isMine, IHeroSDS _sds, int _pos)
        {
            battle = _battle;

            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            nowHp = sds.GetHp();

            nowShield = sds.GetShield();

            SetAction(HeroAction.NULL);

            HeroAura.Init(battle, this);
        }

        internal void SetAction(HeroAction _action, int _actionTarget)
        {
            action = _action;

            actionTarget = _actionTarget;
        }

        internal void SetAction(HeroAction _action)
        {
            action = _action;

            actionTarget = -1;
        }

        internal void DoAttack()
        {
            attackTimes++;

            if (attackTimes == sds.GetHeroType().GetAttackTimes())
            {
                SetAction(HeroAction.ATTACK_OVER, actionTarget);
            }
        }

        internal void BeAttacked()
        {
            beAttackedTimes++;
        }

        internal void DoCounter()
        {
            if (sds.GetHeroType().GetCounterTimes() > 0)
            {
                counterTimes++;

                if (counterTimes == sds.GetHeroType().GetCounterTimes())
                {
                    SetAction(HeroAction.NULL);
                }
            }
        }

        internal void PosChange(int _pos)
        {
            pos = _pos;
        }

        internal void BeDamage(int _value)
        {
            damage += _value;
        }

        internal void ShieldChange(int _value)
        {
            shieldChange += _value;
        }

        internal void HpChange(int _value)
        {
            hpChange += _value;
        }

        internal void ProcessDamage()
        {
            nowShield += shieldChange;

            nowHp += hpChange;

            if (nowShield < 1)
            {
                nowHp -= damage;
            }
            else if (damage > nowShield)
            {
                damage -= nowShield;

                nowShield = 0;

                nowHp -= damage;
            }
            else
            {
                nowShield -= damage;
            }

            if (nowShield < 0)
            {
                nowShield = 0;
            }

            if (nowHp < 0)
            {
                nowHp = 0;
            }
            else if (nowHp > sds.GetHp())
            {
                nowHp = sds.GetHp();
            }

            shieldChange = hpChange = damage = 0;
        }

        public void ProcessDamage(out int _nowShield, out int _nowHp)
        {
            _nowShield = nowShield;

            _nowHp = nowHp;

            _nowShield += shieldChange;

            _nowHp += hpChange;

            int tmpDamage = damage;

            if (_nowShield < 1)
            {
                _nowHp -= tmpDamage;
            }
            else if (tmpDamage > _nowShield)
            {
                tmpDamage -= _nowShield;

                _nowShield = 0;

                _nowHp -= tmpDamage;
            }
            else
            {
                _nowShield -= tmpDamage;
            }
        }

        public bool GetCanAction()
        {
            return !battle.GetFearActionContainsKey(pos);
        }

        internal int GetAttackSpeed(Hero _hero)
        {
            int speed = sds.GetHeroType().GetAttackSpeed() + GetSpeedFix(_hero);

            return FixSpeed(speed);
        }

        internal int GetDefenseSpeed(Hero _hero)
        {
            int speed = sds.GetHeroType().GetDefenseSpeed() + GetSpeedFix(_hero);

            return FixSpeed(speed);
        }

        internal int GetSupportSpeed(Hero _hero)
        {
            int speed = sds.GetHeroType().GetSupportSpeed() + GetSpeedFix(_hero);

            return FixSpeed(speed);
        }

        public int GetSpeedFixByClient()
        {
            return GetSpeedFix(null);
        }

        private int GetSpeedFix(Hero _hero)
        {
            int tmpSpeedFix = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_SPEED, ref tmpSpeedFix, this, _hero);

            return tmpSpeedFix;
        }

        private int FixSpeed(int _speed)
        {
            if (_speed > BattleConst.MAX_SPEED)
            {
                _speed = BattleConst.MAX_SPEED;
            }
            else if (_speed < BattleConst.MIN_SPEED)
            {
                _speed = BattleConst.MIN_SPEED;
            }

            return _speed;
        }

        internal bool IsAlive()
        {
            return nowHp > 0 && !beKilled;
        }

        internal void ChangeHero(int _id)
        {
            sds = Battle.GetHeroData(_id);

            if (nowHp > sds.GetHp())
            {
                nowHp = sds.GetHp();
            }

            LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.REMOVE_BORN_AURA, ref funcList, this, null);

            HeroAura.Init(battle, this);
        }

        internal void BeKilled()
        {
            beKilled = true;
        }

        internal void ForceFear(int _data)
        {
            if (_data < 0)
            {
                battle.AddFearAction(pos, pos);
            }
            else if (_data < 6)
            {
                int targetPos = battle.mapData.neighbourPosMap[pos][_data];

                if (targetPos != -1)
                {
                    battle.AddFearAction(pos, targetPos);
                }
            }
        }

        internal bool GetCanMove()
        {
            int tmpCanMove = 1;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_CAN_MOVE, ref tmpCanMove, this, null);

            return tmpCanMove > 0;
        }

        internal void RoundStart(ref LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.ROUND_START, ref _funcList, this, null);
        }

        internal void RoundOver(ref LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.ROUND_OVER, ref _funcList, this, null);
        }

        internal void Recover(ref LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            if (nowShield > sds.GetShield())
            {
                nowShield = sds.GetShield();
            }
            else if (nowShield < sds.GetShield())
            {
                int recoverShield = sds.GetHeroType().GetRecoverShield() + GetRecoverShieldValueFix();

                if (recoverShield > beAttackedTimes)
                {
                    nowShield = sds.GetShield();
                }
            }

            beAttackedTimes = 0;

            attackTimes = 0;

            counterTimes = 0;

            bool b = battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.CHECK_FORCE_FEAR, ref _funcList, this, null);

            if (!b && CheckFear())
            {
                CheckFearReal();
            }

            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.RECOVER, ref _funcList, this, null);
        }

        private bool CheckFear()
        {
            int numDiff = GetFearNumDiff();

            if (numDiff > 0)
            {
                int randomValue = battle.GetRandomValue(BattleConst.MAX_FEAR_VALUE);

                if (numDiff > randomValue)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetFearNumDiff()
        {
            List<int> list = BattlePublicTools.GetNeighbourPos(battle.mapData, pos);

            if (list.Count > 0)
            {
                Dictionary<int, int> tmpDic = null;

                int oppNum = 0;

                bool hasOpp = false;

                for (int i = 0; i < list.Count; i++)
                {
                    int pos2 = list[i];

                    if (battle.GetPosIsMine(pos2) != isMine)
                    {
                        Hero oppHero;

                        if (battle.heroMapDic.TryGetValue(pos2, out oppHero))
                        {
                            hasOpp = true;

                            int oppFearValue = oppHero.GetFearValue();

                            List<int> list2 = BattlePublicTools.GetNeighbourPos(battle.mapData, pos2);

                            for (int m = 0; m < list2.Count; m++)
                            {
                                int pos3 = list2[m];

                                if (pos3 != pos && battle.GetPosIsMine(pos3) == isMine)
                                {
                                    Hero myHero;

                                    if (battle.heroMapDic.TryGetValue(pos3, out myHero))
                                    {
                                        if (tmpDic == null)
                                        {
                                            tmpDic = new Dictionary<int, int>();
                                        }

                                        int myFearValue;

                                        if (!tmpDic.TryGetValue(pos3, out myFearValue))
                                        {
                                            myFearValue = myHero.GetFearValue();

                                            tmpDic.Add(pos3, myFearValue);
                                        }

                                        if (oppFearValue > myFearValue)
                                        {
                                            oppFearValue -= myFearValue;

                                            myFearValue = 0;
                                        }
                                        else
                                        {
                                            myFearValue -= oppFearValue;

                                            oppFearValue = 0;
                                        }

                                        tmpDic[pos3] = myFearValue;
                                    }
                                }
                            }

                            if (oppFearValue > 0)
                            {
                                oppNum += oppFearValue;
                            }
                        }
                    }
                }

                if (hasOpp)
                {
                    int myNum = GetFearValue();

                    int numDiff = oppNum - myNum + BattleConst.FEAR_DIFF_FIX;

                    return numDiff;
                }
            }

            return 0;
        }

        private int GetFearValue()
        {
            int fearValue = sds.GetHeroType().GetFearValue() + GetFearValueFix();

            if (fearValue < 0)
            {
                fearValue = 0;
            }

            return fearValue;
        }

        public int GetFearValueFix()
        {
            int fearValueFix = 0;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_FEAR, ref fearValueFix, this, null);

            return fearValueFix;
        }

        public int GetRecoverShieldValueFix()
        {
            int recoverShieldValueFix = 0;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_RECOVER_SHIELD, ref recoverShieldValueFix, this, null);

            return recoverShieldValueFix;
        }

        private void CheckFearReal()
        {
            int num = battle.GetRandomValue(sds.GetHeroType().GetFearAttackWeight() + sds.GetHeroType().GetFearDefenseWeight());

            if (num < sds.GetHeroType().GetFearAttackWeight())
            {
                List<int> tmpList = BattlePublicTools.GetCanAttackPos(battle, this);

                if (tmpList != null)
                {
                    int index = battle.GetRandomValue(tmpList.Count);

                    battle.AddFearAction(pos, tmpList[index]);
                }
                else
                {
                    battle.AddFearAction(pos, pos);
                }
            }
            else
            {
                battle.AddFearAction(pos, pos);
            }
        }

        internal void BeClean()
        {
            LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> funcList = null;

            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.BE_CLEAN, ref funcList, this, null);
        }

        public int GetAttackByClient()
        {
            int recNowHp = nowHp;

            int recNowShield = nowShield;

            int tmpNowHp;

            int tmpNowShield;

            ProcessDamage(out tmpNowShield, out tmpNowHp);

            nowHp = tmpNowHp;

            nowShield = tmpNowShield;

            int attack = sds.GetAttack();

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_ATTACK_DAMAGE, ref attack, this, null);

            nowHp = recNowHp;

            nowShield = recNowShield;

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
        }

        internal void Attack(Hero _hero, ref LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            int doDamage = sds.GetAttack();

            int shieldToDamage = 1;

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK_SHIELD_TO_DAMAGE, ref shieldToDamage, this, _hero);

            battle.eventListener.DispatchEvent(BattleConst.FIX_BE_ATTACKED_SHIELD_TO_DAMAGE, ref shieldToDamage, _hero, this);

            if (shieldToDamage > 0)
            {
                doDamage += nowShield;
            }

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK_DAMAGE, ref doDamage, this, _hero);

            battle.eventListener.DispatchEvent(BattleConst.FIX_BE_ATTACKED_DAMAGE, ref doDamage, _hero, this);

            if (doDamage < 0)
            {
                doDamage = 0;
            }

            int doShieldDamage = 0;

            int doHpDamage = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK_SHIELD_DAMAGE, ref doShieldDamage, this, _hero);

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK_HP_DAMAGE, ref doHpDamage, this, _hero);

            int canPierceShield = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK_PIERCE_SHIELD, ref canPierceShield, this, _hero);

            battle.eventListener.DispatchEvent(BattleConst.FIX_BE_ATTACKED_PIERCE_SHIELD, ref canPierceShield, _hero, this);

            if (canPierceShield > 0)
            {
                _hero.HpChange(-doDamage);
            }
            else
            {
                _hero.BeDamage(doDamage);
            }

            _hero.ShieldChange(-doShieldDamage);

            _hero.HpChange(-doHpDamage);

            battle.eventListener.DispatchEvent(BattleConst.DO_DAMAGE, ref _funcList, this, _hero);

            battle.eventListener.DispatchEvent(BattleConst.BE_DAMAGED, ref _funcList, _hero, this);
        }

        internal void MoneyChange(int _num)
        {
            battle.MoneyChangeReal(isMine, _num);
        }

        internal void Die(ref LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.DIE, ref _funcList, this, null);
        }

        internal void CaptureArea(ref LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>> _funcList)
        {
            battle.eventListener.DispatchEvent<LinkedList<KeyValuePair<int, Func<BattleTriggerAuraVO>>>, Hero, Hero>(BattleConst.CAPTURE_MAP_AREA, ref _funcList, this, null);
        }

        internal bool GetAttackFirstWithHigherSpeed(Hero _hero)
        {
            int attackFirstWithHigherSpeed = 0;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_ATTACK_FIRST_WITH_HIGHER_SPEED, ref attackFirstWithHigherSpeed, this, _hero);

            return attackFirstWithHigherSpeed > 0;
        }

        internal bool GetCanCounterWhenDead(Hero _hero)
        {
            int canCounterWhenDead = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_COUNTER_WHEN_DEAD, ref canCounterWhenDead, this, _hero);

            return canCounterWhenDead > 0;
        }

        internal bool GetEqualOppSpeed(Hero _hero)
        {
            int equalSpeed = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_EQUAL_OPP_SPEED, ref equalSpeed, this, _hero);

            return equalSpeed > 0;
        }






        public void GetDesc(ref List<int> _list)
        {
            battle.eventListener.DispatchEvent(BattleConst.GET_AURA_DESC, ref _list, this);
        }

        public List<int> GetCanAttackPos()
        {
            return BattlePublicTools.GetCanAttackPos(battle, this);
        }

        internal int GetData(HeroData _type)
        {
            switch (_type)
            {
                case HeroData.LEVEL:

                    return sds.GetCost();

                case HeroData.ATTACK:

                    return sds.GetAttack();

                case HeroData.MAXHP:

                    return sds.GetHp();

                case HeroData.NOWHP:

                    return nowHp;

                case HeroData.NOWSHIELD:

                    return nowShield;

                case HeroData.MAXSHIELD:

                    return sds.GetShield();

                case HeroData.NEIGHBOUR_ALLY_NUM:

                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(battle.mapData, pos);

                    int num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine == isMine)
                            {
                                num++;
                            }
                        }
                    }

                    return num;

                case HeroData.NEIGHBOUR_ENEMY_NUM:

                    tmpList = BattlePublicTools.GetNeighbourPos(battle.mapData, pos);

                    num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            if (tmpHero.isMine != isMine)
                            {
                                num++;
                            }
                        }
                    }

                    return num;

                case HeroData.NEIGHBOUR_NUM:

                    tmpList = BattlePublicTools.GetNeighbourPos(battle.mapData, pos);

                    num = 0;

                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        int tmpPos = tmpList[i];

                        Hero tmpHero;

                        if (battle.heroMapDic.TryGetValue(tmpPos, out tmpHero))
                        {
                            num++;
                        }
                    }

                    return num;

                case HeroData.BE_ATTACKED_TIMES:

                    return beAttackedTimes;

                case HeroData.SCORE_DIFF:

                    return isMine ? battle.mScore - battle.oScore : battle.oScore - battle.mScore;

                case HeroData.LOSE_HP:

                    int loseHp = sds.GetHp() - nowHp;

                    if (loseHp < 0)
                    {
                        loseHp = 0;
                    }

                    return loseHp;

                case HeroData.LOSE_SHIELD:

                    int loseShield = sds.GetShield() - nowShield;

                    if (loseShield < 0)
                    {
                        loseShield = 0;
                    }

                    return loseShield;

                case HeroData.ISALIVE:

                    return IsAlive() ? 1 : 0;

                default:

                    throw new Exception("Unknown AuraConditionType:" + _type);
            }
        }
    }
}

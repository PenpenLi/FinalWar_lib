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

        internal int nowHp { get; private set; }

        private int nowShield;

        internal int attackTimes { get; private set; }

        private Battle battle;

        private int shieldChange = 0;

        private int hpChange = 0;

        private int damage = 0;

        private bool beKilled = false;

        private bool isAttacked = false;

        internal Hero(Battle _battle, bool _isMine, IHeroSDS _sds, int _pos)
        {
            battle = _battle;

            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            nowHp = sds.GetHp();

            nowShield = sds.GetShield();

            attackTimes = sds.GetHeroType().GetAttackTimes();

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
            attackTimes--;
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

            if (_nowShield < 0)
            {
                _nowShield = 0;
            }

            if (_nowHp < 0)
            {
                _nowHp = 0;
            }
            else if (_nowHp > sds.GetHp())
            {
                _nowHp = sds.GetHp();
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

        public int GetSpeedFix()
        {
            return GetSpeedFix(null);
        }

        internal int GetSpeedFix(Hero _hero)
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

            List<Func<BattleTriggerAuraVO>> funcList = null;

            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.REMOVE_BORN_AURA, ref funcList, this, null);

            HeroAura.Init(battle, this);
        }

        internal void BeKilled()
        {
            beKilled = true;
        }

        internal bool GetCanMove()
        {
            bool tmpCanMove = true;

            battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.FIX_CAN_MOVE, ref tmpCanMove, this, null);

            return tmpCanMove;
        }

        public int GetDamage()
        {
            int attack = sds.GetAttack() + GetAttackFix() + nowShield;

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
        }

        internal int GetDamage(Hero _hero)
        {
            int attack = sds.GetAttack() + GetAttackFix(_hero) + nowShield;

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
        }

        public int GetDamageWithoutShield()
        {
            int attack = sds.GetAttack() + GetAttackFix();

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
        }

        internal int GetDamageWithoutShield(Hero _hero)
        {
            int attack = sds.GetAttack() + GetAttackFix(_hero);

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
        }

        private int GetAttackFix(Hero _hero)
        {
            int attackFix = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK, ref attackFix, this, _hero);

            battle.eventListener.DispatchEvent(BattleConst.FIX_BE_ATTACK, ref attackFix, _hero, this);

            return attackFix;
        }

        private int GetAttackFix()
        {
            int attackFix = 0;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_ATTACK, ref attackFix, this, null);

            return attackFix;
        }

        internal void RoundStart(ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.ROUND_START, ref _funcList, this, null);
        }

        internal void Recover(ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.RECOVER, ref _funcList, this, null);
        }

        internal void RoundOver(ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            if (!isAttacked)
            {
                bool recoverShield = true;

                //List<int> list = BattlePublicTools.GetNeighbourPos(battle.mapData, pos);

                //for (int i = 0; i < list.Count; i++)
                //{
                //    int tmpPos = list[i];

                //    if (battle.GetPosIsMine(tmpPos) != isMine && battle.heroMapDic.ContainsKey(tmpPos))
                //    {
                //        recoverShield = false;

                //        break;
                //    }
                //}

                battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.FIX_CAN_RECOVER_SHIELD, ref recoverShield, this, null);

                if (recoverShield)
                {
                    nowShield = sds.GetShield();
                }
            }
            else
            {
                isAttacked = false;
            }

            if (nowShield > sds.GetShield())
            {
                nowShield = sds.GetShield();
            }

            attackTimes = sds.GetHeroType().GetAttackTimes();

            switch (sds.GetHeroType().GetFearType())
            {
                case FearType.ALWAYS:

                    CheckFearReal();

                    break;

                case FearType.NEVER:

                    break;

                default:

                    if (CheckFear())
                    {
                        CheckFearReal();
                    }

                    break;
            }

            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.ROUND_OVER, ref _funcList, this, null);
        }

        private bool CheckFear()
        {
            bool willeFear = true;

            battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.FIX_FEAR, ref willeFear, this, null);

            if (willeFear)
            {
                int myNum = 0;

                int oppNum = 0;

                List<int> list = BattlePublicTools.GetNeighbourPos(battle.mapData, pos);

                for (int i = 0; i < list.Count; i++)
                {
                    Hero hero;

                    if (battle.heroMapDic.TryGetValue(list[i], out hero))
                    {
                        if (hero.isMine == isMine)
                        {
                            myNum += hero.sds.GetHeroType().GetFearValue();
                        }
                        else
                        {
                            oppNum += hero.sds.GetHeroType().GetFearValue();
                        }
                    }
                }

                int numDiff = oppNum - myNum;

                if (numDiff > 0)
                {
                    int v = (numDiff + 1) * numDiff / 2;

                    int randomValue = battle.GetRandomValue(6);

                    if (randomValue < v)
                    {
                        return true;
                    }
                }
            }

            return false;
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
            List<Func<BattleTriggerAuraVO>> funcList = null;

            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.BE_CLEAN, ref funcList, this, null);
        }

        internal BattleHeroEffectVO Attack(Hero _hero, int _damage, ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            battle.eventListener.DispatchEvent(BattleConst.ATTACK, ref _funcList, this, _hero);

            return DoDamage(_hero, _damage, ref _funcList);
        }

        internal BattleHeroEffectVO Rush(Hero _hero, int _damage, ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            battle.eventListener.DispatchEvent(BattleConst.RUSH, ref _funcList, this, _hero);

            return DoDamage(_hero, _damage, ref _funcList);
        }

        private BattleHeroEffectVO DoDamage(Hero _hero, int _damage, ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            _hero.isAttacked = true;

            bool tmpCanPierceShield = false;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_PIERCE_SHIELD, ref tmpCanPierceShield, this, _hero);

            if (!tmpCanPierceShield)
            {
                battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_BE_PIERCE_SHIELD, ref tmpCanPierceShield, _hero, this);
            }

            BattleHeroEffectVO vo;

            if (tmpCanPierceShield)
            {
                _hero.HpChange(-_damage);

                vo = new BattleHeroEffectVO(Effect.HP_CHANGE, -_damage);
            }
            else
            {
                _hero.BeDamage(_damage);

                vo = new BattleHeroEffectVO(Effect.DAMAGE, _damage);
            }

            battle.eventListener.DispatchEvent(BattleConst.DO_DAMAGE, ref _funcList, this, _hero);

            return vo;
        }

        internal void MoneyChange(int _num)
        {
            battle.MoneyChangeReal(isMine, _num);
        }

        internal void Die(ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.DIE, ref _funcList, this, null);
        }

        internal void CaptureArea(ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.CAPTURE_MAP_AREA, ref _funcList, this, null);
        }

        public void GetDesc(ref List<string> _list)
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

                default:

                    throw new Exception("Unknown AuraConditionType:" + _type);
            }
        }
    }
}

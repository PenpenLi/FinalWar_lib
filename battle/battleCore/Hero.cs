using System.Collections.Generic;
using System;

namespace FinalWar
{
    public class Hero
    {
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

        private bool initAura = false;

        private int shieldChange = 0;

        private int hpChange = 0;

        private int damage = 0;

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
            return nowHp > 0;
        }

        internal void ChangeHero(int _id)
        {
            sds = Battle.GetHeroData(_id);

            if (nowHp > sds.GetHp())
            {
                nowHp = sds.GetHp();
            }

            UnregisterAura();
        }

        internal bool GetCanMove()
        {
            bool tmpCanMove = true;

            battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.FIX_CAN_MOVE, ref tmpCanMove, this, null);

            return tmpCanMove;
        }

        public int GetDamage()
        {
            return GetDamage(null);
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
            return GetDamageWithoutShield(null);
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
            int attackFixAura = 0;

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK, ref attackFixAura, this, _hero);

            return attackFixAura;
        }

        internal void Recover(ref List<Func<BattleTriggerAuraVO>> _funcList)
        {
            bool recoverShield = true;

            battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.FIX_CAN_RECOVER_SHIELD, ref recoverShield, this, null);

            if (recoverShield)
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

            if (!initAura)
            {
                initAura = true;

                HeroAura.Init(battle, this);
            }
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

        internal void BeSilence()
        {
            UnregisterAura();
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
            bool tmpCanPierceShield = false;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_PIERCE_SHIELD, ref tmpCanPierceShield, this, _hero);

            BattleHeroEffectVO vo;

            if (tmpCanPierceShield)
            {
                _hero.HpChange(-_damage);

                vo = new BattleHeroEffectVO(Effect.HP_CHANGE, new int[] { -_damage });
            }
            else
            {
                _hero.BeDamage(_damage);

                vo = new BattleHeroEffectVO(Effect.DAMAGE, new int[] { _damage });
            }

            battle.eventListener.DispatchEvent(BattleConst.DO_DAMAGE, ref _funcList, this, _hero);

            return vo;
        }

        private void UnregisterAura()
        {
            initAura = false;

            List<Func<BattleTriggerAuraVO>> funcList = null;

            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.BE_SILENCE, ref funcList, this, null);
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
    }
}

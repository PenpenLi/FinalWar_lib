using System.Collections.Generic;
using System.Collections;
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
            SUPPORT_OVER,
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

        internal int GetAttackSpeed(int _speedBonus)
        {
            int speed = sds.GetHeroType().GetAttackSpeed() + _speedBonus + GetSpeedFix();

            return FixSpeed(speed);
        }

        internal int GetDefenseSpeed(int _speedBonus)
        {
            int speed = sds.GetHeroType().GetDefenseSpeed() + _speedBonus + GetSpeedFix();

            return FixSpeed(speed);
        }

        internal int GetSupportSpeed(int _speedBonus)
        {
            int speed = sds.GetHeroType().GetSupportSpeed() + _speedBonus + GetSpeedFix();

            return FixSpeed(speed);
        }

        private int GetSpeedFix()
        {
            int tmpSpeedFix = 0;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_SPEED, ref tmpSpeedFix, this, null);

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
            int attack = sds.GetAttack() + GetAttackFix() + nowShield;

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

        private int GetAttackFix()
        {
            int attackFixAura = 0;

            battle.eventListener.DispatchEvent<int, Hero, Hero>(BattleConst.FIX_ATTACK, ref attackFixAura, this, null);

            return attackFixAura;
        }

        internal void Recover()
        {
            bool recoverShield = true;

            battle.eventListener.DispatchEvent<bool, Hero, Hero>(BattleConst.FIX_CAN_RECOVER_SHIELD, ref recoverShield, this, null);

            if (recoverShield)
            {
                nowShield = sds.GetShield();
            }

            attackTimes = sds.GetHeroType().GetAttackTimes();

            if (!initAura)
            {
                initAura = true;

                HeroAura.Init(battle, this);
            }

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
                            myNum++;
                        }
                        else
                        {
                            oppNum++;
                        }
                    }
                }

                int numDiff = oppNum - myNum;

                if (numDiff > 0)
                {
                    int randomValue = battle.GetRandomValue(6);

                    if (randomValue < numDiff)
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

                int index = battle.GetRandomValue(tmpList.Count);

                battle.AddFearAction(pos, tmpList[index]);
            }
            else
            {
                battle.AddFearAction(pos, pos);
            }
        }

        internal void Silence()
        {
            UnregisterAura();
        }

        internal IEnumerator Die()
        {
            List<Func<BattleTriggerAuraVO>> list = null;

            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.DIE, ref list, this, null);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i]();
                }
            }
        }

        internal void Attack(Hero _hero, int _damage, ref List<BattleHeroEffectVO> _attackerEffectList, ref List<BattleHeroEffectVO> _defenderEffectList)
        {
            bool tmpCanPierceShield = false;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_PIERCE_SHIELD, ref tmpCanPierceShield, this, _hero);

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

            _defenderEffectList = new List<BattleHeroEffectVO>();

            _defenderEffectList.Add(vo);

            List<Func<BattleTriggerAuraVO>> list = null;

            battle.eventListener.DispatchEvent(BattleConst.ATTACK, ref list, this, _hero);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    BattleTriggerAuraVO triggerVO = list[i]();

                    Dictionary<int, List<BattleHeroEffectVO>>.Enumerator enumerator = triggerVO.data.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        int tmpPos = enumerator.Current.Key;

                        List<BattleHeroEffectVO> tmpList = enumerator.Current.Value;

                        if (tmpPos == pos)
                        {
                            if (_attackerEffectList == null)
                            {
                                _attackerEffectList = new List<BattleHeroEffectVO>();
                            }

                            for (int m = 0; m < tmpList.Count; m++)
                            {
                                _attackerEffectList.Add(tmpList[m]);
                            }
                        }
                        else if (enumerator.Current.Key == _hero.pos)
                        {
                            for (int m = 0; m < tmpList.Count; m++)
                            {
                                _defenderEffectList.Add(tmpList[m]);
                            }
                        }
                        else
                        {
                            throw new Exception("Attack error:" + enumerator.Current.Key);
                        }
                    }
                }
            }
        }

        private void UnregisterAura()
        {
            initAura = false;

            List<Func<BattleTriggerAuraVO>> list = null;

            battle.eventListener.DispatchEvent<List<Func<BattleTriggerAuraVO>>, Hero, Hero>(BattleConst.BE_SILENCE, ref list, this, null);

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i]();
                }
            }
        }

        internal void MoneyChange(int _num)
        {
            battle.MoneyChangeReal(isMine, _num);
        }
    }
}

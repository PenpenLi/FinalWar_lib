using System.Collections.Generic;

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

        private int canAction;

        internal int attackTimes { get; private set; }

        private Battle battle;

        private bool initAura = false;

        private int shieldChange = 0;

        private int hpChange = 0;

        private int damage = 0;

        internal Hero(Battle _battle, bool _isMine, IHeroSDS _sds, int _pos, bool _initAura)
        {
            battle = _battle;

            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            nowHp = sds.GetHp();

            nowShield = sds.GetShield();

            if (sds.GetHeroType().GetCanDoAction())
            {
                canAction = 0;
            }
            else
            {
                canAction = -1;
            }

            attackTimes = sds.GetHeroType().GetAttackTimes();

            SetAction(HeroAction.NULL);

            if (_initAura)
            {
                initAura = true;

                HeroAura.Init(battle, this);
            }
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
            return canAction == 0;
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

            battle.eventListener.DispatchEvent(BattleConst.FIX_SPEED, ref tmpSpeedFix, this);

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

        internal void LevelUp(int _id)
        {
            sds = Battle.GetHeroData(_id);

            UnregisterAura();
        }

        internal bool GetCanPierceShield()
        {
            bool tmpCanPierceShield = false;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_PIERCE_SHIELD, ref tmpCanPierceShield, this);

            return tmpCanPierceShield;
        }

        internal bool GetCanMove()
        {
            bool tmpCanMove = true;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_MOVE, ref tmpCanMove, this);

            return tmpCanMove;
        }

        internal void DisableAction()
        {
            if (canAction > -1)
            {
                canAction = 2;
            }
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

            battle.eventListener.DispatchEvent(BattleConst.FIX_ATTACK, ref attackFixAura, this);

            return attackFixAura;
        }

        internal void Recover()
        {
            bool recoverShield = true;

            battle.eventListener.DispatchEvent(BattleConst.FIX_CAN_RECOVER_SHIELD, ref recoverShield, this);

            if (recoverShield)
            {
                nowShield = sds.GetShield();
            }

            if (canAction > 0)
            {
                canAction--;
            }

            attackTimes = sds.GetHeroType().GetAttackTimes();

            if (!initAura)
            {
                initAura = true;

                HeroAura.Init(battle, this);
            }

            if (GetCanAction())
            {
                CheckFear();
            }
        }

        private void CheckFear()
        {
            bool willeFear = true;

            battle.eventListener.DispatchEvent(BattleConst.FIX_FEAR, ref willeFear, this);

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
                        canAction = 1;
                    }
                }
            }
        }

        internal void Silence()
        {
            UnregisterAura();
        }

        internal void Die()
        {
            battle.eventListener.DispatchEvent(BattleConst.DIE, this);
        }

        internal List<BattleHeroEffectVO> Attack(Hero _hero, int _damage)
        {
            List<BattleHeroEffectVO> effectList = new List<BattleHeroEffectVO>();

            if (GetCanPierceShield())
            {
                _hero.HpChange(-_damage);

                BattleHeroEffectVO vo = new BattleHeroEffectVO(Effect.HP_CHANGE, -_damage);

                effectList.Add(vo);
            }
            else
            {
                _hero.BeDamage(_damage);

                BattleHeroEffectVO vo = new BattleHeroEffectVO(Effect.DAMAGE, _damage);

                effectList.Add(vo);
            }

            battle.eventListener.DispatchEvent(BattleConst.ATTACK, ref effectList, this, _hero);

            return effectList;
        }

        private void UnregisterAura()
        {
            initAura = false;

            battle.eventListener.DispatchEvent(BattleConst.BE_SILENCE, this);
        }

        internal void MoneyChange(int _num)
        {
            battle.MoneyChangeReal(isMine, _num);
        }
    }
}

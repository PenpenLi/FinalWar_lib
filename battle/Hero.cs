using superEvent;
using System.IO;
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

        private int nowHp;

        private int nowShield;

        private int attackFix = 0;

        private int speedFix = 0;

        private bool recoverShield = true;

        private bool canMove = true;

        private int canAction;

        internal int attackTimes { get; private set; }

        private Battle battle;

        private SuperEventListener eventListener;

        private bool initAura = false;

        private int shieldChange = 0;

        private int hpChange = 0;

        private int damage = 0;

        private bool canPierceShield = false;

        internal Hero(Battle _battle, SuperEventListener _eventListener, bool _isMine, IHeroSDS _sds, int _pos, bool _initAura)
        {
            battle = _battle;

            eventListener = _eventListener;

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

                HeroAura.Init(battle, eventListener, this);
            }
        }

        internal Hero(Battle _battle, SuperEventListener _eventListener)
        {
            battle = _battle;

            eventListener = _eventListener;
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
                nowHp = sds.GetHp();
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

        public int GetSpeedFix()
        {
            int tmpSpeedFix = speedFix;

            eventListener.DispatchEvent(HeroAura.FIX_SPEED, ref tmpSpeedFix, this);

            return tmpSpeedFix;
        }

        private int FixSpeed(int _speed)
        {
            if (_speed > Battle.MAX_SPEED)
            {
                _speed = Battle.MAX_SPEED;
            }
            else if (_speed < Battle.MIN_SPEED)
            {
                _speed = Battle.MIN_SPEED;
            }

            return _speed;
        }

        internal bool IsAlive()
        {
            return nowHp > 0;
        }

        internal void LevelUp()
        {
            if (sds.GetLevelUp() != 0)
            {
                sds = Battle.GetHeroData(sds.GetLevelUp());

                UnregisterAura();
            }
            else
            {
                nowHp = sds.GetHp();
            }
        }

        internal void SetAttackFix(int _value)
        {
            attackFix += _value;
        }

        internal void SetSpeedFix(int _value)
        {
            speedFix += _value;
        }

        internal void DisableRecoverShield()
        {
            recoverShield = false;
        }

        internal void DisableMove()
        {
            canMove = false;
        }

        internal void EnablePierceShield()
        {
            canPierceShield = true;
        }

        internal bool GetCanPierceShield()
        {
            if (canPierceShield)
            {
                return true;
            }
            else
            {
                bool tmpCanPierceShield = false;

                eventListener.DispatchEvent(HeroAura.FIX_CAN_PIERCE_SHIELD, ref tmpCanPierceShield, this);

                return tmpCanPierceShield;
            }
        }

        internal bool GetCanMove()
        {
            if (!canMove)
            {
                return false;
            }
            else
            {
                bool tmpCanMove = true;

                eventListener.DispatchEvent(HeroAura.FIX_CAN_MOVE, ref tmpCanMove, this);

                return tmpCanMove;
            }
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
            int attackFixAura = attackFix;

            eventListener.DispatchEvent(HeroAura.FIX_ATTACK, ref attackFixAura, this);

            return attackFixAura;
        }

        internal void Recover()
        {
            if (recoverShield)
            {
                nowShield = sds.GetShield();
            }
            else
            {
                recoverShield = true;
            }

            speedFix = attackFix = 0;

            canMove = true;

            canPierceShield = false;

            if (canAction > 0)
            {
                canAction--;
            }

            attackTimes = sds.GetHeroType().GetAttackTimes();

            if (!initAura)
            {
                initAura = true;

                HeroAura.Init(battle, eventListener, this);
            }
        }

        internal void Silence()
        {
            UnregisterAura();
        }

        internal void Die()
        {
            UnregisterAura();
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

            eventListener.DispatchEvent(HeroAura.ATTACK, ref effectList, this, _hero);

            return effectList;
        }

        private void UnregisterAura()
        {
            initAura = false;

            eventListener.DispatchEvent(HeroAura.REMOVE_AURA, this);
        }

        internal void WriteToStream(BinaryWriter _bw)
        {
            _bw.Write(sds.GetID());

            _bw.Write(isMine);

            _bw.Write(pos);

            _bw.Write(nowHp);

            _bw.Write(nowShield);

            _bw.Write(canAction);
        }

        internal void ReadFromStream(BinaryReader _br)
        {
            sds = Battle.GetHeroData(_br.ReadInt32());

            isMine = _br.ReadBoolean();

            PosChange(_br.ReadInt32());

            nowHp = _br.ReadInt32();

            nowShield = _br.ReadInt32();

            canAction = _br.ReadInt32();

            attackTimes = sds.GetHeroType().GetAttackTimes();

            SetAction(HeroAction.NULL);

            initAura = true;

            HeroAura.Init(battle, eventListener, this);
        }
    }
}

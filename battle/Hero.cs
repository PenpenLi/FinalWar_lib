using superEvent;
using System.Collections.Generic;
using System.Collections;

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

        internal int uid { get; private set; }

        public IHeroSDS sds { get; private set; }

        public int pos { get; private set; }

        internal HeroAction action { get; private set; }

        internal int actionTarget { get; private set; }

        public int nowHp { get; private set; }

        public int nowShield { get; private set; }

        private int attackFix = 0;

        private int abilityFix = 0;

        private bool recoverShield = true;

        public bool canMove { get; private set; }

        internal int attackTimes;

        internal Hero(bool _isMine, IHeroSDS _sds, int _pos, int _uid)
        {
            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            uid = _uid;

            nowHp = sds.GetHp();

            nowShield = sds.GetShield();

            canMove = true;

            attackTimes = sds.GetAttackTimes();

            SetAction(HeroAction.NULL);
        }

        internal Hero(bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowShield)
        {
            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            nowHp = _nowHp;

            nowShield = _nowShield;

            canMove = true;

            attackTimes = sds.GetAttackTimes();

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
        }

        internal void PosChange(int _pos)
        {
            pos = _pos;
        }

        internal void BeDamage(int _value)
        {
            if (_value > nowShield)
            {
                nowShield = 0;

                _value -= nowShield;

                nowHp -= _value;
            }
            else
            {
                nowShield -= _value;
            }
        }

        internal void BeDamage(int _value, out int _shieldDamage, out int _hpDamage)
        {
            if(_value > nowShield)
            {
                _shieldDamage = -nowShield;

                _value -= nowShield;

                nowShield = 0;

                _hpDamage = -_value;

                nowHp -= _value;
            }
            else
            {
                _shieldDamage = -_value;

                nowShield -= _value;

                _hpDamage = 0;
            }
        }

        internal void BeHpDamage(int _value)
        {
            nowHp -= _value;
        }

        internal bool IsAlive()
        {
            return nowHp > 0;
        }

        //internal void ShieldChange(int _value)
        //{
        //    nowShield += _value;

        //    if (nowShield < 0)
        //    {
        //        nowShield = 0;
        //    }
        //}

        //internal bool HpChange(int _value)
        //{
        //    nowHp += _value;

        //    if (nowHp > sds.GetHp())
        //    {
        //        nowHp = sds.GetHp();
        //    }
        //    else if (nowHp < 1)
        //    {
        //        nowHp = 0;

        //        return true;
        //    }

        //    return false;
        //}

        internal void LevelUp(IHeroSDS _sds)
        {
            sds = _sds;
        }

        internal void SetAttackFix(int _value)
        {
            attackFix += _value;
        }

        internal void SetAbilityFix(int _value)
        {
            abilityFix += _value;
        }

        internal void DisableRecoverShield()
        {
            recoverShield = false;
        }

        internal void DisableMove()
        {
            canMove = false;
        }

        internal int GetDamage()
        {
            int attack = sds.GetAttack() + attackFix;

            return attack;
        }

        internal IEnumerator Recover()
        {
            if (recoverShield)
            {
                nowShield = sds.GetShield();

                yield return new BattleRecoverShieldVO(pos);
            }
            else
            {
                recoverShield = true;
            }

            attackFix = abilityFix = 0;

            canMove = true;

            attackTimes = sds.GetAttackTimes();
        }
    }
}

using superEvent;
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

        private SuperEventListenerV eventListenerV;

        internal Hero(SuperEventListenerV _eventListenerV, bool _isMine, IHeroSDS _sds, int _pos, int _uid)
        {
            eventListenerV = _eventListenerV;

            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            uid = _uid;

            nowHp = sds.GetHp();

            nowShield = sds.GetShield();

            canMove = true;

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

        internal void ShieldChange(int _value)
        {
            nowShield += _value;

            if (nowShield < 0)
            {
                nowShield = 0;
            }
        }

        internal bool HpChange(int _value)
        {
            nowHp += _value;

            if (nowHp > sds.GetHp())
            {
                nowHp = sds.GetHp();
            }
            else if (nowHp < 1)
            {
                nowHp = 0;

                return true;
            }

            return false;
        }

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

        private int GetAttackFix()
        {
            int attack = sds.GetAttack() + attackFix;

            eventListenerV.DispatchEvent(AuraEffect.FIX_ATTACK.ToString(), ref attack, this);

            return attack;
        }

        private int GetAbilityFix()
        {
            int result = abilityFix;

            eventListenerV.DispatchEvent(AuraEffect.FIX_ABILITY.ToString(), ref result, this);

            return result;
        }

        internal int GetAttackDamage()
        {
            int attackDamage;

            if (sds.GetAbilityType() == AbilityType.Null || sds.GetAbilityType() == AbilityType.Attacker)
            {
                attackDamage = GetAttackFix() + GetAbilityFix();
            }
            else
            {
                attackDamage = GetAttackFix();
            }

            if (attackDamage < 0)
            {
                attackDamage = 0;
            }

            return attackDamage;
        }

        internal int GetShootDamage()
        {
            int shootDamage = GetAttackFix() + GetAbilityFix();

            if (shootDamage < 0)
            {
                shootDamage = 0;
            }

            return shootDamage;
        }

        internal int GetCounterDamage()
        {
            int counterDamage;

            if (sds.GetAbilityType() == AbilityType.Counter)
            {
                counterDamage = GetAttackFix() + GetAbilityFix();
            }
            else
            {
                counterDamage = GetAttackFix();
            }

            if (counterDamage < 0)
            {
                counterDamage = 0;
            }

            return counterDamage;
        }

        internal int GetSupportDamage()
        {
            int supportDamage = GetAttackFix() + GetAbilityFix();

            if (supportDamage < 0)
            {
                supportDamage = 0;
            }

            return supportDamage;
        }

        internal int GetHelpDamage()
        {
            int helpDamage = GetAttackFix() + GetAbilityFix();

            if (helpDamage < 0)
            {
                helpDamage = 0;
            }

            return helpDamage;
        }

        internal void ServerRecover(LinkedList<IBattleVO> _voList)
        {
            if (recoverShield)
            {
                bool tmpRecoverShield = true;

                eventListenerV.DispatchEvent<bool>(AuraEffect.DISABLE_RECOVER_SHIELD.ToString(), ref tmpRecoverShield, this);

                if (tmpRecoverShield)
                {
                    nowShield = sds.GetShield();

                    _voList.AddLast(new BattleRecoverShieldVO(pos));
                }
            }
            else
            {
                recoverShield = true;
            }

            attackFix = abilityFix = 0;

            canMove = true;
        }

        internal void ClientRecover()
        {
            nowShield = sds.GetShield();
        }
    }
}

using superEvent;

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

        internal int nowHp;

        internal int nowShield;

        private int attackFix = 0;

        private int speedFix = 0;

        private bool recoverShield = true;

        internal bool canMove { get; private set; }

        public int canAction { get; private set; }

        internal int attackTimes;

        private Battle battle;

        private SuperEventListener eventListener;

        private bool initAura = false;

        private int shieldDamage = 0;

        private int hpDamage = 0;

        private int damage = 0;

        internal Hero(Battle _battle, SuperEventListener _eventListener, bool _isMine, IHeroSDS _sds, int _pos, int _uid, bool _initAura)
        {
            battle = _battle;

            eventListener = _eventListener;

            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            uid = _uid;

            nowHp = sds.GetHp();

            nowShield = sds.GetShield();

            canMove = true;

            canAction = 0;

            attackTimes = sds.GetHeroType().GetAttackTimes();

            SetAction(HeroAction.NULL);

            if (_initAura)
            {
                initAura = true;

                HeroAura.Init(battle, eventListener, this);
            }
        }

        internal Hero(Battle _battle, SuperEventListener _eventListener, bool _isMine, IHeroSDS _sds, int _pos, int _nowHp, int _nowShield, int _canAction)
        {
            battle = _battle;

            eventListener = _eventListener;

            isMine = _isMine;

            sds = _sds;

            PosChange(_pos);

            nowHp = _nowHp;

            nowShield = _nowShield;

            canMove = true;

            canAction = _canAction;

            attackTimes = sds.GetHeroType().GetAttackTimes();

            SetAction(HeroAction.NULL);

            initAura = true;

            HeroAura.Init(battle, eventListener, this);
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
            damage += _value;
        }

        internal void BeShieldDamage(int _value)
        {
            shieldDamage += _value;
        }

        internal void BeHpDamage(int _value)
        {
            hpDamage += _value;
        }

        internal void ProcessDamage()
        {
            nowShield -= shieldDamage;

            nowHp -= hpDamage;

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

            shieldDamage = hpDamage = damage = 0;
        }

        public void ProcessDamage(out int _nowShield, out int _nowHp)
        {
            _nowShield = nowShield;

            _nowHp = nowHp;

            _nowShield -= shieldDamage;

            _nowHp -= hpDamage;

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

        internal void LevelUp(IHeroSDS _sds)
        {
            sds = _sds;
        }

        internal void SetAttackFix(int _value)
        {
            attackFix += _value;
        }

        internal void DisableRecoverShield()
        {
            recoverShield = false;
        }

        internal void DisableMove()
        {
            canMove = false;
        }

        internal void DisableAction()
        {
            canAction = 2;
        }

        public int GetDamage()
        {
            int attackFixAura = 0;

            eventListener.DispatchEvent(HeroAura.FIX_ATTACK, ref attackFixAura, this);

            int attack = sds.GetAttack() + attackFix + attackFixAura + nowShield;

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
        }

        public int GetDamageWithoutShield()
        {
            int attackFixAura = 0;

            eventListener.DispatchEvent(HeroAura.FIX_ATTACK, ref attackFixAura, this);

            int attack = sds.GetAttack() + attackFix + attackFixAura;

            if (attack < 0)
            {
                attack = 0;
            }

            return attack;
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

            attackFix = 0;

            speedFix = 0;

            canMove = true;

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

        internal void Die()
        {
            eventListener.DispatchEvent(HeroAura.DIE, this);
        }
    }
}

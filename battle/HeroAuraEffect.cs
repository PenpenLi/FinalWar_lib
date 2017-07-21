using superEvent;
using System.Collections.Generic;
using System;

namespace FinalWar
{
    internal static partial class HeroAura
    {
        public const string REMOVE_AURA = "removeAura";
        public const string FIX_ATTACK = "fixAttack";
        public const string FIX_SPEED = "fixSpeed";
        public const string FIX_CAN_PIERCE_SHIELD = "fixCanPierceShield";
        public const string FIX_CAN_MOVE = "fixCanMove";

        private static int RegisterAura(AuraEffect _effect, Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            int result;

            switch (_effect)
            {
                case AuraEffect.FIX_ALLY_ATTACK:

                    result = FixAllyAttack(_battle, _eventListener, _hero, _data);

                    break;

                case AuraEffect.FIX_ALLY_SPEED:

                    result = FixAllySpeed(_battle, _eventListener, _hero, _data);

                    break;

                case AuraEffect.FIX_SELF_SPEED:

                    result = FixSelfSpeed(_battle, _eventListener, _hero, _data);

                    break;

                case AuraEffect.FIX_SELF_ATTACK:

                    result = FixSelfAttack(_battle, _eventListener, _hero, _data);

                    break;

                case AuraEffect.FIX_ENEMY_SPEED:

                    result = FixEnemySpeed(_battle, _eventListener, _hero, _data);

                    break;

                case AuraEffect.FIX_ENEMY_ATTACK:

                    result = FixEnemyAttack(_battle, _eventListener, _hero, _data);

                    break;

                case AuraEffect.FIX_ENEMY_CAN_MOVE:

                    result = FixEnemyCanMove(_battle, _eventListener, _hero);

                    break;

                case AuraEffect.FIX_SELF_CAN_PIERCE_SHIELD:

                    result = FixSelfCanPierceShield(_battle, _eventListener, _hero);

                    break;

                case AuraEffect.FIX_ALLY_CAN_PIERCE_SHIELD:

                    result = FixAllyCanPierceShield(_battle, _eventListener, _hero);

                    break;

                default:

                    throw new Exception("unknown auraeffect");
            }

            return result;
        }






        private static int FixAllyAttack(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _attackFix, Hero _triggerHero)
            {
                if (_triggerHero != _hero && _triggerHero.isMine == _hero.isMine)
                {
                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (tmpList.Contains(_triggerHero.pos))
                    {
                        _attackFix += _data;
                    }
                }
            };

            return _eventListener.AddListener(FIX_ATTACK, dele);
        }

        private static int FixAllySpeed(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _speedFix, Hero _triggerHero)
            {
                if (_triggerHero != _hero && _triggerHero.isMine == _hero.isMine)
                {
                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (tmpList.Contains(_triggerHero.pos))
                    {
                        _speedFix += _data;
                    }
                }
            };

            return _eventListener.AddListener(FIX_SPEED, dele);
        }

        private static int FixSelfSpeed(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _speedFix, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _speedFix += _data;
                }
            };

            return _eventListener.AddListener(FIX_SPEED, dele);
        }

        private static int FixSelfAttack(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _attackFix, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _attackFix += _data;
                }
            };

            return _eventListener.AddListener(FIX_ATTACK, dele);
        }

        private static int FixEnemySpeed(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _speedFix, Hero _triggerHero)
            {
                if (_triggerHero.isMine != _hero.isMine)
                {
                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (tmpList.Contains(_triggerHero.pos))
                    {
                        _speedFix += _data;
                    }
                }
            };

            return _eventListener.AddListener(FIX_SPEED, dele);
        }

        private static int FixEnemyAttack(Battle _battle, SuperEventListener _eventListener, Hero _hero, int _data)
        {
            SuperEventListener.SuperFunctionCallBackV1<int, Hero> dele = delegate (int _index, ref int _attackFix, Hero _triggerHero)
            {
                if (_triggerHero.isMine != _hero.isMine)
                {
                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (tmpList.Contains(_triggerHero.pos))
                    {
                        _attackFix += _data;
                    }
                }
            };

            return _eventListener.AddListener(FIX_ATTACK, dele);
        }

        private static int FixEnemyCanMove(Battle _battle, SuperEventListener _eventListener, Hero _hero)
        {
            SuperEventListener.SuperFunctionCallBackV1<bool, Hero> dele = delegate (int _index, ref bool _canMove, Hero _triggerHero)
            {
                if (_triggerHero.isMine != _hero.isMine)
                {
                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (tmpList.Contains(_triggerHero.pos))
                    {
                        _canMove = false;
                    }
                }
            };

            return _eventListener.AddListener(FIX_CAN_MOVE, dele);
        }

        private static int FixSelfCanPierceShield(Battle _battle, SuperEventListener _eventListener, Hero _hero)
        {
            SuperEventListener.SuperFunctionCallBackV1<bool, Hero> dele = delegate (int _index, ref bool _canPierceShield, Hero _triggerHero)
            {
                if (_triggerHero == _hero)
                {
                    _canPierceShield = true;
                }
            };

            return _eventListener.AddListener(FIX_CAN_PIERCE_SHIELD, dele);
        }

        private static int FixAllyCanPierceShield(Battle _battle, SuperEventListener _eventListener, Hero _hero)
        {
            SuperEventListener.SuperFunctionCallBackV1<bool, Hero> dele = delegate (int _index, ref bool _canPierceShield, Hero _triggerHero)
            {
                if (_triggerHero != _hero && _triggerHero.isMine == _hero.isMine)
                {
                    List<int> tmpList = BattlePublicTools.GetNeighbourPos(_battle.mapData, _hero.pos);

                    if (tmpList.Contains(_triggerHero.pos))
                    {
                        _canPierceShield = true;
                    }
                }
            };

            return _eventListener.AddListener(FIX_CAN_PIERCE_SHIELD, dele);
        }
    }
}

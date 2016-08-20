using System;
using System.Collections.Generic;
using System.IO;

namespace FinalWar
{
    public class BattleVOTools
    {
        private enum BattleVOType
        {
            SUMMON,
            MOVE,
            RUSH,
            SHOOT,
            ATTACK,
            DEATH
        }
        public static void WriteDataToStream(List<ValueType> _voList, BinaryWriter _bw)
        {
            _bw.Write(_voList.Count);

            for (int i = 0; i < _voList.Count; i++)
            {
                ValueType vo = _voList[i];

                if(vo is BattleSummonVO)
                {
                    _bw.Write((int)BattleVOType.SUMMON);

                    BattleSummonVO summon = (BattleSummonVO)vo;

                    _bw.Write(summon.cardUid);

                    _bw.Write(summon.heroID);

                    _bw.Write(summon.pos);
                }
                else if(vo is BattleMoveVO)
                {
                    _bw.Write((int)BattleVOType.MOVE);

                    BattleMoveVO move = (BattleMoveVO)vo;

                    _bw.Write(move.moves.Count);

                    Dictionary<int, int>.Enumerator enumerator = move.moves.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        _bw.Write(enumerator.Current.Key);

                        _bw.Write(enumerator.Current.Value);
                    }
                }
                else if(vo is BattleRushVO)
                {
                    _bw.Write((int)BattleVOType.RUSH);

                    BattleRushVO rush = (BattleRushVO)vo;

                    _bw.Write(rush.attackers.Count);

                    for(int m = 0; m < rush.attackers.Count; m++)
                    {
                        KeyValuePair<int, int> pair = rush.attackers[m];

                        _bw.Write(pair.Key);

                        _bw.Write(pair.Value);
                    }

                    _bw.Write(rush.stander);
                }
                else if(vo is BattleShootVO)
                {
                    _bw.Write((int)BattleVOType.SHOOT);

                    BattleShootVO shoot = (BattleShootVO)vo;

                    _bw.Write(shoot.shooters.Count);

                    for (int m = 0; m < shoot.shooters.Count; m++)
                    {
                        KeyValuePair<int, int> pair = shoot.shooters[m];

                        _bw.Write(pair.Key);

                        _bw.Write(pair.Value);
                    }

                    _bw.Write(shoot.stander);
                }
                else if(vo is BattleAttackVO)
                {
                    _bw.Write((int)BattleVOType.ATTACK);

                    BattleAttackVO attack = (BattleAttackVO)vo;

                    _bw.Write(attack.attacker);

                    _bw.Write(attack.defender);

                    _bw.Write(attack.supporter);

                    _bw.Write(attack.damage);

                    _bw.Write(attack.damageSelf);
                }
                else if(vo is BattleDeathVO)
                {
                    _bw.Write((int)BattleVOType.DEATH);

                    BattleDeathVO death = (BattleDeathVO)vo;

                    _bw.Write(death.deads.Count);

                    for(int m = 0; m < death.deads.Count; m++)
                    {
                        _bw.Write(death.deads[m]);
                    }
                }
            }
        }

        public static List<ValueType> ReadDataFromStream(BinaryReader _br)
        {
            List<ValueType> result = new List<ValueType>();

            int num = _br.ReadInt32();

            for(int i = 0; i < num; i++)
            {
                BattleVOType type = (BattleVOType)_br.ReadInt32();

                switch (type)
                {
                    case BattleVOType.SUMMON:

                        int cardUid = _br.ReadInt32();

                        int heroID = _br.ReadInt32();

                        int summonPos = _br.ReadInt32();

                        result.Add(new BattleSummonVO(cardUid, heroID, summonPos));

                        break;

                    case BattleVOType.MOVE:

                        Dictionary<int, int> moveDic = new Dictionary<int, int>();

                        int moveNum = _br.ReadInt32();

                        for(int m = 0; m < moveNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int targetPos = _br.ReadInt32();

                            moveDic.Add(pos, targetPos);
                        }

                        result.Add(new BattleMoveVO(moveDic));

                        break;

                    case BattleVOType.RUSH:

                        List<KeyValuePair<int, int>> attackers = new List<KeyValuePair<int, int>>();

                        int attackerNum = _br.ReadInt32();

                        for(int m = 0; m < attackerNum; m++)
                        {
                            int rusher = _br.ReadInt32();

                            int damage = _br.ReadInt32();

                            attackers.Add(new KeyValuePair<int, int>(rusher, damage));
                        }

                        int stander = _br.ReadInt32();

                        result.Add(new BattleRushVO(attackers, stander));

                        break;

                    case BattleVOType.SHOOT:

                        List<KeyValuePair<int, int>> shooters = new List<KeyValuePair<int, int>>();

                        int shooterNum = _br.ReadInt32();

                        for (int m = 0; m < shooterNum; m++)
                        {
                            int shooter = _br.ReadInt32();

                            int damage = _br.ReadInt32();

                            shooters.Add(new KeyValuePair<int, int>(shooter, damage));
                        }

                        stander = _br.ReadInt32();

                        result.Add(new BattleShootVO(shooters, stander));

                        break;

                    case BattleVOType.ATTACK:

                        int attacker = _br.ReadInt32();

                        int defender = _br.ReadInt32();

                        int supporter = _br.ReadInt32();

                        int damageEnemy = _br.ReadInt32();

                        int damageSelf = _br.ReadInt32();

                        result.Add(new BattleAttackVO(attacker, defender, supporter, damageEnemy, damageSelf));

                        break;

                    case BattleVOType.DEATH:

                        List<int> deads = new List<int>();

                        int deadsNum = _br.ReadInt32();

                        for(int m = 0; m < deadsNum; m++)
                        {
                            int deadPos = _br.ReadInt32();

                            deads.Add(deadPos);
                        }

                        result.Add(new BattleDeathVO(deads));

                        break;
                }
            }

            return result;
        }
    }
}

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
            DEATH,
            POWERCHANGE
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
                        _bw.Write(rush.attackers[m]);
                    }

                    _bw.Write(rush.stander);

                    _bw.Write(rush.damage);
                }
                else if(vo is BattleShootVO)
                {
                    _bw.Write((int)BattleVOType.SHOOT);

                    BattleShootVO shoot = (BattleShootVO)vo;

                    _bw.Write(shoot.shooters.Count);

                    for (int m = 0; m < shoot.shooters.Count; m++)
                    {
                        _bw.Write(shoot.shooters[m]);
                    }

                    _bw.Write(shoot.stander);

                    _bw.Write(shoot.damage);
                }
                else if(vo is BattleAttackVO)
                {
                    _bw.Write((int)BattleVOType.ATTACK);

                    BattleAttackVO attack = (BattleAttackVO)vo;

                    _bw.Write(attack.attackers.Count);

                    for(int m = 0; m < attack.attackers.Count; m++)
                    {
                        _bw.Write(attack.attackers[m]);

                        _bw.Write(attack.attackersDamage[m]);
                    }

                    _bw.Write(attack.supporters.Count);

                    for (int m = 0; m < attack.supporters.Count; m++)
                    {
                        _bw.Write(attack.supporters[m]);

                        _bw.Write(attack.supportersDamage[m]);
                    }

                    _bw.Write(attack.defender);

                    _bw.Write(attack.defenderDamage);
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
                else if(vo is BattlePowerChangeVO)
                {
                    _bw.Write((int)BattleVOType.POWERCHANGE);

                    BattlePowerChangeVO powerChange = (BattlePowerChangeVO)vo;

                    _bw.Write(powerChange.powerChanges.Count);

                    Dictionary<int, int>.Enumerator enumerator = powerChange.powerChanges.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        _bw.Write(enumerator.Current.Key);

                        _bw.Write(enumerator.Current.Value);
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

                        List<int> attackers = new List<int>();

                        int attackerNum = _br.ReadInt32();

                        for(int m = 0; m < attackerNum; m++)
                        {
                            int rusher = _br.ReadInt32();

                            attackers.Add(rusher);
                        }

                        int stander = _br.ReadInt32();

                        int damage = _br.ReadInt32();

                        result.Add(new BattleRushVO(attackers, stander, damage));

                        break;

                    case BattleVOType.SHOOT:

                        List<int> shooters = new List<int>();

                        int shooterNum = _br.ReadInt32();

                        for (int m = 0; m < shooterNum; m++)
                        {
                            int shooter = _br.ReadInt32();

                            shooters.Add(shooter);
                        }

                        stander = _br.ReadInt32();

                        damage = _br.ReadInt32();

                        result.Add(new BattleShootVO(shooters, stander, damage));

                        break;

                    case BattleVOType.ATTACK:

                        attackers = new List<int>();

                        List<int> attackersDamage = new List<int>();

                        List<int> supporters = new List<int>();

                        List<int> supportersDamage = new List<int>();

                        attackerNum = _br.ReadInt32();

                        for (int m = 0; m < attackerNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int attackerDamage = _br.ReadInt32();

                            attackers.Add(pos);

                            attackersDamage.Add(attackerDamage);
                        }

                        attackerNum = _br.ReadInt32();

                        for (int m = 0; m < attackerNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int supporterDamage = _br.ReadInt32();

                            supporters.Add(pos);

                            supportersDamage.Add(supporterDamage);
                        }

                        int defender = _br.ReadInt32();

                        int defenderDamage = _br.ReadInt32();

                        result.Add(new BattleAttackVO(attackers, supporters, defender, attackersDamage, supportersDamage, defenderDamage));

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

                    case BattleVOType.POWERCHANGE:

                        Dictionary<int, int> powerChangeDic = new Dictionary<int, int>();

                        int powerChangeNum = _br.ReadInt32();

                        for (int m = 0; m < powerChangeNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int powerChange = _br.ReadInt32();

                            powerChangeDic.Add(pos, powerChange);
                        }

                        result.Add(new BattlePowerChangeVO(powerChangeDic));

                        break;
                }
            }

            return result;
        }
    }
}

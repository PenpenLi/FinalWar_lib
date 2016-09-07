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

                    _bw.Write(move.powerChange.Count);

                    Dictionary<int, Dictionary<int, int>>.Enumerator enumerator2 = move.powerChange.GetEnumerator();

                    while (enumerator2.MoveNext())
                    {
                        _bw.Write(enumerator2.Current.Key);

                        Dictionary<int, int> tmpDic = enumerator2.Current.Value;

                        _bw.Write(tmpDic.Count);

                        Dictionary<int, int>.Enumerator enumerator3 = tmpDic.GetEnumerator();

                        while (enumerator3.MoveNext())
                        {
                            _bw.Write(enumerator3.Current.Key);

                            _bw.Write(enumerator3.Current.Value);
                        }
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

                        _bw.Write(rush.attackersPowerChange[m]);
                    }

                    _bw.Write(rush.stander);

                    _bw.Write(rush.damage);

                    _bw.Write(rush.standerPowerChange);
                }
                else if(vo is BattleShootVO)
                {
                    _bw.Write((int)BattleVOType.SHOOT);

                    BattleShootVO shoot = (BattleShootVO)vo;

                    _bw.Write(shoot.shooters.Count);

                    for (int m = 0; m < shoot.shooters.Count; m++)
                    {
                        _bw.Write(shoot.shooters[m]);

                        _bw.Write(shoot.shootersPowerChange[m]);
                    }

                    _bw.Write(shoot.stander);

                    _bw.Write(shoot.damage);

                    _bw.Write(shoot.standerPowerChange);
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

                        _bw.Write(attack.attackersPowerChange[m]);
                    }

                    _bw.Write(attack.supporters.Count);

                    for (int m = 0; m < attack.supporters.Count; m++)
                    {
                        _bw.Write(attack.supporters[m]);

                        _bw.Write(attack.supportersDamage[m]);

                        _bw.Write(attack.supportersPowerChange[m]);
                    }

                    _bw.Write(attack.defender);

                    _bw.Write(attack.defenderDamage);

                    _bw.Write(attack.defenderPowerChange);
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

                    _bw.Write(death.powerChange.Count);

                    Dictionary<int, int>.Enumerator enumerator = death.powerChange.GetEnumerator();

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

                        Dictionary<int, Dictionary<int, int>> movePowerChange = new Dictionary<int, Dictionary<int, int>>();

                        int moveNum = _br.ReadInt32();

                        for(int m = 0; m < moveNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int targetPos = _br.ReadInt32();

                            moveDic.Add(pos, targetPos);

                            Dictionary<int, int> tmpDic = new Dictionary<int, int>();

                            int powerChangeNum = _br.ReadInt32();

                            for(int n = 0; n < powerChangeNum; n++)
                            {
                                int powerChangePos = _br.ReadInt32();

                                int powerChange = _br.ReadInt32();

                                tmpDic.Add(powerChangePos, powerChange);
                            }

                            movePowerChange.Add(pos, tmpDic);
                        }

                        result.Add(new BattleMoveVO(moveDic, movePowerChange));

                        break;

                    case BattleVOType.RUSH:

                        List<int> attackers = new List<int>();

                        List<int> attackersPowerChange = new List<int>();

                        int attackerNum = _br.ReadInt32();

                        for(int m = 0; m < attackerNum; m++)
                        {
                            int rusher = _br.ReadInt32();

                            int powerChange = _br.ReadInt32();

                            attackers.Add(rusher);

                            attackersPowerChange.Add(powerChange);
                        }

                        int stander = _br.ReadInt32();

                        int damage = _br.ReadInt32();

                        int standerPowerChange = _br.ReadInt32();

                        result.Add(new BattleRushVO(attackers, stander, damage, attackersPowerChange, standerPowerChange));

                        break;

                    case BattleVOType.SHOOT:

                        List<int> shooters = new List<int>();

                        List<int> shootersPowerChange = new List<int>();

                        int shooterNum = _br.ReadInt32();

                        for (int m = 0; m < shooterNum; m++)
                        {
                            int shooter = _br.ReadInt32();

                            int powerChange = _br.ReadInt32();

                            shooters.Add(shooter);

                            shootersPowerChange.Add(powerChange);
                        }

                        stander = _br.ReadInt32();

                        damage = _br.ReadInt32();

                        standerPowerChange = _br.ReadInt32();

                        result.Add(new BattleShootVO(shooters, stander, damage, shootersPowerChange, standerPowerChange));

                        break;

                    case BattleVOType.ATTACK:

                        attackers = new List<int>();

                        List<int> attackersDamage = new List<int>();

                        attackersPowerChange = new List<int>();

                        List<int> supporters = new List<int>();

                        List<int> supportersDamage = new List<int>();

                        List<int> supportersPowerChange = new List<int>();

                        attackerNum = _br.ReadInt32();

                        for (int m = 0; m < attackerNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int attackerDamage = _br.ReadInt32();

                            int attackerPowerChange = _br.ReadInt32();

                            attackers.Add(pos);

                            attackersDamage.Add(attackerDamage);

                            attackersPowerChange.Add(attackerPowerChange);
                        }

                        attackerNum = _br.ReadInt32();

                        for (int m = 0; m < attackerNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int supporterDamage = _br.ReadInt32();

                            int supporterPowerChange = _br.ReadInt32();

                            supporters.Add(pos);

                            supportersDamage.Add(supporterDamage);

                            supportersPowerChange.Add(supporterPowerChange);
                        }

                        int defender = _br.ReadInt32();

                        int defenderDamage = _br.ReadInt32();

                        int defenderPowerChange = _br.ReadInt32();

                        result.Add(new BattleAttackVO(attackers, supporters, defender, attackersDamage, supportersDamage, defenderDamage, attackersPowerChange, supportersPowerChange, defenderPowerChange));

                        break;

                    case BattleVOType.DEATH:

                        List<int> deads = new List<int>();

                        Dictionary<int, int> deathPowerChange = new Dictionary<int, int>();

                        int deadsNum = _br.ReadInt32();

                        for(int m = 0; m < deadsNum; m++)
                        {
                            int deadPos = _br.ReadInt32();

                            deads.Add(deadPos);
                        }

                        int deathPowerChangeNum = _br.ReadInt32();

                        for(int m = 0; m < deathPowerChangeNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            int powerChange = _br.ReadInt32();

                            deathPowerChange.Add(pos, powerChange);
                        }

                        result.Add(new BattleDeathVO(deads, deathPowerChange));

                        break;
                }
            }

            return result;
        }
    }
}

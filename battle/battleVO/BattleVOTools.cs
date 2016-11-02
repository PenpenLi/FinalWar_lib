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
            CHANGE
        }
        public static void WriteDataToStream(List<ValueType> _voList, BinaryWriter _bw)
        {
            _bw.Write(_voList.Count);

            for (int i = 0; i < _voList.Count; i++)
            {
                ValueType vo = _voList[i];

                if (vo is BattleSummonVO)
                {
                    _bw.Write((int)BattleVOType.SUMMON);

                    BattleSummonVO summon = (BattleSummonVO)vo;

                    _bw.Write(summon.cardUid);

                    _bw.Write(summon.heroID);

                    _bw.Write(summon.pos);
                }
                else if (vo is BattleMoveVO)
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
                else if (vo is BattleRushVO)
                {
                    _bw.Write((int)BattleVOType.RUSH);

                    BattleRushVO rush = (BattleRushVO)vo;

                    _bw.Write(rush.attackers.Count);

                    for(int m = 0; m < rush.attackers.Count; m++)
                    {
                        _bw.Write(rush.attackers[m]);
                    }

                    _bw.Write(rush.stander);

                    _bw.Write(rush.shieldDamage);

                    _bw.Write(rush.hpDamage);
                }
                else if (vo is BattleShootVO)
                {
                    _bw.Write((int)BattleVOType.SHOOT);

                    BattleShootVO shoot = (BattleShootVO)vo;

                    _bw.Write(shoot.shooters.Count);

                    for (int m = 0; m < shoot.shooters.Count; m++)
                    {
                        _bw.Write(shoot.shooters[m]);
                    }

                    _bw.Write(shoot.stander);

                    _bw.Write(shoot.shieldDamage);

                    _bw.Write(shoot.hpDamage);
                }
                else if (vo is BattleAttackVO)
                {
                    _bw.Write((int)BattleVOType.ATTACK);

                    BattleAttackVO attack = (BattleAttackVO)vo;

                    _bw.Write(attack.attackers.Count);

                    for(int m = 0; m < attack.attackers.Count; m++)
                    {
                        _bw.Write(attack.attackers[m]);

                        _bw.Write(attack.attackersShieldDamage[m]);

                        _bw.Write(attack.attackersHpDamage[m]);
                    }

                    _bw.Write(attack.supporters.Count);

                    for (int m = 0; m < attack.supporters.Count; m++)
                    {
                        _bw.Write(attack.supporters[m]);

                        _bw.Write(attack.supportersShieldDamage[m]);

                        _bw.Write(attack.supportersHpDamage[m]);
                    }

                    _bw.Write(attack.defender);

                    _bw.Write(attack.defenderShieldDamage);

                    _bw.Write(attack.defenderHpDamage);
                }
                else if (vo is BattleDeathVO)
                {
                    _bw.Write((int)BattleVOType.DEATH);

                    BattleDeathVO death = (BattleDeathVO)vo;

                    _bw.Write(death.deads.Count);

                    for(int m = 0; m < death.deads.Count; m++)
                    {
                        _bw.Write(death.deads[m]);
                    }
                }
                else if(vo is BattleChangeVO)
                {
                    _bw.Write((int)BattleVOType.CHANGE);

                    BattleChangeVO change = (BattleChangeVO)vo;

                    _bw.Write(change.pos.Count);

                    for (int m = 0; m < change.pos.Count; m++)
                    {
                        _bw.Write(change.pos[m]);

                        _bw.Write(change.shieldChange[m]);

                        _bw.Write(change.hpChange[m]);
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

                        int shieldDamage = _br.ReadInt32();

                        int hpDamage = _br.ReadInt32();

                        result.Add(new BattleRushVO(attackers, stander, shieldDamage, hpDamage));

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

                        shieldDamage = _br.ReadInt32();

                        hpDamage = _br.ReadInt32();

                        result.Add(new BattleShootVO(shooters, stander, shieldDamage, hpDamage));

                        break;

                    case BattleVOType.ATTACK:

                        attackers = new List<int>();

                        List<int> attackersShieldDamage = new List<int>();

                        List<int> attackersHpDamage = new List<int>();

                        List<int> supporters = new List<int>();

                        List<int> supportersShieldDamage = new List<int>();

                        List<int> supportersHpDamage = new List<int>();

                        attackerNum = _br.ReadInt32();

                        for (int m = 0; m < attackerNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            attackers.Add(pos);

                            int attackerShieldDamage = _br.ReadInt32();

                            attackersShieldDamage.Add(attackerShieldDamage);

                            int attackerHpDamage = _br.ReadInt32();

                            attackersHpDamage.Add(attackerHpDamage);
                        }

                        attackerNum = _br.ReadInt32();

                        for (int m = 0; m < attackerNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            supporters.Add(pos);

                            int supporterShieldDamage = _br.ReadInt32();

                            supportersShieldDamage.Add(supporterShieldDamage);

                            int supporterHpDamage = _br.ReadInt32();

                            supportersHpDamage.Add(supporterHpDamage);
                        }

                        int defender = _br.ReadInt32();

                        int defenderShieldDamage = _br.ReadInt32();

                        int defenderHpDamage = _br.ReadInt32();

                        result.Add(new BattleAttackVO(attackers, supporters, defender, attackersShieldDamage, attackersHpDamage, supportersShieldDamage, supportersHpDamage, defenderShieldDamage, defenderHpDamage));

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
                        
                    case BattleVOType.CHANGE:

                        List<int>  posList = new List<int>();

                        List<int> shieldChangeList = new List<int>();

                        List<int> hpChangeList = new List<int>();

                        int changeNum = _br.ReadInt32();

                        for (int m = 0; m < changeNum; m++)
                        {
                            int pos = _br.ReadInt32();

                            posList.Add(pos);

                            int shieldChange = _br.ReadInt32();

                            shieldChangeList.Add(shieldChange);

                            int hpChange = _br.ReadInt32();

                            hpChangeList.Add(hpChange);
                        }

                        result.Add(new BattleChangeVO(posList, shieldChangeList, hpChangeList));

                        break;
                }
            }

            return result;
        }
    }
}

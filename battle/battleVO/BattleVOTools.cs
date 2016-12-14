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
            CHANGE,
            ADDCARDS,
            DELCARDS,
            MONEYCHANGE,
            LEVELUP,
            RECOVER_SHIELD
        }

        public static void WriteDataToStream(bool _isMine, LinkedList<IBattleVO> _voList, BinaryWriter _bw)
        {
            _bw.Write(_voList.Count);

            LinkedList<IBattleVO>.Enumerator enumerator = _voList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                IBattleVO vo = enumerator.Current;

                if (vo is BattleSummonVO)
                {
                    _bw.Write((int)BattleVOType.SUMMON);
                }
                else if (vo is BattleMoveVO)
                {
                    _bw.Write((int)BattleVOType.MOVE);
                }
                else if (vo is BattleRushVO)
                {
                    _bw.Write((int)BattleVOType.RUSH);
                }
                else if (vo is BattleShootVO)
                {
                    _bw.Write((int)BattleVOType.SHOOT);
                }
                else if (vo is BattleAttackVO)
                {
                    _bw.Write((int)BattleVOType.ATTACK);
                }
                else if (vo is BattleDeathVO)
                {
                    _bw.Write((int)BattleVOType.DEATH);
                }
                else if (vo is BattleChangeVO)
                {
                    _bw.Write((int)BattleVOType.CHANGE);
                }
                else if (vo is BattleAddCardsVO)
                {
                    _bw.Write((int)BattleVOType.ADDCARDS);
                }
                else if (vo is BattleDelCardsVO)
                {
                    _bw.Write((int)BattleVOType.DELCARDS);
                }
                else if (vo is BattleMoneyChangeVO)
                {
                    _bw.Write((int)BattleVOType.MONEYCHANGE);
                }
                else if (vo is BattleLevelUpVO)
                {
                    _bw.Write((int)BattleVOType.LEVELUP);
                }
                else if (vo is BattleRecoverShieldVO)
                {
                    _bw.Write((int)BattleVOType.RECOVER_SHIELD);
                }

                vo.ToBytes(_isMine, _bw);
            }
        }

        public static LinkedList<IBattleVO> ReadDataFromStream(BinaryReader _br)
        {
            LinkedList<IBattleVO> result = new LinkedList<IBattleVO>();

            int num = _br.ReadInt32();

            for (int i = 0; i < num; i++)
            {
                BattleVOType type = (BattleVOType)_br.ReadInt32();

                IBattleVO vo;

                switch (type)
                {
                    case BattleVOType.SUMMON:

                        vo = new BattleSummonVO();

                        break;

                    case BattleVOType.MOVE:

                        vo = new BattleMoveVO();

                        break;

                    case BattleVOType.RUSH:

                        vo = new BattleRushVO();

                        break;

                    case BattleVOType.SHOOT:

                        vo = new BattleShootVO();

                        break;

                    case BattleVOType.ATTACK:

                        vo = new BattleAttackVO();

                        break;

                    case BattleVOType.DEATH:

                        vo = new BattleDeathVO();

                        break;

                    case BattleVOType.CHANGE:

                        vo = new BattleChangeVO();

                        break;

                    case BattleVOType.ADDCARDS:

                        vo = new BattleAddCardsVO();

                        break;

                    case BattleVOType.DELCARDS:

                        vo = new BattleDelCardsVO();

                        break;

                    case BattleVOType.MONEYCHANGE:

                        vo = new BattleMoneyChangeVO();

                        break;

                    case BattleVOType.RECOVER_SHIELD:

                        vo = new BattleRecoverShieldVO();

                        break;

                    default:

                        vo = new BattleLevelUpVO();

                        break;
                }

                vo.FromBytes(_br);

                result.AddLast(vo);
            }

            return result;
        }
    }
}

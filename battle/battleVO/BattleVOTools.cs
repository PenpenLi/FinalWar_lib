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
        public static void WriteDataToStream(IBattleVO _vo, BinaryWriter _bw)
        {
            _bw.Write(true);

            if (_vo is BattleSummonVO)
            {
                _bw.Write((int)BattleVOType.SUMMON);
            }
            else if (_vo is BattleMoveVO)
            {
                _bw.Write((int)BattleVOType.MOVE);
            }
            else if (_vo is BattleRushVO)
            {
                _bw.Write((int)BattleVOType.RUSH);
            }
            else if (_vo is BattleShootVO)
            {
                _bw.Write((int)BattleVOType.SHOOT);
            }
            else if (_vo is BattleAttackVO)
            {
                _bw.Write((int)BattleVOType.ATTACK);
            }
            else if (_vo is BattleDeathVO)
            {
                _bw.Write((int)BattleVOType.DEATH);
            }
            else if (_vo is BattleChangeVO)
            {
                _bw.Write((int)BattleVOType.CHANGE);
            }
            else if (_vo is BattleAddCardsVO)
            {
                _bw.Write((int)BattleVOType.ADDCARDS);
            }
            else if (_vo is BattleDelCardsVO)
            {
                _bw.Write((int)BattleVOType.DELCARDS);
            }
            else if (_vo is BattleMoneyChangeVO)
            {
                _bw.Write((int)BattleVOType.MONEYCHANGE);
            }
            else if (_vo is BattleLevelUpVO)
            {
                _bw.Write((int)BattleVOType.LEVELUP);
            }
            else if (_vo is BattleRecoverShieldVO)
            {
                _bw.Write((int)BattleVOType.RECOVER_SHIELD);
            }

            _vo.ToBytes(_bw);
        }

        public static void WriteDataToStreamEnd(BinaryWriter _bw)
        {
            _bw.Write(false);
        }

        public static List<IBattleVO> ReadDataFromStream(BinaryReader _br)
        {
            List<IBattleVO> result = new List<IBattleVO>();

            bool b = _br.ReadBoolean();

            while (b)
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

                result.Add(vo);

                b = _br.ReadBoolean();
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using superEvent;

namespace FinalWar
{
    internal class HeroAura
    {
        internal static string GetEventName(bool _isMine, AuraEffect _auraEffect)
        {
            return string.Format("{0}_{1}", _isMine, _auraEffect);
        }

        internal static void Init(Battle _battle, Hero _hero)
        {
            int[] eventIDs = new int[_hero.sds.GetAuras().Length];

            for (int i = 0; i < _hero.sds.GetAuras().Length; i++)
            {
                int auraID = _hero.sds.GetAuras()[i];

                IAuraSDS auraSDS = Battle.auraDataDic[auraID];

                SuperEventListenerV.EventCallBack dele = delegate (SuperEvent e, ref float _value)
                {
                    TriggerAura(_battle, _hero, auraSDS, e, ref _value);
                };

                eventIDs[i] = _battle.eventListenerV.AddListener(GetEventName(auraSDS.GetAuraTarget() == AuraTarget.ALLY ? _hero.isMine : !_hero.isMine, auraSDS.GetAuraEffect()), dele);
            }

            Action<SuperEvent> dieDele = delegate (SuperEvent e)
            {
                for (int i = 0; i < eventIDs.Length; i++)
                {
                    _battle.eventListenerV.RemoveListener(eventIDs[i]);
                }

                _battle.eventListener.RemoveListener(e.index);
            };

            _battle.eventListener.AddListener(HeroSkill.GetEventName(_hero.uid, SkillTime.DIE), dieDele);
        }
        
        private static void TriggerAura(Battle _battle, Hero _hero, IAuraSDS _auraSDS, SuperEvent e, ref float _value)
        {
            Hero targetHero = e.datas[0] as Hero;

            if(targetHero == _hero)
            {
                return;
            }

            List<int> pos = BattlePublicTools.GetNeighbourPos(_battle.mapData.neighbourPosMap, _hero.pos);

            if (pos.Contains(targetHero.pos))
            {
                _value = _value * _auraSDS.GetAuraDatas()[0];
            }
        }
    }
}

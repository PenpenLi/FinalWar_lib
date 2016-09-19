using System;
using System.Collections.Generic;
using superEvent;

namespace FinalWar
{
    internal class HeroSkill
    {
        internal static string GetEventName(int _heroUid, SkillTime _skillTime)
        {
            return string.Format("{0}_{1}", _heroUid, _heroUid);
        }

        private Battle battle;
        private Hero hero;

        private int[] eventIDs;

        internal HeroSkill(Battle _battle,Hero _hero)
        {
            battle = _battle;
            hero = _hero;

            eventIDs = new int[hero.sds.GetSkills().Length];

            for (int i = 0; i < hero.sds.GetSkills().Length; i++)
            {
                int skillID = hero.sds.GetSkills()[i];

                ISkillSDS skillSDS = Battle.skillDataDic[skillID];

                string eventName = GetEventName(hero.uid, skillSDS.GetSkillTime());

                Action<SuperEvent> dele = delegate(SuperEvent e)
                {
                    TriggerSkill(skillSDS, e);
                };

                eventIDs[i] = battle.eventListener.AddListener(eventName, dele);
            }
        }

        private void TriggerSkill(ISkillSDS _skillSDS, SuperEvent e)
        {
            switch (_skillSDS.GetSkillTime())
            {
                case SkillTime.SHOOT:

                    Shoot(e.datas[0] as Hero, e.datas[1] as Dictionary<Hero, int>, e.datas[2] as Dictionary<Hero, int>);

                    break;

                case SkillTime.ATTACK:



                    break;
            }
        }

        private void Shoot(Hero _target, Dictionary<Hero, int> _hpChangeDic, Dictionary<Hero, int> _powerChangeDic)
        {

        }
    }
}

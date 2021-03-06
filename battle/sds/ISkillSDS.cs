﻿public enum SkillTime
{
    ROUNDSTART,
    SUMMON,
    SHOOT,
    RUSH,
    ATTACK,
    COUNTER,
    RECOVER,
    DIE
}

public enum SkillTarget
{
    SELF,
    ALLY,
    ENEMY
}

public enum SkillEffect
{
    DAMAGE,
    DAMAGE_WITH_LEADER,
    FIX_ATTACK,
    FIX_SHOOT,
    FIX_COUNTER,
    FIX_DEFENSE,
    RECOVER,
    POWERCHANGE
}

public interface ISkillSDS
{
    SkillTime GetSkillTime();
    SkillTarget GetSkillTarget();
    int GetTargetNum();
    SkillEffect GetSkillEffect();
    float[] GetSkillDatas();
}

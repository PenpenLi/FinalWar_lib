public enum SkillTime
{
    ROUNDSTART,
    SUMMON,
    SHOOT,
    RUSH,
    ATTACK,
    COUNTER,
    RECOVER,
    DIE,
    CAPTURE
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
    SHIELD_DAMAGE,
    HP_CHANGE,
    FIX_ATTACK,
    FIX_SHOOT,
    RECOVER,
}

public interface ISkillSDS
{
    SkillTime GetSkillTime();
    SkillTarget GetSkillTarget();
    int GetTargetNum();
    SkillEffect GetSkillEffect();
    int[] GetSkillDatas();
}

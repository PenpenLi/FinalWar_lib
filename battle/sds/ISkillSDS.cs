public enum SkillTime
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
    RECOVER,
    POWERCHANGE
}

public interface ISkillSDS
{
    SkillTime GetSkillTime();
    SkillTarget GetSkillTarget();
    int GetTargetNum();
    SkillEffect GetSkillEffect();
    int[] GetSkillDatas();
}

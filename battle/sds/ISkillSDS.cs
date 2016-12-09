public enum SkillTime
{
    ROUNDSTART,
    SUMMON,
    SHOOT,
    RUSH,
    ATTACK,
    SUPPORT,
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
    FIX_ABILITY,
    RECOVER_ALL_HP,
    LEVEL_UP,
    DISABLE_RECOVER_SHIELD
}

public interface ISkillSDS
{
    SkillTime GetSkillTime();
    int GetPriority();
    SkillTarget GetSkillTarget();
    int GetTargetNum();
    SkillEffect GetSkillEffect();
    int[] GetSkillDatas();
}

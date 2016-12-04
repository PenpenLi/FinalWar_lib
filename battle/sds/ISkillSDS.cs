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
    CAPTURE,
    LEVELUP
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
    RECOVER_ALL_HP,
}

public interface ISkillSDS
{
    SkillTime GetSkillTime();
    SkillTarget GetSkillTarget();
    int GetTargetNum();
    SkillEffect GetSkillEffect();
    int[] GetSkillDatas();
}

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
    SHIELD_CHANGE,
    HP_CHANGE,
    FIX_ATTACK,
    FIX_ABILITY,
    RECOVER_ALL_HP,
    LEVEL_UP,
    DISABLE_RECOVER_SHIELD,
    ADD_CARDS,
    DEL_CARDS,
    DISABLE_MOVE
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

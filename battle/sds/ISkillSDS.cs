public enum SkillTime
{
    SHOOT,
    SUMMON,
    ATTACK,
    COUNTER,
    RECOVER
}

public enum SkillTarget
{
    SELF,
    ALLY,
    ENEMY,
    NEIGHBOUR_ALLY,
    NEIGHBOUR_ENEMY
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
    int[] GetSkillDatas();
}

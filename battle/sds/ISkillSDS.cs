public enum SkillEffect
{
    DAMAGE,
    HP_CHANGE,
    FIX_ATTACK,
    DISABLE_RECOVER_SHIELD,
    DISABLE_MOVE,
    DISABLE_ACTION,
    SHIELD_CHANGE,
    SILENCE,
    FIX_SPEED,
}

public interface ISkillSDS
{
    bool GetIsStop();
    SkillEffect GetSkillEffect();
    int[] GetSkillData();
}

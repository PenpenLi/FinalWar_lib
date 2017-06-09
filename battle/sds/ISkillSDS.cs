public enum SkillEffect
{
    DAMAGE,
    HP_DAMAGE,
    FIX_ATTACK,
    DISABLE_RECOVER_SHIELD,
    DISABLE_MOVE,
    DISABLE_ACTION
}

public interface ISkillSDS
{
    bool GetIsStop();
    SkillEffect GetSkillEffect();
    int GetSkillData();
}

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
    bool GetSkillTargetAlly();
    SkillEffect GetSkillEffect();
    int[] GetSkillDatas();
}

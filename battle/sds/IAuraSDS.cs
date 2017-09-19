﻿public enum AuraType
{
    FIX_INT,
    FIX_BOOL,
    CAST_SKILL
}

public enum AuraTarget
{
    NULL,
    OWNER,
    OWNER_NEIGHBOUR_ALLY,
    OWNER_NEIGHBOUR_ENEMY,
    TRIGGER,
    TRIGGER_TARGET
}

//public enum AuraCondition
//{
//    NULL,
//    INJURED,
//    HEALTHY,
//    LEVEL_HIGHER_THAN,
//    LEVEL_LOWER_THAN,
//    NEIGHBOUR_ALLY_MORE_THAN,
//    NEIGHBOUR_ALLY_LESS_THAN,
//    NEIGHBOUR_ENEMY_MORE_THAN,
//    NEIGHBOUR_ENEMY_LESS_THAN,
//}

public enum AuraConditionCompare
{
    NULL,
    EQUAL,
    BIGGER,
    SMALLER
}

public enum AuraConditionType
{
    DATA,
    NOWHP,
    MAXHP,
    LEVEL,
    ATTACK,
    NEIGHBOUR_ALLY_NUM,
    NEIGHBOUR_ENEMY_NUM
}

public interface IAuraSDS
{
    string GetEventName();
    AuraTarget GetAuraTrigger();
    //AuraCondition GetAuraCondition();
    AuraConditionCompare GetAuraConditionCompare();
    AuraConditionType[] GetAuraConditionType();
    AuraTarget[] GetAuraConditionTarget();
    int[] GetAuraConditionData();
    AuraType GetAuraType();
    AuraTarget GetAuraTarget();
    int GetAuraTargetNum();
    int[] GetAuraData();
}

public interface IAuraConditionSDS
{
    AuraConditionType GetConditionType();
    int GetConditionData();
    AuraTarget GetConditionTarget();
}

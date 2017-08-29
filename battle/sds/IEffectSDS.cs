﻿public enum Effect
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
    CHANGE_HERO,
    ADD_MONEY,
}

public interface IEffectSDS
{
    Effect GetEffect();
    int[] GetData();
}

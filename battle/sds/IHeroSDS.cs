﻿public enum AbilityType
{
    Null,
    Shoot,
    Support,
    Counter,
    Help,
    Throw,
    Attacker,
    Building
}

public interface IHeroSDS
{
    int GetID();
    int GetHp();
    int GetShield();
    int GetCost();
    bool GetCanControl();
    int GetAttack();
    AbilityType GetAbilityType();
    int[] GetSkills();
    int[] GetAuras();
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Damage
{
    public static int DamageOvertimeInterval { get; } = 1;       // Seconds
    public static double StatusEffectApplyChance { get; } = 0.3;        // Chance to proc

    public static (double, double) GetStatusEffectValues(StatusEffect statusEffect)     // Returns (value, seconds) - value per second
    {
        switch (statusEffect)
        {
            case StatusEffect.None:
                return (0,0);
            case StatusEffect.Fire:         // x damage every DamageOvertimeInterval for y seconds
                return (5, 3);
            case StatusEffect.Frost:        // x% movement speed reduction for y seconds
                return (0.2, 2);
            case StatusEffect.Blast:        // x damage on death
                return (8, 0);
        }

        return (0,0);
    }
}

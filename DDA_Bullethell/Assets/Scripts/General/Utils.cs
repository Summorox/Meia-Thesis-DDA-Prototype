using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static float ValidateObservation(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return -1f;
        }
        else
        {
            return value;
        }
    }

    public static float CalculateRewardOptimal(float baseReward, float currentValue, float lowerBound, float optimal, float upperBound)
    {
        if (currentValue < lowerBound || currentValue > upperBound)
        {
            return -baseReward;
        }
        float range = upperBound - lowerBound;
        if (range == 0) return baseReward; //Prevent division by zero
        float normalizedValue = (currentValue - lowerBound) / range;
        float optimalNormalized = (optimal - lowerBound) / range;
        float modifier = 1 - 4 * (normalizedValue - optimalNormalized) * (normalizedValue - optimalNormalized);
        float reward = baseReward * (1 + modifier);
        return Math.Max(baseReward, Math.Min(2.5f * baseReward, reward));
    }
}

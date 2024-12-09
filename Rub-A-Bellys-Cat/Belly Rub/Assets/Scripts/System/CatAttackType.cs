using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BellyRub
{
    public enum CatAttackSource
    {
        Paw = 0,
        Eye = 10,
        Mouth = 20,
        Tail = 30
    }

    public enum CatAttackType
    {
        PawClaw = 0,
        PawBulletPatternA = 1,
        EyeLaser = 10,
        EyeBulletPatternA = 11,
        MouthHairball = 20,
        MouthRolf = 21,
        MouthBulletPatternA = 22,
        TailSwing = 30,
        Lightning = 40,
    }
}

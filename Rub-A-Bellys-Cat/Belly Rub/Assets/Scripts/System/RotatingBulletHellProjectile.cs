using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BellyRub
{
    public class RotatingBulletHellProjectile : BulletHellProjectile
    {
        [field: SerializeField]
        public float[] RotationSpeeds { get; private set; } = new float[] { 0 };

        protected override void AnimateProjectile()
        {
            for (int i = 0; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].transform.Rotate(Vector3.forward * Time.deltaTime * RotationSpeeds[i]);
            }
        }
    }
}

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BellyRub
{
    public class AnimatedBulletHellProjectile : BulletHellProjectile
    {
        [field: SerializeField]
        public float LengthPerFrame = 1f;

        [field: SerializeField, TableMatrix]
        public Sprite[][] Sprites { get; private set; } = new Sprite[][] { };

        protected override void AnimateProjectile()
        {
            for (int i = 0; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].sprite = Sprites[i][Mathf.FloorToInt(Time.time / LengthPerFrame % Sprites[i].Length)];
            }
        }
    }
}

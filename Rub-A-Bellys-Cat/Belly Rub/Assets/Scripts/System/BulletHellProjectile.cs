using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BellyRub
{
    public class BulletHellProjectile : SerializedMonoBehaviour
    {
        static Color StartColor { get; set; } = new Color(1, 1, 1, 0);
        static Color EndColor { get; set; } = new Color(1, 1, 1, 1);

        [field: SerializeField]
        public SpriteRenderer[] SpriteRenderers { get; private set; }

        [field: SerializeField]
        public Collider2D Collider { get; private set; }

        public Vector2 Position => transform.position;
        public Vector3 Velocity { get; private set; }
        public BulletHellPool Pool { get; private set; }

        public void AddToPool(BulletHellPool pool)
        {
            Pool = pool;
        }

        public void Fire(Vector2 position, float speed, float angle, float spawnDelay = .1f)
        {
            gameObject.SetActive(true);
            transform.position = position;
            Velocity = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)) * speed;

            foreach (SpriteRenderer spriteRenderer in SpriteRenderers)
                spriteRenderer.transform.localEulerAngles = Vector3.forward * angle;

            if (spawnDelay > 0)
            {
                StartCoroutine(SpawnAnimationCo());
            }

            IEnumerator SpawnAnimationCo()
            {
                float startTime = Time.time;
                float t;

                Collider.enabled = false;
                Velocity *= .1f;

                while(Time.time - startTime < spawnDelay)
                {
                    t = (Time.time - startTime) / spawnDelay;
                    foreach (SpriteRenderer spriteRenderer in SpriteRenderers)
                        spriteRenderer.color = Color.Lerp(StartColor, EndColor, t);
                    transform.localScale = Vector3.one * Mathf.Lerp(2, 1, t);
                    yield return null;
                }
                transform.localScale = Vector3.one;
                Collider.enabled = true;
                Velocity *= 10f;
            }
        }

        protected void Update()
        {
            MoveProjectile();
            AnimateProjectile();
        }

        protected void MoveProjectile()
        {
            transform.position += Velocity * Time.deltaTime;
        }

        protected virtual void AnimateProjectile()
        {
            /*
            for (int i = 0; i < SpriteRenderers.Length; i++)
            {
                SpriteRenderers[i].transform.Rotate(Vector3.forward * Time.deltaTime * RotationSpeed[i]);
            }*/
        }

        public void Fire(Vector3 direction)
        {
            transform.forward = direction;
        }

        public void Recycle()
        {
            StopAllCoroutines();
            Pool.Recycle(this);
        }

        public void OnHit(SessionState session)
        {
            session.TakeDamage(CatAttackType.MouthBulletPatternA);
            Recycle();
        }
    }
}

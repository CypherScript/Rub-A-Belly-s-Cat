
using UnityEngine;

namespace BellyRub
{
    public abstract class Projectile : MonoBehaviour
    {
        [SerializeField] protected float speed = 10f;
        [SerializeField] protected float range = 10f;
        protected Vector3 startingPosition;

        protected virtual void Start()
        {
            startingPosition = transform.position;
        }

        protected virtual void Update()
        {
            MoveProjectile();

            if (Vector3.Distance(startingPosition, transform.position) > range)
            {
                DestroyProjectile();
            }
        }

        protected virtual void MoveProjectile()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        public virtual void Fire(Vector3 direction)
        {
            transform.forward = direction;
        }

        public virtual void OnHit(SessionState session)
        {
            DestroyProjectile();
        }

        protected virtual void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }
}

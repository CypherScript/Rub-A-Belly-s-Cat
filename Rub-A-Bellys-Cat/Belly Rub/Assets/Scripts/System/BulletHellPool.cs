using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BellyRub
{
    public class BulletHellPool : MonoBehaviour
    {
        public static BulletHellPool GetPool(GameObject prefab)
        {
            return AllPools[prefab];
        }

        public static Dictionary<GameObject, BulletHellPool> AllPools { get; private set; } = new Dictionary<GameObject, BulletHellPool>();

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        [field: SerializeField]
        public int InitialCount { get; private set; }

        [field: SerializeField]
        public int MinExpandCount { get; private set; }

        [field: SerializeField]
        public Bounds AutoRecycleBounds { get; private set; }

        [ShowInInspector]
        public int InactivePoolSize => InactiveProjectiles.Count;

        [ShowInInspector]
        public int ActivePoolSize => ActiveProjectiles.Count;

        protected List<BulletHellProjectile> InactiveProjectiles { get; private set; } = new List<BulletHellProjectile>();
        protected List<BulletHellProjectile> ActiveProjectiles { get; private set; } = new List<BulletHellProjectile>();
        protected bool IsInitialized { get; private set; } = false;

        void OnEnable()
        {
            SessionState.OnSessionEnd.AddListener(OnSessionEnd);

            if (Prefab.GetComponent<BulletHellProjectile>() == null)
            {
                Debug.LogWarning("BulletHellPool prefab must have a BulletHellProjectile component.");
                Destroy(gameObject);
            }

            if(AllPools.ContainsKey(Prefab))
            {
                Debug.LogWarning("BulletHellPool has the same prefab as an existing pool.");
                Destroy(gameObject);
            }

            if (!IsInitialized)
            {
                ExpandPool(InitialCount);
                IsInitialized = true;
            }

            AllPools[Prefab] = this;
        }

        void LateUpdate()
        {
            RecycleOutOfBoundsProjectiles();
        }

        void OnDisable()
        {
            SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);

            if (AllPools[Prefab] == this)
            {
                AllPools.Remove(Prefab);
            }
        }

        void OnSessionEnd(SessionState session)
        {
            SetEntirePoolInactive();
        }

        void ExpandPool(int count)
        {
            GameObject newObj;
            BulletHellProjectile newProj;
            for (int i = 0; i < count; i++)
            {
                newObj = Instantiate(Prefab, transform);
                newObj.SetActive(false);
                newProj = newObj.GetComponent<BulletHellProjectile>();
                newProj.AddToPool(this);
                InactiveProjectiles.Add(newProj);
            }
        }

        void SetEntirePoolInactive()
        {
            IEnumerable<BulletHellProjectile> toRecycle = ActiveProjectiles.ToList();
            foreach (BulletHellProjectile projectile in toRecycle)
            {
                MoveToInactivePool(projectile);
            }
        }

        void MoveToActivePool(BulletHellProjectile projectile)
        {
            InactiveProjectiles.Remove(projectile);
            ActiveProjectiles.Add(projectile);
            projectile.gameObject.SetActive(true);
        }

        void MoveToInactivePool(BulletHellProjectile projectile)
        {
            InactiveProjectiles.Add(projectile);
            ActiveProjectiles.Remove(projectile);
            projectile.gameObject.SetActive(false);
        }

        void RecycleOutOfBoundsProjectiles()
        {
            IEnumerable<BulletHellProjectile> toRecycle = ActiveProjectiles.Where(p => !AutoRecycleBounds.Contains(p.Position)).ToList();
            foreach(BulletHellProjectile projectile in toRecycle)
            {
                MoveToInactivePool(projectile);
            }
        }

        public void Recycle(BulletHellProjectile projectile)
        {
            MoveToInactivePool(projectile);
        }

        public BulletHellProjectile GetProjectile()
        {
            BulletHellProjectile projectile;
            if (InactiveProjectiles.Count < 1)
            {
                ExpandPool(Mathf.Max(MinExpandCount, 1));
            }
            projectile = InactiveProjectiles[0];
            MoveToActivePool(projectile);
            return projectile;
        }

        public IEnumerable<BulletHellProjectile> GetProjectiles(int count)
        {
            IEnumerable<BulletHellProjectile> projectiles;
            if (InactiveProjectiles.Count < count)
            {
                ExpandPool(Mathf.Max(MinExpandCount, count));
            }
            projectiles = InactiveProjectiles.Take(count).ToList();
            foreach(BulletHellProjectile projectile in projectiles)
            {
                MoveToActivePool(projectile);
            }
            return projectiles;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(AutoRecycleBounds.center, AutoRecycleBounds.extents * 2f);
        }
    }
}

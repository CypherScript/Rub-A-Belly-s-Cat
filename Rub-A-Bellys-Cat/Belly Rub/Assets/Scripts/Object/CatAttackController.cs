using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BellyRub
{
    /// <summary>
    /// Abstract component that tracks danger level and availability of a specific attack.
    /// </summary>
    public abstract class CatAttackController : MonoBehaviour
    {
        public int _dangerLevel;
        public bool _isActivated;
        public bool _isReserved;
        public SessionState _session;
        
        
        [SerializeField]
        protected List<ParticleSystem> _glintEffectPrefab; // Glint effect


        protected enum GlintType
        {
            Alert = 0,
            Small = 1,
            Big = 2
        }

        public virtual bool IsValidToActivate(CatAttackSource attackSource)
        {
            return !_isActivated;
        }

        public virtual bool IsValidToReserve(CatAttackType attackType)
        {
            return !_isReserved && _isActivated;
        }

        public abstract void Activate();
        public abstract void IncreaseDanger(CatAttackType attackType);
        public abstract void DecreaseDanger(CatAttackType attackType);
        

        
        //New Glint Function
        protected void StartGlint(GlintType type, Transform spawnPoint = null)
        {
            
            foreach (var glint in _glintEffectPrefab)
            {
                if (glint == null)
                {
                    Debug.LogError("Glint effect prefab not assigned!");
                    return;
                }
            }

            if (spawnPoint == null)
                Instantiate(_glintEffectPrefab[(int)type], transform.position, Quaternion.identity);
            else
                Instantiate(_glintEffectPrefab[(int)type], spawnPoint.position, Quaternion.identity);
        }
        
        
        
        
        protected virtual void OnEnable()
        {
            TutorialState.OnTutorialStart.AddListener(OnTutorialStart);
            TutorialState.OnTutorialEnd.AddListener(OnTutorialEnd);

            SessionState.OnSessionStart.AddListener(OnSessionStart);
            SessionState.OnSessionEnd.AddListener(OnSessionEnd);
        }

        protected virtual void OnDisable()
        {
            TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);
            TutorialState.OnTutorialEnd.RemoveListener(OnTutorialEnd);

            SessionState.OnSessionStart.RemoveListener(OnSessionStart);
            SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);
        }

        protected virtual void Update()
        {
            
        }

        protected virtual void OnTutorialStart(TutorialState tutorialState)
        {
            _session = tutorialState.SessionState;
        }

        protected virtual void OnTutorialEnd(TutorialState tutorialState)
        {
            _session = null;
        }

        protected virtual void OnSessionStart(SessionState session)
        {
            _session = session;
        }

        protected virtual void OnSessionEnd(SessionState session)
        {
            _session = null;
            _dangerLevel = 0;
            _isReserved = false;
        }

        public virtual void CounterAttack()
        {
            if(_session != null)
            {
                _session.TakeDamage();
            }
        }
        
        public void FireProjectile(GameObject projectilePrefab, Vector2 firePosition, Vector2 direction)
        {
            Projectile newProjectile = Instantiate(projectilePrefab, firePosition, Quaternion.identity).GetComponent<Projectile>();
            newProjectile.Fire(direction);
        }
    }
}
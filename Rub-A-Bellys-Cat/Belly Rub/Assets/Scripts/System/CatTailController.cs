using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BellyRub
{
    /// <summary>
    /// Controls the Cat's tail behavior and visuals.
    /// </summary>
    public class CatTailController : CatAttackController
    {
        [SerializeField]
        private Transform vfxSpawnPoint = null;
        
        #region Serialized Fields
        
        [Header("Timings")]

        [Tooltip("Duration for the counter-attack phase.")]
        [SerializeField]
        private float _lightningAttackDuration = 2.0f;
        
        [Tooltip("Time between each lightning strike")]
        [SerializeField]
        private float _lightningAttackTick = .2f;
        
        [Header("Visuals")]

        [Tooltip("Renderer responsible for controlling the tail's appearance.")]
        [SerializeField]
        private SpriteRenderer _tailSpriteRenderer = null;
        
        [SerializeField]
        private ParticleSystem _tailLightning;
        
        [Tooltip("Renderer responsible for controlling the bottom tail's appearance.")]
        [SerializeField]
        private SpriteRenderer _longTailSpriteRenderer = null;

        [Header("Projectile")]
        [SerializeField] private GameObject _lightningProjectile;
          
        [SerializeField]
        private Animator _animator; // Animator for controlling the cat's animations

        #endregion

        #region Private Fields

        private bool IsAttacking;
        private bool CanCheckRub;
        private float LastRubTime;
        private float AttackStartTime;

        #endregion

        #region Overridden Methods

        protected override void OnSessionStart(SessionState session)
        {
            // Custom logic for the child class
            if (_longTailSpriteRenderer != null)
            {
                _longTailSpriteRenderer.enabled = false;
            }
            else
            {
                Debug.LogWarning("LongTailSpriteRenderer is not assigned!");
            }

            base.OnSessionStart(session);
            _session.OnBellyRubbed.AddListener(TrySpawnLightning);

        }

        protected override void OnSessionEnd(SessionState session)
        {
            _session.OnBellyRubbed.RemoveListener(TrySpawnLightning);
            base.OnSessionEnd(session);
            _animator.Rebind();
            _animator.Update(0.0f);
            _longTailSpriteRenderer.enabled = false;
            StopAllCoroutines();
            ResetDangerLevel();
        }

        public override bool IsValidToActivate(CatAttackSource attackSource)
        {
            return base.IsValidToActivate(attackSource) && attackSource == CatAttackSource.Tail;
        }

        public override bool IsValidToReserve(CatAttackType attackType)
        {
            return base.IsValidToReserve(attackType) && attackType == CatAttackType.TailSwing;
        }

        public override void Activate()
        {
            _isActivated = true;
        }

        public override void IncreaseDanger(CatAttackType attackType)
        {
            _dangerLevel++; // Increment the danger level.
            
            UpdateTailVisuals(); // Update the visuals according to the new danger level.

            if (_dangerLevel >= 2) // If danger level reaches 2 or more.
            {
                ShowTail(); // Show the claw.
            }
        }

        public override void DecreaseDanger(CatAttackType attackType)
        {
            if (_dangerLevel == 0) return;
            _dangerLevel--;
            
            UpdateTailVisuals(); // Update the visuals according to the new danger level.

        }

        protected override void Update()
        {
            if (!IsAttacking) return;
            
            if (Time.fixedTime > AttackStartTime + _lightningAttackDuration)
            {
                ResetDangerLevel();
            }
            
            if (Time.fixedTime > LastRubTime + _lightningAttackTick)
            {
                CanCheckRub = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the visuals of the tail based on danger level.
        /// </summary>
        private void UpdateTailVisuals()
        {
            // Enable or disable the long tail sprite renderer based on danger level
            _longTailSpriteRenderer.enabled = (_dangerLevel == 2);
            
            // Set the animator's DangerLevel parameter
            _animator.SetInteger("DangerLevel", _dangerLevel);
        }

        /// <summary>
        /// Reset the danger level and update visuals.
        /// </summary>
        private void ResetDangerLevel()
        {
            _session?.OnTailStatic?.Invoke(false);
            IsAttacking = false;
            _dangerLevel = 0;
            _tailLightning.Stop();
            UpdateTailVisuals();
        }

        #endregion

        #region Attack Functions

        private void TrySpawnLightning()
        {
            if (!CanCheckRub || !IsAttacking) return;
            SpawnLightning();
            LastRubTime = Time.fixedTime;
            CanCheckRub = false;
        }
        
        [Button]
        public void ShowTail()
        {
            _session?.OnTailStatic?.Invoke(true);
            AttackStartTime = Time.fixedTime;
            CanCheckRub = true;
            _tailLightning.Play();
            IsAttacking = true;
        }

        private void SpawnLightning()
        {
            var origin = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Instantiate(_lightningProjectile, origin, Quaternion.identity);

            StartCoroutine(LightningSFXCo());

            IEnumerator LightningSFXCo()
            {
                yield return new WaitForSeconds(.5f);
                _session?.OnLightningStrike?.Invoke();
            }
        }
        #endregion
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BellyRub
{
public class CatPawController : CatAttackController
    {
        [SerializeField]
        private SpriteRenderer _clawSpriteRenderer = null; // Renderer for controlling the claw's visual appearance

        [SerializeField]
        private float _gracePeriodDuration = 0.3f; // Delay in seconds between reaching max danger level and starting counter attack

        [SerializeField]
        private float _counterAttackDuration = 2.0f; // Delay in seconds between starting couunter attack and resetting to normal
        
        [SerializeField]
        private Animator _animator; // Animator for controlling the cat's animations

        [SerializeField]
        private GameObject _bulletHellPrefab;

        [SerializeField]
        private Vector3 _bulletHellOffset;

        [SerializeField]
        private Transform GlintSpawnPoint = null;

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnSessionStart(SessionState session)
        {
            // Your custom logic for the child class

            base.OnSessionStart(session); // Call the parent class's OnSessionStart
        }

        protected override void OnSessionEnd(SessionState session)
        {
            base.OnSessionEnd(session);
            _animator.Rebind();
            _animator.Update(0.0f);
            StopAllCoroutines();
        }
        
        protected override void OnTutorialStart(TutorialState tutorialState)
        {
            // Your custom logic for the child class
    
            base.OnTutorialStart(tutorialState); 
        }

        protected override void OnTutorialEnd(TutorialState tutorialState)
        {
            base.OnTutorialEnd(tutorialState);
        }

        public override bool IsValidToActivate(CatAttackSource attackSource)
        {
            return base.IsValidToActivate(attackSource) && attackSource == CatAttackSource.Paw;
        }

        public override bool IsValidToReserve(CatAttackType attackType)
        {
            return base.IsValidToReserve(attackType) && (int)attackType >= 0 && (int)attackType < 10;
        }

        // Activates the cat paw controller for attacking
        public override void Activate()
        {
            _isActivated = true;
        }

        // Increases the danger level of the cat paw attack
        public override void IncreaseDanger(CatAttackType attackType)
        {
            _dangerLevel++; // Increment the danger level

            UpdateClawVisuals(); // Update the visuals according to the new danger level

            if (_dangerLevel == 1)
                StartGlint(GlintType.Alert, GlintSpawnPoint);
            

            if (_dangerLevel >= 2) // If danger level reaches 2 or more
            {
                _session.OnClawExtended?.Invoke();
                switch (attackType)
                {
                    case CatAttackType.PawClaw:
                        ShowClaw(); // Show the claw
                        break;
                    case CatAttackType.PawBulletPatternA:
                        FireBullets();
                        break;
                }
                
            }
        }

        // Decreases the danger level of the cat paw attack
        public override void DecreaseDanger(CatAttackType attackType)
        {
            if (_dangerLevel == 0) return; // Return if danger level is already 0
            
            _dangerLevel--; // Decrement the danger level
            
            UpdateClawVisuals(); // Update the visuals according to the new danger level
        }

        // Shows the cat's claw and initiates a reset after a delay
        private void ShowClaw()
        {
            StopAllCoroutines();
            StartCoroutine(ClawCo());

            IEnumerator ClawCo()
            {
                StartGlint(GlintType.Small, GlintSpawnPoint);
                yield return new WaitForSeconds(_gracePeriodDuration);
                _session?.RegisterCounterAttack(this);
                StartGlint(GlintType.Big, GlintSpawnPoint);
                yield return new WaitForSeconds(_counterAttackDuration);
                _session?.DeregisterCounterAttack(this);
                ResetDangerLevel();
            }
        }

        // Update the claw visuals based on the current danger level
        private void UpdateClawVisuals()
        {
            _animator.SetInteger("DangerLevel", _dangerLevel);
            
        }

        // Resets the danger level to 0 and updates visuals
        private void ResetDangerLevel()
        {
            _session.OnClawRetracted?.Invoke();
            _dangerLevel = 0;
            UpdateClawVisuals();
        }

        protected virtual void FireBullets()
        {
            BulletHellPool pool = BulletHellPool.GetPool(_bulletHellPrefab);
            IEnumerable<BulletHellProjectile> projectiles;
            int bulletDensity = 25;

            UpdateClawVisuals();

            StopAllCoroutines();
            StartCoroutine(FireBulletsCo());

            IEnumerator FireBulletsCo()
            {
                for(int i = 0; i < 6; i++)
                {
                    _animator.SetInteger("DangerLevel", (i % 2)*2);
                    yield return new WaitForSeconds(.15f);
                }

                float bulletAngle = Random.Range(0f, 360f);
                projectiles = pool.GetProjectiles(bulletDensity);

                foreach (BulletHellProjectile projectile in projectiles)
                {
                    projectile.Fire(transform.position + _bulletHellOffset, 6f, bulletAngle);
                    bulletAngle += 360f / bulletDensity;
                }
                yield return new WaitForSeconds(.3f);
                ResetDangerLevel();
            }
        }
    }
}



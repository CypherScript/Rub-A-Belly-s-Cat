

using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.Events;
using UnityEngine;
using System.Collections;

namespace BellyRub
{
    /// <summary>
    /// Controls the visual and logical aspects of the cat's eyes during various attack states.
    /// </summary>
    public class CatEyeController : CatAttackController
    {
        // Grouping and organizing fields by their functionality ensures clarity.

        #region Serialized Fields

        // ---- Timing Parameters ----
        [Header("Timings")]
        // Duration for the grace period before any action.
        [Tooltip("Duration for the grace period before any action.")] [SerializeField]
        protected float _gracePeriodDuration = 0.3f;

        // Duration for the counter-attack phase.
        [FormerlySerializedAs("_counterAttackDuration")] [Tooltip("Duration for the counter-attack phase.")] [SerializeField]
        protected float _laserDuration = 2.0f;

        // Duration for the second danger level to persist.
        [Tooltip("Duration for the second danger level to persist.")] [SerializeField]
        protected float _dangerLevel2Duration = 5.0f;

        // ---- Visual Parameters ----
        [Header("Visuals")]
        // Renderer responsible for controlling the eye's appearance.
        [Tooltip("Renderer responsible for controlling the eye's appearance.")]
        [SerializeField]
        protected SpriteRenderer _eyeLidsSpriteRendererSprite = null;
        [SerializeField]
        private SpriteRenderer _closedEyeLidsSpriteRendererL = null;
        [SerializeField]
        private SpriteRenderer _closedEyeLidsSpriteRendererR = null;

        // List of cat pupils to manage.
        [Tooltip("List of cat pupils to manage.")] [SerializeField]
        protected  List<CatPupil> catEyes = new List<CatPupil>();

        // Speed at which the pupil moves.
        [Tooltip("Speed at which the pupil moves.")] [SerializeField]
        protected float pupilMoveSpeed = 5f;

        // Factor to shrink the pupil in danger level 2.
        [Tooltip("Factor to shrink the pupil in danger level 2.")]
        [Range(0f, 1f)] // Limiting to prevent inversion or disappearance of the pupil.
        [SerializeField]
        protected float pupilShrinkFactor = 0.5f;

        // Radius of the eyeball.
        [Tooltip("Radius of the eyeball.")] [SerializeField]
        protected float eyeballRadius = 0.6f;

        // Radius of the pupil. Restricted to ensure it doesn't exceed the eyeball.
        [Tooltip("Radius of the pupil.")] [Range(0f, 0.6f)] [SerializeField]
        protected float pupilRadius = 0.2f;
        
        [Header("Projectile")]
        [SerializeField] protected Projectile projectilePrefab;
        [SerializeField] protected Transform firePoint; // Point from which projectiles are fired

        [SerializeField] private GameObject _bulletHellPrefab;

        [Header("Laser Boundary Range")]
        public float range = 10f;  // Defaulting to a value of 10 units.
        #endregion

        // --- Non-Serialized Fields ---
        private List<ParticleSystem> activeGlintEffects = new List<ParticleSystem>(); // A list to store all currently playing glint effects

        // Initial scale of the pupil for reference.
        protected Vector3 initialPupilScale = Vector3.one;

        // Central position of the eyeball.
        private Vector2 eyeballCenter = new Vector2(0, 0);
        
        //Keep Track of Active Lasers helps for clean Up
        private List<Laser> activeLasers = new List<Laser>();

        // Flag to control transitions between danger levels.
        //private bool _canIncreaseToLevel3 = true; 

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Store the initial pupil positions as the eyeball centers
            foreach (var pupil in catEyes)
                pupil.eyeballCenter = pupil.CatPupilPosition.position;
            
            StartCoroutine(BlinkingRoutine());
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            StopCoroutine(BlinkingRoutine());
        }
        
        
        private IEnumerator BlinkingRoutine()
        {
            while (true)
            {
                // Randomly decide how long to wait until the next blink.
                float timeToWait = Random.Range(3f, 7f);
                yield return new WaitForSeconds(timeToWait);

                // If the cat is not attacking, blink!
                if (_dangerLevel == 0) 
                {
                    // Close the eyes.
                    _closedEyeLidsSpriteRendererL.enabled = true;
                    _closedEyeLidsSpriteRendererR.enabled = true;
                    yield return new WaitForSeconds(0.1f); // Duration of the blink
                    // Open the eyes.
                    _closedEyeLidsSpriteRendererL.enabled = false;
                    _closedEyeLidsSpriteRendererR.enabled = false;
                }
            }
        }
        
        
        protected virtual void UpdateEyeMovements()
        {
            Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosInWorld.z = 0;  // Ensure the Z position is 0 for 2D calculations.

            foreach (var pupil in catEyes)
            {
                Vector2 eyeballCenter = pupil.eyeballCenter;
                Vector2 directionToMouse = (mousePosInWorld - (Vector3)eyeballCenter).normalized;
                float maxAllowedDistance = eyeballRadius - pupilRadius;

                if (_dangerLevel == 1) // Level 2 in game terms
                {
                    // Pupil movements towards the mouse.
                    Vector2 newPupilPosition = eyeballCenter + directionToMouse * maxAllowedDistance;
                    pupil.CatPupilPosition.position = Vector2.Lerp(pupil.CatPupilPosition.position, newPupilPosition,
                        Time.deltaTime * 5);
                }
                else
                {
                    // Move pupils back to the center of the eyeball when not in Level 2.
                    pupil.CatPupilPosition.position = Vector2.Lerp(pupil.CatPupilPosition.position, eyeballCenter,
                        Time.deltaTime * pupilMoveSpeed);
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            UpdateEyeMovements();
            UpdateEyeAppearance();
        }
        
        protected virtual void UpdateEyeAppearance()
        {
            foreach (var pupil in catEyes)
            {
                if (_dangerLevel == 1 || _dangerLevel == 2) // Level 2 in game terms
                {
                    // Shrink pupil size
                    pupil.CatPupilSpriteRenderer.transform.localScale = Vector3.Lerp(pupil.CatPupilSpriteRenderer.transform.localScale,
                        initialPupilScale * pupilShrinkFactor, Time.deltaTime);
                }
                else
                {
                    // Grow pupil back to its original size if not in Level 2
                    pupil.CatPupilSpriteRenderer.transform.localScale = Vector3.Lerp(pupil.CatPupilSpriteRenderer.transform.localScale,
                        initialPupilScale, Time.deltaTime);
                }
            }
        }
        
        protected override void OnSessionStart(SessionState session)
        {
            base.OnSessionStart(session);
        }
        
        protected override void OnSessionEnd(SessionState session)
        {
            base.OnSessionEnd(session);
            _closedEyeLidsSpriteRendererL.enabled = false;
            _closedEyeLidsSpriteRendererR.enabled = false;
            foreach (var pupils in catEyes)
            {
                pupils.CatPupilSpriteRenderer.sprite = pupils.CatPupilLevel1Sprite;
                pupils.CatPupilSpriteRenderer.transform.localScale = initialPupilScale;
                pupils.CatPupilPosition.position = pupils.eyeballCenter;
                if (!_eyeLidsSpriteRendererSprite.enabled)
                    _eyeLidsSpriteRendererSprite.enabled = true;
            }

            // Cleanup active glint effects
            foreach (var glintEffect in activeGlintEffects)
            {
                if (glintEffect != null)
                {
                    glintEffect.Stop();
                    Destroy(glintEffect.gameObject);
                }
            }
            activeGlintEffects.Clear();

            // Create a temporary list to hold the lasers for deactivation
            List<Laser> lasersToDeactivate = new List<Laser>(activeLasers);
    
            foreach (var laser in lasersToDeactivate)
            {
                if (laser != null)
                {
                    laser.ResetStaticFlags();
                    laser.DestroyLaser();
                }
            }

            activeLasers.Clear();
            StopAllCoroutines();
        }

        public override bool IsValidToActivate(CatAttackSource attackSource)
        {
            return base.IsValidToActivate(attackSource) && attackSource == CatAttackSource.Eye;
        }

        public override bool IsValidToReserve(CatAttackType attackType)
        {
            return base.IsValidToReserve(attackType) && (int)attackType >= 10 && (int)attackType < 20;
        }

        public override void Activate()
        {
            Debug.Log("Eye Activate");
            _isActivated = true;
        }

        public override void IncreaseDanger(CatAttackType attackType)
        {
            _dangerLevel++;

            UpdateEyeVisuals();

            if (_dangerLevel >= 2)
            {
                switch (attackType)
                {
                    case CatAttackType.EyeLaser:
                        FireLaser();
                        break;
                    default:
                        FireBullets();
                        break;
                }
            }
        }

        public override void DecreaseDanger(CatAttackType attackType)
        {
            if (_dangerLevel == 0) return;
           
                _dangerLevel--;
                
                UpdateEyeVisuals();
        }
        
        
        protected void PlayEyeGlintVFX(ParticleSystem glintPrefab, Transform spawnTransform, string sortingLayerName = "Default", int sortingOrder = 11)
        {
            if (glintPrefab == null)
            {
                Debug.LogError("Glint effect prefab not assigned!");
                return;
            }

            // Instantiate the glint effect at the specified transform's position and rotation
            ParticleSystem glintInstance = Instantiate(glintPrefab, spawnTransform.position, spawnTransform.rotation);
    
            // Optionally, parent the instantiated glint to this object
            glintInstance.transform.SetParent(spawnTransform);

            // Adjust the sorting layer and order in layer for the main particle system
            ParticleSystemRenderer psRenderer = glintInstance.GetComponent<ParticleSystemRenderer>();
            psRenderer.sortingLayerName = sortingLayerName;
            psRenderer.sortingOrder = sortingOrder;

            // Play the particle system
            glintInstance.Play();

            // Store the glintInstance for later destruction
            activeGlintEffects.Add(glintInstance);
        }
        
        protected void PlayGlintForPupilType(CatPupil pupil)
        {
            // Here you can distinguish between the different eyes and handle them accordingly
            switch (pupil.TypeOfEye)
            {
                case CatPupil.EyeType.LeftEye:
                    // Handle left eye specific logic
                    break;
                case CatPupil.EyeType.RightEye:
                    // Handle right eye specific logic
                    break;
                case CatPupil.EyeType.ThirdEye:
                    // Handle third eye specific logic, if you have one
                    break;
            }

            // Fetch the VFX prefab from the CatPupil instance
            ParticleSystem vfxPrefab = pupil.EyeVFXPrefab;

            // Call the base PlayGlint method or your overridden one using the VFX prefab from CatPupil
            PlayEyeGlintVFX(vfxPrefab, pupil.VFXSpawnPoint);
        }
        
        /// <summary>
        /// Updates the visual state of the cat's eyes based on the current danger level.
        /// </summary>
        protected virtual void UpdateEyeVisuals()
        {
            if (_dangerLevel == 1 || _dangerLevel == 2)
                _eyeLidsSpriteRendererSprite.enabled = false;
            
            else
                _eyeLidsSpriteRendererSprite.enabled = true;


            if (_dangerLevel >= 1)
            {
                foreach (var cateyes in catEyes)
                {
                    cateyes.CatPupilSpriteRenderer.sprite = cateyes.CatPupilLevel2Sprite;
                }
            }
            else
            {
                foreach (var cateyes in catEyes)
                {
                    cateyes.CatPupilSpriteRenderer.sprite = cateyes.CatPupilLevel1Sprite;
                }
            }
        }

        /// <summary>
        /// Resets the danger level to its default state.
        /// </summary>
        protected void ResetDangerLevel()
        {
            _dangerLevel = 0;
            UpdateEyeVisuals();
        }

        protected virtual void FireLaser()
        {
            int selectedCurveIndex = Random.Range(0, projectilePrefab.GetComponent<Laser>().bezierCurves.Count);
            
            bool isReverseDirection = (Random.Range(0, 2) == 0); // 50% chance to reverse
            
            bool regularEyeLaserFired = false; // Track if a laser from a regular eye has already been fired

            foreach (var pupil in catEyes)
            {
                Vector2 startPosition = pupil.eyeballCenter;
                Laser laserProjectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity).GetComponent<Laser>();
                
                // If this is a regular eye and we've already fired one, set the flag to not instantiate the particle
                if(laserProjectile.origin == Laser.EyeOrigin.Regular && regularEyeLaserFired)
                {
                    laserProjectile.SkipParticleInstantiation = true;
                }

                if(laserProjectile.origin == Laser.EyeOrigin.Regular)
                {
                    regularEyeLaserFired = true;
                }
                
                // Add the instantiated laser to the list
                activeLasers.Add(laserProjectile);

                // Start the sound if this is the first active laser
                if (activeLasers.Count == 1)
                {
                    if (_session != null )
                        _session.OnLaserFired.Invoke(true);
                }

                UnityAction laserDeactivationAction = () => HandleLaserDeactivated(laserProjectile);
                laserProjectile.OnLaserDeactivated.AddListener(laserDeactivationAction);
                laserProjectile.DeactivationAction = laserDeactivationAction;
                laserProjectile.InitializeLaser(startPosition, selectedCurveIndex, isReverseDirection);
                
                // Play the eye glint VFX for each pupil when firing the laser
                PlayGlintForPupilType(pupil);
            }
        }
        
        protected virtual void HandleLaserDeactivated(Laser laser)
        {
            // Remove the laser from the list
            if (activeLasers.Contains(laser))
            {
                activeLasers.Remove(laser);
            }
            
            // Destroy all active glint effects
            foreach (var glintEffect in activeGlintEffects)
            {
                glintEffect.Stop();
                Destroy(glintEffect.gameObject);
            }
            
            activeGlintEffects.Clear();

            // If no lasers are active, stop the sound
            if (activeLasers.Count <= 0)
            {
                _session.OnLaserFired.Invoke(false);
            }

            // Remove the listener using the stored action
            if (laser.DeactivationAction != null)
            {
                laser.OnLaserDeactivated.RemoveListener(laser.DeactivationAction);
            }
            
            ResetDangerLevel();
        }

        protected virtual void FireBullets()
        {
            BulletHellPool pool = BulletHellPool.GetPool(_bulletHellPrefab);
            IEnumerable<BulletHellProjectile> projectiles;
            int bulletDensity = catEyes.Count == 1 ? 16 : 8;
            int bulletWaveCount = 12;

            UpdateEyeVisuals();

            StopAllCoroutines();
            StartCoroutine(FireBulletsCo());

            IEnumerator FireBulletsCo()
            {
                yield return new WaitForSeconds(.3f);
                for(int i = 0; i < bulletWaveCount; i++)
                {
                    foreach(CatPupil eye in catEyes)
                    {
                        float bulletAngle = Random.Range(0f, 360f);
                        projectiles = pool.GetProjectiles(bulletDensity);

                        foreach(BulletHellProjectile projectile in projectiles)
                        {
                            projectile.Fire(eye.eyeballCenter, 10f + (i%2 *2f), bulletAngle);
                            bulletAngle += 360f / bulletDensity;
                        }
                    }
                    yield return new WaitForSeconds(.3f);
                }
                ResetDangerLevel();
            }
        }
    }
}

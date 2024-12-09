
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BellyRub
{
    // Class to represent the behavior of the Laser which inherits from the Projectile class.
    public class Laser : Projectile
    {
        // ------------------------------------------
        // Laser Properties
        // ------------------------------------------
        [Header("Laser Visualization and Behavior")] [SerializeField]
        private LineRenderer telegraphLine; // For showing where the laser will hit.

        [SerializeField] private Material telegraphMaterial; // Material for telegraph line.
        [SerializeField] private LineRenderer activeLaserLine; // The actual active laser.
        [SerializeField] private float telegraphDuration = 1f; // Duration to show the telegraph.
        [SerializeField] private BoxCollider2D laserCollider; // Collider for laser hit detection.
        
        [Header("Laser Particles")] [SerializeField]
        private ParticleSystem laserParticles; // Particles for laser hit effect.
        private ParticleSystem activeLaserParticles;
        [SerializeField]
        private ParticleSystem _telegrachLineParticles;
        private ParticleSystem instantiatedParticles;
        public bool SkipParticleInstantiation { get; set; } = false;
        public enum EyeOrigin
        {
            Regular,
            ThirdEye
        }

        public EyeOrigin origin;

        private static bool regularEyeTargetParticleActive = false;
        private static bool thirdEyeTargetParticleActive = false;
        
        [Header("Laser Interpolation Values")] [SerializeField]
        private float startInnerValue = 0.1f;

        [SerializeField] private float startOuterValue = 0.8f;
        [SerializeField] private float targetInnerValue = 0f;
        [SerializeField] private float targetOuterValue = 0.25f;

        // ------------------------------------------
        // Laser Sweep Properties
        // ------------------------------------------
        [Header("Laser Sweep")] [SerializeField]
        private float laserSweepSpeed = 0.20f; // Speed of laser sweep across the screen.
        private Vector3 originalPosition; // Laser's origin.
        private Vector3 targetPosition; // Laser's destination.
        private bool isMovingForward = true; // For tracking laser's direction.
        private float tValue = 0; // For Bezier curve calculations.

        // ------------------------------------------
        // Bezier Curve Properties
        // ------------------------------------------
        [Header("Bezier Curves")] [SerializeField]
        public List<BezierCurve> bezierCurves; // Collection of Bezier curves.

        [SerializeField] private BezierCurve currentCurve; // Currently selected Bezier curve.
        private bool reverseDirection; // To reverse curve direction.

        // ------------------------------------------
        // Sound Effects & Events
        // ------------------------------------------
        [Header("SFX and Events")]
        public UnityEvent OnLaserDeactivated = new UnityEvent(); // Event for laser deactivation.

        public UnityAction DeactivationAction { get; set; }
        
       
        
        private void Awake()
        {
            // Instantiate the telegraph line and disable the laser's collider initially.
            telegraphLine = Instantiate(telegraphLine, transform);
            telegraphLine.enabled = false;

            // Create a unique instance of the material for this telegraphLine
            telegraphLine.material = new Material(telegraphMaterial);

            // Now modify the material properties without affecting other objects
            telegraphLine.material.SetFloat("_Inner_Laser_Thinness", startInnerValue);
            telegraphLine.material.SetFloat("_Outer_Laser_Thinness", startOuterValue);

            telegraphLine.startWidth = 0.2f;
            telegraphLine.endWidth = 0.2f;

            laserCollider.enabled = false;
            
            StartCoroutine(TelegraphCo());
            
            IEnumerator TelegraphCo()
            {
                yield return new WaitForSeconds(0.3f);
                telegraphLine.enabled = true;
            }
        }

        // Initialize laser settings based on given parameters.
        public void InitializeLaser(Vector3 eyePosition, int selectedCurveIndex, bool isreverseDirection)
        {
            originalPosition = eyePosition;
            currentCurve = bezierCurves[selectedCurveIndex];
            reverseDirection = isreverseDirection;
            targetPosition = reverseDirection ? currentCurve.EndPoint : currentCurve.StartPoint;

            // Setting up range and direction for laser sweep.
            range = Vector3.Distance(originalPosition, targetPosition);
           // sweepDirection = (currentCurve.EndPoint.x > currentCurve.StartPoint.x) ? Vector3.right : Vector3.left;

            // Set the telegraph line's positions and enable it.
            telegraphLine.SetPositions(new Vector3[] { originalPosition, targetPosition });
            
            // Start the telegraphing sequence.
            StartCoroutine(TelegraphAndFireLaser());
        }

        // On hitting target, log the hit and apply damage to the session.
        public override void OnHit(SessionState session)
        {
            session.TakeDamage(CatAttackType.EyeLaser);
        }

        // Coroutine to show the telegraph and then activate the laser.
        private IEnumerator TelegraphAndFireLaser()
        {
            float elapsedTime = 0f; // Time elapsed since the interpolation started

            Material lineMaterial = telegraphLine.material; // Get the unique instance of the material
            
            instantiatedParticles = Instantiate(_telegrachLineParticles, Vector3.zero, Quaternion.identity);
            
            instantiatedParticles.transform.position = originalPosition;
            
            // Adjust the sorting layer and order in layer for the main particle system
            ParticleSystemRenderer psRenderer = instantiatedParticles.GetComponent<ParticleSystemRenderer>();
            psRenderer.sortingLayerName = "Default";
            psRenderer.sortingOrder = 11;
            
            instantiatedParticles.Play();
            
            while (elapsedTime < telegraphDuration)
            {
                float t = elapsedTime / telegraphDuration;

                
                // Compute the interpolated values using Mathf.Lerp
                float newInnerValue = Mathf.Lerp(startInnerValue, targetInnerValue, t);
                float newOuterValue = Mathf.Lerp(startOuterValue, targetOuterValue, t);

                // Set the values on the material instance
                lineMaterial.SetFloat("_Inner_Laser_Thinness", newInnerValue);
                lineMaterial.SetFloat("_Outer_Laser_Thinness", newOuterValue);

                elapsedTime += Time.deltaTime; // Update elapsed time
                yield return null; // Wait for the next frame
            }
            
           
            telegraphLine.enabled = false; // Turn off the telegraph.
            ActivateLaser(); // Activate the real laser.
        }

        // Method to set up and activate the actual laser.
        private void ActivateLaser()
        {
            Destroy(instantiatedParticles.gameObject);  
            activeLaserLine.SetPosition(0, originalPosition);
            activeLaserLine.SetPosition(1, targetPosition);
            activeLaserLine.enabled = true;
            laserCollider.enabled = true;

            if (laserParticles == null || SkipParticleInstantiation) return;

            if (origin == EyeOrigin.ThirdEye && !thirdEyeTargetParticleActive)
            {
                InstantiateTargetParticle();
                thirdEyeTargetParticleActive = true;
            }
            else if (origin == EyeOrigin.Regular && !regularEyeTargetParticleActive)
            {
                InstantiateTargetParticle();
                regularEyeTargetParticleActive = true;
            }
        }

        private void InstantiateTargetParticle() 
        {
            activeLaserParticles = Instantiate(laserParticles, targetPosition, Quaternion.identity);
            activeLaserParticles.transform.SetParent(transform);
        }
        
        public static void ResetParticleFlags() 
        {
            regularEyeTargetParticleActive = false;
            thirdEyeTargetParticleActive = false;
        }

        // Update the laser's position, based on Bezier curves, during each frame.
        protected override void Update()
        {
            if (laserCollider.enabled)
            {
                tValue += Time.deltaTime * laserSweepSpeed;
                if (tValue >= 1.0f)
                {
                    tValue = 1.0f;
                    DeactivateLaser();
                }
                else
                {
                    // Calculate Bezier curve position for laser's target.
                    Vector3 bezierPosition = reverseDirection
                        ? CalculateBezierPoint(tValue, currentCurve.EndPoint, currentCurve.ControlPoint,
                            currentCurve.StartPoint)
                        : CalculateBezierPoint(tValue, currentCurve.StartPoint, currentCurve.ControlPoint,
                            currentCurve.EndPoint);

                    targetPosition = bezierPosition;
                    activeLaserLine.SetPosition(0, originalPosition);
                    activeLaserLine.SetPosition(1, targetPosition);

                    // Adjust laser's collider based on the new target position.
                    AdjustCollider(originalPosition, targetPosition);

                    if (activeLaserParticles != null)
                    {
                        activeLaserParticles.transform.position = targetPosition;
                    }
                }
            }
        }

        // Calculates a point on a Bezier curve based on the given control points and 't' value.
        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float u = 1 - t;
            return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
        }

        // Deactivate the laser and reset certain properties.
        public void DeactivateLaser()
        {
            OnLaserDeactivated.Invoke();
            activeLaserLine.enabled = false;
            laserCollider.enabled = false;
            tValue = 0;
            DestroyActiveParticles();

            // Reset the flag based on the type of the laser
            if (origin == EyeOrigin.ThirdEye)
                thirdEyeTargetParticleActive = false;
            else if (origin == EyeOrigin.Regular)
                regularEyeTargetParticleActive = false;
            
            StartCoroutine(DestroyNextFrame());
        }

        // Coroutine to destroy the laser in the next frame.
        private IEnumerator DestroyNextFrame()
        {
            yield return null;
            DestroyProjectile();
        }

        public void DestroyLaser()
        {
            StartCoroutine(DestroyNextFrame());
        }
        
        
        public void ResetStaticFlags()
        {
            regularEyeTargetParticleActive = false;
            thirdEyeTargetParticleActive = false;
        }

        private void DestroyActiveParticles()
        {
            if (activeLaserParticles != null)
            {
                Destroy(activeLaserParticles.gameObject);
                activeLaserParticles = null;
            }
        }

        // Gizmo-related methods for visual debugging in the Unity editor.

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                DrawBezierCurves();
            }
        }

        private void DrawBezierCurves()
        {
            if (bezierCurves != null && bezierCurves.Count > 0)
            {
                foreach (BezierCurve curve in bezierCurves)
                {
                    DrawBezierCurve(curve);
                }
            }
        }

        private void DrawBezierCurve(BezierCurve curve)
        {
            int segmentCount = 20; // Number of line segments for visualization.
            Vector3 prevPoint = curve.StartPoint;
            Gizmos.color = Color.red;
            for (int i = 1; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector3 nextPoint = CalculateBezierPoint(t, curve.StartPoint, curve.ControlPoint, curve.EndPoint);
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            // Visualization for control, start, and end points.
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(curve.ControlPoint, 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(curve.StartPoint, new Vector3(0.3f, 0.3f, 0.3f));
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(curve.EndPoint, new Vector3(0.3f, 0.3f, 0.3f));
        }

        // Adjusts the collider position, size, and rotation based on given start and end points.
        private void AdjustCollider(Vector3 startPoint, Vector3 endPoint)
        {
            Vector3 direction = endPoint - startPoint;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Vector3 midPoint = (startPoint + endPoint) * 0.5f;
            float distance = Vector3.Distance(startPoint, endPoint);

            laserCollider.transform.position = midPoint;
            laserCollider.size = new Vector2(distance, laserCollider.size.y);
            laserCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
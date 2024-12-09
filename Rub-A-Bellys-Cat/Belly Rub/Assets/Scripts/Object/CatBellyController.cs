
using UnityEngine;
using Zoodle;

namespace BellyRub
{
    /// <summary>
    /// Component that tracks cursor and receives belly rub input
    /// </summary>
    public class CatBellyController : MonoBehaviour
    {
        [field: SerializeField]
        protected LayerMask TargetLayer { get; private set; }

        [field: SerializeField]
        protected bool ShowDebugWindow { get; private set; }

        [DisplayInDebugWindow]
        public bool IsRubbingBelly { get; private set; } = false;

        [DisplayInDebugWindow]
        public float RubIntensity { get; private set; } = 0f;

        [DisplayInDebugWindow]
        private bool IsTouchAllowed => _session == null || _session.IsTouchAllowed;

        [SerializeField] private ParticleSystem rubbingHearts;
        
        private SessionState _session;
        private CatGlobalSetting GlobalSetting => _session.CatGlobalSetting;
        private float TickLength => GlobalSetting.TickLength;
        private float MinDistancePerTick => GlobalSetting.MinDistancePerTick;
        private float MaxDistancePerTick => GlobalSetting.MaxDistancePerTick;

        private Vector2 currentPosition = Vector2.zero;
        private Vector2 previousPosition = Vector2.zero;
        private float distanceSinceLastTick = 0f;
        private float tickCooldown = 0f;

        private void OnEnable()
        {
            TutorialState.OnTutorialStart.AddListener(OnTutorialStart);
            TutorialState.OnTutorialEnd.AddListener(OnTutorialEnd);

            SessionState.OnSessionStart.AddListener(OnSessionStart);
            SessionState.OnSessionEnd.AddListener(OnSessionEnd);
        }

        private void OnDisable()
        {
            TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);
            TutorialState.OnTutorialEnd.RemoveListener(OnTutorialEnd);

            SessionState.OnSessionStart.RemoveListener(OnSessionStart);
            SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);
        }

        private void OnSessionStart(SessionState session)
        {
            _session = session;
            _session.OnHeartSpawned.AddListener(OnHeartSpawned);
        }

        private void OnSessionEnd(SessionState session)
        {
            _session.OnHeartSpawned.RemoveListener(OnHeartSpawned);
            _session = null;
            rubbingHearts.Clear(true);
        }

        private void OnTutorialStart(TutorialState tutorialState)
        {
           _session = tutorialState.SessionState;
        }

        private void OnTutorialEnd(TutorialState tutorialState)
        {
            _session = null;
        }

        private void Update()
        {
            if (_session == null) return;

            if (IsRubbingBelly)
            {
               _session.UpdateMultiplierOnRubbing();
            }
            else
            {
               _session.ResetMultiplierOnInactivity();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.TogglePause();
            }
            
            if(!GameManager.Instance.StateHandler.TryGetState(out PauseState pauseState))
                CheckBellyRub();
        }

        private void CheckBellyRub()
        {
            previousPosition = currentPosition;
            currentPosition = Input.mousePosition;

            // If touching the belly...
            if (Input.GetMouseButton(0) && TryRaycastCat(currentPosition) && IsTouchAllowed)
            {
                
                // Clear distance on initial click
                if (Input.GetMouseButtonDown(0))
                {
                    previousPosition = currentPosition;
                }

                // Check if mouse has moved far enough to trigger a tick, and enough time has passed since last tick
                distanceSinceLastTick += Vector2.Distance(currentPosition, previousPosition);
                if (tickCooldown <= 0)
                {
                    float shorterScreenAxisLength = Mathf.Min(Screen.width, Screen.height);
                    if (distanceSinceLastTick / shorterScreenAxisLength >= MinDistancePerTick && !IsRubbingBelly)
                    {
                        // On rub tick, calculate rub intensity and enter rub state
                        RubIntensity = Mathf.InverseLerp(MinDistancePerTick, MaxDistancePerTick, distanceSinceLastTick / shorterScreenAxisLength);

                        _session.OnRubBellyTick(RubIntensity);
                        tickCooldown = TickLength;
                        IsRubbingBelly = true;
                    }
                    else
                    {
                        RubIntensity = 0;
                        IsRubbingBelly = false;
                    }
                    distanceSinceLastTick = 0;
                }
                
                    _session.OnTouchingBelly();
                
            }
            else
            {
                // If not touching belly, reset distance and intensity, and exit rub state if in it
                distanceSinceLastTick = 0;
                RubIntensity = 0;
                if (IsRubbingBelly)
                {
                    IsRubbingBelly = false;
                }
                
            }

            if (tickCooldown > 0)
            {
                tickCooldown -= Time.deltaTime;
            }
        }

        private void OnHeartSpawned(int count)
        {
            rubbingHearts.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rubbingHearts.Emit(count);
        }

        // Checks if a collider on the target layer is touched, and outputs position
        private bool TryRaycastCat(Vector2 position)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, TargetLayer);
            return hit.collider != null;
        }

        private void OnGUI()
        {
            if (ShowDebugWindow)
                DebugUtility.DisplayDebugWindow(this);
        }
    }
}

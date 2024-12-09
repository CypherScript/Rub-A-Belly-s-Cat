
using DG.Tweening.Core.Easing;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BellyRub
{
    public class ThirdEyeController : CatEyeController
    {
        #region Serialized Fields

        [Header("Third Eye Visuals")]
        [SerializeField]
        private SpriteRenderer _thirdEyeSpriteRenderer = null;
        [SerializeField]
        private Animator _animator; // Animator for controlling the cat's animations

        #endregion
        

        protected void Awake()
        {
            ValidatePupils();
            InitializeEyeVisuals();
        }
        
        protected override void OnSessionStart(SessionState session)
        {
            base.OnSessionStart(session);
        }

        private void InitializeEyeVisuals()
        {
            OnShowThirdEye(false);
        }

        private void ValidatePupils()
        {
            if (catEyes.Count == 0)
            {
                Debug.LogWarning("No pupil assigned to ThirdEyeController. Please assign one in the inspector.");
            }
            else if (catEyes.Count > 1)
            {
                catEyes.RemoveRange(1, catEyes.Count - 1); // Remove any extra pupils beyond the first.
                Debug.LogWarning("ThirdEyeController should have only one pupil. Extra pupils have been removed.");
            }
        }
        
        private void OnShowThirdEye(bool show)
        {
            _animator.SetBool("IsThirdEyeVisible", show);
        }

        protected override void OnSessionEnd(SessionState session)
        {
            foreach (var pupils in catEyes)
            {
                pupils.CatPupilSpriteRenderer.sprite = pupils.CatPupilLevel1Sprite;
                pupils.CatPupilSpriteRenderer.transform.localScale = initialPupilScale;
                pupils.CatPupilPosition.position = pupils.eyeballCenter;
            }
            OnShowThirdEye(false);
        }

        protected override void Update()
        {
            if (_isActivated)
            {
                base.Update();
            }
        }

        private void OnAnimationFinish()
        {
            base.Activate();
            Debug.Log("Animation Event Fire!");
        }
        
        public override void Activate()
        {
            OnShowThirdEye(true);
           
            if (_session != null)
            {
                _session.OnfleshyGrowth.Invoke(true);
            }

            _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            GameManager.Instance.StateHandler.EnterState(new HitstopState(0.005f, 1f));

            StartCoroutine(WaitForHitstop());
            IEnumerator WaitForHitstop()
            {
                while (GameManager.Instance.StateHandler.HasState<HitstopState>())
                {
                    Debug.Log("WAITING FOR HITSTOP!");
                    yield return null;
                }

                _animator.updateMode = AnimatorUpdateMode.Normal;
            }
        }

        protected override void FireLaser()
        {
            base.FireLaser();
        }

        protected override void HandleLaserDeactivated(Laser laser)
        {
            base.HandleLaserDeactivated(laser);
        }

        protected override void UpdateEyeVisuals()
        {
            base.UpdateEyeVisuals();
        }
    }
}

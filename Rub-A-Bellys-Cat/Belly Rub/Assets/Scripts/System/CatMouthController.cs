

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BellyRub
{
    public class CatMouthController : CatAttackController
    {
        [SerializeField]
        private float _gracePeriodDuration = 0.3f;

        [SerializeField]
        private float _counterAttackDuration = 2.0f;

        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private GameObject _hairballGO = null;

        //[SerializeField]
        //private GameObject _hairballShadowGO = null;

        [SerializeField]
        private GameObject _rolfballGO = null;

        [SerializeField]
        private GameObject _bulletHellPrefab = null;

        protected override void OnTutorialStart(TutorialState tutorialState)
        {
            // Your custom logic for the child class

            base.OnTutorialStart(tutorialState);
        }

        protected override void OnTutorialEnd(TutorialState tutorialState)
        {
            base.OnTutorialEnd(tutorialState);
        }

        protected override void OnSessionStart(SessionState session)
        {
            // Your custom logic for the child class

            base.OnSessionStart(session); 
        }

        protected override void OnSessionEnd(SessionState session)
        {
            StopAllCoroutines();
            base.OnSessionEnd(session);
            _animator.Rebind();
            _animator.Update(0.0f);
        }

        public override bool IsValidToActivate(CatAttackSource attackSource)
        {
            return base.IsValidToActivate(CatAttackSource.Mouth) && attackSource == CatAttackSource.Mouth;
        }

        public override bool IsValidToReserve(CatAttackType attackType)
        {
            if (attackType == CatAttackType.MouthHairball && _hairballGO.activeSelf)
            {
                return false;
            }

            if (attackType == CatAttackType.MouthRolf && _rolfballGO.activeSelf)
            {
                return false;
            }

            return base.IsValidToReserve(attackType) && (int)(attackType) >= 20 && (int)(attackType) < 30;
        }

        // Activates the cat mouth controller for attacking.
        public override void Activate()
        {
            _isActivated = true;
        }

        // Increases the danger level of the cat mouth attack.
        public override void IncreaseDanger(CatAttackType attackType)
        {
            _dangerLevel++;
            UpdateMouthVisuals();

            if (_dangerLevel >= 2) 
            {
                Debug.Log("ATTACK TYPE: " + attackType);
                switch (attackType)
                {
                    case CatAttackType.MouthHairball:
                        FireHairball();
                        break;
                    case CatAttackType.MouthRolf:
                        FireRolf();
                        break;
                    case CatAttackType.MouthBulletPatternA:
                        FireBullets();
                        break;
                }
            }
        }

        // Decreases the danger level of the cat mouth attack.
        public override void DecreaseDanger(CatAttackType attackType)
        {
            if (_dangerLevel == 0) return;

            _dangerLevel--;
            UpdateMouthVisuals();
        }

        private void UpdateMouthVisuals()
        {
            _animator.SetInteger("DangerLevel", _dangerLevel);
        }

        private void FireHairball()
        {
            StopAllCoroutines();
            StartCoroutine(FireHairballCo());

            IEnumerator FireHairballCo()
            {
                yield return new WaitForSeconds(_gracePeriodDuration);
                _hairballGO.GetComponent<Hairball>().EnableHairball(_session);
                //_hairballShadowGO.SetActive(true);
                yield return new WaitForSeconds(_counterAttackDuration);
                ResetDangerLevel();
            }
        }

        private void FireRolf()
        {
            StopAllCoroutines();
            StartCoroutine(FireRolfCo());

            IEnumerator FireRolfCo()
            {
                yield return new WaitForSeconds(_gracePeriodDuration);
                UnityEngine.Debug.Log("Fire one of the 5 elemental Rolfs");
                _rolfballGO.GetComponent<Rolfball>().EnableRolfball(_session);
                yield return new WaitForSeconds(_counterAttackDuration);
                ResetDangerLevel();
            }
        }

        private void FireBullets()
        {
            BulletHellPool pool = BulletHellPool.GetPool(_bulletHellPrefab);
            int numLines = 3;
            int bulletsPerLine = 10;
            int numWaves = 3;

            StopAllCoroutines();
            StartCoroutine(FireBulletsCo());

            IEnumerator FireBulletsCo()
            {
                for (int i = 0; i < numWaves; i++)
                {
                    Vector2 vectorToMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    float angleToMouse = Mathf.Rad2Deg * Mathf.Atan2(vectorToMouse.y, vectorToMouse.x);
                    IEnumerable<BulletHellProjectile> projectiles;
                    for (int j = 0; j < bulletsPerLine; j++)
                    {
                        float lineAngle = angleToMouse + Random.Range(-2, 2) - (numLines-1) * 22.5f;
                        projectiles = pool.GetProjectiles(numLines);
                        foreach(BulletHellProjectile projectile in projectiles)
                        {
                            projectile.Fire(transform.position, 6f + j * 1.5f, lineAngle);
                            lineAngle += 45f;
                        }
                        yield return new WaitForSeconds(.05f);
                    }
                    yield return new WaitForSeconds(.5f);
                }

                ResetDangerLevel();
            }
        }

        private void ResetDangerLevel()
        {
            _dangerLevel = 0;
            UpdateMouthVisuals();
        }
    }
}

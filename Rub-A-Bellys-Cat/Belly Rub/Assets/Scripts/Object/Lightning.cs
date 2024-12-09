using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;
using UnityEngine.Events;

public class Lightning : Projectile
{
    [SerializeField] private CircleCollider2D _hitCollider;
    [SerializeField] private float _lightningDamageDelayTime = 0.1f;
    [SerializeField] private float _lightningDamageDurationTime = 0.2f;
    [SerializeField] private float _lightningVisualsDurationTime = 1.8f;
    
    private Coroutine _followCoroutine;
    public static UnityEvent OnLightningStrike = new();
    
    private void OnEnable()
    {
        if (_followCoroutine != null)
        {
            StopCoroutine(_followCoroutine);
        }
        _followCoroutine = StartCoroutine(WarmUp());
    }

    public override void OnHit(SessionState session)
    {
        session.TakeDamage(CatAttackType.Lightning);
    }

    protected override void DestroyProjectile()
    {
        _hitCollider.enabled = false;
    }

    IEnumerator WarmUp()
    {
        yield return new WaitForSeconds(_lightningDamageDelayTime);
        _hitCollider.enabled = true;
        OnLightningStrike.Invoke();
        yield return new WaitForSeconds(_lightningDamageDurationTime);
        _hitCollider.enabled = false;
        yield return new WaitForSeconds(_lightningVisualsDurationTime);
        Destroy(this.gameObject);
    }
}

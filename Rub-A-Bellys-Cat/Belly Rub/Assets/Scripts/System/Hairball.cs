using BellyRub;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using SessionState = BellyRub.SessionState;

public class Hairball : Projectile
{
    [SerializeField] private CircleCollider2D _bellyCollider = null;
    [SerializeField] private GameObject _projectileGO = null;
    [SerializeField] private GameObject _hairballGO = null;
    [SerializeField] private SpriteRenderer _hairballSR = null;
    [SerializeField] private ParticleSystem _hairballPE = null;
    //[SerializeField] private GameObject _shadowGO = null;
    [SerializeField] private float _flightDuration = 1.5f;
    [SerializeField] private float _shrinkDuration = 0.25f;
    [SerializeField] private float _idleDuration = 1f;
    [SerializeField] private int _blinkCount = 6;
    [SerializeField] private float _blinkDuration = 0.1f;
    [SerializeField] private int _numProjectileWaves = 3;
    [SerializeField] private int _numProjectilesPerWave = 4;
    [SerializeField] private float _shootDelay = 0.5f;
    [SerializeField] private Vector2 _hairballStartSize = Vector2.zero;
    [SerializeField] private Vector2 _hairballMinSize = Vector2.zero;
    [SerializeField] private Vector2 _hairballMaxSize = Vector2.zero;

    private Vector2 _endPoint = Vector2.zero;
    private Vector2 _hairballStartPosition = Vector2.zero;
    private Vector2 _shadowStartPosition = Vector2.zero;
    private Vector2 _shadowOffset = Vector2.zero;
    private bool _canMove = false;
    private bool _isShrinking = false;
    private float timer = 0;
    private float _growthDuration = 0f;
    private SessionState _session = null;

    private void Awake()
    {
        _hairballStartPosition = transform.position;
        _hairballGO.transform.localScale = _hairballStartSize;
        _growthDuration = _flightDuration / 2f;
        //_shadowStartPosition = _shadowGO.transform.position;
        _shadowOffset = _shadowStartPosition - (Vector2)transform.position;
        //_shadowGO.transform.position = _hairballGO.transform.position;
    }

    private void OnEnable()
    {
        TutorialState.OnTutorialStart.AddListener(OnTutorialStart);
        TutorialState.OnTutorialEnd.AddListener(OnTutorialEnd);

        SessionState.OnSessionStart.AddListener(OnSessionStart);
        SessionState.OnSessionEnd.AddListener(OnSessionEnd);

        StartCoroutine(GrowHairball());
    }

    private void OnDisable()
    {
        ResetHairball();

        TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);
        TutorialState.OnTutorialEnd.RemoveListener(OnTutorialEnd);

        SessionState.OnSessionStart.RemoveListener(OnSessionStart);
        SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);
    }

    private void Update()
    {
        if (!_canMove) return;

        if ((Vector2)transform.position != _endPoint)
            transform.position = Vector2.Lerp(_hairballStartPosition, _endPoint, Mathf.SmoothStep(0, timer / _flightDuration, timer / _flightDuration));

        if (!_isShrinking)
            _hairballGO.transform.localScale = Vector2.Lerp(_hairballMinSize, _hairballMaxSize, timer / _growthDuration);

        timer += Time.deltaTime;

        if ((Vector2)_hairballGO.transform.localScale == _hairballMaxSize)
            StartCoroutine(ShrinkHairball());
    }

    private IEnumerator GrowHairball()
    {
        float timer = 0;
        //_shadowGO.SetActive(true);

        while (timer < _growthDuration && (Vector2)_hairballGO.transform.localScale != _hairballMinSize)
        {
            timer += Time.deltaTime;

            _hairballGO.transform.localScale = Vector2.Lerp(_hairballStartSize, _hairballMinSize, timer / _growthDuration);
            //_shadowGO.transform.localScale = Vector2.Lerp(_hairballStartSize, new Vector3(0.6f, 0.6f), timer / _growthDuration);
            //_shadowGO.transform.position = Vector2.Lerp(_shadowStartPosition, (Vector2)_hairballGO.transform.position + _shadowOffset, timer / _growthDuration);
            yield return null;
        }
        
        _endPoint = GenerateRandomEndPoint(_bellyCollider.transform.position, _bellyCollider.radius);
        yield return new WaitForSeconds(_shootDelay);
        _canMove = true;

        _hairballPE.Emit(50);
        _session.OnHairballFired?.Invoke();
    }

    private IEnumerator ShrinkHairball()
    {
        _isShrinking = true;

        float timer = 0;

        while (timer < _growthDuration && (Vector2)_hairballGO.transform.localScale != _hairballMinSize)
        {
            timer += Time.deltaTime;

            _hairballGO.transform.localScale = Vector2.Lerp(_hairballMaxSize, _hairballMinSize, (timer / _growthDuration));
            yield return null;
        }

        timer = 0;

        yield return new WaitForSeconds(_idleDuration);

        for(int i = 0; i < _blinkCount; i++)
        {
            _hairballSR.color = _hairballSR.color == Color.white ? Color.red : Color.white;
            yield return new WaitForSeconds(_blinkDuration);
        }


        BulletHellPool pool = BulletHellPool.GetPool(_projectileGO);
        float offset = Random.Range(0f, 360f);
        for (int i = 0; i < _numProjectileWaves; i++)
        {
            float angle = offset + Random.Range(-5f, 5f);
            IEnumerable<BulletHellProjectile> projectiles = pool.GetProjectiles(_numProjectilesPerWave);
            foreach (BulletHellProjectile projectile in projectiles)
            {
                projectile.Fire(transform.position, 4f + i/2f, angle);
                angle += 360f / _numProjectilesPerWave;
            }
            yield return new WaitForSeconds(.1f);
        }

        while (timer < _growthDuration && (Vector2)_hairballGO.transform.localScale != _hairballStartSize)
        {
            timer += Time.deltaTime;

            _hairballGO.transform.localScale = Vector2.Lerp(_hairballMinSize, _hairballStartSize, (timer / _shrinkDuration));
            //_shadowGO.transform.localScale = Vector2.Lerp(new Vector3(0.6f, 0.6f), _hairballStartSize, (timer / _growthDuration) * 2);
            yield return null;
        }

        ResetHairball();
    }

    private Vector2 GenerateRandomEndPoint(Vector2 centerPoint, float radius)
    {
        Vector2 position =  centerPoint + Random.insideUnitCircle * radius * Random.Range(-2, 5);
        return position;
    }

    private void ResetHairball()
    {
        gameObject.SetActive(false);
        //_shadowGO.SetActive(false);
        _canMove = false;
        _isShrinking = false;
        transform.position = _hairballStartPosition;
        _hairballGO.transform.localScale = _hairballStartSize;
        //_shadowGO.transform.position = _shadowStartPosition;
        //_shadowGO.transform.localScale = new Vector3(0.6f, 0.6f);
        timer = 0;
        _session = null;
    }

    public override void OnHit(SessionState session)
    {
        session.TakeDamage(CatAttackType.MouthHairball);
    }

    public void EnableHairball(SessionState session)
    {
        _session = session;
        gameObject.SetActive(true);
    }

    public void OnSessionStart(SessionState session)
    {
        _session = session;
    }

    public void OnSessionEnd(SessionState session)
    {
        ResetHairball();
        _session = null;
    }

    public void OnTutorialStart(TutorialState state)
    {
        _session = state.SessionState;
    }

    public void OnTutorialEnd(TutorialState state)
    {
        ResetHairball();
        _session = null;
    }
}

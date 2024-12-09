using BellyRub;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rolfball : MonoBehaviour
{
    [SerializeField] private GameObject _headGO = null;
    [SerializeField] private GameObject _armsGO = null;
    [SerializeField] private GameObject _legsGO = null;
    [SerializeField] private float _flightDuration = 1.0f;
    [SerializeField] private float _armSproutDelay = 0.25f;
    [SerializeField] private float _legsSproutDelay = 0.25f;
    [SerializeField] private float _flipDelay = 1f;
    [SerializeField] private float _shootDelay = 1f;
    [SerializeField] private Vector2 _ballStartSize = Vector2.zero;
    [SerializeField] private Vector2 _ballMinSize = Vector2.zero;
    [SerializeField] private Vector2 _ballMaxSize = Vector2.zero;

    private Animator _animator = null;
    private SpriteRenderer _headRenderer = null;
    private SpriteRenderer _legsRenderer = null;
    private float _growthDuration = 0f;
    private float _timer = 0f;
    private Vector2 _ballStartPosition = Vector2.zero;
    private Vector2 _legsStartPosition = Vector2.zero;
    private Vector2 _headPosition = Vector2.zero;
    private Vector2 _headWalkRightDestination = Vector2.zero;
    private Vector2 _headWalkLeftDestination = Vector2.zero;
    private Vector2 _legsPosition = Vector2.zero;
    private Vector2 _legsFlipPosition = Vector2.zero;
    private Vector2 _endPoint = Vector2.zero;
    private bool _canMove = false;
    private bool _isFlying = false;
    private bool _canWalk = false;
    private SessionState _session = null;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _headRenderer = _headGO.GetComponent<SpriteRenderer>();
        _legsRenderer = _legsGO.GetComponent<SpriteRenderer>();

        transform.localScale = _ballStartSize;
        _growthDuration = _flightDuration / 2;
        _ballStartPosition = transform.position;
        _legsStartPosition = _legsGO.transform.localPosition;

        _endPoint = new Vector2(-7, -7);
        _headPosition = new Vector2(_endPoint.x, -1.5f);
        _headWalkRightDestination = new Vector2(6, _headPosition.y);
        _headWalkLeftDestination = new Vector2(-20, _headPosition.y);
        _legsPosition = new Vector2(_legsGO.transform.localPosition.x, -5f);
        _legsFlipPosition = new Vector2(0.29f, _legsPosition.y);
    }

    private void OnEnable()
    {
        TutorialState.OnTutorialStart.AddListener(OnTutorialStart);
        TutorialState.OnTutorialEnd.AddListener(OnTutorialEnd);

        SessionState.OnSessionStart.AddListener(OnSessionStart);
        SessionState.OnSessionEnd.AddListener(OnSessionEnd);

        StartCoroutine(GrowRolfball());
    }

    private void OnDisable()
    {
        ResetRolfball();

        TutorialState.OnTutorialStart.RemoveListener(OnTutorialStart);
        TutorialState.OnTutorialEnd.RemoveListener(OnTutorialEnd);

        SessionState.OnSessionStart.RemoveListener(OnSessionStart);
        SessionState.OnSessionEnd.RemoveListener(OnSessionEnd);
    }

    private void Update()
    {
        if (!_canMove) return;

        if ((Vector2)transform.position != _endPoint)
            transform.position = Vector2.Lerp(_ballStartPosition, _endPoint, Mathf.SmoothStep(0, _timer / _flightDuration, _timer / _flightDuration));

        if (_isFlying)
            StartCoroutine(ScaleRolfball());

        if (_canWalk)
            StartCoroutine(RolfWalk());

        _timer += Time.deltaTime;
    }

    private IEnumerator RolfWalk()
    {
        _canWalk = false;
        _canMove = false;
        StopCoroutine(ScaleRolfball());

        yield return new WaitForSeconds(_armSproutDelay);
        _armsGO.SetActive(true);

        yield return new WaitForSeconds(_legsSproutDelay);

        while ((Vector2)transform.localPosition != _headPosition && (Vector2)_legsGO.transform.localPosition != _legsPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, _headPosition, 1.5f * Time.deltaTime);
            _legsGO.transform.localPosition = Vector2.MoveTowards(_legsGO.transform.localPosition, _legsPosition, 2f * Time.deltaTime);

            yield return null;
        }

        yield return new WaitForSeconds(_flipDelay);

        _legsGO.transform.localPosition = _legsFlipPosition;
        _headRenderer.flipX = true;
        _legsRenderer.flipX = true;
        _animator.SetBool("isWalking", true);

        while ((Vector2)transform.position != _headWalkRightDestination)
        {
            transform.position = Vector2.MoveTowards(transform.position, _headWalkRightDestination, 5 * Time.deltaTime);
            yield return null;
        }

        _animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(_flipDelay);

        _legsFlipPosition.x *= -1;
        _legsGO.transform.localPosition = _legsFlipPosition;
        _headRenderer.flipX = false;
        _legsRenderer.flipX = false;
        _animator.SetBool("isWalking", true);

        while ((Vector2)transform.position != _headWalkLeftDestination)
        {
            transform.position = Vector2.MoveTowards(transform.position, _headWalkLeftDestination, 5 * Time.deltaTime);
            yield return null;
        }

        ResetRolfball();
    }

    private IEnumerator ScaleRolfball()
    {
        _isFlying = false;
        float timer = 0;

        while (timer < _growthDuration && (Vector2)transform.localScale != _ballMaxSize)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector2.Lerp(_ballMinSize, _ballMaxSize, timer / _growthDuration);
            yield return null;
        }

        timer = 0;

        while (timer < _growthDuration && (Vector2)transform.localScale != _ballMinSize)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector2.Lerp(_ballMaxSize, _ballMinSize, timer / _growthDuration);
            yield return null;
        }

        _canWalk = true;
    }

    private IEnumerator GrowRolfball()
    {
        float timer = 0;

        while (timer < _growthDuration && (Vector2)transform.localScale != _ballMinSize)
        {
            timer += Time.deltaTime;

            transform.localScale = Vector2.Lerp(_ballStartSize, _ballMinSize, timer / _growthDuration);
            yield return null;
        }

        yield return new WaitForSeconds(_shootDelay);
        _canMove = true;
        _isFlying = true;
        _session.OnHairballFired?.Invoke();
    }


    private void ResetRolfball()
    {
        _animator.SetBool("isWalking", false);
        transform.position = _ballStartPosition;
        transform.localScale = _ballStartSize;
        _legsGO.transform.localPosition = _legsStartPosition;
        _legsFlipPosition = new Vector2(0.29f, _legsPosition.y);
        _timer = 0;
        _canMove = false;
        _isFlying = false;
        _canWalk = false;

        StopCoroutine(GrowRolfball());
        StopCoroutine(ScaleRolfball());
        StopCoroutine(RolfWalk());

        _armsGO.SetActive(false);
        gameObject.SetActive(false);

        _session = null;
    }

    public void EnableRolfball(SessionState session)
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
        ResetRolfball();
        _session = null;
    }

    public void OnTutorialStart(TutorialState state)
    {
        _session = state.SessionState;
    }

    public void OnTutorialEnd(TutorialState state)
    {
        ResetRolfball();
        _session = null;
    }
}

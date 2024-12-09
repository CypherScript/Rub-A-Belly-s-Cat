using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    /// <summary>
    /// Cursors
    /// 0 - Default
    /// 1 - Rubbing
    /// 2 - Take Damage
    /// </summary>
    [SerializeField] private GameObject[] cursors;
    [SerializeField] private Image HandDamagedImage;
    [SerializeField] private Image HandInactiveImage;
    [SerializeField] private Image HandInactiveOutlineImage;
    [SerializeField] private float _hurtTime = 1f;
    [SerializeField] private float _rotationScale = 10f;
    [SerializeField] private float _minAngle = -45f;
    [SerializeField] private float _maxAngle = 45f;
    [SerializeField] private float flickerRate = 0.1f;
    [SerializeField] private float flickerTransparency = 0.3f;

    private bool _locked;
    private SessionState _sessionState = null;
    private Vector2 _previousPosition = Vector2.zero;
    private Vector2 _currentPosition = Vector2.zero;

    private void OnEnable()
    {
        Cursor.visible = false;
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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.visible = false;
        }

        if (GameManager.Instance.StateHandler.HasState<PauseState>() 
            ||
            GameManager.Instance.StateHandler.HasState<MainMenuState>() 
            ||
            GameManager.Instance.StateHandler.HasState<GameoverState>())
        {
            HandInactiveImage.color = new Color(HandInactiveImage.color.r, HandInactiveImage.color.g, HandInactiveImage.color.b, 1f);
            HandInactiveOutlineImage.color = new Color(HandInactiveOutlineImage.color.r, HandInactiveOutlineImage.color.g, HandInactiveOutlineImage.color.b, 1f);
        }
        else
        {
            HandInactiveImage.color = new Color(HandInactiveImage.color.r, HandInactiveImage.color.g, HandInactiveImage.color.b, 0.6f);
            HandInactiveOutlineImage.color = new Color(HandInactiveOutlineImage.color.r, HandInactiveOutlineImage.color.g, HandInactiveOutlineImage.color.b, 0.6f);
        }
        
    
        _currentPosition = Input.mousePosition;
        
        if (_sessionState == null || _locked) return;

        if (Input.GetMouseButton(0) && _sessionState.IsTouchAllowed)
            Rubbing();
        else
            ResetCursor();
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        transform.position = _currentPosition;
        _previousPosition = transform.position;
    }

    private void Rubbing()
    {
        SetCursor(1);
        if (_currentPosition == _previousPosition) return;

        Vector2 delta = _previousPosition - _currentPosition;

        Quaternion rot = new Quaternion(0, 0, 0, 0);
        rot.eulerAngles = transform.rotation.eulerAngles + new Vector3(0, 0, _rotationScale * delta.normalized.x);
        rot.z = Mathf.Clamp(rot.z, _minAngle / 100, _maxAngle / 100);
        transform.rotation = rot;
    }

    private void ResetCursor()
    {
        SetCursor(0);
        transform.rotation = Quaternion.identity;
    }

    private void SetCursor(int index)
    {
        for (var i = 0; i < cursors.Length; i++)
        {
            cursors[i].SetActive(index == i);
        }
    }

    private void ShowHurtHand()
    {
        StartCoroutine(HurtHandCo());
    }
    
    private IEnumerator HurtHandCo()
    {
        SetCursor(2);
        transform.rotation = Quaternion.identity;
        _locked = true;
        
        Color originalColor = HandDamagedImage.color;

        float elapsed = 0f;
        bool isFlickerOn = true;
      

        while (elapsed < CatGlobalSetting.Instance.DamagedLockOutTime)
        {
            if (isFlickerOn)
            {
                HandDamagedImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, flickerTransparency); // 50% transparency
            }
            else
            {
                HandDamagedImage.color = originalColor; // full visibility
            }
            isFlickerOn = !isFlickerOn;

            yield return new WaitForSeconds(flickerRate);
            elapsed += flickerRate;
        }
        _locked = false;
        ResetCursor();
        HandDamagedImage.color = originalColor; // restore the original color at the end
    }
    
    public void OnSessionStart(SessionState session)
    {
        _sessionState = session;
        _sessionState.OnTakeDamage.AddListener(ShowHurtHand);
    }

    public void OnSessionEnd(SessionState session)
    {
        _sessionState.OnTakeDamage.RemoveListener(ShowHurtHand);
        _sessionState = null;
        ResetCursor();
    }

    public void OnTutorialStart(TutorialState state)
    {
        _sessionState = state.SessionState;
    }

    public void OnTutorialEnd(TutorialState state)
    {
        _sessionState = state.SessionState;
    }
}

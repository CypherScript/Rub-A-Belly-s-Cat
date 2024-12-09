using System;
using System.Collections;
using System.Collections.Generic;
using BellyRub;
using UnityEngine;
using UnityEngine.UI;

public class Smiler : MonoBehaviour
{
    [Serializable]
    private struct SmilerThreshold
    {
        public Vector2 range;
        public Sprite icon;
        public Color thresholdColor;
    }

    [SerializeField] private Image smilerIcon;
    [SerializeField] private Sprite takeDamageIcon;
    [SerializeField] private List<SmilerThreshold> thresholds;
    [SerializeField] private RectTransform smilerRect;
    [SerializeField] private RectTransform fillbarRect;
    [SerializeField] private Vector2 smilerOffset;
    [SerializeField] private Image fillbar;
    [SerializeField] private Animator animator;
    [SerializeField] private float attackFaceLockTime;
    [SerializeField] private float meterColorChangeSpeed = 2f;

    private bool isLocked;
    private int currentThreshold = -1;
    private SessionState session;

    private void Update()
    {
        if (session == null) return;
        for (int i = 0; i < thresholds.Count; i++)
        {
            if (!(session.patienceLevel >= thresholds[i].range.x) || !(session.patienceLevel <= thresholds[i].range.y) ||
                currentThreshold == i || isLocked) continue;
            
            currentThreshold = i;
            smilerIcon.sprite = thresholds[i].icon;
            animator.SetTrigger("ChangeState");
            break;
        }

        fillbar.color = Color.Lerp(fillbar.color,thresholds[currentThreshold].thresholdColor,meterColorChangeSpeed* Time.deltaTime);

        float xPos = fillbar.fillAmount * fillbarRect.rect.width;
        smilerRect.anchoredPosition = new Vector2(xPos + smilerOffset.x, smilerRect.anchoredPosition.y + smilerOffset.y);
    }

    IEnumerator LockAttackFace()
    {
        isLocked = true;
        animator.SetTrigger("ChangeState");
        smilerIcon.sprite = takeDamageIcon;
        yield return new WaitForSeconds(attackFaceLockTime);
        smilerIcon.sprite = thresholds[currentThreshold].icon;
        isLocked = false;
    }
    
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
        StopAllCoroutines();
    }

    private void OnSessionStart(SessionState session)
    {
        this.session = session;
        session.OnTakeDamage.AddListener(delegate { StartCoroutine(LockAttackFace()); });
    }

    private void OnSessionEnd(SessionState session)
    {
        this.session = null;
        session.OnTakeDamage.RemoveListener(delegate { StartCoroutine(LockAttackFace()); });
    }

    private void OnTutorialStart(TutorialState tutorialState)
    {
        session = tutorialState.SessionState;
    }

    private void OnTutorialEnd(TutorialState tutorialState)
    {
        session = null;
    }
}

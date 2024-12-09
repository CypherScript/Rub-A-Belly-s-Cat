using BellyRub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDisplay : MonoBehaviour
{
    [SerializeField] private float _duration = 0.4f;
    private CanvasGroup _canvasGroup;

    private void OnEnable()
    {
        TutorialState.OnTutorialStart.AddListener(EnableTutorialDisplay);
        TutorialState.OnTutorialEnd.AddListener(StartFade);
    }

    private void OnDisable()
    {
        TutorialState.OnTutorialStart.RemoveListener(EnableTutorialDisplay);
        TutorialState.OnTutorialEnd.RemoveListener(StartFade);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void EnableTutorialDisplay(TutorialState tutorialState)
    {
        gameObject.SetActive(true);
    }

    private void DisableTutorialDisplay(TutorialState tutorialState)
    {
        gameObject.SetActive(false); 
    }

    public void StartFade(TutorialState tutorialState)
    {
        StartCoroutine(Fade(_canvasGroup, 1, 0));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float start, float end)
    {
        float timer = 0f;

        while(timer < _duration)
        {
            timer += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(start, end, timer / _duration);

            yield return null;
        }
    }
}

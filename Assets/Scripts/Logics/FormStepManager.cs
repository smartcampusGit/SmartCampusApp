using UnityEngine;
using System.Collections;

public class FormStepManager : MonoBehaviour
{
    public CanvasGroup step1Group;
    public CanvasGroup step2Group;
    public float fadeDuration = 0.4f;

    private void Start()
    {
        StartCoroutine(FadeToStep(step1Group, step2Group)); // Start on step 1
    }

    public void ShowStep1()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToStep(step1Group, step2Group));
    }

    public void ShowStep2()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToStep(step2Group, step1Group));
    }

    private IEnumerator FadeToStep(CanvasGroup show, CanvasGroup hide)
    {
        // הופך את השני ללא פעיל
        hide.interactable = false;
        hide.blocksRaycasts = false;

        // מופיע בהדרגה
        show.gameObject.SetActive(true);
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            show.alpha = t / fadeDuration;
            hide.alpha = 1 - (t / fadeDuration);
            yield return null;
        }

        show.alpha = 1;
        hide.alpha = 0;
        hide.gameObject.SetActive(false);

        show.interactable = true;
        show.blocksRaycasts = true;
    }
}

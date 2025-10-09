using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles fading a serialized black image in and out to shade the screen during loop resets.
/// Also hides the hints panel so that only hints are blocked by the overlay.
/// </summary>
public class ScreenShading : MonoBehaviour
{
    public static ScreenShading Instance { get; private set; }

    [SerializeField] private Image shadeImage;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private HintsPanelController hintsPanel;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[ScreenShading] Duplicate instance detected on {name}, destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (shadeImage == null)
            shadeImage = GetComponent<Image>();

        if (shadeImage != null)
        {
            EnsureImageActive();
            SetAlpha(0f);
            shadeImage.raycastTarget = false; // do not block regular UI interactions.
            shadeImage.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Starts fading the overlay to full black.
    /// </summary>
    public void Shade()
    {
        if (shadeImage == null)
            return;

        EnsureImageActive();

        if (isActiveAndEnabled)
        {
            StartFade(1f);
        }
        else
        {
            SetAlpha(1f);
        }

        if (hintsPanel != null && hintsPanel.IsPanelActive())
            hintsPanel.HidePanel();
    }

    /// <summary>
    /// Starts fading the overlay back to transparent.
    /// </summary>
    public void Unshade()
    {
        if (shadeImage == null)
            return;

        if (isActiveAndEnabled)
        {
            StartFade(0f);
        }
        else
        {
            SetAlpha(0f);
            shadeImage.gameObject.SetActive(false);
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (!gameObject.activeInHierarchy)
        {
            SetAlpha(targetAlpha);
            shadeImage.gameObject.SetActive(targetAlpha > 0f);
            return;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = shadeImage.color.a;
        float elapsed = 0f;

        if (targetAlpha > 0f && !shadeImage.gameObject.activeSelf)
            shadeImage.gameObject.SetActive(true);

        if (Mathf.Approximately(fadeDuration, 0f))
        {
            SetAlpha(targetAlpha);
            if (Mathf.Approximately(targetAlpha, 0f))
                shadeImage.gameObject.SetActive(false);
            fadeRoutine = null;
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);

        if (Mathf.Approximately(targetAlpha, 0f))
            shadeImage.gameObject.SetActive(false);

        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (shadeImage == null)
            return;

        Color c = shadeImage.color;
        c.a = Mathf.Clamp01(alpha);
        shadeImage.color = c;
    }

    private void EnsureImageActive()
    {
        if (shadeImage != null && !shadeImage.gameObject.activeSelf)
            shadeImage.gameObject.SetActive(true);
    }
}

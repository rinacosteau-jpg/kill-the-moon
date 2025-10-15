using Articy.World_Of_Red_Moon.GlobalVariables;
using UnityEngine;

/// <summary>
/// Listens for the Articy RFLG.endOfDemo flag and, when triggered,
/// activates the end-of-demo UI and disables player controls.
/// </summary>
public class EndOfDemo : MonoBehaviour
{
    [SerializeField] private GameObject endOfDemoPanel;

    private bool hasTriggered;

    private void Update()
    {
        if (hasTriggered)
            return;

        var globals = ArticyGlobalVariables.Default;
        var rflg = globals?.RFLG;
        if (rflg == null || !rflg.endOfDemo)
            return;

        TriggerEndOfDemo();
    }

    private void TriggerEndOfDemo()
    {
        hasTriggered = true;

        DisablePlayerControls();
        ActivateEndOfDemoPanel();

        Debug.Log("[EndOfDemo] Demo completed.");
    }

    private void ActivateEndOfDemoPanel()
    {
        var panel = endOfDemoPanel;
        if (panel == null)
            panel = FindPanelByName("EndOfTheDemo");

        if (panel == null)
        {
            Debug.LogWarning("[EndOfDemo] EndOfTheDemo panel not found in scene.");
            return;
        }

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        var canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private static GameObject FindPanelByName(string panelName)
    {
        if (string.IsNullOrEmpty(panelName))
            return null;

        var rectTransforms = Object.FindObjectsOfType<RectTransform>(true);
        foreach (var rect in rectTransforms)
        {
            if (rect == null)
                continue;

            var go = rect.gameObject;
            if (go != null && go.name == panelName)
                return go;
        }

        return GameObject.Find(panelName);
    }

    private static void DisablePlayerControls()
    {
        foreach (var movement in Object.FindObjectsOfType<PlayerMovementScript>(true))
        {
            if (movement != null)
                movement.enabled = false;
        }

        foreach (var interact in Object.FindObjectsOfType<PlayerInteractScript>(true))
        {
            if (interact != null)
                interact.enabled = false;
        }
    }
}

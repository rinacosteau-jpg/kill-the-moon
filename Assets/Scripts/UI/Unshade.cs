using UnityEngine;

/// <summary>
/// Helper component to stop screen shading through UnityEvents or animation events.
/// </summary>
public class Unshade : MonoBehaviour
{
    [SerializeField] private ScreenShading screenShading;

    public void Execute()
    {
        var target = ResolveShading();
        if (target != null)
            target.Unshade();
    }

    private ScreenShading ResolveShading()
    {
        if (screenShading == null)
            screenShading = ScreenShading.Instance != null ? ScreenShading.Instance : FindObjectOfType<ScreenShading>(true);

        return screenShading;
    }
}

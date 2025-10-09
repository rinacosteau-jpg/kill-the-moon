using UnityEngine;

/// <summary>
/// Helper component to trigger screen shading through UnityEvents or animation events.
/// </summary>
public class Shade : MonoBehaviour
{
    [SerializeField] private ScreenShading screenShading;

    public void Execute()
    {
        var target = ResolveShading();
        if (target != null)
            target.Shade();
    }

    private ScreenShading ResolveShading()
    {
        if (screenShading == null)
            screenShading = ScreenShading.Instance != null ? ScreenShading.Instance : FindObjectOfType<ScreenShading>(true);

        return screenShading;
    }
}

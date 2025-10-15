using UnityEngine;

/// <summary>
/// Ensures every object that has a mesh but lacks a collider
/// receives a BoxCollider when the game starts.
/// </summary>
public static class ColliderBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureColliders()
    {
        foreach (var meshFilter in Object.FindObjectsOfType<MeshFilter>(true))
        {
            TryAddCollider(meshFilter.gameObject, meshFilter.sharedMesh != null);
        }

        foreach (var skinnedMeshRenderer in Object.FindObjectsOfType<SkinnedMeshRenderer>(true))
        {
            TryAddCollider(skinnedMeshRenderer.gameObject, skinnedMeshRenderer.sharedMesh != null);
        }
    }

    private static void TryAddCollider(GameObject gameObject, bool hasMesh)
    {
        if (!hasMesh)
            return;

        if (gameObject.TryGetComponent<Collider>(out _))
            return;

        gameObject.AddComponent<BoxCollider>();
    }
}

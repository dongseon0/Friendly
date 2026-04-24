using UnityEngine;

public class FlashlightPickup : ItemPickup
{
    protected override void OnPickupSuccess()
    {
        HideFlashlightAsset();
        FlashlightRuntimeController.Instance.AcquireFlashlight();
    }

    private void HideFlashlightAsset()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            r.enabled = false;

        var colliders = GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
            c.enabled = false;
    }
}
using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerCollisionHandler2D : MonoBehaviour
{
    private bool isCollisionDetectionEnabled;

    public event Action<Collision2D> CollisionDetected;

    public bool IsCollisionDetectionEnabled => isCollisionDetectionEnabled;

    public void SetCollisionDetectionEnabled(bool isEnabled)
    {
        isCollisionDetectionEnabled = isEnabled;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCollisionDetectionEnabled)
        {
            return;
        }

        CollisionDetected?.Invoke(collision);
    }

    private void OnDisable()
    {
        isCollisionDetectionEnabled = false;
    }
}
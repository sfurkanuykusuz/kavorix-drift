using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerCollisionHandler2D : MonoBehaviour
{
    public event Action<Collision2D> CollisionDetected;

    private bool isCollisionDetectionEnabled;

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
}
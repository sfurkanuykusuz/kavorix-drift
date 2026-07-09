using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerBoostEffect : MonoBehaviour
{
    [Header("Boost Effect")]
    [SerializeField] private ParticleSystem boostFlameParticles;
    [SerializeField] private AudioSource boostFlameAudioSource;

    public void SetBoosting(bool isBoosting)
    {
        if (isBoosting)
        {
            Play();
        }
        else
        {
            Stop();
        }
    }

    public void Play()
    {
        if (boostFlameParticles != null && !boostFlameParticles.isPlaying)
        {
            boostFlameParticles.Play();
        }

        if (boostFlameAudioSource != null && !boostFlameAudioSource.isPlaying)
        {
            boostFlameAudioSource.Play();
        }
    }

    public void Stop()
    {
        if (boostFlameParticles != null && boostFlameParticles.isPlaying)
        {
            boostFlameParticles.Stop();
        }

        if (boostFlameAudioSource != null && boostFlameAudioSource.isPlaying)
        {
            boostFlameAudioSource.Stop();
        }
    }
}
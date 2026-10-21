using Lean.Pool;
using UnityEngine;

/// <summary>
/// 자식 파티클까지 재생이 끝나면 이펙트를 Lean Pool에 반환합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public class PooledParticleEffect : MonoBehaviour, IPoolable
{
    private ParticleSystem _particles;
    private bool _isPlaying;

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
    }

    public void OnSpawn()
    {
        _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _particles.Play(true);
        _isPlaying = true;
    }

    public void OnDespawn()
    {
        _isPlaying = false;
        _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (_isPlaying && !_particles.IsAlive(true))
        {
            _isPlaying = false;
            LeanPool.Despawn(gameObject);
        }
    }
}

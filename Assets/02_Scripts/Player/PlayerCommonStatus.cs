using System;
using UnityEngine;

public class PlayerCommonStatus : MonoBehaviour, IDamageable
{
    [Header("Status-Health")]
    [SerializeField] private float _currentHealth = 0f;
    [SerializeField] private float _maxHealth = 300f;

    [Header("Status - Stamina")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _currentStamina;
    [SerializeField] private float _staminaRecovery = 15f;


    [Header("Movement Status")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _rotationSpeed = 360f;
    [SerializeField] private float _jumpPower = 7f;
    [SerializeField] private int _maxJumps = 2;

    [Header("Attack Status")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _fireInterval = 0.1f;
    [SerializeField] private float _range = 100f;


    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float CurrentHealthRatio => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;
    public float CurrentStamina => _currentStamina;
    public float MaxStamina => _maxStamina;
    public float CurrentStaminaRatio => _maxStamina > 0f ? _currentStamina / _maxStamina : 0f;
    public float MoveSpeed => _moveSpeed;
    public float SprintSpeed => _sprintSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float JumpPower => _jumpPower;
    public int MaxJumps => _maxJumps;
    public float Damage => _damage;
    public float FireInterval => _fireInterval;
    public float Range => _range;

    public event Action OnTakeHit;

    private void Awake()
    {
        _currentHealth = Mathf.Max(0f, _maxHealth);
        _currentStamina = Mathf.Max(0f, _maxStamina);
    }

    private void Update()
    {
        _currentStamina = Mathf.Clamp(_currentStamina + _staminaRecovery * Time.deltaTime,
            0f, Mathf.Max(0f, _maxStamina));
    }

    public bool TryUseStamina(float amount)
    {
        if (amount < 0f || _currentStamina < amount) return false;

        _currentStamina -= amount;
        return true;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _currentHealth = Mathf.Clamp(_currentHealth - Mathf.Max(0f, damageInfo.DamageValue), 0f, Mathf.Max(0f, _maxHealth));
        OnTakeHit?.Invoke();
        if (_currentHealth <= 0f)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        GameLogger.Log($"{name} : Player is Dead!");
        GameManager.Instance.GameOver();
    }

}

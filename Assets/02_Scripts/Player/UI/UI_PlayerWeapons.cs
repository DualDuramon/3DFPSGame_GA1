using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerWeapons : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _bulletCountText;
    [SerializeField] private TextMeshProUGUI _bombCountText;

    [SerializeField] private PlayerGunFire _gunSubject;
    [SerializeField] private PlayerBombFire _bombSubject;

    [Header("Weapon Icons")]
    [SerializeField] private Sprite _normalWeaponIcon;
    [SerializeField] private Sprite _sniperWeaponIcon;

    private void Awake()
    {
        if (_weaponIcon == null)
        {
            _weaponIcon = GetComponentInChildren<Image>();
        }

        if (_gunSubject == null)
        {
            _gunSubject = FindAnyObjectByType<PlayerGunFire>();
            _bombSubject = FindAnyObjectByType<PlayerBombFire>();
        }
    }

    private void OnEnable()
    {
        _gunSubject.OnGunFired += UpdateBulletCount;
        _gunSubject.OnWeaponReloadStart += UpdateBulletCount;
        _gunSubject.OnWeaponReloadEnd += UpdateBulletCount;

        _gunSubject.OnWeaponModeSwitched += UpdateWeaponIcon;
        _bombSubject.OnBombThrowed += UpdateBombCount;


    }

    private void OnDisable()
    {
        _gunSubject.OnGunFired -= UpdateBulletCount;
        _gunSubject.OnWeaponReloadStart -= UpdateBulletCount;
        _gunSubject.OnWeaponReloadEnd -= UpdateBulletCount;

        _gunSubject.OnWeaponModeSwitched -= UpdateWeaponIcon;

        _bombSubject.OnBombThrowed -= UpdateBombCount;
    }

    private void Start()
    {
        InitializeField();
    }

    private void InitializeField()
    {
        UpdateWeaponIcon();
        UpdateBulletCount();
        UpdateBombCount();
    }

    private void UpdateBulletCount()
    {
        _bulletCountText.text = $"{_gunSubject.CurrentBulletCount} / {_gunSubject.MaxBulletCount}";
    }
    private void UpdateBombCount()
    {
        _bombCountText.text = $"ÆøÅº : {_bombSubject.CurrentBombCount}";
    }

    private void UpdateWeaponIcon()
    {
        _weaponIcon.sprite = _gunSubject.WeaponMode switch
        {
            WeaponMode.Normal => _normalWeaponIcon,
            WeaponMode.Sniper => _sniperWeaponIcon,
            _ => null
        };

    }
}

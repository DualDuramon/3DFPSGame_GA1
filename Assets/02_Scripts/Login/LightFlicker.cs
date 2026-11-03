using UnityEngine;

// 고장 난 작업등처럼 평소에는 미세하게 일렁이다가 가끔 몇 번 끊기듯 깜빡인다
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("일렁임")]
    [SerializeField] private float _noiseSpeed = 6f;
    [SerializeField] private float _noiseAmount = 0.15f;

    [Header("끊김")]
    [SerializeField] private Vector2 _blackoutInterval = new Vector2(2.5f, 6f);
    [SerializeField] private int _maxBlinkCount = 4;
    [SerializeField] private float _blinkDuration = 0.06f;
    [SerializeField] private float _blackoutIntensityRatio = 0.08f;

    [Header("같이 깜빡일 발광 렌더러 (선택)")]
    [SerializeField] private Renderer _emissiveRenderer;
    [SerializeField] private Color _emissionColor = new Color(1f, 0.75f, 0.45f);
    [SerializeField] private float _emissionIntensity = 4f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Light _light;
    private MaterialPropertyBlock _block;
    private float _baseIntensity;
    private float _seed;
    private float _nextBlackoutTime;
    private float _blinkEndTime;
    private int _blinkRemain;

    private void Awake()
    {
        _light = GetComponent<Light>();
        _block = new MaterialPropertyBlock();
        _baseIntensity = _light.intensity;
        _seed = Random.value * 100f;
        ScheduleBlackout();
    }

    private void Update()
    {
        float ratio = 1f - _noiseAmount + Mathf.PerlinNoise(_seed, Time.time * _noiseSpeed) * _noiseAmount;

        if (_blinkRemain > 0)
        {
            // 끊김 구간: 짝수 번째는 꺼지고 홀수 번째는 켜진다
            if (_blinkRemain % 2 == 0) ratio = _blackoutIntensityRatio;

            if (Time.time >= _blinkEndTime)
            {
                _blinkRemain--;
                _blinkEndTime = Time.time + _blinkDuration * Random.Range(0.5f, 1.5f);
            }
        }
        else if (Time.time >= _nextBlackoutTime)
        {
            _blinkRemain = Random.Range(1, _maxBlinkCount + 1) * 2;
            _blinkEndTime = Time.time + _blinkDuration;
            ScheduleBlackout();
        }

        _light.intensity = _baseIntensity * ratio;
        ApplyEmission(ratio);
    }

    private void ScheduleBlackout()
    {
        _nextBlackoutTime = Time.time + Random.Range(_blackoutInterval.x, _blackoutInterval.y);
    }

    private void ApplyEmission(float ratio)
    {
        if (_emissiveRenderer == null) return;

        _emissiveRenderer.GetPropertyBlock(_block);
        _block.SetColor(EmissionColorId, _emissionColor * (_emissionIntensity * ratio));
        _emissiveRenderer.SetPropertyBlock(_block);
    }
}

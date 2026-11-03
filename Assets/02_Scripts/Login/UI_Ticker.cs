using UnityEngine;

// 자식 텍스트를 왼쪽으로 흘려보내고, 화면 밖으로 나가면 오른쪽 끝에서 다시 들어오게 한다
public class UI_Ticker : MonoBehaviour
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private float _speed = 120f;

    private RectTransform _viewport;

    private void Awake()
    {
        _viewport = (RectTransform)transform;
    }

    private void Update()
    {
        Vector2 pos = _content.anchoredPosition;
        pos.x -= _speed * Time.unscaledDeltaTime;

        // 콘텐츠의 오른쪽 끝이 뷰포트 왼쪽을 지나면 뷰포트 오른쪽 끝으로 되돌린다
        if (pos.x + _content.rect.width < 0f)
        {
            pos.x = _viewport.rect.width;
        }

        _content.anchoredPosition = pos;
    }
}

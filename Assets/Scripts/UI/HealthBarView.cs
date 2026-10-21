using UnityEngine;
using UnityEngine.UI;

// HP바 그림 — Health 이벤트를 구독해서 그리기만 한다 (체력을 바꾸지 않는다)
[RequireComponent(typeof(Image))]
public class HealthBarView : MonoBehaviour
{
    [SerializeField] Health target;

    Image bar;

    void Awake() => bar = GetComponent<Image>();

    void OnEnable()
    {
        if (target != null) target.Changed += Show;
    }

    void OnDisable()
    {
        if (target != null) target.Changed -= Show;
    }

    void Start()
    {
        if (target != null) Show(target.Current, target.Max);
    }

    void Show(float current, float max) => bar.fillAmount = max > 0f ? current / max : 0f;
}

using System;
using UnityEngine;

// 체력 숫자와 사건(바뀜 · 쓰러짐)만 가진다.
// HP바 그리기나 승패 처리는 이 이벤트를 구독하는 쪽(HealthBarView · GameDirector)이 한다.
public class Health : MonoBehaviour
{
    [Tooltip("예전 HP바 fillAmount(0~1) 숫자를 그대로 쓰도록 1")]
    [SerializeField] float max = 1f;

    public float Max => max;
    public float Current { get; private set; }
    public bool IsDead => Current <= 0f;

    public event Action<float, float> Changed;  // (현재, 최대)
    public event Action<Health> Died;

    void Awake() => Current = max;

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        Current = Mathf.Max(0f, Current - amount);
        Changed?.Invoke(Current, max);
        if (IsDead) Died?.Invoke(this);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;

        Current = Mathf.Min(max, Current + amount);
        Changed?.Invoke(Current, max);
    }
}

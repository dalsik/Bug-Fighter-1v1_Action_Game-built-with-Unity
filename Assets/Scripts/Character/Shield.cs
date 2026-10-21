using UnityEngine;

// 쉴드 — 켜져 있는 동안 Hurtbox 가 들어오는 공격을 막는다
public class Shield : MonoBehaviour
{
    [SerializeField] GameObject effectPrefab;
    [SerializeField] float duration = 1f;
    [SerializeField] float cooldown = 5f;

    float activeUntil;
    float readyAt;

    public bool IsActive => Time.time < activeUntil;

    public bool TryRaise()
    {
        if (Time.time < readyAt) return false;

        activeUntil = Time.time + duration;
        readyAt = Time.time + cooldown;

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            effect.transform.SetParent(transform);
            Destroy(effect, duration);
        }
        return true;
    }
}

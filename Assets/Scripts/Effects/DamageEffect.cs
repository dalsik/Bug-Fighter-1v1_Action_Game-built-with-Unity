using UnityEngine;

// 피해 — 체력바(1 = 가득)에서 깎을 양
public class DamageEffect : MonoBehaviour, IEffect
{
    [SerializeField] float amount = 0.1f;

    public void Apply(HitInfo hit, CharacterRoot target) => target.Health.TakeDamage(amount);
}

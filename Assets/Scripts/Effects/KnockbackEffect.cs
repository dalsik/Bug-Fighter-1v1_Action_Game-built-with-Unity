using UnityEngine;

// 넉백 — 공격이 정한 방향으로, 살짝 위로 띄우며 민다
public class KnockbackEffect : MonoBehaviour, IEffect
{
    [SerializeField] float force = 10f;
    [SerializeField] float lift = 0.5f;

    public void Apply(HitInfo hit, CharacterRoot target)
    {
        Vector2 direction = new Vector2(hit.Direction, lift).normalized;
        target.Body.velocity = Vector2.zero;
        target.Body.AddForce(direction * force, ForceMode2D.Impulse);
    }
}

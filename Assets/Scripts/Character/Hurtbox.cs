using UnityEngine;

// 피격 창구 — "이 공격이 나에게 들어오나"를 정하는 곳은 여기 하나뿐.
// 같은 편 · 이미 쓰러짐 · 쉴드 중이면 막고, 아니면 공격이 건넨 효과를 하나씩 자기에게 건다.
[RequireComponent(typeof(CharacterRoot))]
public class Hurtbox : MonoBehaviour, IDamageable
{
    CharacterRoot owner;
    Shield shield;

    public Team Team => owner.Team;
    public Vector2 Position => owner.Position;

    void Awake()
    {
        owner = GetComponent<CharacterRoot>();
        shield = GetComponent<Shield>();
    }

    public void ReceiveHit(HitInfo hit)
    {
        if (hit.Attacker != null && hit.Attacker.Team == Team) return;  // 자기 자신 · 같은 편
        if (owner.Health.IsDead) return;
        if (shield != null && shield.IsActive) return;                  // 쉴드는 피해 · 넉백 · 경직을 전부 막는다

        for (int i = 0; i < hit.Effects.Count; i++)
            hit.Effects[i].Apply(hit, owner);
    }
}

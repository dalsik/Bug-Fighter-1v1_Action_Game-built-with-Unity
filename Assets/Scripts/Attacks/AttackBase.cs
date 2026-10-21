using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 모든 공격이 똑같이 밟는 절차: 쓸 수 있나(쿨다운 · 횟수) → 애니 → 실행(Perform).
// 공격마다 다른 건 Perform 한 칸뿐이다 — 자식 클래스는 그것만 채운다.
// 효과는 같은 오브젝트에 붙은 IEffect 컴포넌트들이다 (Add Component 로 추가 · 제거).
public abstract class AttackBase : MonoBehaviour, IAttack
{
    [SerializeField] AttackSlotType slot;
    [SerializeField] string animTrigger;
    [SerializeField] float cooldown;
    [Tooltip("판당 쓸 수 있는 횟수. 0 = 무제한 (필살기는 1)")]
    [SerializeField] int maxUses;

    // 몸통 콜라이더만 맞힌다 — 젤리 · 발사체 같은 트리거는 거른다
    static readonly ContactFilter2D BodiesOnly = new ContactFilter2D();

    readonly List<Collider2D> found = new List<Collider2D>(8);
    readonly List<IDamageable> alreadyHit = new List<IDamageable>(2);
    IEffect[] effects;
    float readyAt;
    int uses;

    public AttackSlotType Slot => slot;
    protected IReadOnlyList<IEffect> Effects => effects;

    protected virtual void Awake() => effects = GetComponents<IEffect>();

    public bool TryUse(CharacterRoot owner)
    {
        if (Time.time < readyAt) return false;
        if (maxUses > 0 && uses >= maxUses) return false;

        readyAt = Time.time + cooldown;
        uses++;
        if (owner.Animator != null && !string.IsNullOrEmpty(animTrigger))
            owner.Animator.SetTrigger(animTrigger);

        StartCoroutine(Perform(owner));
        return true;
    }

    protected abstract IEnumerator Perform(CharacterRoot owner);

    // ── 판정 도우미 — 걸린 대상에게 한 번씩만 효과를 건넨다 ──────────────────
    // pushDirection 이 0 이면 공격자에게서 멀어지는 쪽으로 민다.

    protected void BeginHits() => alreadyHit.Clear();

    protected void HitBox(CharacterRoot owner, Vector2 center, Vector2 size, float pushDirection)
    {
        Physics2D.OverlapBox(center, size, 0f, BodiesOnly, found);
        Deliver(owner, pushDirection);
    }

    protected void HitArea(CharacterRoot owner, Vector2 cornerA, Vector2 cornerB, float pushDirection)
    {
        Physics2D.OverlapArea(cornerA, cornerB, BodiesOnly, found);
        Deliver(owner, pushDirection);
    }

    protected void HitCollider(CharacterRoot owner, Collider2D area, float pushDirection)
    {
        area.OverlapCollider(BodiesOnly, found);
        Deliver(owner, pushDirection);
    }

    void Deliver(CharacterRoot owner, float pushDirection)
    {
        for (int i = 0; i < found.Count; i++)
        {
            IDamageable target = found[i].GetComponentInParent<IDamageable>();
            if (target == null || alreadyHit.Contains(target)) continue;

            alreadyHit.Add(target);
            float push = pushDirection != 0f ? pushDirection : Mathf.Sign(target.Position.x - owner.Position.x);
            target.ReceiveHit(new HitInfo(owner, push, effects));
        }
    }
}

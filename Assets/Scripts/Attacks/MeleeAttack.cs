using System.Collections;
using UnityEngine;

// 앞쪽 박스 판정. 필요하면 먼저 앞으로 튀어 나간다(돌진) — 개미 강근접 (20, 8), 벌 강근접 (15, 0)
public class MeleeAttack : AttackBase
{
    [SerializeField] Vector2 boxSize = new Vector2(0.7f, 1f);
    [Tooltip("캐릭터 위치에서 박스 중심까지 — x 는 바라보는 방향 기준")]
    [SerializeField] Vector2 boxOffset = new Vector2(0.35f, 0f);
    [Tooltip("판정 전에 주는 순간 힘 — x 는 바라보는 방향 기준. (0, 0)이면 돌진 없음")]
    [SerializeField] Vector2 lungeImpulse;
    [SerializeField] float hitDelay;

    protected override IEnumerator Perform(CharacterRoot owner)
    {
        float facing = owner.Facing;
        if (lungeImpulse != Vector2.zero)
        {
            owner.Body.velocity = Vector2.zero;
            owner.Body.AddForce(new Vector2(lungeImpulse.x * facing, lungeImpulse.y), ForceMode2D.Impulse);
        }
        if (hitDelay > 0f) yield return new WaitForSeconds(hitDelay);

        Vector2 center = owner.Position + new Vector2(boxOffset.x * facing, boxOffset.y);
        BeginHits();
        HitBox(owner, center, boxSize, facing);

        Debug.DrawLine(center - boxSize * 0.5f, center + boxSize * 0.5f, Color.red, 0.1f);
    }
}

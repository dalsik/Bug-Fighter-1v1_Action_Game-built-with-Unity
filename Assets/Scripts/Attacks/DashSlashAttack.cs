using System.Collections;
using UnityEngine;

// 개미 필살기: 준비 → 앞으로 순간이동 → 지나온 구간 전체를 판정하고 베기 이펙트를 남긴다
public class DashSlashAttack : AttackBase
{
    [SerializeField] GameObject slashEffectPrefab;
    [SerializeField] float windup = 0.5f;
    [SerializeField] float distance = 5f;
    [SerializeField] float hitHeight = 1f;
    [SerializeField] Vector2 effectScale = new Vector2(2.5f, 2f);
    [SerializeField] float effectDuration = 1f;

    protected override IEnumerator Perform(CharacterRoot owner)
    {
        yield return new WaitForSeconds(windup);

        float facing = owner.Facing;
        Transform body = owner.transform;
        float startX = body.position.x;
        body.position += new Vector3(distance * facing, 0f, 0f);
        float endX = body.position.x;
        float y = body.position.y;

        if (slashEffectPrefab != null)
        {
            Vector3 middle = new Vector3((startX + endX) * 0.5f, y, 0f);
            GameObject slash = Instantiate(slashEffectPrefab, middle, Quaternion.identity);
            slash.transform.localScale = new Vector3(facing * effectScale.x, effectScale.y, 1f);
            Destroy(slash, effectDuration);
        }

        // 지나온 구간에 있던 상대는 개미에게서 멀어지는 쪽으로 밀린다
        BeginHits();
        HitArea(owner,
            new Vector2(Mathf.Min(startX, endX), y - hitHeight * 0.5f),
            new Vector2(Mathf.Max(startX, endX), y + hitHeight * 0.5f),
            0f);
    }
}

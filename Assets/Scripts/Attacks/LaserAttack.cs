using System.Collections;
using UnityEngine;

// 벌 필살기: 제자리에 고정된 채 레이저를 띄우고, 충전이 끝난 뒤에만 판정한다
public class LaserAttack : AttackBase
{
    [SerializeField] GameObject laserPrefab;
    [Tooltip("캐릭터 위치에서 레이저 중심까지 — x 는 바라보는 방향 기준")]
    [SerializeField] Vector2 spawnOffset = new Vector2(10f, -0.5f);
    [SerializeField] Vector2 laserScale = new Vector2(2f, 1f);
    [Tooltip("고정된 채 기를 모으는 시간 — 이때는 맞지 않는다")]
    [SerializeField] float chargeTime = 1f;
    [Tooltip("판정이 살아 있는 시간 — 한 대상은 한 번만 맞는다")]
    [SerializeField] float fireTime = 1f;

    protected override IEnumerator Perform(CharacterRoot owner)
    {
        float facing = owner.Facing;
        Vector2 pinned = owner.Position;
        float total = chargeTime + fireTime;
        owner.Status.Lock(total);

        Vector2 position = pinned + new Vector2(spawnOffset.x * facing, spawnOffset.y);
        GameObject laser = Instantiate(laserPrefab, position, Quaternion.identity);
        laser.transform.localScale = new Vector3(laserScale.x * facing, laserScale.y, 1f);

        Collider2D area = laser.GetComponent<Collider2D>();
        if (area == null) Debug.LogError($"{laserPrefab.name} 에 Collider2D 가 없어 레이저가 맞지 않습니다", laserPrefab);
        else area.isTrigger = true;     // 몸으로 밀어내지 않게

        BeginHits();
        for (float elapsed = 0f; elapsed < total; elapsed += Time.fixedDeltaTime)
        {
            owner.Body.velocity = Vector2.zero;     // 쏘는 동안 공중에서도 그 자리에 고정
            owner.Body.position = pinned;
            if (area != null && elapsed >= chargeTime) HitCollider(owner, area, facing);
            yield return new WaitForFixedUpdate();
        }

        Destroy(laser);
    }
}

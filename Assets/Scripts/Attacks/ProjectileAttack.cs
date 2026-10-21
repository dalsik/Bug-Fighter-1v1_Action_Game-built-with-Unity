using System.Collections;
using UnityEngine;

// 발사체를 쏜다. 발 수 = 발사 위치 개수 (벌 강원거리는 3발)
public class ProjectileAttack : AttackBase
{
    [SerializeField] Projectile prefab;
    [SerializeField] float speed = 5f;
    [SerializeField] float maxDistance = 5f;
    [Tooltip("캐릭터 위치에서 발사 위치까지 — x 는 바라보는 방향 기준. 하나에 한 발")]
    [SerializeField] Vector2[] spawnOffsets = { Vector2.zero };

    protected override IEnumerator Perform(CharacterRoot owner)
    {
        float facing = owner.Facing;
        foreach (Vector2 offset in spawnOffsets)
        {
            Vector2 position = owner.Position + new Vector2(offset.x * facing, offset.y);
            Projectile shot = Instantiate(prefab, position, Quaternion.identity);
            shot.Launch(owner, Effects, facing, speed, maxDistance);
        }
        yield break;
    }
}

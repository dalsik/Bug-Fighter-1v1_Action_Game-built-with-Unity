using System.Collections.Generic;
using UnityEngine;

// 날아가다 상대 몸에 닿으면, 쏜 공격의 효과를 건네고 사라진다.
// 피해량 같은 숫자는 모른다 — 효과는 쏜 공격 오브젝트에 붙어 있다.
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] float visualScale = 1f;
    [Tooltip("그림이 왼쪽을 보고 그려져 있으면 켠다 (개미 침)")]
    [SerializeField] bool spriteFacesLeft;

    CharacterRoot owner;
    IReadOnlyList<IEffect> effects;
    float direction;
    float maxDistance;
    Vector2 start;

    public void Launch(CharacterRoot shooter, IReadOnlyList<IEffect> hitEffects, float dir, float speed, float range)
    {
        owner = shooter;
        effects = hitEffects;
        direction = dir;
        maxDistance = range;
        start = transform.position;

        GetComponent<Rigidbody2D>().velocity = new Vector2(dir * speed, 0f);
        float scaleX = dir * (spriteFacesLeft ? -1f : 1f) * visualScale;
        transform.localScale = new Vector3(scaleX, visualScale, visualScale);
    }

    void Update()
    {
        if (Vector2.Distance(start, transform.position) > maxDistance) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || target.Team == owner.Team) return;   // 쏜 사람 몸은 그냥 지나간다

        target.ReceiveHit(new HitInfo(owner, direction, effects));
        Destroy(gameObject);
    }
}

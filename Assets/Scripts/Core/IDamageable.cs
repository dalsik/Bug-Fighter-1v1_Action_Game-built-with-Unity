using UnityEngine;

// 맞는 쪽 창구 — 공격은 상대에 대해 이것만 안다
public interface IDamageable
{
    Team Team { get; }
    Vector2 Position { get; }
    void ReceiveHit(HitInfo hit);
}

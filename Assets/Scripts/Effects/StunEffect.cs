using UnityEngine;

// 경직 — 이 시간 동안 맞은 캐릭터는 입력을 받지 않는다 (시간은 맞은 쪽 StatusEffects 가 잰다)
public class StunEffect : MonoBehaviour, IEffect
{
    [SerializeField] float duration = 0.2f;

    public void Apply(HitInfo hit, CharacterRoot target) => target.Status.Stun(duration);
}

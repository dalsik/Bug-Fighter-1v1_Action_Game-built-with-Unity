using UnityEngine;

// 시간이 걸리는 상태는 맞은 쪽이 직접 잰다.
// 예전에는 발사체가 "0.2초 뒤 풀기" 코루틴을 들고 있다가 자기가 사라지면 풀기도 같이 사라졌다.
public class StatusEffects : MonoBehaviour
{
    float stunnedUntil;     // 맞아서 조작 불가
    float lockedUntil;      // 자기 기술 때문에 조작 불가 (예: 레이저 쏘는 동안)

    public bool IsStunned => Time.time < stunnedUntil;
    public bool CanAct => !IsStunned && Time.time >= lockedUntil;

    public void Stun(float duration) => stunnedUntil = Mathf.Max(stunnedUntil, Time.time + duration);
    public void Lock(float duration) => lockedUntil = Mathf.Max(lockedUntil, Time.time + duration);
}

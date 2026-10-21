using UnityEngine;

// 키 배치 한 벌 (1P용 · 2P용) — 캐릭터가 아니라 플레이어에게 붙는다.
// 개미 프리팹에 1P 배치를 꽂으면 1P가 개미를 쓴다.
[CreateAssetMenu(menuName = "Bug Fighter/Input Profile", fileName = "InputProfile")]
public class InputProfile : ScriptableObject
{
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode jump = KeyCode.W;
    public KeyCode shield = KeyCode.S;
    public float doubleTapWindow = 0.3f;    // 같은 방향을 이 시간 안에 두 번 누르면 대시
    public AttackKeyBinding[] attacks = System.Array.Empty<AttackKeyBinding>();
}

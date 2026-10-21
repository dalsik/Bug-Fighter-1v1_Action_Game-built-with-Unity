// 공격 "자리" 이름 — 키도 아니고 공격 클래스도 아니다.
// 키 → 자리 는 InputProfile(플레이어별)이, 자리 → 공격 은 공격 컴포넌트(캐릭터별)가 정한다.
public enum AttackSlotType
{
    NormalMelee,    // 일반 근접
    StrongMelee,    // 강 근접
    NormalRanged,   // 일반 원거리
    StrongRanged,   // 강 원거리
    Ultimate        // 필살기
}

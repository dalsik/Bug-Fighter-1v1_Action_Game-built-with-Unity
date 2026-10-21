using System.Collections.Generic;
using UnityEngine;

// 캐릭터 조립 루트 — 자기 몸의 부품을 모으고, 입력이 시키는 일을 부품에 넘긴다.
// 규칙은 하나만 든다: "지금 행동해도 되나"(경직 · 기술 고정 중엔 입력을 받지 않는다).
// 키도, 공격이 무엇을 하는지도 모른다.
[RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(StatusEffects))]
[RequireComponent(typeof(Mover))]
public class CharacterRoot : MonoBehaviour
{
    [SerializeField] Team team;

    readonly Dictionary<AttackSlotType, IAttack> attacks = new Dictionary<AttackSlotType, IAttack>();
    ICharacterInput input;
    Jumper jumper;
    Shield shield;

    public Team Team => team;
    public Rigidbody2D Body { get; private set; }
    public Animator Animator { get; private set; }
    public Health Health { get; private set; }
    public StatusEffects Status { get; private set; }
    public Mover Mover { get; private set; }
    public Vector2 Position => Body.position;
    public float Facing => Mover.Facing;

    void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        Health = GetComponent<Health>();
        Status = GetComponent<StatusEffects>();
        Mover = GetComponent<Mover>();
        jumper = GetComponent<Jumper>();
        shield = GetComponent<Shield>();
        input = GetComponent<ICharacterInput>();

        // 공격 자식들이 자기 자리를 알고 있다 — 인스펙터에서 끌어다 놓을 필요가 없다
        foreach (IAttack attack in GetComponentsInChildren<IAttack>())
        {
            if (attacks.ContainsKey(attack.Slot))
            {
                Debug.LogError($"{name}: '{attack.Slot}' 자리에 공격이 둘 붙어 있습니다", this);
                continue;
            }
            attacks.Add(attack.Slot, attack);
        }
    }

    // 키보드 대신 AI · 리플레이 · 테스트 입력을 꽂을 때
    public void SetInput(ICharacterInput source) => input = source;

    public bool HasAttack(AttackSlotType slot) => attacks.ContainsKey(slot);

    void Update()
    {
        if (input == null || !Status.CanAct) return; // 입력이 없거나 행동할 수 없는 상태라면

        if (input.DashDirection != 0) Mover.TryDash(input.DashDirection);
        if (jumper != null && input.JumpPressed) jumper.TryJump();
        if (shield != null && input.ShieldPressed) shield.TryRaise();

        foreach (KeyValuePair<AttackSlotType, IAttack> pair in attacks)
            if (input.AttackPressed(pair.Key))
                pair.Value.TryUse(this);

        Mover.Move(input.Horizontal);
    }
}

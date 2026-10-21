using UnityEngine;

// 키보드로 ICharacterInput 을 채운다 — 키를 아는 곳은 여기 하나뿐.
// CharacterRoot 보다 먼저 돌아서, 같은 프레임에 읽은 값을 넘긴다.
[DefaultExecutionOrder(-100)]
public class KeyboardInput : MonoBehaviour, ICharacterInput
{
    [SerializeField] InputProfile keys;

    static readonly int SlotCount = System.Enum.GetValues(typeof(AttackSlotType)).Length;

    readonly bool[] attackPressed = new bool[SlotCount];
    float lastLeftTap = float.NegativeInfinity;
    float lastRightTap = float.NegativeInfinity;

    public float Horizontal { get; private set; }
    public int DashDirection { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool ShieldPressed { get; private set; }

    public bool AttackPressed(AttackSlotType slot) => attackPressed[(int)slot];

    void Update()
    {
        if (keys == null) return;

        float horizontal = 0f;
        if (Input.GetKey(keys.left)) horizontal -= 1f;
        if (Input.GetKey(keys.right)) horizontal += 1f;
        Horizontal = horizontal;

        DashDirection = 0;
        if (Input.GetKeyDown(keys.left) && IsDoubleTap(ref lastLeftTap)) DashDirection = -1;
        if (Input.GetKeyDown(keys.right) && IsDoubleTap(ref lastRightTap)) DashDirection = 1;

        JumpPressed = Input.GetKeyDown(keys.jump);
        ShieldPressed = Input.GetKeyDown(keys.shield);

        System.Array.Clear(attackPressed, 0, attackPressed.Length);
        foreach (AttackKeyBinding binding in keys.attacks)
            if (Input.GetKeyDown(binding.key))
                attackPressed[(int)binding.slot] = true;
    }

    bool IsDoubleTap(ref float lastTap)
    {
        bool isDouble = Time.time - lastTap < keys.doubleTapWindow;
        lastTap = Time.time;
        return isDouble;
    }
}

using UnityEngine;

// 점프 — 2단 · 3단은 다른 동작이 아니라 횟수(maxJumps) 차이일 뿐이다 (개미 1, 벌 3)
[RequireComponent(typeof(Rigidbody2D))]
public class Jumper : MonoBehaviour
{
    [SerializeField] float jumpForce = 700f;
    [SerializeField] int maxJumps = 1;
    [Tooltip("점프 직전에 떨어지던 속도를 지운다 — 공중 점프 높이가 일정해진다 (벌)")]
    [SerializeField] bool resetFallSpeed;

    // 막 뛴 직후에는 아직 발이 바닥에 붙어 있어서, 이 시간 동안은 착지로 치지 않는다
    const float GroundIgnoreAfterJump = 0.1f;

    Rigidbody2D body;
    ContactFilter2D groundFilter;
    int used;
    float ignoreGroundUntil;

    public int JumpsLeft => maxJumps - used;
    public bool IsGrounded => body.IsTouching(groundFilter);

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        groundFilter = new ContactFilter2D();
        groundFilter.SetNormalAngle(45f, 135f);     // 발밑(위를 향한 면)에 닿은 것만 바닥
    }

    void Update()
    {
        if (Time.time < ignoreGroundUntil) return;

        if (IsGrounded) used = 0;                   // 착지할 때마다 횟수를 되돌린다
        else if (used == 0) used = 1;               // 걸어서 떨어지면 땅 점프는 쓴 것으로 친다
    }

    public bool TryJump()
    {
        if (used >= maxJumps) return false;

        used++;
        ignoreGroundUntil = Time.time + GroundIgnoreAfterJump;
        if (resetFallSpeed) body.velocity = new Vector2(body.velocity.x, 0.1f);
        body.AddForce(Vector2.up * jumpForce);
        return true;
    }
}

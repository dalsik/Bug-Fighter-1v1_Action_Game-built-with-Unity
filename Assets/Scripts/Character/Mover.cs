using UnityEngine;

// 걷기 · 대시 · 바라보는 방향.
// 바라보는 방향(Facing)은 여기 한 곳에서만 정한다 — 공격들은 이 값을 읽기만 한다.
[RequireComponent(typeof(Rigidbody2D))]
public class Mover : MonoBehaviour
{
    [SerializeField] float walkForce = 30f;
    [SerializeField] float maxWalkSpeed = 4f;
    [SerializeField] float dashForce = 15f;
    [SerializeField] float dashDuration = 0.1f;
    [Tooltip("오른쪽을 볼 때 localScale.x 의 부호. 개미는 Y축으로 180° 돌아가 있어서 -1")]
    [SerializeField] float spriteFacingSign = 1f;

    Rigidbody2D body;
    Animator animator;
    float dashingUntil;

    public float Facing { get; private set; } = 1f;    // 월드 기준: +1 오른쪽, -1 왼쪽
    public bool IsDashing => Time.time < dashingUntil;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Facing = Mathf.Sign(transform.localScale.x) * spriteFacingSign;
    }

    public void Move(float horizontal)
    {
        if (Mathf.Abs(body.velocity.x) < maxWalkSpeed)
            body.AddForce(Vector2.right * horizontal * walkForce);

        if (horizontal != 0f)
        {
            Face(Mathf.Sign(horizontal));
            if (animator != null) animator.SetTrigger("WalkTrigger");
        }
        if (animator != null && body.velocity.x != 0f) animator.speed = 2f;
    }

    public bool TryDash(float direction)
    {
        if (IsDashing) return false;

        body.velocity = Vector2.zero;
        body.AddForce(Vector2.right * direction * dashForce, ForceMode2D.Impulse);
        dashingUntil = Time.time + dashDuration;
        return true;
    }

    public void Face(float direction)
    {
        Facing = direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction * spriteFacingSign;
        transform.localScale = scale;
    }
}

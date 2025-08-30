using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitController1 : MonoBehaviour
{
    public float knockbackForce = 5.0f; // 넉백의 강도
    public float knockbackDuration = 0.2f; // 넉백 지속 시간

    public void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject director = GameObject.Find("GameDirector");
        float direction = transform.localScale.x > 0 ? -1f : 1f;
        // 충돌한 대상이 상대 플레이어인지 태그로 확인
        if (collision.CompareTag("Player2")) // Player2가 맞았을 때
        {
            director.GetComponent<GameDirector>().DecreaseHP2(0.025f); // 상대 캐릭터의 HP 감소 처리
            ApplyKnockback(collision.gameObject, direction); // 넉백 효과 추가 (-1은 왼쪽 방향)
            Destroy(gameObject); // 발사체 삭제
        }
        else if (collision.CompareTag("Player1")) // Player1이 맞았을 때
        {
            director.GetComponent<GameDirector>().DecreaseHP1(0.025f); // 상대 캐릭터의 HP 감소 처리
            ApplyKnockback(collision.gameObject, direction); // 넉백 효과 추가 (1은 오른쪽 방향)
            Destroy(gameObject); // 발사체 삭제
        }
    }

    void ApplyKnockback(GameObject target, float direction)
    {
        Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();

        if (targetRigidBody != null)
        {
            // 넉백 방향 설정 (약간 위로 밀리는 효과 포함)
            Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

            // 기존 속도 초기화 후 넉백 적용
            targetRigidBody.velocity = Vector2.zero;
            targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // 넉백 중 플레이어 컨트롤 제한
            StartCoroutine(DisableControlForDuration(target, knockbackDuration));
        }
    }

    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.enabled = false; // 플레이어 컨트롤 비활성화
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // 플레이어 컨트롤 재활성화
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSpitController : MonoBehaviour
{
    private bool isCollided = false; // �浹 ���� Ȯ�ο� �÷���
    public float knockbackForce = 8.0f; // �˹��� ����
    public float knockbackDuration = 0.3f; // �˹� ���� �ð�

    void Update()
    {
        // �浹�� �Ŀ��� ������Ʈ�� ��� ������ ��
        if (isCollided)
        {
            // �浹 �� �ð��� ȿ���� �߰��ϰų� ������Ʈ ����
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        // �浹�� ����� ��� �÷��̾����� �±׷� Ȯ��
        if (collision.CompareTag("Player2")) // ��� ĳ���Ϳ� "Player2" �±� ����
        {
            HandleCollision("Player2", collision, direction); // -1�� ���� ���� �˹�
        }
        else if (collision.CompareTag("Player1")) // ��� ĳ���Ϳ� "Player1" �±� ����
        {
            HandleCollision("Player1", collision, direction); // 1�� ������ ���� �˹�
        }
    }

    private void HandleCollision(string playerTag, Collider2D collision, float direction)
    {
        // GameDirector�� ã�Ƽ� ������ ó��
        GameObject director = GameObject.Find("GameDirector");
        if (director != null)
        {
            GameDirector gameDirector = director.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                if (playerTag == "Player2")
                {
                    gameDirector.DecreaseHP2(0.05f); // Player2 HP ����
                }
                else if (playerTag == "Player1")
                {
                    gameDirector.DecreaseHP1(0.05f); // Player1 HP ����
                }
            }
        }

        // �浹 �÷��� Ȱ��ȭ
        isCollided = true;
        // �˹� ȿ�� �߰�
        Rigidbody2D targetRigidBody = collision.GetComponent<Rigidbody2D>();
        if (targetRigidBody != null)
        {
            ApplyKnockback(targetRigidBody, direction);
        }

        // 3�� �� ������Ʈ ����
        StartCoroutine(DestroyAfterDelay());
    }

    private void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        // �˹� ���� ���� (�ణ ���� �и��� ȿ�� ����)
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

        // ���� �ӵ� �ʱ�ȭ �� �˹� ����
        targetRigidBody.velocity = Vector2.zero;
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // �˹� �� �÷��̾� ��Ʈ�� ����
        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    private IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.enabled = false; // �÷��̾� ��Ʈ�� ��Ȱ��ȭ
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // �÷��̾� ��Ʈ�� ��Ȱ��ȭ
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3.0f); // 3�� ��ٸ�
        Destroy(gameObject); // ������Ʈ ����
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpitController1 : MonoBehaviour
{
    public float knockbackForce = 5.0f; // �˹��� ����
    public float knockbackDuration = 0.2f; // �˹� ���� �ð�

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(gameObject.tag)) return; // 자신과 같은 태그는 무시
        
        GameObject director = GameObject.Find("GameDirector");
        float direction = transform.localScale.x > 0 ? -1f : 1f;
        // �浹�� ����� ��� �÷��̾����� �±׷� Ȯ��
        if (collision.CompareTag("Player2")) // Player2�� �¾��� ��
        {
            director.GetComponent<GameDirector>().DecreaseHP2(0.025f); // ��� ĳ������ HP ���� ó��
            ApplyKnockback(collision.gameObject, direction); // �˹� ȿ�� �߰� (-1�� ���� ����)
            Destroy(gameObject); // �߻�ü ����
        }
        else if (collision.CompareTag("Player1")) // Player1�� �¾��� ��
        {
            director.GetComponent<GameDirector>().DecreaseHP1(0.025f); // ��� ĳ������ HP ���� ó��
            ApplyKnockback(collision.gameObject, direction); // �˹� ȿ�� �߰� (1�� ������ ����)
            Destroy(gameObject); // �߻�ü ����
        }
    }

    void ApplyKnockback(GameObject target, float direction)
    {
        Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();

        if (targetRigidBody != null)
        {
            // �˹� ���� ���� (�ణ ���� �и��� ȿ�� ����)
            Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

            // ���� �ӵ� �ʱ�ȭ �� �˹� ����
            targetRigidBody.velocity = Vector2.zero;
            targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // �˹� �� �÷��̾� ��Ʈ�� ����
            StartCoroutine(DisableControlForDuration(target, knockbackDuration));
        }
    }

    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        AntController controller = target.GetComponent<AntController>();

        if (controller != null)
        {
            controller.enabled = false; // �÷��̾� ��Ʈ�� ��Ȱ��ȭ
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // �÷��̾� ��Ʈ�� ��Ȱ��ȭ
        }
    }
}

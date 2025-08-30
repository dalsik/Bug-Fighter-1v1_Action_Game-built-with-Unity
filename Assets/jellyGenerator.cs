using UnityEngine;
using System.Collections;

public class jellyGenerator : MonoBehaviour
{
    public GameObject jellyPrefab;
    public float spawnInterval = 30f; // ���� ���� ����
    public int maxJellies = 10;        // ���ÿ� ������ �� �ִ� ���� ��

    private int currentJellies = 0;   // ���� ȭ�鿡 �ִ� ���� ��

    void Start()
    {
        StartCoroutine(SpawnJelly());
    }

    IEnumerator SpawnJelly()
    {
        while (true)
        {
            // ���� ���� ������ maxJellies���� ������ �� ���� ����
            if (currentJellies < maxJellies)
            {
                float randomX = Random.Range(-9.5f, 9.5f);
                Vector3 spawnPosition = new Vector3(randomX, 5.5f, 0);
                Instantiate(jellyPrefab, spawnPosition, Quaternion.identity);

                currentJellies++; // ���� �� ����
            }

            // ���� �������� ���
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void JellyDestroyed()
    {
        // ������ �ı��Ǹ� ���� �� ����
        currentJellies = Mathf.Max(currentJellies - 1, 0);
        maxJellies--;
    }
}

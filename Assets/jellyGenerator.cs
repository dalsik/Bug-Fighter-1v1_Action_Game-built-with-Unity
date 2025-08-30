using UnityEngine;
using System.Collections;

public class jellyGenerator : MonoBehaviour
{
    public GameObject jellyPrefab;
    public float spawnInterval = 30f; // 젤리 생성 간격
    public int maxJellies = 10;        // 동시에 존재할 수 있는 젤리 수

    private int currentJellies = 0;   // 현재 화면에 있는 젤리 수

    void Start()
    {
        StartCoroutine(SpawnJelly());
    }

    IEnumerator SpawnJelly()
    {
        while (true)
        {
            // 현재 젤리 개수가 maxJellies보다 작으면 새 젤리 생성
            if (currentJellies < maxJellies)
            {
                float randomX = Random.Range(-9.5f, 9.5f);
                Vector3 spawnPosition = new Vector3(randomX, 5.5f, 0);
                Instantiate(jellyPrefab, spawnPosition, Quaternion.identity);

                currentJellies++; // 젤리 수 증가
            }

            // 다음 생성까지 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void JellyDestroyed()
    {
        // 젤리가 파괴되면 젤리 수 감소
        currentJellies = Mathf.Max(currentJellies - 1, 0);
        maxJellies--;
    }
}

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 10;

    public void AddHealth(int amount)
    {
        health += amount;
        Debug.Log(gameObject.name + " Health: " + health);
    }
}
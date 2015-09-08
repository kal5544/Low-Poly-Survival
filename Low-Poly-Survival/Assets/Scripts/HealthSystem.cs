using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour {

    public int health;
    public int stamina;
    public int staminaTime;
    public int thirst;
    public int thirstTime;
    public int hunger;
    public int hungerTime;

    public bool running;
    bool boolStam;
    bool boolThirst;
    bool boolHunger;

    void Update()
    {
        if (!boolStam && stamina != 0) StartCoroutine(staminaDecrease());
        if (!boolThirst && thirst != 0) StartCoroutine(thirstDecrease());
        if (!boolHunger && hunger != 0) StartCoroutine(hungerDecrease());
    }
    IEnumerator staminaDecrease()
    {
        boolStam = true;
        yield return new WaitForSeconds(staminaTime);
        stamina --;
        boolStam = false;
    }
    IEnumerator thirstDecrease()
    {
        boolThirst = true;
        yield return new WaitForSeconds(thirstTime);
        thirst--;
        boolThirst = false;
    }
    IEnumerator hungerDecrease()
    {
        boolHunger = true;
        yield return new WaitForSeconds(hungerTime);
        hunger--;
        boolHunger = false;
    }
    void loseHealth(int amountToLose)
    {
        if (health != 0)
        {
            health -= amountToLose;

            if (health < 0)
            {
                health = 0;
            }
        }
    }
    
	
}

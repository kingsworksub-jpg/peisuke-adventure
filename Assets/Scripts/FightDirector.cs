using System.Collections;
using UnityEngine;

public class FightDirector : MonoBehaviour
{
    public Animator fighterA;
    public Animator fighterB;
    public int rounds = 3;
    public float beatDelay = 0.9f;
    public float startDelay = 0.6f;

    void Start()
    {
        StartCoroutine(RunFight());
    }

    IEnumerator RunFight()
    {
        yield return new WaitForSeconds(startDelay);
        for (int i = 0; i < rounds; i++)
        {
            var attacker = (i % 2 == 0) ? fighterA : fighterB;
            var defender = (i % 2 == 0) ? fighterB : fighterA;

            attacker.SetTrigger("Attack");
            yield return new WaitForSeconds(0.28f);
            defender.SetTrigger("Hit");
            yield return new WaitForSeconds(beatDelay - 0.28f);
        }
    }
}

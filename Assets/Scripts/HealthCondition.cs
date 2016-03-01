using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthCondition : MonoBehaviour
{
    public enum Condition
    {
        Regeneration,
        Degenration,
        None
    };

    public Condition conditionState = Condition.None;//Default None.
    [Range(0f, 20f)]
    public float duration;
    [Range(0f, 50f)]
    public float amount;
    PlayerStats playerStats;

    // Use this for initialization
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    /// <summary>
    /// Receive a psuedo random condition.
    /// </summary>
    public void RandomCondition()
    {
        System.Array values = Condition.GetValues(typeof(Condition));
        System.Random random = new System.Random();
        conditionState = (Condition)values.GetValue(random.Next(values.Length));
        while (conditionState == Condition.None)
        {
            conditionState = (Condition)values.GetValue(random.Next(values.Length));
        }
        if (conditionState == Condition.Regeneration)
            StartCoroutine(Regenerate(this.amount, this.duration));
        if (conditionState == Condition.Degenration)
            StartCoroutine(Degenerate(this.amount, this.duration));
    }

    /// <summary>
    /// Degenrate health over time.
    /// </summary>
    /// <param name="degenerationAmount"></param>
    /// <param name="duration"></param>
    IEnumerator Degenerate(float degenerationAmount, float duration)
    {
        float countDown = duration;
        while (countDown > 0f)
        {
            yield return new WaitForSeconds(1.0f);
            playerStats.CmdTakeDmg((degenerationAmount / duration));
            countDown--;
        }
    }

    /// <summary>
    /// Regenrate health over time.
    /// </summary>
    /// <param name="regenerationAmount"></param>
    /// <param name="duration"></param>
    IEnumerator Regenerate(float regenerationAmount, float duration)
    {
        float countDown = duration;
        while (countDown > 0f)
        {
            yield return new WaitForSeconds(1.0f);
            playerStats.CmdHealing((regenerationAmount / duration));
            countDown--;
        }
    }
}

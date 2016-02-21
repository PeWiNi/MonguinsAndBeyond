using UnityEngine;
using System.Collections;

public class Camouflage : MonoBehaviour
{
    public float duration = 0f;
    public float durationInitialValue = 5f;
    public float visibilityRangeRadius = 20f;// This should be affected based on the values of the Role selected (Supportor, Defender, Attacker).
    public int durationPlantValue = 5;//Any additional plant adds 5sec to the 'duration'.
    public int amountOfPlants = 2;//Should be in a Items Class or something.
    public int plantsRequirement = 2;//The required number of plants before you can use camouflage.
    public bool isCamouflaged = false;
    public bool hasMovedFromCamouflagePoint = false;
    public bool isPartlySpotted = false;

    Vector3 camouflagePosition;
    Vector3 playerCurrentPosition;

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("UpdateVisibilityState", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        //Check whether the Player wants to become camouflaged or not.
        if (Input.GetKeyDown(KeyCode.C) && amountOfPlants >= plantsRequirement)
        {
            BeginCamouflage();
        }
        //Update Player Position.
        if (playerCurrentPosition != gameObject.transform.position)
        {
            playerCurrentPosition = gameObject.transform.position;
        }
        //Should Check whether the Player has moved or used and Ability.
        if (camouflagePosition != playerCurrentPosition && !hasMovedFromCamouflagePoint)
        {
            hasMovedFromCamouflagePoint = true;
            StartCoroutine(CamouflageDuration());
        }
    }

    /// <summary>
    /// Camouflage the Player.
    /// </summary>
    void BeginCamouflage()
    {
        isCamouflaged = true;//The Player is now camouflaged.
        duration = durationInitialValue + (durationInitialValue * amountOfPlants);//Set the duration.
        amountOfPlants = 0;//Consume plants.
        camouflagePosition = playerCurrentPosition;//Set the current player position to be the camouflage position.
        Hide();
    }

    /// <summary>
    /// Hide the Player.
    /// </summary>
    private void Hide()
    {
        Color colorOrigin = gameObject.GetComponent<Renderer>().material.color;
        Color newColor = new Color(colorOrigin.r, colorOrigin.g, colorOrigin.b, 0f);
        gameObject.GetComponent<Renderer>().material.color = newColor;
    }

    /// <summary>
    /// Make the Player appear again.
    /// </summary>
    void Appear()
    {
        Color colorOrigin = gameObject.GetComponent<Renderer>().material.color;
        Color newColor = new Color(colorOrigin.r, colorOrigin.g, colorOrigin.b, 1f);
        gameObject.GetComponent<Renderer>().material.color = newColor;
    }

    /// <summary>
    /// The Player will be 'almost' spotted resulting in being more vivisble.
    /// </summary>
    /// <returns></returns>
    IEnumerator PartlySpotted()
    {
        float partlySpottedValue = 0.8f;
        float visibilityValue = 0.2f;
        while (partlySpottedValue > visibilityValue)
        {
            Color colorOrigin = gameObject.GetComponent<Renderer>().material.color;
            Color newColor = new Color(colorOrigin.r, colorOrigin.g, colorOrigin.b, partlySpottedValue);
            gameObject.GetComponent<Renderer>().material.color = newColor;
            yield return new WaitForSeconds(0.2f);
            partlySpottedValue -= 0.1f;
        }
    }

    /// <summary>
    /// Start the camouflage duration.
    /// When it ends the Player will appear again.
    /// </summary>
    /// <returns></returns>
    IEnumerator CamouflageDuration()
    {
        while (duration > 0f)
        {
            yield return new WaitForSeconds(1.0f);
            duration--;
        }
        Appear();
        isCamouflaged = false;
        hasMovedFromCamouflagePoint = false;
        yield return null;
    }

    void UpdateVisibilityState()
    {
        if (isCamouflaged)
        {
            Collider[] hitColliders = Physics.OverlapSphere(playerCurrentPosition, visibilityRangeRadius);
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (!isPartlySpotted && hitColliders[i].transform.name == "SomeEnemy" && Vector3.Distance(playerCurrentPosition, hitColliders[i].transform.position) <= visibilityRangeRadius/2)
                {
                    print("Enemy is wthin spot range! GET AWAY OR YOU WILL BE SPOTTED!!!");
                    isPartlySpotted = true;
                    StartCoroutine(PartlySpotted());
                }
                else if (hitColliders[i].transform.name == "SomeEnemy" && Vector3.Distance(playerCurrentPosition, hitColliders[i].transform.position) > visibilityRangeRadius / 2)
                {
                    isPartlySpotted = false;
                    Hide();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(this.playerCurrentPosition, this.visibilityRangeRadius);
    }
}

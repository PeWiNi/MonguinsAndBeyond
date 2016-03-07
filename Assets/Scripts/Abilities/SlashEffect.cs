using UnityEngine;
using System.Collections;

/// <summary>
/// Temp class for faking animation, adding a slash-effect to TailSlap and PunchDance
/// To be used in Trigger() in the respective abilities
/// THOUGHT: Should add particle effect when hitting taget for more signifiers
/// </summary>
public class SlashEffect : MonoBehaviour {
    [SerializeField]
    GameObject tail;

	// Use this for initialization
	void Start () {
        tail.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public IEnumerator TailSlap() {
        tail.SetActive(true);
        transform.Rotate(0, 0, -40);
        yield return new WaitForSeconds(.1f);
        transform.Rotate(90, 0, 0);
        yield return new WaitForSeconds(1f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        tail.SetActive(false);
    }

    public IEnumerator PunchDance() {
        tail.SetActive(true);
        transform.Rotate(90, 0, -40);
        yield return new WaitForSeconds(.1f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        transform.Rotate(90, 0, -40);
        yield return new WaitForSeconds(.1f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        transform.Rotate(90, 0, -40);
        yield return new WaitForSeconds(.5f);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        tail.SetActive(false);
    }

    /// <summary>
    /// Lerp towards the target rotation as smoothly possible in the timespan
    /// </summary>
    /// <param name="target">Target rotation</param>
    /// <param name="time">Time available for lerping</param>
    void RotateTowards(Vector3 target, float time) {

    }
}

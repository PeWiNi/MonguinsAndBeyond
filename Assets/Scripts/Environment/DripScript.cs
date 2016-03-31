using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DripScript : RotateMe {
    [SyncVar]
    Transform host;

	// Update is called once per frame
	void FixedUpdate () {
	    if (host != null) {
            transform.position = host.position + new Vector3(0f, .5f, -0.1f);
            if (!host.GetComponent<PlayerStats>().isSlowed) {
                var em = GetComponent<ParticleSystem>().emission;
                em.enabled = false;
                StartCoroutine(KillMe(25));
                host = null;
            }
        }
	}

    IEnumerator KillMe(float countdown) {
        yield return new WaitForSeconds(countdown);
        Destroy(gameObject);
    }

    public void SetParent(Transform parent) {
        host = parent;
        transform.position = host.position + new Vector3(0f, .5f, -0.1f);
    }
}

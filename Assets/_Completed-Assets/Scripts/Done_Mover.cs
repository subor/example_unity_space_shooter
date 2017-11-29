using UnityEngine;
using UnityEngine.Networking;

public class Done_Mover : MonoBehaviour
{
    public NetworkInstanceId PlayerNetId;
    public float speed;

	void Start ()
	{
		GetComponent<Rigidbody>().velocity = transform.forward * speed;
	}
}

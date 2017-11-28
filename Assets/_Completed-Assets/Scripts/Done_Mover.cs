using UnityEngine;

public class Done_Mover : MonoBehaviour
{
	public float speed;
    public int Index;
    public int Strength;

	void Start ()
	{
		GetComponent<Rigidbody>().velocity = transform.forward * speed;
	}
}

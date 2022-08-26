using UnityEngine;

public class Camera : MonoBehaviour
{
    private Transform _player;

    public void Start()
    {
        _player = GameObject.FindObjectOfType<PlayerController>().transform;
    }

    public void Update()
    {
        transform.position = new Vector3(_player.position.x, 0, -10);
    }
}

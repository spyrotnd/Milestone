using UnityEngine;

public class DustSpawner : MonoBehaviour
{
    [SerializeField] GameObject dustPrefab;
    [SerializeField] Transform feetPoint;
    [SerializeField] float speedThreshold = 3f;
    [SerializeField] float spawnInterval = 0.15f;
    Vector3 lastPosition;
    float timer;

    void Start() => lastPosition = transform.position;

    void Update()
    {
        timer -= Time.deltaTime;

        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (speed > speedThreshold && timer <= 0f)
        {
            Instantiate(dustPrefab, feetPoint.position, Quaternion.identity);
            timer = spawnInterval;
        }
    }
}
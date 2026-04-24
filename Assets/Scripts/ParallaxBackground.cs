using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Multiplicador Parallax")]
    [SerializeField] [Range(0f, 1f)] private float parallaxX = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float parallaxY = 0f;

    private Vector3 lastTargetPosition;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target != null)
            lastTargetPosition = target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 delta = target.position - lastTargetPosition;

        transform.position += new Vector3(delta.x * parallaxX, delta.y * parallaxY, 0f);

        lastTargetPosition = target.position;
    }
}
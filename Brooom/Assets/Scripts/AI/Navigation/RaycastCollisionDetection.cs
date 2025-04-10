using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastCollisionDetection : MonoBehaviour {

    [Tooltip("Only collisions with objects from these layers will be detected.")]
    [SerializeField] LayerMask layersToDetect;

    [Tooltip("The maximum distance in whoch collisions are detected.")]
    [SerializeField] float maxDistance = 5;

    [Tooltip("Parameters of all rays which are to be used.")]
    [SerializeField] List<RayParameters> rays;

    [SerializeField] bool debugLogs = false;

    public List<CollisionInfo> GetListOfCollisions() {
        List<CollisionInfo> collisions = new List<CollisionInfo>();

        // Fill the list
        RaycastHit hit;
        foreach (var ray in rays) {
            if (Physics.Raycast(transform.position, transform.TransformDirection(ray.rayDirection).normalized, out hit, maxDistance, layersToDetect)) {
                // Determine the collision type
                string layer = LayerMask.LayerToName(hit.transform.gameObject.layer);
                CollisionObjectType type = CollisionObjectType.Unknown;
                if (layer == "Terrain") type = CollisionObjectType.Terrain;
                else if (layer == "Track") type = CollisionObjectType.Track;
                else if (layer == "Obstacles") type = CollisionObjectType.Obstacle;
                else if (layer == "Characters") type = CollisionObjectType.Opponent;
                // Collect collision info
                collisions.Add(new CollisionInfo {
                    direction = ray.simplifiedDirection,
                    distance = hit.distance,
                    normalizedDistance = hit.distance / maxDistance,
                    collisionObject = hit.transform.gameObject,
                    objectType = type
                });
                if (debugLogs)
                    Debug.DrawRay(transform.position, transform.TransformDirection(ray.rayDirection).normalized * hit.distance, Color.red);
            } else {
                if (debugLogs)
                    Debug.DrawRay(transform.position, transform.TransformDirection(ray.rayDirection).normalized * maxDistance, Color.green);
            }
            
        }

        return collisions;
    }

}

[System.Serializable]
internal class RayParameters {
    [Tooltip("The actual direction of the ray.")]
    public Vector3 rayDirection;
    [Tooltip("Which simplified direction this ray represents, e.g. (-1, 0) for left, (0, 1) for up etc.")]
    public Vector2 simplifiedDirection;
}

public class CollisionInfo {
    public Vector2 direction; // omiting forward direction (just left/right, up/down)
    public float distance;
    public float normalizedDistance;
    public GameObject collisionObject;
    public CollisionObjectType objectType;
}

public enum CollisionObjectType { 
    Unknown,
    Opponent,
    Track,
    Terrain,
    Obstacle
}
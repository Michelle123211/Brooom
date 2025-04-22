using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component detecting objects in front of the object to which it is attached.
/// It uses raycasts, where number and directions of rays can be parametrized.
/// </summary>
public class RaycastCollisionDetection : MonoBehaviour {

    [Tooltip("Only collisions with objects from these layers will be detected.")]
    [SerializeField] LayerMask layersToDetect;

    [Tooltip("The maximum distance in whoch collisions are detected.")]
    [SerializeField] float maxDistance = 5;

    [Tooltip("Parameters of all rays which are to be used.")]
    [SerializeField] List<RayParameters> rays;

    [Tooltip("Whether debug messages should be logged.")]
    [SerializeField] bool debugLogs = false;

    /// <summary>
    /// Detects any potential collisions (based on raycasts) and then returns a list of information for all of them.
    /// </summary>
    /// <returns>A list of information about all potential collisions (e.g., direction, distance, detected object).</returns>
    public List<CollisionInfo> GetListOfCollisions() {
        List<CollisionInfo> collisions = new List<CollisionInfo>();

        // For each ray, detect a potential collision
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

/// <summary>
/// A class containing all important information about raycast collision (e.g., direction, distance, detected object).
/// </summary>
public class CollisionInfo {
    /// <summary>Simplified collision direction, e.g., (-1, 0) for left, (0, 1) for up.</summary>
    public Vector2 direction; // omiting forward direction (just left/right, up/down)
    /// <summary>How far the detected object is.</summary>
    public float distance;
    /// <summary>How far the detected object is, relatively to some maximum distance.</summary>
    public float normalizedDistance;
    /// <summary>Object with which a potential collision was detected.</summary>
    public GameObject collisionObject;
    /// <summary>A type of the object with which potential collision was detected.</summary>
    public CollisionObjectType objectType;
}

/// <summary>
/// A type of the object with which potential collision was detected.
/// </summary>
public enum CollisionObjectType { 
    Unknown,
    Opponent,
    Track,
    Terrain,
    Obstacle
}
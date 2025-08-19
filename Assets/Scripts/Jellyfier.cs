using UnityEngine;

// Source: https://github.com/KristinLague/Jelly-Mesh-Deformation/blob/main/Assets/Scripts/Jellyfier.cs
public class Jellyfier : MonoBehaviour
{
    public float bounceSpeed;
    public float fallForce;
    public float stiffness;

    private MeshFilter meshFilter;
    private Mesh mesh;

    Vector3[] initialVertices;
    Vector3[] currentVertices;
    Vector3[] vertexVelocities;

    // To track object velocity
    private Vector3 lastPosition;
    private Vector3 objectVelocity;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        initialVertices = mesh.vertices;
        currentVertices = new Vector3[initialVertices.Length];
        vertexVelocities = new Vector3[initialVertices.Length];

        for (int i = 0; i < initialVertices.Length; i++)
            currentVertices[i] = initialVertices[i];

        lastPosition = transform.position;
    }

    private void Update()
    {
        // Track world velocity of this object
        objectVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        UpdateVertices();
    }

    private void UpdateVertices()
    {
        Vector3 objectOrigin = transform.position; // parent or object's origin in world space

        for (int i = 0; i < currentVertices.Length; i++)
        {
            // Compute distance from vertex to object's origin
            Vector3 worldVertexPos = transform.TransformPoint(currentVertices[i]);
            Vector3 toOrigin = worldVertexPos - objectOrigin;
            float distanceFactor = toOrigin.magnitude; // can scale this later if needed

            // Modulate bounce and stiffness based on distance (example: farther vertices bounce more)
            float vertexBounce = bounceSpeed * (0.5f + distanceFactor); // adjust 0.5f as min bounce
            float vertexStiffness = stiffness * (0.5f + distanceFactor); // adjust 0.5f as min stiffness

            // Bounce back toward initial position
            Vector3 displacement = currentVertices[i] - initialVertices[i];
            vertexVelocities[i] -= displacement * vertexBounce * Time.deltaTime;

            // Damping
            vertexVelocities[i] *= 1f - vertexStiffness * Time.deltaTime;

            // Apply object motion
            vertexVelocities[i] += transform.InverseTransformDirection(objectVelocity) * Time.deltaTime;

            // Add slight directional variation based on distance from origin
            Vector3 directionalOffset = Vector3.Cross(toOrigin.normalized, Vector3.up) * 0.05f * distanceFactor;
            currentVertices[i] += (vertexVelocities[i] + directionalOffset) * Time.deltaTime;
        }

        mesh.vertices = currentVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }


    public void OnCollisionEnter(Collision other)
    {
        foreach (var contact in other.contacts)
        {
            Vector3 inputPoint = contact.point + (contact.point * .1f);
            ApplyPressureToPoint(inputPoint, fallForce);
        }
    }

    public void ApplyPressureToPoint(Vector3 point, float pressure)
    {
        for (int i = 0; i < currentVertices.Length; i++)
            ApplyPressureToVertex(i, point, pressure);
    }

    public void ApplyPressureToVertex(int index, Vector3 position, float pressure)
    {
        Vector3 distanceVerticePoint = currentVertices[index] - transform.InverseTransformPoint(position);
        float adaptedPressure = pressure / (1f + distanceVerticePoint.sqrMagnitude);

        float velocity = adaptedPressure * Time.deltaTime;
        vertexVelocities[index] += distanceVerticePoint.normalized * velocity;
    }
}

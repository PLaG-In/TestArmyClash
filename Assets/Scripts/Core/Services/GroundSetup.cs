using UnityEngine;
using ArmyClash.Utilities;

namespace ArmyClash.View
{
    /// <summary>
    /// Sets up the battlefield ground with proper collision
    /// </summary>
    public class GroundSetup : MonoBehaviour
    {
        [Header("Ground Settings")]
        [SerializeField] private Vector3 _groundSize = new Vector3(50f, 1f, 50f);
        [SerializeField] private Material _groundMaterial;
        [SerializeField] private Color _groundColor = new Color(0.4f, 0.6f, 0.3f);

        private MeshRenderer _renderer;
        private BoxCollider _collider;

        private void Awake()
        {
            SetupGround();
        }

        private void SetupGround()
        {
            // Add or get mesh filter
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            // Create plane mesh if not assigned
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = CreatePlaneMesh();
            }

            // Add or get renderer
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            // Setup material
            if (_groundMaterial != null)
            {
                _renderer.material = _groundMaterial;
            }
            else
            {
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = _groundColor;
                _renderer.material = defaultMaterial;
            }

            // Set scale
            transform.localScale = _groundSize;

            // Set layer
            gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_GROUND);
        }

        private Mesh CreatePlaneMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "GroundPlane";

            // Vertices
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f)
            };

            // Triangles
            int[] triangles = new int[]
            {
                0, 2, 1,
                2, 3, 1
            };

            // UVs
            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            // Normals
            Vector3[] normals = new Vector3[]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = normals;

            return mesh;
        }

        /// <summary>
        /// Check if a position is within ground bounds
        /// </summary>
        public bool IsWithinBounds(Vector3 position)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);
            
            return Mathf.Abs(localPos.x) <= 0.5f && 
                   Mathf.Abs(localPos.z) <= 0.5f;
        }

        /// <summary>
        /// Get random position on ground
        /// </summary>
        public Vector3 GetRandomPosition()
        {
            float halfWidth = _groundSize.x * 0.5f;
            float halfDepth = _groundSize.z * 0.5f;

            float x = Random.Range(-halfWidth, halfWidth);
            float z = Random.Range(-halfDepth, halfDepth);

            return transform.position + new Vector3(x, 0, z);
        }

        /// <summary>
        /// Clamp position to stay within ground bounds
        /// </summary>
        public Vector3 ClampToGround(Vector3 position)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);
            
            localPos.x = Mathf.Clamp(localPos.x, -0.5f, 0.5f);
            localPos.z = Mathf.Clamp(localPos.z, -0.5f, 0.5f);
            localPos.y = 0;

            return transform.TransformPoint(localPos);
        }

        private void OnValidate()
        {
            if (_groundSize.y < 0.1f)
            {
                _groundSize.y = 0.1f;
            }
        }

        private void OnDrawGizmos()
        {
            // Draw ground bounds in editor
            Gizmos.color = new Color(0.4f, 0.6f, 0.3f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            
            Gizmos.color = new Color(0.2f, 0.4f, 0.1f, 1f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ARFruitSlasher.Procedural
{
    public static class ProceduralMeshBuilder
    {
        /// <summary>
        /// Generates a sphere mesh with radius and segments
        /// </summary>
        public static Mesh CreateSphereMesh(float radius = 0.25f, int latSegments = 16, int lonSegments = 16)
        {
            Mesh mesh = new Mesh { name = "Procedural_Sphere" };

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = lat * Mathf.PI / latSegments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = lon * 2f * Mathf.PI / lonSegments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 normal = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                    normals.Add(normal);
                    vertices.Add(normal * radius);
                    uvs.Add(new Vector2((float)lon / lonSegments, (float)lat / latSegments));
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int first = (lat * (lonSegments + 1)) + lon;
                    int second = first + lonSegments + 1;

                    triangles.Add(first);
                    triangles.Add(second);
                    triangles.Add(first + 1);

                    triangles.Add(second);
                    triangles.Add(second + 1);
                    triangles.Add(first + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Generates a sliced hemisphere with flat inner cap (leftSide = true for -X, false for +X)
        /// </summary>
        public static Mesh CreateHemisphereMesh(float radius = 0.25f, bool isLeftHalf = true, int latSegments = 16, int lonSegments = 16)
        {
            Mesh mesh = new Mesh { name = isLeftHalf ? "Fruit_LeftHalf" : "Fruit_RightHalf" };

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            // Curved outer shell
            int halfLonSegments = lonSegments / 2;
            int startLon = isLeftHalf ? halfLonSegments : 0;
            int endLon = isLeftHalf ? lonSegments : halfLonSegments;

            List<Vector3> rimVertices = new List<Vector3>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float theta = lat * Mathf.PI / latSegments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int i = 0; i <= halfLonSegments; i++)
                {
                    int lon = startLon + i;
                    float phi = lon * 2f * Mathf.PI / lonSegments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 normal = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                    normals.Add(normal);
                    vertices.Add(normal * radius);
                    uvs.Add(new Vector2((float)i / halfLonSegments, (float)lat / latSegments));

                    // Keep track of rim vertices for cap
                    if (i == 0 || i == halfLonSegments)
                    {
                        rimVertices.Add(normal * radius);
                    }
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int i = 0; i < halfLonSegments; i++)
                {
                    int first = (lat * (halfLonSegments + 1)) + i;
                    int second = first + halfLonSegments + 1;

                    triangles.Add(first);
                    triangles.Add(second);
                    triangles.Add(first + 1);

                    triangles.Add(second);
                    triangles.Add(second + 1);
                    triangles.Add(first + 1);
                }
            }

            // Planar flat cut cap
            int centerIndex = vertices.Count;
            Vector3 center = Vector3.zero;
            Vector3 capNormal = isLeftHalf ? Vector3.right : Vector3.left;
            vertices.Add(center);
            normals.Add(capNormal);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int rimCount = 20;
            int firstRimIdx = vertices.Count;
            for (int r = 0; r <= rimCount; r++)
            {
                float angle = r * 2f * Mathf.PI / rimCount;
                Vector3 rimPos = new Vector3(0f, Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
                vertices.Add(rimPos);
                normals.Add(capNormal);
                uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.45f, 0.5f + Mathf.Sin(angle) * 0.45f));
            }

            for (int r = 0; r < rimCount; r++)
            {
                int r1 = firstRimIdx + r;
                int r2 = firstRimIdx + r + 1;

                if (isLeftHalf)
                {
                    triangles.Add(centerIndex);
                    triangles.Add(r1);
                    triangles.Add(r2);
                }
                else
                {
                    triangles.Add(centerIndex);
                    triangles.Add(r2);
                    triangles.Add(r1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Creates a simple material with solid color or standard shader
        /// </summary>
        public static Material CreateColorMaterial(Color color, string name = "ProceduralMat", float smoothness = 0.5f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Material mat = new Material(shader) { name = name };
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }
    }
}

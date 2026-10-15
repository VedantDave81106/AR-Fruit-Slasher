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
        /// Generates an Apple shaped mesh (slightly indented top/bottom)
        /// </summary>
        public static Mesh CreateAppleMesh(float radius = 0.25f, int latSegments = 18, int lonSegments = 18)
        {
            Mesh mesh = new Mesh { name = "Procedural_Apple" };

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float t = (float)lat / latSegments;
                float theta = t * Mathf.PI;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                // Indentation at top (t near 0) and bottom (t near 1)
                float indent = 1.0f;
                if (t < 0.15f) indent = Mathf.Lerp(0.7f, 1.0f, t / 0.15f);
                else if (t > 0.85f) indent = Mathf.Lerp(1.0f, 0.75f, (t - 0.85f) / 0.15f);

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float phi = lon * 2f * Mathf.PI / lonSegments;
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    Vector3 normal = new Vector3(cosPhi * sinTheta, cosTheta, sinPhi * sinTheta);
                    normals.Add(normal);
                    Vector3 pos = new Vector3(normal.x * radius * indent, normal.y * radius * 1.08f, normal.z * radius * indent);
                    vertices.Add(pos);
                    uvs.Add(new Vector2((float)lon / lonSegments, t));
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

            int halfLonSegments = lonSegments / 2;
            int startLon = isLeftHalf ? halfLonSegments : 0;

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
        /// Generates procedural texture for Watermelon Rind (green with dark wavy stripes)
        /// </summary>
        public static Texture2D CreateWatermelonTexture()
        {
            int w = 256, h = 128;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            Color baseGreen = new Color(0.14f, 0.58f, 0.24f);
            Color darkGreen = new Color(0.06f, 0.30f, 0.11f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float wave = Mathf.Sin((float)y / h * Mathf.PI * 8f + (float)x / 16f) * 6f;
                    float stripe = Mathf.Sin(((x + wave) / (float)w) * Mathf.PI * 14f);
                    Color c = stripe > 0.2f ? darkGreen : baseGreen;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Creates a simple material with solid color or standard shader
        /// </summary>
        public static Material CreateColorMaterial(Color color, string name = "ProceduralMat", float smoothness = 0.5f, Texture2D mainTex = null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Material mat = new Material(shader) { name = name };
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mainTex != null)
            {
                mat.mainTexture = mainTex;
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            }
            return mat;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;

namespace CPATool {
    public class Vertex {
        public Vector3 position;
        public Vector3 normal;
    }
    public class UV {
        public Vector2 coords;
    }
    public class Face {
        public int v1, v2, v3;
        public Vector3 normal;
        public int uv1, uv2, uv3;
    }


    public class CPAObject {
        public override string ToString() => name;
        public string name;
    }

    // Named classes

    public class Geometric : CPAObject {
        public List<Vertex> vertices = new List<Vertex>();
        public List<string> elements = new List<string>();
    }

    public class ElementIndexedTriangles : CPAObject {
        public string material;
        public List<Face> faces = new List<Face>();
        public List<UV> uvs = new List<UV>();
    }

    public class Material : CPAObject {
        public override string ToString() => $"TGA Path: {texture}";
        public enum Type { Gouraud, Flat }
        public Type type;
        public Vector3 ambientColor = new Vector3(0.588f);
        public Vector3 diffuseColor = new Vector3(0.588f);
        public Vector4 specularColor = new Vector4(0.9f, 0.9f, 0.9f, 0);
        public string texture;
        public bool backface = true;
    }

    public class Matrix : CPAObject {
        public override string ToString() => $"Translation: {translation.ToString()}";
        public Vector3 translation;
    }

    public class SuperObject : CPAObject {
        public string matrix;
        public string geometric;
        public List<string> children = new List<string>();
    }
}

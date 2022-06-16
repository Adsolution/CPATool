using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using System.Linq;
using System.Text;

namespace CPATool {
    public partial class ModFile {
        public Dictionary<Type, Dictionary<string, CPAObject>>
            data = new Dictionary<Type, Dictionary<string, CPAObject>>();

        public string name;
        public bool swapYZ = true;
        public bool exportMaterials = true;
        public const string authMessage = "Generated with CPA Tool";

        public bool hasMaterials => exportMaterials && data.ContainsKey(typeof(Material));


        public Dictionary<string, T> GetObjectDictionary<T>() where T : CPAObject {
            var d = new Dictionary<string, T>();
            if (!data.ContainsKey(typeof(T)))
                data.Add(typeof(T), new Dictionary<string, CPAObject>());

            foreach (var a in data[typeof(T)])
                d.Add(a.Key, (T)a.Value);
            return d;
        }


        public T AddOrGetObject<T>(string key) where T : CPAObject {
            key = key.Replace('.', '_').Replace(' ', '_');

            if (!data.ContainsKey(typeof(T)))
                data.Add(typeof(T), new Dictionary<string, CPAObject>());

            if (!data[typeof(T)].ContainsKey(key)) {
                var o = (CPAObject)Activator.CreateInstance(typeof(T));
                o.name = key;
                data[typeof(T)].Add(key, o);
            }

            return (T)data[typeof(T)][key];
        }


        public T GetObject<T>(string key) where T : CPAObject {
            if (data.ContainsKey(typeof(T)) && data[typeof(T)].ContainsKey(key))
            return (T)data[typeof(T)][key];
            else return default;
        }

        public CPAObject GetObject(Type type, string key) {
            if (data.ContainsKey(type) && data[type].ContainsKey(key))
                return data[type][key];
            else return default;
        }


        public int GetUVStartIndex(string geo, string element) {
            int i = 0;
            foreach (var e in GetObject<Geometric>(geo).elements.Select(x => GetObject<ElementIndexedTriangles>(x))) {
                if (e.name == element) break;
                i += e.uvs.Count();
            }
            return i;
        }


        void UniversalImportStuff(string path, bool swapYZ) {
            name = new FileInfo(path).Name.Split('.')[0];
            this.swapYZ = swapYZ;
        }
    }
}

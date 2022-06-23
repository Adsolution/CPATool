using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.IO;
using System.Linq;
using ImageMagick;
using ImageMagick.Formats;

namespace CPATool {
    public partial class ModFile {
        string[] DissectModCommand(string line) {
            var l = line.Replace("\"", "").Replace("\t", "").Replace("*^", "")
                .Split(new string[] { ",", "(", ")" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < l.Length; i++)
                if (l[i].Contains(":"))
                    l[i] = l[i].Split(new string[] { ":" }, StringSplitOptions.None)[1];

            return l;
        }



        public void ImportMOD(string path, bool alsoImportSPO = true) {
            UniversalImportStuff(path, false);
            var file = new StreamReader(path);

            // Gather all object descriptions
            var objDescs = file.ReadToEnd().Replace("}", "").Split(new string[] { "{" }, StringSplitOptions.RemoveEmptyEntries);
            file.Close();

            // Parse object descriptions
            foreach (var o in objDescs) {
                var lines = o.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    continue;

                var firstLine = lines[0].Replace("\r\n", "").Split(new string[] { ":", "(" }, StringSplitOptions.RemoveEmptyEntries);

                // (Ignore empty lines, comments & global commands)
                if (firstLine.Length == 0 || firstLine[0][0] == ';' || firstLine[0][0] == '$')
                    continue;


                switch (firstLine[0]) {


                    case "Material":
                        var mat = AddOrGetObject<Material>(firstLine[1]);
                        mat.name = firstLine[1];
                        foreach (var l in lines) {
                            var c = DissectModCommand(l);
                            switch (c[0]) {

                                case "Type":
                                    mat.type = (Material.Type)Enum.Parse(typeof(Material.Type), c[1]);
                                    break;

                                case "AmbientColor":
                                    mat.ambientColor = new Vector3(float.Parse(c[1]), float.Parse(c[2]), float.Parse(c[3]));
                                    break;

                                case "DiffuseColor":
                                    mat.diffuseColor = new Vector3(float.Parse(c[1]), float.Parse(c[2]), float.Parse(c[3]));
                                    break;

                                case "SpecularColor":
                                    mat.specularColor = new Vector4(float.Parse(c[1]), float.Parse(c[2]), float.Parse(c[3]), float.Parse(c[4]));
                                    break;

                                case "Texture":
                                    mat.texture = new FileInfo($"{new FileInfo(path).DirectoryName}\\..\\..\\graphics\\Textures\\{name}\\{c[1]}").FullName;
                                    break;

                                case "Backface":
                                    mat.backface = c[1] == "ON";
                                    break;
                            }
                        }
                        break;


                    case "ElementIndexedTriangles":
                        var el = AddOrGetObject<ElementIndexedTriangles>(firstLine[1]);
                        el.name = firstLine[1];
                        foreach (var l in lines) {
                            var c = DissectModCommand(l);
                            switch (c[0]) {

                                case "Material":
                                    el.material = c[1];
                                    break;

                                case "AddFaceUV":
                                    el.faces.Add(new Face {
                                        v1 = int.Parse(c[2]),
                                        v2 = int.Parse(c[3]),
                                        v3 = int.Parse(c[4]),
                                        normal = new Vector3(float.Parse(c[5]), float.Parse(c[6]), float.Parse(c[7])),
                                        uv1 = int.Parse(c[8]),
                                        uv2 = int.Parse(c[9]),
                                        uv3 = int.Parse(c[10]),
                                    });
                                    break;

                                case "AddUV":
                                    el.uvs.Add(new UV {
                                        coords = new Vector2(float.Parse(c[2]), float.Parse(c[3])),
                                    });
                                    break;
                            }
                        }
                        break;


                    case "Geometric":
                        var geo = AddOrGetObject<Geometric>(firstLine[1]);
                        geo.name = firstLine[1];
                        foreach (var l in lines) {
                            var c = DissectModCommand(l);
                            switch (c[0]) {

                                case "AddVertex":
                                    geo.vertices.Add(new Vertex {
                                        position = new Vector3(float.Parse(c[2]), float.Parse(c[3]), float.Parse(c[4])),
                                        normal = new Vector3(float.Parse(c[5]), float.Parse(c[6]), float.Parse(c[7])),
                                    });
                                    break;

                                case "AddElement":
                                    geo.elements.Add(c[3]);
                                    break;
                            }
                        }
                        break;


                    case "Matrix":
                        var mtx = AddOrGetObject<Matrix>(firstLine[1]);
                        mtx.name = firstLine[1];
                        foreach (var l in lines) {
                            var c = DissectModCommand(l);
                            switch (c[0]) {

                                case "MatrixTranslation":
                                    mtx.translation = new Vector3(float.Parse(c[1]), float.Parse(c[2]), float.Parse(c[3]));
                                    break;
                            }
                        }
                        break;


                    case "SuperObject":
                        var spo = AddOrGetObject<SuperObject>(firstLine[1]);
                        spo.name = firstLine[1];
                        foreach (var l in lines) {
                            var c = DissectModCommand(l);
                            switch (c[0]) {

                                case "PutMatrix":
                                    spo.matrix = c[1];
                                    break;

                                case "Geometric":
                                    spo.geometric = c[1];
                                    break;

                                case "AddChild":
                                    spo.children.Add(c[1]);
                                    break;
                            }
                        }
                        break;
                }
            }

            string spoPath = path.Replace(".mod", ".spo").Replace(".MOD", ".SPO");

            if (alsoImportSPO && File.Exists(spoPath))
                ImportMOD(spoPath, false);
        }




        public void ExportMOD(string path, bool createSPO = true) {
            swapYZ = true;
            tClear();
            tWrite($"; {authMessage}\n");

            tWrite("$SetCurrentFileDouble(0,\"5.20\")");
            tWrite("$SetCurrentFileDouble(1,1)\n");

            var geovals = GetObjectDictionary<Geometric>().Values;

            foreach (var g in geovals) {
                tWrite("{" + $"Geometric:{g.name}({g.vertices.Count},0,{g.elements.Count})");
                tIndent++;

                foreach (var v in g.vertices) {
                    var vp = v.position * scale;
                    var vn = v.normal;
                    if (swapYZ) {
                        vp = new Vector3(vp.X, -vp.Z, vp.Y);
                        vn = new Vector3(vn.X, -vn.Z, vn.Y);
                    }
                    tWrite($"AddVertex({g.vertices.IndexOf(v)},\"{vp.X}\",\"{vp.Y}\",\"{vp.Z}\",\"{vn.X}\",\"{vn.Y}\",\"{vn.Z}\",0,0,0)");
                }

                int ei = 0;
                foreach (var e in g.elements)
                    tWrite($"AddElement({ei++},IndexedTriangles,\"*^ElementIndexedTriangles:{e}\")");

                tIndent--;
                tWrite("}");


                foreach (var e in g.elements.Select(x => GetObject<ElementIndexedTriangles>(x))) {
                    tWrite("{" + $"ElementIndexedTriangles:{e.name}({e.faces.Count},{e.uvs.Count})");
                    tIndent++;

                    tWrite($"Material(\"*^Material:{e.material}\")");
                    foreach (var f in e.faces) {
                        var fn = f.normal;
                        if (swapYZ) fn = new Vector3(fn.X, -fn.Y, fn.Z);
                        tWrite($"AddFaceUV({e.faces.IndexOf(f)},{f.v1},{(flipFaces ? f.v3 : f.v2)},{(flipFaces ? f.v2 : f.v3)},\"{fn.X}\",\"{fn.Y}\",\"{fn.Z}\",{f.uv1},{(flipFaces ? f.uv3 : f.uv2)},{(flipFaces ? f.uv2 : f.uv3)})");
                    }
                    foreach (var uv in e.uvs)
                        tWrite($"AddUV({e.uvs.IndexOf(uv)},\"{uv.coords.X}\",\"{uv.coords.Y}\")");
                    tIndent--;
                    tWrite("}");
                }
            }

            foreach (var m in GetObjectDictionary<Material>().Values) {
                if (string.IsNullOrEmpty(m.texture))
                    continue;
                var fi = new FileInfo(m.texture);
                string newDir = $"{path}\\..\\..\\..\\graphics\\Textures\\{name}";
                string newName = fi.Name.Replace(fi.Extension, ".tga");
                string newPath = $"{newDir}\\{newName}";
                
                tWrite("{" + $"Material:{m.name}");
                tIndent++;

                tWrite($"Type({m.type})");
                tWrite($"AmbientColor(\"{m.ambientColor.X}\",\"{m.ambientColor.Y}\",\"{m.ambientColor.Z}\")");
                tWrite($"DiffuseColor(\"{m.diffuseColor.X}\",\"{m.diffuseColor.Y}\",\"{m.diffuseColor.Z}\")");
                tWrite($"SpecularColor(\"{m.specularColor.X}\",\"{m.specularColor.Y}\",\"{m.specularColor.Z}\",\"{m.specularColor.W}\")");
                tWrite($"Texture(\"{newName}\")");
                tWrite($"Backface({(m.backface ? "ON" : "OFF")})");

                tIndent--;
                tWrite("}");


                // ==== Texture convert ====
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);

                var tex = new MagickImage(m.texture);
                int max = 256;
                int texW = tex.Width < max ? tex.Width : max;
                int texH = tex.Height < max ? tex.Height : max;

                tex.Resize(texW, texH);
                tex.Flip();
                tex.Orientation = OrientationType.BottomLeft;
                tex.Write(newPath, MagickFormat.Tga);
                tex.Dispose();
            }


            var file = new StreamWriter(path + ".mod");
            file.Write(tOut);
            file.Close();


            // ============== SPO ==============

            if (!createSPO || !data.ContainsKey(typeof(SuperObject)))
                return;

            tClear();
            tWrite($"; {authMessage}\n");

            foreach (var s in GetObjectDictionary<SuperObject>().Values) {
                tWrite("{" + $"SuperObject:{s.name}");
                tIndent++;

                tWrite($"PutMatrix(\"*^Matrix:{s.matrix}\")");
                if (s.geometric != null)
                    tWrite($"Geometric(\"{name}.MOD^Geometric:{s.geometric}\")");
                foreach (var c in s.children)
                    tWrite($"AddChild(\"*^SuperObject:{c}\")");

                tIndent--;
                tWrite("}");

                if (s.matrix == null)
                    continue;

                var m = GetObject<Matrix>(s.matrix);
                tWrite("{" + $"Matrix:{m.name}");
                tIndent++;
                tWrite($"MatrixTranslation(\"{m.translation.X}\",\"{m.translation.Y}\",\"{m.translation.Z}\")");
                tIndent--;
                tWrite("}");
            }

            file = new StreamWriter(path + ".spo");
            file.Write(tOut);
            file.Close();
        }
    }
}

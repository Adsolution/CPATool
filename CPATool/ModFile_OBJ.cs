using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Globalization;

namespace CPATool {
    public partial class ModFile {
        public void ExportOBJ(string path) {
            swapYZ = true;

            var geos = GetObjectDictionary<Geometric>();
            bool hasSPO = data.ContainsKey(typeof(SuperObject));

            string t = "";
            int vi = 0, vi_inc = 1, vti = 0, vti_inc = 1;

            t += $"# {authMessage}\n\n";

            if (hasMaterials)
                t += $"mtllib {name}.mtl\n\n";

            foreach (var g in geos) {
                vi = vi_inc;
                vti = vti_inc;

                var pos = new Vector3();

                if (hasSPO) {
                    var SPOs = GetObjectDictionary<SuperObject>();
                    if (SPOs.ContainsKey("SPO_" + g.Key))
                        pos = GetObject<Matrix>(SPOs["SPO_" + g.Key].matrix).translation;
                    else if (SPOs.ContainsKey(g.Key))
                        pos = GetObject<Matrix>(SPOs[g.Key].matrix).translation;

                    var parentSPO = SPOs.Values.Where(x => x.children.Contains("SPO_" + g.Key)).FirstOrDefault();
                    if (parentSPO != null)
                        pos += GetObject<Matrix>(parentSPO.matrix).translation;
                }


                t += $"o {g.Key}\n\n";

                foreach (var v in g.Value.vertices) {
                    vi_inc++;
                    var p = pos + v.position;
                    if (swapYZ) p = new Vector3(p.X, p.Z, -p.Y);
                    t += $"v {p.X} {p.Y} {p.Z}\n";
                }
                foreach (var v in g.Value.vertices) {
                    var n = -v.normal;
                    if (swapYZ) n = new Vector3(n.X, n.Z, -n.Y);
                    t += $"vn {n.X} {n.Y} {n.Z}\n";
                }
                foreach (var v in g.Value.elements.Select(x => GetObject<ElementIndexedTriangles>(x))) {
                    foreach (var uv in v.uvs) {
                        vti_inc++;
                        t += $"vt {uv.coords.X} {uv.coords.Y}\n";
                    }
                }
                foreach (var e in g.Value.elements.Select(x => GetObject<ElementIndexedTriangles>(x))) {
                    if (hasMaterials)
                        t += $"\nusemtl {e.material}\n\n";

                    for (int i = 0; i < e.faces.Count; i++) {
                        var face = e.faces[i];
                        t += $"f " +
                            $"{face.v1 + vi}/{face.uv1 + vti} " +
                            $"{face.v2 + vi}/{face.uv2 + vti} " +
                            $"{face.v3 + vi}/{face.uv3 + vti}\n";
                    }
                }

                t += $"\n\n";
            }


            var file = new StreamWriter(path + ".obj");
            file.Write(t);
            file.Close();


            if (!hasMaterials)
                return;

            file = new StreamWriter(path + ".mtl");
            t = $"# {authMessage}\n\n";

            foreach (var m in GetObjectDictionary<Material>().Values) {
                t += $"newmtl {m.name}\n";
                t += $"map_Kd {m.texture}\n\n";
            }
            file.Write(t);
            file.Close();
        }



        Geometric newObj(string name) {
            var g = AddOrGetObject<Geometric>(name);
            var root = GetObject<SuperObject>("Root");

            // SPO & Matrix
            var s = AddOrGetObject<SuperObject>("SPO_" + g.name);
            AddOrGetObject<Matrix>("SPO_" + g.name);
            s.matrix = "SPO_" + g.name;
            s.geometric = g.name;

            // Name-based sector linking/generation
            string secname = "Default";
            if (g.name.Contains('@'))
                secname = g.name.Split('@')[0];
            var sct = AddOrGetObject<SuperObject>("SPO_sct" + secname);

            if (sct.matrix == null) {
                var sctMat = AddOrGetObject<Matrix>(sct.name);
                sct.matrix = sctMat.name;
                root.children.Add(sctMat.name);
            }
            if (!sct.children.Contains(s.name))
                sct.children.Add(s.name);

            return g;
        }



        public void ImportOBJ(string path) {
            UniversalImportStuff(path, true);

            // OBJ has no hierarchy, so it will be generated
            var root = AddOrGetObject<SuperObject>("Root");
            var rootMat = AddOrGetObject<Matrix>("Root");
            root.matrix = rootMat.name;

            // Import process
            Geometric g = null;
            ElementIndexedTriangles e = null;
            Material m = null;
            string mtllib = null;
            bool smooth = true;

            var verts = new List<Vector3>();
            var nrms = new List<Vector3>();
            var uvs = new List<Vector2>();

            var file = new StreamReader(path);


            while (!file.EndOfStream) {
                string _line = file.ReadLine();
                if (_line.Length == 0 || _line[0] == '#')
                    continue;

                var line = _line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


                switch (line[0]) {
                    case "mtllib": mtllib = _line.Substring(7); break;

                    case "v": verts.Add(new Vector3(float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3]))); break;
                    case "vn": nrms.Add(new Vector3(float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3]))); break;
                    case "vt": uvs.Add(new Vector2(float.Parse(line[1]), float.Parse(line[2]))); break;

                    //case "s": smooth = line[1] != "off"; break;

                    case "o": g = newObj(line[1]); break;


                    case "usemtl":
                        if (g == null)
                            g = newObj("Object_" + Guid.NewGuid().ToString().ToUpper().Substring(0, 5));

                        m = AddOrGetObject<Material>(line[1]);
                        e = AddOrGetObject<ElementIndexedTriangles>($"{g.name}_{line[1]}");
                        e.material = m.name;
                        if (!g.elements.Contains(e.name))
                            g.elements.Add(e.name);
                        break;


                    case "f":
                        if (g == null)
                            g = newObj("Object_" + Guid.NewGuid().ToString().Substring(0, 6));

                        var fline = line.Select(x => x.Split('/')).ToArray();

                        for (int l = 1; l < fline.Length - 2; l++) {
                            int i1 = int.Parse(fline[1 + 0][0]) - 1;
                            int i2 = int.Parse(fline[l + 1][0]) - 1;
                            int i3 = int.Parse(fline[l + 2][0]) - 1;

                            // vertex position
                            Vertex v1, v2, v3;

                            var v1a = g.vertices.Where(x => x.position == verts[i1]);
                            if (smooth == false || v1a.Count() == 0) g.vertices.Add(v1 = new Vertex { position = verts[i1] });
                            else v1 = v1a.First();

                            var v2a = g.vertices.Where(x => x.position == verts[i2]);
                            if (smooth == false || v2a.Count() == 0) g.vertices.Add(v2 = new Vertex { position = verts[i2] });
                            else v2 = v2a.First();

                            var v3a = g.vertices.Where(x => x.position == verts[i3]);
                            if (smooth == false || v3a.Count() == 0) g.vertices.Add(v3 = new Vertex { position = verts[i3] });
                            else v3 = v3a.First();

                            // vertex UV
                            UV uv1 = null, uv2 = null, uv3 = null;
                            if (uvs.Count > 0) {
                                uv1 = new UV { coords = uvs[fline[1 + 0].Length > 1 && !string.IsNullOrEmpty(fline[1 + 0][1]) ? int.Parse(fline[1 + 0][1]) - 1 : i1] };
                                uv2 = new UV { coords = uvs[fline[l + 1].Length > 1 && !string.IsNullOrEmpty(fline[l + 1][1]) ? int.Parse(fline[l + 1][1]) - 1 : i2] };
                                uv3 = new UV { coords = uvs[fline[l + 2].Length > 1 && !string.IsNullOrEmpty(fline[l + 2][1]) ? int.Parse(fline[l + 2][1]) - 1 : i3] };
                                e.uvs.AddRange(new UV[] { uv1, uv2, uv3 });
                            }

                            // vertex normal
                            if (nrms.Count > 0) {
                                v1.normal = nrms[fline[1 + 0].Length > 2 ? int.Parse(fline[1 + 0][2]) - 1 : i1];
                                v2.normal = nrms[fline[l + 1].Length > 2 ? int.Parse(fline[l + 1][2]) - 1 : i2];
                                v3.normal = nrms[fline[l + 2].Length > 2 ? int.Parse(fline[l + 2][2]) - 1 : i3];
                            }


                            e.faces.Add(new Face {
                                v1 = g.vertices.IndexOf(v1),
                                v2 = g.vertices.IndexOf(v2),
                                v3 = g.vertices.IndexOf(v3),
                                uv1 = e.uvs.IndexOf(uv1),
                                uv2 = e.uvs.IndexOf(uv2),
                                uv3 = e.uvs.IndexOf(uv3),
                            });
                        }
                        break;
                }
            }
            file.Close();

            // Materials
            if (mtllib == null)
                return;
            string directoryName = new FileInfo(path).DirectoryName;
            string mtlPath = $"{directoryName}\\{mtllib}";
            file = new StreamReader(mtlPath);

            while (!file.EndOfStream) {
                string _line = file.ReadLine().Replace("\t", "");
                if (_line.Length == 0 || _line[0] == '#')
                    continue;

                var line = _line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                switch (line[0]) {

                    case "newmtl":
                        m = AddOrGetObject<Material>(line[1]);
                        break;

                    case "map_Kd":
                        string li = _line.Substring(7);
                        m.texture = li.Contains(':') ? li : $"{directoryName}\\{li}";
                        break;
                }
            }
            file.Close();
        }
    }
}

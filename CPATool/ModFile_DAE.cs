using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CPATool {
    public partial class ModFile {
        public void ExportDAE(string path) {
            var geos = GetObjectDictionary<Geometric>();
            var spos = GetObjectDictionary<SuperObject>();

            tIndent = 0;
            tClear();

            tWrite($"<?xml version=\"1.0\" encoding=\"utf - 8\"?>");
            tWrite($"<COLLADA xmlns=\"http://www.collada.org/2005/11/COLLADASchema\" version=\"1.4.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            tIndent++;
            tWrite($"<asset/>");
            tWrite($"<library_geometries>");
            tIndent++;

            foreach (var g in geos) {
                tIndent++;
                tWrite($"<geometry id=\"{g.Key}-mesh\" name=\"{g.Key}\"");
                tIndent++;
                tWrite($"<mesh>");
                tIndent++;

                // POSITIONS
                tWrite($"<source id=\"{g.Key}-mesh-positions\">");
                tIndent++;
                tWrite($"<float_array id=\"{g.Key}-mesh-positions-array\" count=\"{g.Value.vertices.Count * 3}\"", false);

                foreach (var v in g.Value.vertices) {
                    var pos = v.position;
                    if (swapYZ) pos = new Vector3(pos.X, pos.Z, pos.Y);
                    tWrite($"{pos.X} {pos.Y} {pos.Z} ", false);
                }

                tWrite($"<technique_common>");
                tIndent++;
                tWrite($"<accessor source=\"#{g.Key}-mesh-positions-array\" count=\"{g.Value.vertices.Count}\" stride=\"3\">");
                tIndent++;
                tWrite($"<param name=\"X\" type=\"float\"/>");
                tWrite($"<param name=\"Y\" type=\"float\"/>");
                tWrite($"<param name=\"Z\" type=\"float\"/>");
                tIndent--;
                tWrite($"</accessor>");
                tIndent--;
                tWrite($"</technique_common>");
                tIndent--;
                tWrite($"</source>");

                // NORMALS
                tWrite($"<source id=\"{g.Key}-mesh-normals\">");
                tIndent++;
                tWrite($"<float_array id=\"{g.Key}-mesh-normals-array\" count=\"{g.Value.vertices.Count * 3}\"", false);

                foreach (var v in g.Value.vertices) {
                    var pos = v.normal;
                    if (swapYZ) pos = new Vector3(pos.X, pos.Z, pos.Y);
                    tWrite($"{pos.X} {pos.Y} {pos.Z} ", false);
                }
                tWrite($"<technique_common>");
                tIndent++;
                tWrite($"<accessor source=\"#{g.Key}-mesh-normals-array\" count=\"{g.Value.vertices.Count}\" stride=\"3\">");
                tIndent++;
                tWrite($"<param name=\"X\" type=\"float\"/>");
                tWrite($"<param name=\"Y\" type=\"float\"/>");
                tWrite($"<param name=\"Z\" type=\"float\"/>");
                tIndent--;
                tWrite($"</accessor>");
                tIndent--;
                tWrite($"</technique_common>");
                tIndent--;
                tWrite($"</source>");


                // MAP


                // ASSIGN
                /*
                daeWrite($"</source>");
                daeWrite($"<vertices id=\"{g.Key}-mesh-vertices\">");
                daeIndent++;
                daeWrite($"<input demtantic=\"POSITION\" source=\"#{g.Key}-mesh-positions\"/>");
                daeIndent--;
                daeWrite($"<vertices/>");*/
                //daeWrite($"<triangles material=\"Material-material\" count=\"{g.Value./>");
            }



            /*
            foreach (var g in geos) {
                vi = vi_inc;
                vti = vti_inc;

                var pos = new Vector3();


                t += $"o {g.Key}\n\n";

                foreach (var v in g.Value.vertices) {
                    vi_inc++;
                    var p = pos + v.position;
                    if (swapYZ) p = new Vector3(p.X, -p.Z, p.Y);
                    t += $"v {p.X} {p.Y} {p.Z}\n";
                }
                foreach (var v in g.Value.vertices) {
                    var n = v.normal;
                    if (swapYZ) n = new Vector3(n.X, -n.Z, n.Y);
                    t += $"vn {n.X} {n.Y} {n.Z}\n";
                }
                foreach (var v in g.Value.elements) {
                    foreach (var uv in v.Value.uvs) {
                        vti_inc++;
                        t += $"vt {uv.coords.X} {uv.coords.Y}\n";
                    }
                }
                foreach (var e in g.Value.elements.Values) {
                    for (int i = 0; i < e.faces.Count; i++) {
                        var face = e.faces[i];
                        var vt = e.uvs[face.uv1];
                        t += $"f " +
                            $"{e.faces[i].v1 + vi}/{face.uv1 + vti} " +
                            $"{e.faces[i].v2 + vi}/{face.uv2 + vti} " +
                            $"{e.faces[i].v3 + vi}/{face.uv3 + vti}\n";
                    }
                }

                t += $"\n\n";
            }

            */
            var file = new StreamWriter(path);
            file.Write(tOut);
            file.Close();
        }
    }
}

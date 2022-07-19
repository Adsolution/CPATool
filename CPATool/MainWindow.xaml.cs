using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Media;
using Microsoft.Win32;
using System.Windows.Input;

namespace CPATool {
    public partial class MainWindow : Window {
        public static ModFile mod = new ModFile();
        public static FileInfo importPath, exportPath;
        public static string namePath => $"{importPath.DirectoryName}\\{mod.name}";

        public MainWindow() {
            InitializeComponent();
        }


        public void UpdateInspector(string label, object o) {
            list_properties.Children.Clear();
            if (o == null) return;
            ins_name.Text = label;

            foreach (var f in o.GetType().GetFields().Where(x => o.GetType() == x.DeclaringType)) {
                var n = new Field(o);
                list_properties.Children.Add(n);

                var val = f.GetValue(o);

                if (val is IList ie)
                    val = $"Count: {ie.Count}";
                else if (val is IDictionary id)
                    val = $"Count: {id.Count}";

                n.label_name.Text = f.Name;
                n.label_value.Text = val?.ToString();
            };
        }


        public void OpenLoadFileBrowser() {
            var d = new OpenFileDialog();

            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Adsolution\CPATool");
            if (key != null && key.GetValueNames().Contains("ImportPath")) {
                importPath = new FileInfo(key.GetValue("ImportPath").ToString());
                d.InitialDirectory = importPath.DirectoryName;
            }
            //d.Filter = "CPA Model file|*.MOD";
            d.CheckFileExists = true;
            if (!(bool)d.ShowDialog(this))
                return;

            importPath = new FileInfo(d.FileName);

            key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Adsolution\CPATool");
            key.SetValue("ImportPath", importPath.FullName);
            key.Close();

            ///////////////////

            tree_data.Items.Clear();
            tree_hier.Items.Clear();
             
            mod = new ModFile();

            if (importPath.Name.EndsWith(".mod", true, CultureInfo.InvariantCulture))
                mod.ImportMOD(importPath.FullName);

            else if (importPath.Name.EndsWith(".obj", true, CultureInfo.InvariantCulture))
                mod.ImportOBJ(importPath.FullName);

            Title = $"CPA Tool - {importPath.Name}";


            foreach (var t in mod.data) {
                var tvi = new TreeViewItem { Header = t.Key.Name, Foreground = Brushes.LightGray };
                foreach (var i in t.Value)
                    tvi.Items.Add(new TreeViewItem { Header = i.Value.name, Tag = i.Value, Foreground = Brushes.LightGray });
                tree_data.Items.Add(tvi);
            }

            if (!mod.data.ContainsKey(typeof(SuperObject)))
                return;
            foreach (SuperObject sct in mod.data[typeof(SuperObject)].Values.Where(x => x.name.StartsWith("SPO_sct"))) {
                var hierRoot = new TreeViewItem { Header = sct.name.Substring(4), Foreground = Brushes.LightGray };
                tree_hier.Items.Add(hierRoot);

                foreach (var s in sct.children) {
                    var tvi = new TreeViewItem { Header = s.Replace("SPO_", ""), Foreground = Brushes.LightGray };
                    hierRoot.Items.Add(tvi);
                }
            }
        }




        // ====================================== //
        // ========== GUI INTERACTION =========== //
        // ====================================== //


        void button_load_Click(object sender, RoutedEventArgs e) {
            try {
                OpenLoadFileBrowser();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Error");
            }
        }


        void button_export_Click(object sender, RoutedEventArgs e) {
            Cursor = Cursors.Wait;

            try {
                switch (combo_filetype.SelectedItem.ToString()) {
                    case "MOD": mod.ExportMOD(namePath); break;
                    case "OBJ": mod.ExportOBJ(namePath); break;
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, "Error");
            }

            Cursor = Cursors.Arrow;
        }



        private void tree_data_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var item = (TreeViewItem)tree_data.SelectedItem;
            if (item?.Tag == null) return;
            UpdateInspector((string)item.Header, item.Tag);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (mod == null) return;
            float.TryParse(textbox_scale.Text, out mod.scale);
        }



        private void TileMaterials_Checked(object sender, RoutedEventArgs e) {
            mod.tileMaterials = (bool)checkbox_tilematerials.IsChecked;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            mod.flipFaces = (bool)checkbox_flipfaces.IsChecked;
        }
    }
}

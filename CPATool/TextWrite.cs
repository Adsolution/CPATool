using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPATool {
    public partial class ModFile {
        StringBuilder tOut = new StringBuilder();
        string tIndentStr = "\t";
        int tIndent;

        void tClear() {
            tOut.Clear();
            tIndent = 0;
        }

        void tWrite(string s, bool newLine = true) {
            for (int i = 0; i < tIndent; i++)
                tOut.Append(tIndentStr);
            tOut.Append(s);

            if (newLine) tOut.Append("\r\n");
        }
    }
}

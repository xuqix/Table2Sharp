using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotLiquid;

namespace Table2Sharp
{
    static class TextUtils
    {
        public static IEnumerable<string> splitline(string input)
        {
            return from s in input.Split('\n') select s;
        }
    }

    public class Generator
    {
        public static class Configuration
        {
            public static int TYPE_ROW_NUM = 0;
            public static int NAME_ROW_NUM = 1;
            public static int DESCRIPT_ROW_NUM = 2;
            public static int DATA_ROW_NUM = 4;
            public static bool USE_ANNOTATION = false;
        }

        public class ClassInfo : Drop
        {
            public class FieldInfo : Drop
            {
                public string type { get; set; }
                public string name { get; set; }
                public string comment { get; set; }
            }
            public List<FieldInfo> fields { get; set; } = new List<FieldInfo>();

            public string className { get; set; }
            public string comment { get; set; }

            public string fileName { get; set;}

            // extra info
            public int DATA_START_ROW_NUM { get; set;}
        }
        private List<ClassInfo> _classes = new List<ClassInfo>();

        // <csv_filename, config_class>
        private Dictionary<string, string> _classMap = new Dictionary<string, string>();

        public Generator(params TableFile[] tables)
        {
            foreach (var table in tables)
            {
                if (!CheckFormat(Path.GetFileNameWithoutExtension(table.FilePath)))
                    throw new Exception("Format error with filename: " + table.FilePath);

                ClassInfo classInfo = new ClassInfo();

                classInfo.className = CaseConvert(Path.GetFileNameWithoutExtension(table.FilePath), CaseType.Hungary);
                classInfo.comment = "Config File Generate By " + Path.GetFileName(table.FilePath);
                classInfo.fileName = classInfo.className + "Config";
                classInfo.DATA_START_ROW_NUM = Configuration.DATA_ROW_NUM;

                for (int i = 0; i < table.ColCount; i++)
                {
                    if (string.IsNullOrEmpty(table[Configuration.TYPE_ROW_NUM, i])) continue;

                    ClassInfo.FieldInfo field = new ClassInfo.FieldInfo();
                    field.type = table[Configuration.TYPE_ROW_NUM, i];
                    field.name = CaseConvert(table[Configuration.NAME_ROW_NUM, i], CaseType.Camel);
                    field.comment = Configuration.USE_ANNOTATION ? table[Configuration.NAME_ROW_NUM, i].annotate : table[Configuration.DESCRIPT_ROW_NUM, i];
                    classInfo.fields.Add(field);

                    if (!CheckFormat(field.type)) throw new Exception("Format error with " + field.type + " in file: " + table.FilePath); ;
                    if (!CheckFormat(field.name)) throw new Exception("Format error with " + field.name + " in file: " + table.FilePath); ;
                }

                _classes.Add(classInfo);
                _classMap.Add(Path.GetFileNameWithoutExtension(table.FilePath), classInfo.fileName);
            }
        }

        private bool CheckFormat(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z_0-9]*$");
        }

        public void DoGenerateAll(string outDir)
        {
            DotLiquid.Template.RegisterFilter(typeof(TextUtils));
            GenerateCSharpClassFiles(outDir);
        }

        private void GenerateCSharpClassFiles(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            WriteUTF8File(Path.Combine(dir, "ConfigBase.cs"), Template.BaseClassTemplate);
            WriteUTF8File(Path.Combine(dir, "ConfigManager.cs"), Template.ConfigManagerTemplate);

            var template = DotLiquid.Template.Parse(Template.ClassTemplate);
            foreach (var classInfo in _classes)
            {
                var content = template.Render(Hash.FromAnonymousObject(new { Model = classInfo }));
                string path = Path.Combine(dir, classInfo.fileName + ".cs");
                WriteUTF8File(path, content);
            }

            var template2 = DotLiquid.Template.Parse(Template.ClassFactoryTemplate);
            var dict = from d in _classMap select new { key = d.Key, value = d.Value };
            var s = template2.Render(Hash.FromAnonymousObject(new { Model = dict } ));
            WriteUTF8File(Path.Combine(dir, "ConfigFactory.cs"), s);
        }

        private void WriteUTF8File(string path, string text)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
            {
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(System.Text.Encoding.UTF8.GetBytes(text));
                }
            }
        }

        enum CaseType
        {
            Camel,
            Hungary
        }

        private string CaseConvert(string str, CaseType type)
        {
            StringBuilder result = new StringBuilder();
            var arr = str.Trim().Split('_', '-');
            for(int i = 0; i < arr.Length; i++)
            {
                var a = arr[i].ToCharArray();
                if(i == 0 && type == CaseType.Camel)
                    a[0] = Char.ToLower(a[0]);
                else
                    a[0] = Char.ToUpper(a[0]);
                result.Append(new string(a));
            }
            return result.ToString();
        }
    }
}

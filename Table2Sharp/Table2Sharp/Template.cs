namespace Table2Sharp
{
    internal static class Template
    {
        public const string BaseClassTemplate =
@"//--------------------------------------------------
//  Generator By Tool, Do Not Modify It Manually!
//--------------------------------------------------

using System.Collections.Generic;

namespace Config
{
    public class ConfigBase
    {
        /// <summary>
        /// unique key
        /// </summary>
        public int ID;
    }

    public class ConfigTable
    {
        /// <summary>
        /// All ID for Config
        /// </summary>
        public List<int> IDS = new List<int>();

        /// <summary>
        /// Config Map< ID, ConfigBase >
        /// </summary>
        public Dictionary<int, ConfigBase> Map = new Dictionary<int, ConfigBase>();

        public virtual void LoadFromCSVString(string text)
        {
            throw new System.Exception("" LoadFromCSVString methd not implement in type "" + this.GetType().Name);
        }

        protected int ParseInt(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(int);

            int v;
            if(int.TryParse(value, out v)) 
                return v;
            else 
                throw new System.Exception(""parse int type error "" + value + "" in "" + this.GetType().Name);
        }

        protected float ParseFloat(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(float);

            float v;
            if (float.TryParse(value, out v))
                return v;
            else
                throw new System.Exception(""parse int type error "" + value + "" in "" + this.GetType().Name);
        }

        protected string ParseString(string value)
        {
            return value;
        }

        protected bool ParseBool(string value)
        {
            if (string.IsNullOrEmpty(value)) return default(bool);

            bool v;
            if (bool.TryParse(value, out v))
                return v;
            else
                throw new System.Exception(""parse bool type error "" + value + "" in "" + this.GetType().Name);
        }

        protected int[] ParseIntArray(string value)
        {
            string[] data = value.Trim('[',']',' ').Split(',');
            var result = new int[data.Length];
            for(int i = 0; i < result.Length; ++i)
                result[i] = ParseInt(data[i]);
            return result;
        }

        protected float[] ParseFloatArray(string value)
        {
            string[] data = value.Trim('[',']',' ').Split(',');
            var result = new float[data.Length];
            for(int i = 0; i < result.Length; ++i)
                result[i] = ParseFloat(data[i]);
            return result;
        }

        protected string[] ParseStringArray(string value)
        {
            string[] data = value.Trim('[',']',' ').Split(',');
            var result = new string[data.Length];
            for(int i = 0; i < result.Length; ++i)
                result[i] = ParseString(data[i]);
            return result;
        }

        protected bool[] ParseBoolArray(string value)
        {
            string[] data = value.Trim('[',']',' ').Split(',');
            var result = new bool[data.Length];
            for(int i = 0; i < result.Length; ++i)
                result[i] = ParseBool(data[i]);
            return result;
        }
    }
}
";

//{{field.comment | strip_newlines}}
        public const string ClassTemplate =
@"//--------------------------------------------------
//  Generator By Tool, Do Not Modify It Manually!
//--------------------------------------------------

namespace Config
{
    /// <summary>
    /// {{Model.comment}}
    /// </summary>
    public class {{Model.class_name}} : ConfigBase
    {
{% for field in Model.fields -%}
{% assign arr = field.comment | splitline -%}
        /// <summary>
{% for line in arr -%}
        /// {{line}}
{% endfor -%}
        /// </summary>
        public {{field.type}} {{field.name}}; 

{% endfor -%}
    }

    /// <summary>
    /// {{Model.comment}}
    /// </summary>
    public class {{Model.file_name}} : ConfigTable
    {
        public override void LoadFromCSVString(string text)
        {
            string[] lines = text.Replace(""\r"", """").Split('\n');
            for(int i = {{Model.DATA_START_ROW_NUM}}; i < lines.Length; i++)
            {
                var cells = lines[i].Split('\t');
                if (cells.Length == 0 || cells[0] == string.Empty) continue;

                var config = new {{Model.class_name}}();

                config.ID = ParseInt(cells[0]);
{% for field in Model.fields -%}
{% assign i = forloop.index0 -%}
{% assign type = field.type | replace:' ','' -%}
{% case type -%}
{% when 'int' -%}
                config.{{field.name}} = ParseInt(cells[{{i}}]);
{% when 'float' -%}
                config.{{field.name}} = ParseFloat(cells[{{i}}]);
{% when 'string' -%}
                config.{{field.name}} = ParseString(cells[{{i}}]);
{% when 'bool' -%}
                config.{{field.name}} = ParseBool(cells[{{i}}]);
{% when 'int[]' -%}
                config.{{field.name}} = ParseIntArray(cells[{{i}}]);
{% when 'float[]' -%}
                config.{{field.name}} = ParseFloatArray(cells[{{i}}]);
{% when 'string[]' -%}
                config.{{field.name}} = ParseStringArray(cells[{{i}}]);
{% when 'bool[]' -%}
                config.{{field.name}} = ParseBoolArray(cells[{{i}}]);
{% endcase -%}
{% endfor -%}

                IDS.Add(config.ID);
                Map.Add(config.ID, config);
            }
        }
    }
}";

        public const string ClassFactoryTemplate =
@"//--------------------------------------------------
//  Generator By Tool, Do Not Modify It Manually!
//--------------------------------------------------

namespace Config
{
    public static class ConfigFactory
    {
        public static ConfigTable Create(string name)
        {
            switch(name)
            {
{% for o in Model -%}
                case ""{{o.key}}"": 
                    return new {{o.value}}();

{% endfor -%}
                default:
                    throw new System.Exception(""Config "" + name + "" not found!"");
            }
        }
    }
}
";

        public const string ConfigManagerTemplate =
@"using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID
using UnityEngine;
#endif

namespace Config
{
    /// <summary>
    /// You can extend this class for initizlize config in other file like this : 
    /// public static partial class ConfigManager {
    ///     public static InitializeFromOther() { do something... } 
    /// }
    /// </summary>
    public static partial class ConfigManager
    {
        // <config name, data>
        private static Dictionary<string, ConfigTable> _ConfigMap = new Dictionary<string, ConfigTable>();

        public static List<int> GetConfigKeys<T>() where T : ConfigBase
        {
            string name = typeof(T).Name + ""Config"";
            return GetConfigKeys(name);
        }

        public static List<int> GetConfigKeys(string configName)
        {
            ConfigTable table;
            if (_ConfigMap.TryGetValue(configName, out table))
                return table.IDS;
            return null;
        }

        public static T GetConfig<T>(int id) where T : ConfigBase
        {
            string name = typeof(T).Name + ""Config"";
            return GetConfig<T>(name, id);
        }

        public static T GetConfig<T>(string configName, int id) where T : ConfigBase
        {
            ConfigTable table;
            if (_ConfigMap.TryGetValue(configName, out table))
            {
                ConfigBase config;
                if (table.Map.TryGetValue(id, out config))
                    return config as T;
            }
            return null;
        }

        public static void InitializeFromFiles(string dir)
        {
            var files = GetAllFiles(AbstractPath(dir), ""*.txt"");
            foreach (var file in files)
            {
                var str = File.ReadAllText(file.FullName);
                LoadConfig(file.FullName, str);
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IPHONE || UNITY_ANDROID
        public static void InitializeFromResources(string dir)
        {
            var configs = Resources.LoadAll(dir);
            foreach(var config in configs)
            {
                string configName = config.name;
                string content = (config as TextAsset).text;
                LoadConfig(configName, content);
            }
        }
#endif

        private static void LoadConfig(string fileName, string content)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var config = ConfigFactory.Create(name);
            config.LoadFromCSVString(content);
            _ConfigMap.Add(config.GetType().Name, config);
        }

        private static string AbstractPath(string path)
        {
            return Path.IsPathRooted(path) ? path : System.IO.Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + path;
        }

        private static List<FileInfo> GetAllFiles(string dirPath, string searchPattern = ""*"")
        {
            List<FileInfo> files = new List<FileInfo>();

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists) return files;
            files.AddRange(dir.GetFiles(searchPattern));

            DirectoryInfo[] subDir = dir.GetDirectories();
            foreach (var d in subDir)
            {
                files.AddRange(GetAllFiles(dirPath + Path.DirectorySeparatorChar + d.ToString(), searchPattern));
            }
            return files;
        }
    }
}
";
    }
}

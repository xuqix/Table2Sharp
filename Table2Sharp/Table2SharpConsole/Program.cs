using System;
using System.IO;
using System.Collections.Generic;
using Table2Sharp;
using CommandLine;
using CommandLine.Text;

namespace Table2SharpConsole
{
    class Options
    {
        [Option('d', "dir", Required = true, DefaultValue = "数据表", HelpText = "要被处理的数据目录,支持相对路径和绝对路径")]
        public string InputDirectory { get; set; }

        [Option('o', "output", Required = true, DefaultValue = "生成表", HelpText = "生成TSV目标目录,支持相对路径和绝对路径")]
        public string OutputDirectory { get; set; }

        [Option('g', "generator", Required = false, HelpText = "代码生成目标目录,支持相对路径和绝对路径")]
        public string SharpOutputDirectory { get; set; }

        [Option("annotation", HelpText = "是否使用批注生成注释")]
        public bool IsAnnotation { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return "\n\t\tExcel格式转换和C#代码生成工具\n\n" + HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
                return;

            string srcDir = Utils.AbstractPath(options.InputDirectory);
            string dstDir = Utils.AbstractPath(options.OutputDirectory);
            if (!Directory.Exists(srcDir))
            {
                Console.WriteLine("目录{0}不存在!", srcDir);
                return;
            }

            List<TableFile> list = new List<TableFile>();
            var files = Utils.GetAllFiles(srcDir, "*.xls");
            foreach (var file in files)
            {
                TableFile excel = TableFile.Create(file.FullName);
                if (excel == null) break;

                string dstFile = Path.Combine(dstDir, Path.GetFileNameWithoutExtension(file.Name)) + ".txt";
                bool ok = excel.SaveToTSV(dstFile);
                if (!ok) break;
                Console.WriteLine("输出文件:{0}", dstFile);
                list.Add(excel);
            }
            if(!string.IsNullOrEmpty(options.SharpOutputDirectory))
            {
                if (options.IsAnnotation)
                    Generator.Configuration.USE_ANNOTATION = true;
                string sharpDir = Utils.AbstractPath(options.SharpOutputDirectory);
                new Generator(list.ToArray()).DoGenerateAll(sharpDir, true);
                Console.WriteLine("\n输出CSharp文件到目录 :{0} ", sharpDir);
            }
        }
    }
}

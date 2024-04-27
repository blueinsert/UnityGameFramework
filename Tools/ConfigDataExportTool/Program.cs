using bluebean.ConfigDataExportTool.Config;
using System;
using System.IO;
using System.Xml.Serialization;

namespace bluebean.ConfigDataExportTool
{
    class Program
    {
        private static void PrepareOutputFolder(string outPath)
        {
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            var dataFolder = outPath + "/" + "Data";
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            var codeFolder = outPath + "/" + "Code";
            if (!Directory.Exists(codeFolder))
            {
                Directory.CreateDirectory(codeFolder);
            }
        }

        private static bool GetParams(string[] args, ref string path,ref string outPath,ref string format)
        {
            bool hasInput = false;
            bool hasOut = false;
            bool hasFormat = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                    case "-I":
                        path = args[i + 1];
                        hasInput = true;
                        break;
                    case "-o":
                    case "-O":
                        outPath = args[i + 1];
                        hasOut = true;
                        break;
                    case "-f":
                    case "-F":
                        format = args[i + 1];
                        hasFormat = true;
                        break;
                }
            }

            //check
            //todo

            var res = hasInput && hasOut && hasFormat;
            return res;
        }

        private static void Test()
        {
            PrepareOutputFolder("./Output");
            var cfg = new ToolConfig();
            cfg.NameSpace = "bluebean.ConfigData";    
            ConfigDataManager.CreateInstance();
            ConfigDataManager.Instance.SetConfig(cfg);
            //for test
            ConfigDataManager.Instance.ProcessFolder("./Input", "./Output", "json");
        }

        private static ToolConfig LoadConfig()
        {
            ToolConfig cfg = null;
            var path = "./config.xml";
            if (!File.Exists(path))
            {
                cfg = new ToolConfig();
                cfg.NameSpace = "bluebean.ConfigData";

                var bytes = SerializationHelper.SerializeToXml(cfg);
                FileStream fs = new FileStream(path, FileMode.Create);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();

                return cfg;
            }
            else
            {
                cfg = SerializationHelper.DeserializeWithXml<ToolConfig>(File.ReadAllBytes(path));
                return cfg;
            }
           
        }

        static void Main(string[] args)
        {
            bool test = false;
            if (test)
            {
                Test(); return;
            }
            Console.WriteLine("\n\nThe Command:\n");
            foreach(var arg in args)
            {
                Console.Write(arg + " ");
            }
            Console.WriteLine();

            //prepare params
            string path = "./";//默认当前路径
            string outPath = "./";
            string format = "json";//数据序列化格式
            if(!GetParams(args, ref path, ref outPath, ref format))
            {
                Console.WriteLine("输入格式有误!\n");
                return;
            }
            
            PrepareOutputFolder(outPath);
            var cfg = LoadConfig();
            ConfigDataManager.CreateInstance();
            ConfigDataManager.Instance.SetConfig(cfg);
            try
            {
                ConfigDataManager.Instance.ProcessFolder(path, outPath, format);
                Console.WriteLine("一键导出成功!");
            }catch(ConfigDataException e1)
            {
                Console.WriteLine(e1.ToString());
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString() + e.StackTrace);
            }
            
        }
    }
}

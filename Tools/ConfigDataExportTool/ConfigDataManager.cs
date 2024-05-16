using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.CodeDom;
using bluebean.ConfigDataExportTool.Config;

namespace bluebean.ConfigDataExportTool
{
    public delegate object GetValueByStr(string valeuStr);

    class ConfigDataManager
    {
        private static ConfigDataManager m_instance;
        public static ConfigDataManager Instance
        {
            get
            {
                return m_instance;
            }
        }
        public static void CreateInstance()
        {
            m_instance = new ConfigDataManager();
        }

        private Dictionary<string, ConfigDataTable> m_configDataDic = new Dictionary<string, ConfigDataTable>();

        private Dictionary<string, Type> m_basicTypeDic = new Dictionary<string, Type>();

        private Dictionary<string, GetValueByStr> m_valueParserDic = new Dictionary<string, GetValueByStr>();

        private Dictionary<string, CodeTypeDeclaration> m_subTypeDeclarationDic = new Dictionary<string, CodeTypeDeclaration>();

        private Dictionary<string, CodeTypeDeclaration> m_configDataTypeDeclarationDic = new Dictionary<string, CodeTypeDeclaration>();

        private Assembly m_codeDefineAssembly;

        //private ForeignKeyCheckInfo m_checking = new ForeignKeyCheckInfo();

        ToolConfig m_cfg;

        private void CollectTypeStrToTypeDic()
        {
            m_basicTypeDic.Add("int16", typeof(Int16));
            m_basicTypeDic.Add("int32", typeof(Int32));
            m_basicTypeDic.Add("int64", typeof(Int64));
            m_basicTypeDic.Add("uint16", typeof(UInt16));
            m_basicTypeDic.Add("uint32", typeof(UInt32));
            m_basicTypeDic.Add("uint64", typeof(UInt64));
            m_basicTypeDic.Add("double", typeof(Double));
            m_basicTypeDic.Add("string", typeof(String));
            m_basicTypeDic.Add("bool", typeof(Boolean));
            m_basicTypeDic.Add("float", typeof(float));
        }

        private void CollectGetValueByStrDic()
        {
            m_valueParserDic.Add("int16", GetInt16);
            m_valueParserDic.Add("int32", GetInt32);
            m_valueParserDic.Add("int64", GetInt64);
            m_valueParserDic.Add("uint16", GetUInt16);
            m_valueParserDic.Add("uint32", GetUInt32);
            m_valueParserDic.Add("uint64", GetUInt64);
            m_valueParserDic.Add("double", GetDouble);
            m_valueParserDic.Add("string", GetString);
            m_valueParserDic.Add("bool", GetBool);
            m_valueParserDic.Add("float", GetFloat);
        }

        private object GetInt16(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Int16.Parse("0");
            }
            return Int16.Parse(str);
        }

        private object GetInt32(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Int32.Parse("0");
            }
            return Int32.Parse(str);
        }

        private object GetInt64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Int64.Parse("0");
            }
            return Int64.Parse(str);
        }

        private object GetUInt16(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return UInt16.Parse("0");
            }
            return UInt16.Parse(str);
        }

        private object GetUInt32(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return UInt32.Parse("0");
            }
            return UInt32.Parse(str);
        }

        private object GetUInt64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return UInt64.Parse("0");
            }
            return UInt64.Parse(str);
        }

        private object GetDouble(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Double.Parse("0");
            }
            return Double.Parse(str);
        }

        private object GetFloat(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return float.Parse("0");
            }
            return float.Parse(str);
        }

        private object GetBool(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            return bool.Parse(str);
        }

        private object GetString(string str)
        {
            return str;
        }

        /// <summary>
        /// 根据类型字符串得到类型(基础数据类型)
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        public Type TypeStr2Type(string typeStr)
        {
            typeStr = typeStr.ToLower();
            if (m_basicTypeDic.ContainsKey(typeStr))
            {
                return m_basicTypeDic[typeStr];
            }
            return null;
        }

        private ConfigDataManager()
        {
            CollectTypeStrToTypeDic();
            CollectGetValueByStrDic();
        }

        public void SetConfig(ToolConfig cfg)
        {
            m_cfg = cfg;
        }

        #region 类型收集

        private void CollectSubTypeDeclaration(ConfigDataTable configData)
        {
            foreach (var pair in configData.HeadInfo.ColumnInfoDic)
            {
                var columnInfo = pair.Value;
                //收集子类型
                if (columnInfo.HasSubTypeDef())
                {
                    var subTypeName = columnInfo.GetSubTypeName();
                    if (!m_subTypeDeclarationDic.ContainsKey(subTypeName))
                    {
                        Console.WriteLine(string.Format("BuildSubTypeDeclaration {0}", subTypeName));
                        var subType = columnInfo.BuildSubTypeDeclaration(subTypeName);
                        m_subTypeDeclarationDic.Add(subTypeName, subType);
                    }
                }
            }
        }

        private void CollectSubTypeDeclarations()
        {
            foreach (var pair in m_configDataDic)
            {
                CollectSubTypeDeclaration(pair.Value);
            }
        }

        public Type GetSubType(string subTypeName)
        {
            var type = m_codeDefineAssembly.GetType(m_cfg.NameSpace +"."+ subTypeName);
            return type;
        }

        private void CollectConfigDataTypeDeclaration(ConfigDataTable configData)
        {
            Console.WriteLine(string.Format("Build TypeDeclaration {0}", configData.HeadInfo.m_name));
            var typeDeclaration = configData.BuildCodeTypeDeclaration();
            m_configDataTypeDeclarationDic.Add(configData.HeadInfo.m_name, typeDeclaration);
        }

        private void CollectConfigDataTypeDeclarations()
        {
            foreach (var pair in m_configDataDic)
            {
                CollectConfigDataTypeDeclaration(pair.Value);
            }
        }

        public Type GetConfigDataType(string tableName)
        {
            var type = m_codeDefineAssembly.GetType(m_cfg.NameSpace + ".ConfigData" + tableName);
            return type;
        }


        private void GenTypesFromTypeDeclarations(string outPath)
        {
            var codeGenerater = new ConfigDataCodeGenerator();
            codeGenerater.SetNamespace(m_cfg.NameSpace);
            codeGenerater.SetCodeFileName("ConfigDataTypeDefine.cs");
            codeGenerater.SetOutPath(outPath + "/Code");
            var typeDeclarationDic = new Dictionary<string, CodeTypeDeclaration>();
            foreach (var pair in m_subTypeDeclarationDic)
            {
                typeDeclarationDic.Add(pair.Key, pair.Value);
            }
            foreach (var pair in m_configDataTypeDeclarationDic)
            {
                typeDeclarationDic.Add(pair.Key, pair.Value);
            }
            //动态产生assembly
            codeGenerater.ConstructCompileUnit(typeDeclarationDic);
            codeGenerater.GenerateCode();
            Console.WriteLine("产生代码文件成功");
            Assembly assembly;
            if(!codeGenerater.GetAeeembly(out assembly))
            {
                throw new ConfigDataException(null, null, 0, "动态产生assembly失败!");
            }
            m_codeDefineAssembly = assembly;
            Console.WriteLine("动态产生assembly成功");
            
        }

        #endregion

        private DataGrid LoadDataGrid(string filePath)
        {
            CsvParser parser = new CsvParser();
            var dataGrid = parser.ParseFromFile(filePath);
            return dataGrid;
        }

        private ConfigDataTable LoadConfigDataAtPath(string filePath)
        {
            Console.WriteLine(string.Format("加载：{0}", filePath));
            var dataGrid = LoadDataGrid(filePath);
            ConfigDataTable configData = new ConfigDataTable();
            configData.SetFilePath(filePath);
            configData.SetDataGrid(dataGrid);
            configData.ParseTableHeadInfo();
            m_configDataDic.Add(configData.HeadInfo.m_name, configData);
            return configData;
        }

        private void LoadAllConfigDataAtPath(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var fileInfos = directoryInfo.GetFiles("*.csv", SearchOption.AllDirectories);
            foreach (var file in fileInfos)
            {
                LoadConfigDataAtPath(file.FullName);
            }
        }

        private ConfigDataTable GetConfigDataByName(string name)
        {
            if (m_configDataDic.ContainsKey(name))
            {
                return m_configDataDic[name];
            }
            return null;
        }

        #region 检查外键约束

        /// <summary>
        /// 外键值是否在主表对应列存在
        /// </summary>
        /// <param name="configData"></param>
        /// <param name="mainColumnInfo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsContainValueInMainConfigData(ConfigDataTableColumnInfo mainColumnInfo, string value)
        {
            var configData = mainColumnInfo.m_configDataTable;
            int startRow = configData.HeadInfo.TableType == ConfigDataType.DATA ? ConfigDataTableConst.DataStartRow : ConfigDataTableConst.EnumStartRow;
            var data = configData.Data;
            for (int i = startRow; i < data.Row; i++)
            {
                if (data.ReadCell(i, mainColumnInfo.m_index) == value)
                {
                    return true;
                }
            }
            return false;
        }

        public void FindMainColumnInfo(string tableName,string columnName,int row, string foreignKey,out ConfigDataTableColumnInfo mainColumnInfo)
        {
            string[] splits = foreignKey.Split(new char[] { '.' });
            if (splits.Length != 2)
            {
                ConfigDataException except = new ConfigDataException(tableName,columnName,row,string.Format("外键{0}定义错误",foreignKey));
                throw except;
            }
            var mainTableName = splits[0];
            var mainColumnName = splits[1];
            var mainConfigData = GetConfigDataByName(mainTableName);
            if (mainConfigData == null)
            {
                ConfigDataException except = new ConfigDataException(tableName, columnName, row, string.Format("不能找到外键{0}对应的主表", foreignKey));
                throw except;
            }
            if (!mainConfigData.HeadInfo.ContainColumn(mainColumnName))
            {
                ConfigDataException except = new ConfigDataException(tableName, columnName, row, string.Format("不能找到外键{0}在主表中对应的列", foreignKey));
                throw except;
            }
            mainColumnInfo = mainConfigData.HeadInfo.GetColumnInfo(mainColumnName);
            //if (!IsContainValueInMainConfigData(mainColumnInfo, value))
            //{
           //     error = string.Format("无法找到值{0}在主表{1}.{2},来源{3}.{4} 行号:{5}", value, mainConfigData.Name, mainColumnInfo.m_name, m_checking.m_columnInfo.m_configData.Name, m_checking.m_columnInfo.m_name, m_checking.m_row); ;
           //     return false;
            //}
        }

        private void CheckForeignKey(ConfigDataTableColumnInfo columnInfo)
        {
            columnInfo.CollectForeignKeyMainColumnInfo();
            if (!columnInfo.m_hasForeignKey)
            {
                return;
            }
            var configData = columnInfo.m_configDataTable;
            var data = configData.Data;
            var tableName = configData.HeadInfo.m_name;
            var columnName = columnInfo.m_name;
            //检查外键的列的每一行，查看是否在主表中存在值
            int startRow = configData.HeadInfo.TableType == ConfigDataType.DATA ? ConfigDataTableConst.DataStartRow : ConfigDataTableConst.EnumStartRow;
            if (!columnInfo.IsListType())
            {
                var mainColumnInfo = columnInfo.m_mainColumnInfoArray[0];
                for (int i = startRow; i < data.Row; i++)
                {
                    string value = data.ReadCell(i, columnInfo.m_index);
                    if (!IsContainValueInMainConfigData(mainColumnInfo, value))
                    {
                        ConfigDataException except = new ConfigDataException(tableName, columnName, i, string.Format("外键列数据{0}在主表列中找不到",value));
                        throw except;
                    }
                }
            }
            else
            {
                
                for (int i =0;i< columnInfo.m_mainColumnInfoArray.Length;i++)
                {
                    var mainColumnInfo = columnInfo.m_mainColumnInfoArray[i];
                    if (mainColumnInfo != null)
                    {
                        for (int j = startRow; j < data.Row; j++)
                        {
                            string cellValue = data.ReadCell(j, columnInfo.m_index);
                            string[] listElemArray = cellValue.Split(new char[] { ConfigDataTableConst.ListElemSplit });
                            string value = listElemArray[i];
                            if (!IsContainValueInMainConfigData(mainColumnInfo, value))
                            {
                                ConfigDataException except = new ConfigDataException(tableName, columnName, j, string.Format("外键列数据{0}在主表列中找不到", value));
                                throw except;
                            }
                        }
                    }
                }
            }
        }
        
        private void CheckForeignKey()
        {
            Console.WriteLine(string.Format("检查外检约束"));
            foreach (var pair in m_configDataDic)
            {
                var configDataTable = pair.Value;
                foreach (var columnPair in configDataTable.HeadInfo.ColumnInfoDic)
                {
                    CheckForeignKey(columnPair.Value); 
                }
            }
        }

        #endregion

        #region 序列化表数据
        /// <summary>
        /// 根据类型字符串和字符串表示的值获得该值(基本类型)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object GetValueByStr(string type, string value)
        {
            type = type.ToLower();
            if (m_valueParserDic.ContainsKey(type))
            {
                return m_valueParserDic[type](value);
            }
            return null;
        }

        public void SetObjectField(object obj,string fieldTypeStr, string fieldName,string valueStr)
        {
            var type = obj.GetType();
            var filedInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var value = GetValueByStr(fieldTypeStr, valueStr);
            filedInfo.SetValue(obj, value);
        }

        public object GetListValueByStr(ConfigDataTableColumnInfo columnInfo, string value)
        {
            if (columnInfo.HasSubTypeDef())
            {
                var subTypeName = columnInfo.GetSubTypeName();
                Type subType = GetSubType(subTypeName);
                var listType = typeof(List<>).MakeGenericType(subType);
                var list = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");
                string[] listElemArray = value.Split(new char[] { ConfigDataTableConst.ListElemSplit });
                int paramCount = columnInfo.m_subTypeParamTypes.Length;
                for (int i = 0; i < listElemArray.Length; i++)
                {
                    var listElemValueStr = listElemArray[i];
                    string[] paramList = listElemValueStr.Split(new char[] { ConfigDataTableConst.ListElemParamSplit });
                    var listElem = Activator.CreateInstance(subType);
                    for (int j = 0; j < paramCount; j++)
                    {
                        string paramValue = paramList[j];
                        string typeStr = columnInfo.m_subTypeParamTypes[j];
                        string typeName = "m_"+columnInfo.m_subTypeParamNames[j];
                        SetObjectField(listElem, typeStr, typeName, paramValue);
                    }
                    addMethod.Invoke((object)list, new object[] { listElem });
                }
                return list;
            }
            else
            {
                Type type = TypeStr2Type(columnInfo.m_subTypeParamTypes[0]);
                var listType = typeof(List<>).MakeGenericType(type);
                var list = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");
                string[] listElemArray = value.Split(new char[] { ConfigDataTableConst.ListElemSplit });
                int paramCount = columnInfo.m_subTypeParamTypes.Length;
                //assert(paramCount == 1);
                for (int i = 0; i < listElemArray.Length; i++)
                {
                    var listElemValueStr = listElemArray[i];
                    string[] paramList = listElemValueStr.Split(new char[] { ConfigDataTableConst.ListElemParamSplit });
                    //assert(paramList.Length == 1);
                    string typeStr = columnInfo.m_subTypeParamTypes[0];
                    string paramValue = paramList[0];
                    var listElem = GetValueByStr(typeStr, paramValue);
                    addMethod.Invoke((object)list, new object[] { listElem });
                }
                return list;
            }
        }

        public object GetValueByStrForColumn(ConfigDataTableColumnInfo columnInfo,string value)
        {
            if (!columnInfo.IsListType())
            {
                return GetValueByStr(columnInfo.m_type, value);
            }
            else
            {
                return GetListValueByStr(columnInfo, value);
            }
        }

        /// <summary>
        /// 使用反射产生对应表的List数据
        /// </summary>
        /// <param name="configTableDataTypeDefine"></param>
        /// <param name="configData"></param>
        /// <returns></returns>
        private System.Object GetValueForConfigData(ConfigDataTable configData)
        {
            var configDataType = GetConfigDataType(configData.HeadInfo.Name);
            var listType = typeof(List<>).MakeGenericType(configDataType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");
            var dataGrid = configData.Data;
            if (dataGrid.Row > ConfigDataTableConst.DataStartRow)
            {
                for (int i = ConfigDataTableConst.DataStartRow; i < dataGrid.Row; i++)
                {
                    var data = Activator.CreateInstance(configDataType);
                    foreach (var pair in configData.HeadInfo.ColumnInfoDic)
                    {
                        var columnIndex = pair.Key;
                        var columnInfo = pair.Value;
                        var filedInfo = configDataType.GetField("m_" + columnInfo.m_name, BindingFlags.NonPublic | BindingFlags.Instance);
                        string valueStr = dataGrid.ReadCell(i, columnIndex);
                        var value = GetValueByStrForColumn(columnInfo, valueStr);
                        //Console.WriteLine(string.Format("{0} {1} {2}", filedInfo.FieldType, value.GetType(), filedInfo.FieldType == value.GetType()));
                        filedInfo.SetValue(data, value);
                    }
                    addMethod.Invoke((object)list, new object[] { data });
                }
            }
            return list;
        }

        /// <summary>
        /// 序列化配置表数据
        /// </summary>
        /// <returns></returns>
        public void SerializeConfigData(ConfigDataTable configData, string outPath, string format)
        {
            if (configData.HeadInfo.TableType == ConfigDataType.ENUM)
            {
                return;
            }
            var listObj = GetValueForConfigData(configData);
            byte[] data;
            string suffix;
            switch (format)
            {
                case "bin":
                    data = SerializationHelper.SerializeToBinary(listObj);
                    suffix = ".bin";
                    break;
                case "json":
                    data = SerializationHelper.SerializeToJson(listObj);
                    suffix = ".json";
                    break;
                case "xml":
                    data = SerializationHelper.SerializeToXml(listObj);
                    suffix = ".xml";
                    break;
                default:
                    data = SerializationHelper.SerializeToBinary(listObj);
                    suffix = ".bin";
                    break;
            }
            FileStream fs = new FileStream(outPath + "/ConfigData" + configData.HeadInfo.Name + suffix, FileMode.Create);
            fs.Write(data, 0, data.Length);
            fs.Close();
        }

        public void SerializeConfigDatas(string path,string format)
        {
            foreach (var pair in m_configDataDic)
            {
                Console.WriteLine(string.Format("序列化表数据：{0}", pair.Value.HeadInfo.Name));
                SerializeConfigData(pair.Value, path + "/Data", format);
            }
        }
        #endregion

        /// <summary>
        /// 产生ConfigDataLoaderAutoGen.cs
        /// </summary>
        private void GenerateConfigDataLoaderAutoGenFile(string outPath)
        {
            var allConfigDataInfos = new List<ConfigDataTable>(m_configDataDic.Values);
            //产生ConfigDataLoaderAutoGen.cs，运行时读取功能
            {
                string template = File.ReadAllText(AppContext.BaseDirectory + "/Templetes/ConfigDataLoaderAutoGen.cs.txt");
                string result = NVelocityHelper.Parse(template, "AllConfigDataInfos", allConfigDataInfos);
                File.WriteAllText(outPath + "/Code/ConfigDataLoaderAutoGen.cs", result);
            }
        }

        public void ProcessFolder(string inPath, string outPath, string format)
        {
            //加载ConfigData
            LoadAllConfigDataAtPath(inPath);
            //检查外键约束
            CheckForeignKey();
            //收集类型
            CollectSubTypeDeclarations();
            CollectConfigDataTypeDeclarations();
            GenTypesFromTypeDeclarations(outPath);
            GenerateConfigDataLoaderAutoGenFile(outPath);
            //序列化表格资源
            SerializeConfigDatas(outPath, format);
        }
    }
}

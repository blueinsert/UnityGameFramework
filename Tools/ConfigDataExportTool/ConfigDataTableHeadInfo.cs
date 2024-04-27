using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{

    /// <summary>
    /// 数据表类型
    /// </summary>
    public enum ConfigDataType
    {
        /// <summary>
        /// 枚举表
        /// </summary>
        ENUM,
        /// <summary>
        /// 数据表
        /// </summary>
        DATA,
    }

    public class ConfigDataTableHeadInfo
    {
        public Dictionary<string, int> EnumDic
        {
            get
            {
                return m_enumDic;
            }
        }

        public Dictionary<int, ConfigDataTableColumnInfo> ColumnInfoDic
        {
            get
            {
                return m_columnInfoDic;
            }
        }

        public ConfigDataType TableType
        {
            get
            {
                return m_type;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public ConfigDataTable m_table;

        /// <summary>
        /// 表名
        /// </summary>
        public string m_name;
        /// <summary>
        /// 说明
        /// </summary>
        private string m_desc;

        ConfigDataType m_type;

        private Dictionary<int, ConfigDataTableColumnInfo> m_columnInfoDic = new Dictionary<int, ConfigDataTableColumnInfo>();

        /// <summary>
        /// 枚举表信息
        /// </summary>
        private Dictionary<string, int> m_enumDic = new Dictionary<string, int>();

        public void SetOwner(ConfigDataTable table)
        {
            m_table = table;
        }

        public void Parse(DataGrid dataGrid)
        {
            m_desc = dataGrid.ReadCell(0, 0);
            string typeStr = dataGrid.ReadCell(1, 0);
            if (!Enum.TryParse<ConfigDataType>(typeStr, out m_type))
            {
                //todo
            }
            m_name = dataGrid.ReadCell(1, 1);

            if (m_type == ConfigDataType.DATA)
            {
                //收集表头信息
                m_columnInfoDic.Clear();
                var exportTypeLine = dataGrid.ReadLine(2);
                for (int i = 0; i < exportTypeLine.Length; i++)
                {
                    if (exportTypeLine[i] == "NONE")
                    {
                        continue;
                    }
                    ConfigDataTableColumnInfo columnInfo = new ConfigDataTableColumnInfo()
                    {
                        m_configDataTable = m_table,
                        m_index = i,
                        m_exportType = dataGrid.ReadCell(2, i),
                        m_name = dataGrid.ReadCell(3, i),
                        m_desc = dataGrid.ReadCell(4, i),
                        m_type = dataGrid.ReadCell(5, i),
                        m_subTypeParamNames = dataGrid.ReadCell(6, i).Split(new char[] { ',' }),
                        m_subTypeParamDescs = dataGrid.ReadCell(7, i).Split(new char[] { ',' }),
                        m_subTypeParamTypes = dataGrid.ReadCell(8, i).Split(new char[] { ',' }),
                        m_subTypeParamDefaultValues = dataGrid.ReadCell(9, i).Split(new char[] { ',' }),
                        m_foreignKeys = dataGrid.ReadCell(10, i).Split(new char[] { ',' }),
                    };
                    columnInfo.Check();
                    m_columnInfoDic.Add(i, columnInfo);
                }
            }
            else if (m_type == ConfigDataType.ENUM)
            {
                m_columnInfoDic.Clear();
                for (int i = 0; i < 2; i++)
                {
                    ConfigDataTableColumnInfo columnInfo = new ConfigDataTableColumnInfo()
                    {
                        m_configDataTable = m_table,
                        m_index = i,
                        m_exportType = dataGrid.ReadCell(2, i),
                        m_name = dataGrid.ReadCell(3, i),
                        m_desc = dataGrid.ReadCell(4, i),
                        m_type = i == 0 ? "INT32" : "STRING",
                    };
                    columnInfo.Check();
                    m_columnInfoDic.Add(i, columnInfo);
                }
                for (int i = ConfigDataTableConst.EnumStartRow; i < dataGrid.Row; i++)
                {
                    try
                    {
                        m_enumDic.Add(dataGrid.ReadCell(i, 1), int.Parse(dataGrid.ReadCell(i, 0)));
                    }
                    catch(Exception e)
                    {
                        ConfigDataException newE = new ConfigDataException(Name, "", 0, "重复key值");
                        throw newE;
                    }
                }
            }
        }

        public bool ContainColumn(string name)
        {
            foreach (var pair in m_columnInfoDic)
            {
                if (pair.Value.m_name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public ConfigDataTableColumnInfo GetColumnInfo(string name)
        {
            foreach (var pair in m_columnInfoDic)
            {
                if (pair.Value.m_name == name)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public string GetColumnType(string name)
        {
            foreach (var pair in m_columnInfoDic)
            {
                if (pair.Value.m_name == name)
                {
                    return pair.Value.m_type;
                }
            }
            return "";
        }
    }
}

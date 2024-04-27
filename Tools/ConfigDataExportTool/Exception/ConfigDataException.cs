using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{
    public class ConfigDataException :Exception
    {
        public string m_info;

        public ConfigDataException(string tableName, string columnName,int row, string detail)
        {
            m_info = string.Format("{3}, in table:{0} column:{1} row:{2}", tableName, columnName, row, detail);
        }

        public override string ToString()
        {
            return m_info;
        }
    }
}

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
            m_info = string.Format("发生异常！\n table:{0}\n column:{1}\n row:{2}\n detail:{3}\n, ", tableName, columnName, row, detail);
        }

        public override string ToString()
        {
            return m_info;
        }
    }
}

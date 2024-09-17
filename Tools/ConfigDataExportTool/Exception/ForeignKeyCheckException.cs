using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{
    public class ForeignKeyCheckException : Exception
    {
        public string m_info;

        public ForeignKeyCheckException(string value, string mainTableName,string mainTableColumnName)
        {
            m_info = string.Format($"检查外检约束异常！ [{value}]无法在主表[{mainTableName}],主列[{mainTableColumnName}]中找到, CurProcess:[{ConfigDataManager.Instance.CurProcessEnv.ToString()}]");
        }

        public override string ToString()
        {
            return m_info;
        }
    }
}

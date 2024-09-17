using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{
    public class StringParseException : Exception
    {
        public string m_info;

        public StringParseException(string value, string targetType)
        {
            m_info = string.Format($"无法将[{value}] 转为目标类型[{targetType}], CurProcess:[{ConfigDataManager.Instance.CurProcessEnv.ToString()}]");
        }

        public override string ToString()
        {
            return m_info;
        }
    }
}

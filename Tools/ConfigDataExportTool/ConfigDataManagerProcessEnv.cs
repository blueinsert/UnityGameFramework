using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{
    public class ConfigDataManagerProcessEnv
    {
        public string m_curFilePath;
        public string m_curTableName;
        public string m_curColumnName;
        public int m_curColumnIndex;
        public int m_curRowIndex;

        public void Clear()
        {
            m_curFilePath = null;
            m_curTableName = null;
            m_curColumnName = null;
            m_curColumnIndex = -1;
            m_curRowIndex = -1;
        }

        public override string ToString()
        {
            return string.Format($"表名:{m_curTableName} 列名:{m_curColumnName} 行号:{m_curRowIndex+1}");
        }
    }
}

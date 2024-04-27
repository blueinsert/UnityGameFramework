using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{
    public class DataGrid
    {
        /// <summary>
        /// 解析后的的string网格
        /// </summary>
        private List<string[]> m_grid = new List<string[]>();

        public DataGrid(List<string[]> grid)
        {
            m_grid = grid;
        }

        public int Row { 
            get {
                return m_grid.Count;
            } 
        }
        int Column { 
            get {
                return m_grid[0].Length;
            } 
        }

        public string[] ReadLine(int n)
        {
            return m_grid[n];
        }

        public string ReadCell(int x, int y)
        {
            return ReadLine(x)[y];
        }
    }
}

using System.Collections.Generic;

namespace bluebean.PathFinding
{
    /// <summary>
    /// 用于Dijkstra算法寻路的节点
    /// </summary>
    public class DJNode
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="links"></param>
        public DJNode(int id, List<DJLink> links = null)
        {
            m_id = id;
            m_links = links;
        }

        /// <summary>
        /// 添加边
        /// </summary>
        /// <param name="link"></param>
        public void AddLink(DJLink link)
        {
            if (m_links == null)
            {
                m_links = new List<DJLink>();
            }
            m_links.Add(link);
        }

        /// <summary>
        /// 节点的ID
        /// </summary>
        public int m_id;

        /// <summary>
        /// 当前节点所连接的路径，通过路径到达其他节点 
        /// </summary>
        public List<DJLink> m_links;
    }
}

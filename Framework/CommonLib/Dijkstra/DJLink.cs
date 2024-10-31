using System.Collections.Generic;

namespace bluebean.PathFinding
{
    /// <summary>
    /// 描述Dijkstra节点之间的连接路径
    /// </summary>
    public class DJLink
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <param name="cost"></param>
        public DJLink(int fromId, int toId, int cost)
        {
            m_fromNodeId = fromId;
            m_toNodeId = toId;
            m_cost = cost;
        }

        /// <summary>
        /// 从哪个节点来
        /// </summary>
        public int m_fromNodeId;

        /// <summary>
        /// 可以去哪个节点
        /// </summary>
        public int m_toNodeId;

        /// <summary>
        /// 通过本路径的消耗 
        /// </summary>           
        public int m_cost;
    }
}

using System.Collections.Generic;

namespace bluebean.PathFinding
{
    /// <summary>
    /// Dijkstra算法实现，Tiny版本
    /// 内部使用Dictionary代替Array来存储节点，对于节点量不大，Id不连续的情况可以有效的节省内存。
    /// </summary>    
    public class DijkstraTiny : Dijkstra
    {
        /// <summary>
        /// 添加一组新节点
        /// </summary>
        /// <param name="nodes"></param>
        public override void NodesAdd(IList<DJNode> nodes)
        {
            // 添加或合并节点
            foreach (var node in nodes)
            {
                if (!m_allNodeDict.TryGetValue(node.m_id, out var oldNode))
                {
                    m_allNodeDict.Add(node.m_id, node);
                }
                else
                {
                    NodeMerge(oldNode, node);
                }
            }
        }

        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <param name="node"></param>
        public override void NodeAdd(DJNode node)
        {
            // 添加或合并节点
            if (!m_allNodeDict.TryGetValue(node.m_id, out var oldNode))
            {
                m_allNodeDict.Add(node.m_id, node);
            }
            else
            {
                NodeMerge(oldNode, node);
            }
        }

        /// <summary>
        /// 通过id反回node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override DJNode DJNodeGetById(int id)
        {
            if (!m_allNodeDict.ContainsKey(id))
            {
                return null;
            }

            return m_allNodeDict[id];
        }

        /// <summary>
        /// 所有的节点，使用Dict存储来节省内存
        /// </summary>
        protected Dictionary<int, DJNode> m_allNodeDict = new Dictionary<int, DJNode>();
    }
}

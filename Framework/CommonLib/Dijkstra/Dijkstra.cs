using System;
using System.Collections.Generic;

namespace bluebean.PathFinding
{
    /// <summary>
    /// Dijkstra算法实现
    /// </summary>    
    public class Dijkstra
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="nodes"></param>
        public void Initialize(IList<DJNode> nodes = null)
        {
            m_defaultFindingCtx = new FindingCtx(this);

            // 如果有初始化的节点列表，添加节点
            if (nodes != null)
            {
                NodesAdd(nodes);
            }
        }

        #region 节点添加、获取

        /// <summary>
        /// 添加一组新节点
        /// </summary>
        /// <param name="nodes"></param>
        public virtual void NodesAdd(IList<DJNode> nodes)
        {
            // 计算最大的节点id，如果maxId超出当前节点数组容量，resize
            int maxId = MaxNodeIdCalc(nodes);
            if (maxId >= m_allNodes.Count)
            {
                m_allNodes.Resize(maxId + 1, null);
            }

            // 添加或合并节点
            foreach (var node in nodes)
            {
                if (m_allNodes[node.m_id] == null)
                {
                    m_allNodes[node.m_id] = node;
                }
                else
                {
                    NodeMerge(m_allNodes[node.m_id], node);
                }
            }
        }

        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <param name="node"></param>
        public virtual void NodeAdd(DJNode node)
        {
            // 如果新增节点的id超出当前节点数组容量，resize
            if (node.m_id >= m_allNodes.Count)
            {
                m_allNodes.Resize(node.m_id + 1, null);
            }

            // 添加或合并节点
            if (m_allNodes[node.m_id] == null)
            {
                m_allNodes[node.m_id] = node;
            }
            else
            {
                NodeMerge(m_allNodes[node.m_id], node);
            }
        }

        /// <summary>
        /// 通过id反回节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual DJNode DJNodeGetById(int id)
        {
            if (id >= m_allNodes.Count)
            {
                return null;
            }
            return m_allNodes[id];
        }

        #endregion

        #region FindingCtx接口

        /// <summary>
        /// 开始寻路，返回可复用的寻路现场
        /// 由于复用寻路现场的实现方案与启发式路径搜索冲突，所以该方法不提供heuristicCostCalc参数
        /// </summary>
        /// <param name="from">起始节点id</param>
        /// <param name="costLimit">cost限制</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <param name="onNodeTouched">节点被touch时的回调，用来给使用者提供可观测的寻路过程，p1: 被touch的节点，p2: 到达节点的总cost，p3: 是否为首次touch</param>
        /// <returns>可复用的寻路现场</returns>
        public object PathFindingBegin(int from, int costLimit = 0,
            Func<DJNode, DJLink, int> costCalc = null,
            Action<DJNode, int, bool> onNodeTouched = null)
        {
            var fromNode = DJNodeGetById(from);
            if (fromNode == null)
            {
                return null;
            }
            var findingCtx = new FindingCtx(this);
            if (PathFindingBeginInternal(findingCtx, fromNode, costLimit, costCalc, null, onNodeTouched))
            {
                return findingCtx;
            }
            return null;
        }

        /// <summary>
        /// 步进路径搜索
        /// </summary>
        /// <param name="findingCtx">复用的寻路现场</param>
        /// <param name="toNode">目标节点</param>
        /// <returns>是否找到到达目标节点的最短路径</returns>
        public bool PathFindingStep(object findingCtx, DJNode toNode)
        {
            // ReSharper disable once InconsistentNaming
            var findingCtx_ = (FindingCtx)findingCtx;

            // 寻路无法继续，无法找到toNode的最短路径
            if (IsFindingCtxOver(findingCtx_))
            {
                return false;
            }

            // 从open队列出队，标记node已经找到最短路径
            var node = findingCtx_.m_openNodeQueue.ExtractMin();
            node.m_isClosed = true;

            // 成功找到toNode的最短路径
            if (toNode != null && node.m_node.m_id == toNode.m_id)
            {
                return true;
            }

            // 处理当前node的边
            NodeLinksProcess(findingCtx_, node, toNode);

            // 单步处理结束，还没找到toNode的最短路径
            return false;
        }

        /// <summary>
        /// 结束寻路
        /// </summary>
        /// <param name="findingCtx"></param>
        public void PathFindingEnd(object findingCtx)
        {
            // ReSharper disable once InconsistentNaming
            var findingCtx_ = (FindingCtx)findingCtx;
            findingCtx_.Clear();
        }

        #endregion

        #region PathFind接口

        /// <summary>
        /// 寻路函数
        /// </summary>
        /// <param name="from">起始节点id</param>
        /// <param name="to">目标节点id</param>
        /// <param name="allCost">返回的最短路径cost</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <param name="heuristicCostCalc">启发式cost计算函数，用来给使用者提供启发式寻路的实现方式，p1: from节点，p2: to节点，return: heuristicCost，注意请不要返还int.MaxValue</param>
        /// <param name="onNodeTouched">节点被touch时的回调，用来给使用者提供可观测的寻路过程，p1: 被touch的节点，p2: 到达节点的总cost，p3: 是否为首次touch</param>
        /// <returns>节点路径，null表示找不到最短路径</returns>
        public List<DJNode> PathFind(int from, int to, out int allCost,
            Func<DJNode, DJLink, int> costCalc = null,
            Func<DJNode, DJNode, int> heuristicCostCalc = null,
            Action<DJNode, int, bool> onNodeTouched = null)
        {
            allCost = 0;

            var fromNode = DJNodeGetById(from);
            var toNode = DJNodeGetById(to);
            if (fromNode == null || toNode == null)
            {
                return null;
            }

            return PathFind(fromNode, toNode, out allCost, costCalc, heuristicCostCalc, onNodeTouched);
        }

        /// <summary>
        /// 寻路函数
        /// </summary>
        /// <param name="from">起始节点</param>
        /// <param name="to">目标节点</param>
        /// <param name="allCost">返回的最短路径cost</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <param name="heuristicCostCalc">启发式cost计算函数，用来给使用者提供启发式寻路的实现方式，p1: from节点，p2: to节点，return: heuristicCost，注意请不要返还int.MaxValue</param>
        /// <param name="onNodeTouched">节点被touch时的回调，用来给使用者提供可观测的寻路过程，p1: 被touch的节点，p2: 到达节点的总cost，p3: 是否为首次touch</param>
        /// <returns>节点路径，null表示找不到最短路径</returns>
        public List<DJNode> PathFind(DJNode from, DJNode to, out int allCost,
            Func<DJNode, DJLink, int> costCalc = null,
            Func<DJNode, DJNode, int> heuristicCostCalc = null,
            Action<DJNode, int, bool> onNodeTouched = null)
        {
            // 使用默认现场
            var findingCtx = m_defaultFindingCtx;
            PathFindingBeginInternal(findingCtx, from, 0, costCalc, heuristicCostCalc, onNodeTouched);

            // 执行寻路
            var resultList = PathFind(findingCtx, to, out allCost);

            // 清空本次现场
            PathFindingEnd(m_defaultFindingCtx);

            return resultList;
        }

        /// <summary>
        /// 寻路函数
        /// </summary>
        /// <param name="findingCtx">复用的寻路现场</param>
        /// <param name="to">目标节点</param>
        /// <param name="allCost">返回的最短路径cost</param>
        /// <returns>节点路径，null表示找不到最短路径</returns>
        public List<DJNode> PathFind(object findingCtx, DJNode to, out int allCost)
        {
            allCost = 0;
            bool bFind = false;

            // ReSharper disable once InconsistentNaming
            var findingCtx_ = (FindingCtx)findingCtx;

            // 检查目标节点是否已经找到最短路径
            if (IsShortestPathFoundInFindingCtx(findingCtx_, to))
            {
                bFind = true;
            }

            // 单步寻路，直到找到到达目标节点的最短路径，或者寻路无法继续
            while (!bFind && !IsFindingCtxOver(findingCtx_))
            {
                // 步进路径搜索
                bFind = PathFindingStep(findingCtx_, to);
            }

            // 获取最终结果
            List<DJNode> resultList = null;
            if (bFind)
            {
                resultList = ResultPathGet(findingCtx_, to, out allCost);
            }

            return resultList;
        }

        #endregion

        #region PathFindWithCost接口

        /// <summary>
        /// 寻路函数，找到指定costLimit内能到达的所有节点
        /// </summary>
        /// <param name="from">起始节点id</param>
        /// <param name="costLimit">指定的最大cost限制</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <returns>在指定costLimit内，能到达的所有节点列表</returns>
        public List<DJNode> NodesFindWithCostLimit(int from, int costLimit,
            Func<DJNode, DJLink, int> costCalc = null)
        {
            var fromNode = DJNodeGetById(from);
            if (fromNode == null)
            {
                return null;
            }

            return NodesFindWithCostLimit(fromNode, costLimit, costCalc);
        }

        /// <summary>
        /// 寻路函数，找到指定costLimit内能到达的所有节点
        /// </summary>
        /// <param name="from">起始节点</param>
        /// <param name="costLimit">指定的最大cost限制</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <returns>在指定costLimit内，能到达的所有节点列表</returns>
        public List<DJNode> NodesFindWithCostLimit(DJNode from, int costLimit,
            Func<DJNode, DJLink, int> costCalc = null)
        {
            var findingCtx = m_defaultFindingCtx;

            // 开始寻路
            PathFindingBeginInternal(findingCtx, from, costLimit, costCalc);

            var result = NodesFindWithCostLimit(findingCtx, costLimit);

            // 清空本次生产的数据
            PathFindingEnd(m_defaultFindingCtx);

            return result;
        }

        /// <summary>
        /// 寻路函数，找到指定costLimit内能到达的所有节点
        /// </summary>
        /// <param name="findingCtx">复用的寻路现场</param>
        /// <param name="costLimit">额外指定的最大cost限制，其值必须小于等于findingCtx中的costLimit，否则没有意义</param>
        /// <returns>在指定costLimit内，能到达的所有节点列表</returns>
        public List<DJNode> NodesFindWithCostLimit(object findingCtx, int costLimit)
        {
            // ReSharper disable once InconsistentNaming
            var findingCtx_ = (FindingCtx)findingCtx;

            // 单步寻路，直到寻路无法继续
            while (!IsFindingCtxOver(findingCtx_))
            {
                PathFindingStep(findingCtx_, null);
            }

            // 收集找到的结果
            List<DJNode> result = null;
            if (findingCtx_.m_touchedNodes.Count != 0)
            {
                result = new List<DJNode>();
                foreach (var item in findingCtx_.m_touchedNodes)
                {
                    if (item.Value.m_allCost <= costLimit)
                    {
                        result.Add(item.Value.m_node);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 寻路函数，找到指定costLimit内能到达的所有节点，以及到达每个节点的cost
        /// </summary>
        /// <param name="findingCtx">复用的寻路现场</param>
        /// <param name="costLimit">额外指定的最大cost限制，其值必须小于等于findingCtx中的costLimit，否则没有意义</param>
        /// <returns>在指定costLimit内，能到达的所有节点列表，以及到每个节点的cost</returns>
        public List<KeyValuePair<DJNode, int>> NodesAndCostFindWithCostLimit(object findingCtx, int costLimit)
        {
            // ReSharper disable once InconsistentNaming
            var findingCtx_ = (FindingCtx)findingCtx;

            // 单步寻路，直到寻路无法继续
            while (!IsFindingCtxOver(findingCtx_))
            {
                PathFindingStep(findingCtx_, null);
            }

            // 收集找到的结果
            List<KeyValuePair<DJNode, int>> result = null;
            if (findingCtx_.m_touchedNodes.Count != 0)
            {
                result = new List<KeyValuePair<DJNode, int>>();
                foreach (var item in findingCtx_.m_touchedNodes)
                {
                    if (item.Value.m_allCost <= costLimit)
                    {
                        result.Add(new KeyValuePair<DJNode, int>(item.Value.m_node, item.Value.m_allCost));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 寻路函数，找到指定costLimit内能到达的所有节点，并按cost进行分组
        /// </summary>
        /// <param name="from">起始节点id</param>
        /// <param name="costLimit">指定的最大cost限制</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <returns>在指定costLimit内，能到达的所有节点列表，按cost分组，数组的索引就是cost值</returns>
        public List<DJNode>[] NodesFindWithCostLimitGroupByCost(int from, int costLimit,
            Func<DJNode, DJLink, int> costCalc = null)
        {
            var resultArray = new List<DJNode>[costLimit + 1];
            for (int i = 0; i <= costLimit; ++i)
            {
                resultArray[i] = new List<DJNode>();
            }

            var fromNode = DJNodeGetById(from);
            if (fromNode == null)
            {
                return resultArray;
            }

            var findingCtx = m_defaultFindingCtx;

            // 开始寻路
            PathFindingBeginInternal(findingCtx, fromNode, costLimit, costCalc);

            // 单步寻路，直到寻路无法继续
            while (!IsFindingCtxOver(findingCtx))
            {
                PathFindingStep(findingCtx, null);
            }

            // 收集找到的结果
            foreach (var touched in findingCtx.m_touchedNodes)
            {
                int cost = touched.Value.m_allCost;
                if (cost < 0 || cost > costLimit)
                {
                    continue;
                }

                if (resultArray[cost] == null)
                {
                    resultArray[cost] = new List<DJNode>();
                }

                resultArray[cost].Add(touched.Value.m_node);
            }

            // 清空本次生产的数据
            PathFindingEnd(m_defaultFindingCtx);

            return resultArray;
        }

        /// <summary>
        /// 寻路函数，找到cost刚好与指定cost相同的所有节点
        /// </summary>
        /// <param name="from">起始节点id</param>
        /// <param name="costEqual">指定的cost</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <returns>与指定cost相同的所有节点</returns>
        public List<DJNode> NodesFindWithCostEqual(int from, int costEqual,
            Func<DJNode, DJLink, int> costCalc = null)
        {
            var fromNode = DJNodeGetById(from);
            if (fromNode == null)
            {
                return null;
            }

            return NodesFindWithCostEqual(fromNode, costEqual, costCalc);
        }

        /// <summary>
        /// 寻路函数，找到cost刚好与指定cost相同的所有节点
        /// </summary>
        /// <param name="from">起始节点</param>
        /// <param name="costEqual">指定的cost</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <returns>与指定cost相同的所有节点</returns>
        public List<DJNode> NodesFindWithCostEqual(DJNode from, int costEqual,
            Func<DJNode, DJLink, int> costCalc = null)
        {
            var findingCtx = m_defaultFindingCtx;

            // 开始寻路
            PathFindingBeginInternal(findingCtx, from, costEqual, costCalc);

            // 单步寻路，直到寻路无法继续
            while (!IsFindingCtxOver(findingCtx))
            {
                PathFindingStep(findingCtx, null);
            }

            // 收集找到的结果
            List<DJNode> result = null;
            if (findingCtx.m_touchedNodes.Count != 0)
            {
                result = new List<DJNode>();
                foreach (var item in findingCtx.m_touchedNodes)
                {
                    // 只收集allCost等于cost的节点
                    if (item.Value.m_allCost != costEqual)
                    {
                        continue;
                    }

                    result.Add(item.Value.m_node);
                }
            }

            // 清空本次生产的数据
            PathFindingEnd(m_defaultFindingCtx);

            return result;
        }

        #endregion

        #region FindPath4Link接口

        /// <summary>
        /// 寻路函数，路径以边的列表的形式返回
        /// </summary>
        /// <param name="from">起始节点id</param>
        /// <param name="to">目标节点id</param>
        /// <param name="allCost">返回的最短路径cost</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <param name="heuristicCostCalc">启发式cost计算函数，用来给使用者提供启发式寻路的实现方式，p1: from节点，p2: to节点，return: heuristicCost，注意请不要返还int.MaxValue</param>
        /// <param name="onNodeTouched">节点被touch时的回调，用来给使用者提供可观测的寻路过程，p1: 被touch的节点，p2: 到达节点的总cost，p3: 是否为首次touch</param>
        /// <returns>边的路径，null表示找不到最短路径</returns>
        public List<DJLink> PathFind4Link(int from, int to, out int allCost,
            Func<DJNode, DJLink, int> costCalc = null,
            Func<DJNode, DJNode, int> heuristicCostCalc = null,
            Action<DJNode, int, bool> onNodeTouched = null)
        {
            allCost = 0;

            var fromNode = DJNodeGetById(from);
            var toNode = DJNodeGetById(to);
            if (fromNode == null || toNode == null)
            {
                return null;
            }

            return PathFind4Link(fromNode, toNode, out allCost, costCalc, heuristicCostCalc, onNodeTouched);
        }

        /// <summary>
        /// 寻路函数，路径以边的列表的形式返回
        /// </summary>
        /// <param name="from">起始节点</param>
        /// <param name="to">目标节点</param>
        /// <param name="allCost">返回的最短路径cost</param>
        /// <param name="costCalc">cost计算函数，用来给使用者提供动态计算路径cost的方式，p1: from节点，p2: from节点的边，return: cost值，int.MaxValue表示不连通</param>
        /// <param name="heuristicCostCalc">启发式cost计算函数，用来给使用者提供启发式寻路的实现方式，p1: from节点，p2: to节点，return: heuristicCost，注意请不要返还int.MaxValue</param>
        /// <param name="onNodeTouched">节点被touch时的回调，用来给使用者提供可观测的寻路过程，p1: 被touch的节点，p2: 到达节点的总cost，p3: 是否为首次touch</param>
        /// <returns>边的路径，null表示找不到最短路径</returns>
        public List<DJLink> PathFind4Link(DJNode from, DJNode to, out int allCost,
            Func<DJNode, DJLink, int> costCalc = null,
            Func<DJNode, DJNode, int> heuristicCostCalc = null,
            Action<DJNode, int, bool> onNodeTouched = null)
        {
            // 使用默认现场
            var findingCtx = m_defaultFindingCtx;
            PathFindingBeginInternal(findingCtx, from, 0, costCalc, heuristicCostCalc: heuristicCostCalc, onNodeTouched);

            // 执行寻路
            var resultList = PathFind4Link(findingCtx, to, out allCost);

            // 清空本次生产的数据
            PathFindingEnd(m_defaultFindingCtx);

            return resultList;
        }

        /// <summary>
        /// 寻路函数，路径以边的列表的形式返回
        /// </summary>
        /// <param name="findingCtx">复用的寻路现场</param>
        /// <param name="to">目标节点</param>
        /// <param name="allCost">返回的最短路径cost</param>
        /// <returns>边的路径，null表示找不到最短路径</returns>
        public List<DJLink> PathFind4Link(object findingCtx, DJNode to, out int allCost)
        {
            allCost = 0;
            bool bFind = false;

            // ReSharper disable once InconsistentNaming
            var findingCtx_ = (FindingCtx)findingCtx;

            // 检查目标节点是否已经找到最短路径
            if (IsShortestPathFoundInFindingCtx(findingCtx_, to))
            {
                bFind = true;
            }

            // 单步寻路，直到找到到达目标节点的最短路径，或者寻路无法继续
            while (!bFind && !IsFindingCtxOver(findingCtx_))
            {
                // 步进路径搜索
                bFind = PathFindingStep(findingCtx_, to);
            }

            // 获取最终结果
            List<DJLink> resultList = null;
            if (bFind)
            {
                resultList = ResultLinksPathGet(findingCtx_, to, out allCost);
            }

            return resultList;
        }

        #endregion

        #region 内部方法

        #region 节点初始化

        /// <summary>
        /// 计算最大节点id
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        protected int MaxNodeIdCalc(IList<DJNode> nodes)
        {
            int maxId = 0;
            foreach (var item in nodes)
            {
                maxId = maxId < item.m_id ? item.m_id : maxId;
            }
            if (maxId >= DJNodeCountMax)
            {
                throw new Exception("too much node for Dijkstra");
            }

            return maxId;
        }

        /// <summary>
        /// 合并节点
        /// </summary>
        protected void NodeMerge(DJNode targetNode, DJNode newNode)
        {
            // 查找所有的link看看是否需要合并
            foreach (var newLink in newNode.m_links)
            {
                bool needAddNewLink = true;
                foreach (var oldLink in targetNode.m_links)
                {
                    if (newLink.m_toNodeId == oldLink.m_toNodeId)
                    {
                        needAddNewLink = false;
                        break;
                    }
                }
                if (needAddNewLink)
                {
                    targetNode.m_links.Add(newLink);
                }
            }
        }

        #endregion

        #region 寻路过程

        /// <summary>
        /// 开始寻路内部实现
        /// </summary>
        /// <returns></returns>
        protected bool PathFindingBeginInternal(FindingCtx findingCtx, DJNode fromNode,
            int costLimit = 0,
            Func<DJNode, DJLink, int> costCalc = null,
            Func<DJNode, DJNode, int> heuristicCostCalc = null,
            Action<DJNode, int, bool> onNodeTouched = null)
        {
            // 初始化寻路现场
            findingCtx.m_fromNode = fromNode;
            findingCtx.m_costLimit = costLimit;
            findingCtx.m_costCalc = costCalc;
            findingCtx.m_heuristicCostCalc = heuristicCostCalc;
            findingCtx.m_onNodeTouched = onNodeTouched;

            // 将起始节点放入处理队列中
            var firstNode = TouchedNodeAlloc();
            firstNode.m_node = fromNode;
            firstNode.m_link = null;
            firstNode.m_fromNode = null;
            firstNode.m_cost = 0;
            firstNode.m_allCost = 0;
            findingCtx.m_openNodeQueue.Add(firstNode);

            // 设置第一个节点已touched
            findingCtx.m_touchedNodes[firstNode.m_node.m_id] = firstNode;

            return true;
        }

        /// <summary>
        /// 是否寻路现场中已经找到到达目标节点的最短路径
        /// </summary>
        /// <param name="findingCtx">寻路现场</param>
        /// <param name="to">目标节点</param>
        /// <returns></returns>
        protected bool IsShortestPathFoundInFindingCtx(FindingCtx findingCtx, DJNode to)
        {
            // 节点已被touch，并且已关闭，则表示已找到最短路径
            return findingCtx.m_touchedNodes.TryGetValue(to.m_id, out var touchedNode) && touchedNode.m_isClosed;
        }

        /// <summary>
        /// 寻路现场是否完成，无法继续寻路
        /// </summary>
        /// <param name="findingCtx"></param>
        /// <returns></returns>
        protected bool IsFindingCtxOver(FindingCtx findingCtx)
        {
            // 当open队列为空时，表示寻路现场完成，无法继续寻路
            return findingCtx.m_openNodeQueue.IsEmpty;
        }

        /// <summary>
        /// 遍历一个节点的所有link，处理新的更短路径或者新到达的节点
        /// </summary>
        /// <param name="findingCtx">搜索现场</param>
        /// <param name="currFromNode">当前这一步出发的节点</param>
        /// <param name="toNode">寻路的目标节点（终点）</param>
        /// <returns></returns>
        protected void NodeLinksProcess(FindingCtx findingCtx, TouchedNode currFromNode, DJNode toNode)
        {
            if (currFromNode.m_node.m_links == null)
            {
                return;
            }

            // 遍历所有的link
            foreach (var link in currFromNode.m_node.m_links)
            {
                // 跳过指向目标不存在的link
                if (DJNodeGetById(link.m_toNodeId) == null)
                {
                    continue;
                }

                // 计算当前这一步的cost，如果寻路现场中有外部提供的cost计算方法，优先使用该方法，否则直接取link上记录的cost
                int currStepCost = findingCtx.m_costCalc?.Invoke(currFromNode.m_node, link) ?? link.m_cost;

                // 当cost为MaxValue的时候表示当前不能走
                if (currStepCost >= int.MaxValue)
                {
                    continue;
                }

                // 计算当前路径的cost，当前这一步的cost + 前一个节点的路径的cost
                int currPathCost = currFromNode.m_allCost + currStepCost;
                if (currPathCost < 0)
                {
                    // 走到这说明计算溢出了
                    continue;
                }

                // 如果寻路现场中设置了costLimit，跳过超出限制的路径
                if (findingCtx.m_costLimit != 0 && currPathCost > findingCtx.m_costLimit)
                {
                    continue;
                }

                // 检查当前节点是否已被touch过
                bool isTouched = findingCtx.m_touchedNodes.TryGetValue(link.m_toNodeId, out var currToNode);
                if (!isTouched)
                {
                    // 如果节点未被touch过，创建新的touchedNode
                    currToNode = TouchedNodeAlloc();
                    currToNode.m_node = DJNodeGetById(link.m_toNodeId);
                    currToNode.m_link = link;
                    currToNode.m_fromNode = currFromNode;
                    currToNode.m_cost = currStepCost;
                    currToNode.m_allCost = currPathCost;

                    // 如果寻路现场中有外部提供的heuristicCost计算方法，计算该节点的heuristicCost，否则heuristicCost为0
                    currToNode.m_heuristicCost = findingCtx.m_heuristicCostCalc?.Invoke(currToNode.m_node, toNode) ?? 0;

                    // 将节点加入已Touch集合
                    findingCtx.m_touchedNodes.Add(currToNode.m_node.m_id, currToNode);

                    // 将节点加入open队列
                    findingCtx.m_openNodeQueue.Add(currToNode);

                    // 如果寻路现场中有外部提供的onNodeTouched回调，通知新节点被touch
                    findingCtx.m_onNodeTouched?.Invoke(currToNode.m_node, currPathCost, true);

                    continue;
                }

                // 如果节点已被touch过，检查是否找到了更短的路径，
                if (currPathCost < currToNode.m_allCost)
                {
                    // 更新节点的路径信息
                    currToNode.m_link = link;
                    currToNode.m_fromNode = currFromNode;
                    currToNode.m_cost = currStepCost;
                    currToNode.m_allCost = currPathCost;

                    // 更新节点在open队列中的位置
                    findingCtx.m_openNodeQueue.RebuildHeap();
                }

                // 如果寻路现场中有外部提供的onNodeTouched回调，通知节点被再次touch
                findingCtx.m_onNodeTouched?.Invoke(currToNode.m_node, currPathCost, false);
            }
        }

        #endregion

        #region 路径获取

        /// <summary>
        /// 从终点获取路径结果（以Node的方式）
        /// </summary>
        /// <param name="findingCtx"></param>
        /// <param name="end"></param>
        /// <param name="allCost"></param>
        protected List<DJNode> ResultPathGet(FindingCtx findingCtx, DJNode end, out int allCost)
        {
            var result = new List<DJNode>();
            var endNode = findingCtx.m_touchedNodes[end.m_id];

            var node = endNode;
            allCost = node.m_allCost;
            while (node != null)
            {
                result.Add(node.m_node);
                node = node.m_fromNode;
            }

            result.Reverse();

            return result;
        }

        /// <summary>
        /// 从终点获取路径结果（以Link的方式）
        /// </summary>
        /// <param name="findingCtx"></param>
        /// <param name="end"></param>
        /// <param name="allCost"></param>
        /// <returns></returns>
        protected List<DJLink> ResultLinksPathGet(FindingCtx findingCtx, DJNode end, out int allCost)
        {
            var result = new List<DJLink>();
            var endNode = findingCtx.m_touchedNodes[end.m_id];

            var node = endNode;
            allCost = node.m_allCost;
            while (node?.m_link != null)
            {
                result.Add(node.m_link);
                node = node.m_fromNode;
            }

            result.Reverse();

            return result;
        }

        #endregion

        #region 对象池管理

        /// <summary>
        /// 分配一个TouchedNode
        /// </summary>
        /// <returns></returns>
        protected TouchedNode TouchedNodeAlloc()
        {
            TouchedNode node = null;
            if (m_touchedNodePool.Count > 0)
            {
                node = m_touchedNodePool.Dequeue();
            }

            if (node == null)
            {
                node = new TouchedNode();
            }

            node.m_node = null;
            node.m_link = null;
            node.m_fromNode = null;
            node.m_cost = 0;
            node.m_allCost = 0;
            node.m_heuristicCost = 0;
            node.m_isClosed = false;

            return node;
        }

        /// <summary>
        /// 释放一个TouchedNode
        /// </summary>
        /// <param name="node"></param>
        protected void TouchedNodeFree(TouchedNode node)
        {
            m_touchedNodePool.Enqueue(node);
        }

        #endregion

        #endregion

        #region 内部字段

        /// <summary>
        /// 所有的Dijkstra节点
        /// </summary>               
        protected DataStructures.Lists.ArrayList<DJNode> m_allNodes = new DataStructures.Lists.ArrayList<DJNode>();

        /// <summary>
        /// 寻路现场
        /// </summary>
        protected FindingCtx m_defaultFindingCtx;

        /// <summary>
        /// TouchedNode的对象池
        /// </summary>
        protected Queue<TouchedNode> m_touchedNodePool = new Queue<TouchedNode>();

        /// <summary>
        /// 最大支持的Dijkstra节点的数量，默认为65535
        /// </summary>
        protected const int DJNodeCountMax = UInt16.MaxValue - 1;

        #endregion

        #region 内部类型定义

        /// <summary>
        /// 已触碰到的节点
        /// 表示已找到路径可到达，但不一定是最短的路径
        /// </summary>
        protected class TouchedNode : IComparable<TouchedNode>
        {
            /// <summary>
            /// 当前节点
            /// </summary>
            public DJNode m_node;

            /// <summary>
            /// 走到当前节点的路径
            /// 两个节点之间可能存在多条边（cost不同）
            /// </summary>
            public DJLink m_link;

            /// <summary>
            /// 走到当前节点的上一步节点
            /// </summary>
            public TouchedNode m_fromNode;

            /// <summary>
            /// 当前路径上一节点总到当前节点的cost
            /// </summary>
            public int m_cost;

            /// <summary>
            /// 当前路径的总cost
            /// </summary>
            public int m_allCost;

            /// <summary>
            /// 当前节点到目标点的启发式预估cost
            /// 如果不使用启发式寻路，则该值为0
            /// </summary>
            public int m_heuristicCost;

            /// <summary>
            /// 节点是否关闭，true表示已找到抵达当前节点的最短路径
            /// </summary>
            public bool m_isClosed;

            #region Relational members
            public int CompareTo(TouchedNode other)
            {
                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                if (ReferenceEquals(null, other))
                {
                    return 1;
                }

                // 使用allCost与heuristicCost的和进行比较
                return (m_allCost + m_heuristicCost).CompareTo(other.m_allCost + other.m_heuristicCost);
            }
            #endregion
        }

        /// <summary>
        /// 寻路现场，表示一次单起始节点的寻路过程中数据现场
        /// 可以复用该现场，在地图没有发生修改的情况下，对相同起点，不同终点的寻路进行优化
        /// 注意：在复用寻路现场寻路时，不能使用启发式路径搜索
        /// </summary>
        protected class FindingCtx
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="owner"></param>
            public FindingCtx(Dijkstra owner)
            {
                m_owner = owner;
            }

            /// <summary>
            /// 本次寻路现场的起始节点
            /// </summary>
            public DJNode m_fromNode;

            /// <summary>
            /// 由外界设置的cost上限，0代表没有上限
            /// </summary>
            public int m_costLimit;

            /// <summary>
            /// 由外界提供的cost计算方法
            /// 该字段可能为null
            /// 参数列表为：
            /// p1: from节点
            /// p2: from节点的边
            /// return: cost值，int.MaxValue表示不连通
            /// </summary>
            public Func<DJNode, DJLink, int> m_costCalc;

            /// <summary>
            /// 由外界提供的heuristicCost计算方法，用于实现启发式路径搜索
            /// 该字段可能为null
            /// 参数列表为：
            /// p1: from节点
            /// p2: to节点
            /// return: heuristicCost，注意请不要返还int.MaxValue
            /// </summary>
            public Func<DJNode, DJNode, int> m_heuristicCostCalc;

            /// <summary>
            /// 当有节点被touch时的回调
            /// 参数列表为：
            /// p1: 被touch的节点
            /// p2: 到达节点的总cost
            /// p3: 是否为首次touch
            /// </summary>
            public Action<DJNode, int, bool> m_onNodeTouched;

            /// <summary>
            /// open队列，其中节点正等待被遍历
            /// 使用二分最小堆实现的优先队列
            /// </summary>
            public DataStructures.Heaps.BinaryMinHeap<TouchedNode> m_openNodeQueue = new DataStructures.Heaps.BinaryMinHeap<TouchedNode>();

            /// <summary>
            /// 所有touch过的节点集合
            /// </summary>
            public Dictionary<int, TouchedNode> m_touchedNodes = new Dictionary<int, TouchedNode>();

            /// <summary>
            /// 清理
            /// </summary>
            public void Clear()
            {
                foreach (var item in m_touchedNodes)
                {
                    m_owner.TouchedNodeFree(item.Value);
                }

                m_fromNode = null;
                m_costLimit = 0;
                m_costCalc = null;
                m_heuristicCostCalc = null;
                m_onNodeTouched = null;
                if (!m_openNodeQueue.IsEmpty)
                {
                    // 在BinaryMinHeap为Empty时调用Clear，会导致内部触发异常
                    m_openNodeQueue.Clear();
                }
                m_touchedNodes.Clear();
            }

            /// <summary>
            /// Dijkstra算法
            /// </summary>
            protected Dijkstra m_owner;
        }

        #endregion
    }
}

using bluebean.UGFramework.Geometry;
using bluebean.UGFramework.Physics;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace bluebean.UGFramework.DataStruct
{
    /// <summary>
    /// 比起层次包围盒bvh
    /// 这个更像是kdtree,会选取范围最大的维度，然后进行空间划分
    /// </summary>
    public class BIH
    {
        public static BIHNode[] Build(ref IBounded[] elements, int maxDepth = 10, float maxOverlap = 0.7f)
        {
            List<BIHNode> nodes = new List<BIHNode> { new BIHNode(0, elements.Length) };

            // auxiliar variables to keep track of current tree depth:
            int depth = 0;
            int nodesToNextLevel = 1;

            var queue = new Queue<int>();
            queue.Enqueue(0);

            while (queue.Count > 0)
            {
                // get current node:
                int index = queue.Dequeue();
                var node = nodes[index];

                // if this node contains enough elements, split it:
                if (node.count > 2)
                {
                    int start = node.start;
                    int end = start + (node.count - 1);

                    // calculate bounding box of all elements:
                    Aabb b = elements[start].GetBounds();
                    for (int k = start + 1; k <= end; ++k)
                        b.Encapsulate(elements[k].GetBounds());
                    node.m_aabb = b;

                    // determine split axis (longest one):
                    Vector3 size = b.size;
                    int axis = node.axis = (size.x > size.y) ?
                                                (size.x > size.z ? 0 : 2) :
                                                (size.y > size.z ? 1 : 2);

                    // place split plane at half the longest axis:
                    float pivot = b.min[axis] + size[axis] * 0.5f;

                    // partition elements according to which side of the split plane they're at:
                    int j = HoarePartition(elements, start, end, pivot, ref node, axis);

                    // create two child nodes:
                    var minChild = new BIHNode(start, j - start + 1);
                    var maxChild = new BIHNode(j + 1, end - j);

                    // calculate child overlap:
                    float overlap = size[axis] > 0 ? Mathf.Max(node.leftSplitPlane - node.rightSplitPlane, 0) / size[axis] : 1;

                    // guard against cases where all elements are on one side of the split plane,
                    // due to all having the same or very similar bounds as the entire group.
                    if (overlap <= maxOverlap && minChild.count > 0 && maxChild.count > 0)
                    {
                        node.firstChild = nodes.Count;
                        //bihNode是结构体，修改后需重新放入
                        nodes[index] = node;

                        queue.Enqueue(nodes.Count);
                        queue.Enqueue(nodes.Count + 1);

                        minChild.parent = index;
                        maxChild.parent = index;
                        // append child nodes to list:
                        nodes.Add(minChild);
                        nodes.Add(maxChild);
                    }

                    // keep track of current depth:
                    if (--nodesToNextLevel == 0)
                    {
                        depth++;
                        if (depth >= maxDepth)
                            return nodes.ToArray();
                        nodesToNextLevel = queue.Count;
                    }
                }
            }
            return nodes.ToArray();
        }

        /// <summary>
        /// 快速排序
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pivot"></param>
        /// <param name="node"></param>
        /// <param name="axis"></param>
        /// <returns>pivot左边最近元素索引</returns>
        private static int HoarePartition(IBounded[] elements, int start, int end, float pivot, ref BIHNode node, int axis)
        {
            int i = start;
            int j = end;

            while (i <= j)
            {
                while (i < end && elements[i].GetBounds().center[axis] < pivot)
                {
                    //更新左区间子元素的最右范围
                    node.leftSplitPlane = Mathf.Max(node.leftSplitPlane, elements[i++].GetBounds().max[axis]);
                }

                while (j > start && elements[j].GetBounds().center[axis] > pivot)
                {
                    //更新右区间子元素的最左范围
                    node.rightSplitPlane = Mathf.Min(node.rightSplitPlane, elements[j--].GetBounds().min[axis]);
                }

                if (i <= j)
                {
                    node.leftSplitPlane = Mathf.Max(node.leftSplitPlane, elements[j].GetBounds().max[axis]);
                    node.rightSplitPlane = Mathf.Min(node.rightSplitPlane, elements[i].GetBounds().min[axis]);
                    PhysicsUtil.Swap(ref elements[i++], ref elements[j--]);
                }
            }

            return j;
        }


        public static string BuildPartionLog(BIHNode[] nodes)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                //leaf node
                if (node.firstChild == -1)
                {
                    StringBuilder pathSb = new StringBuilder();
                    pathSb.Append("[");
                    for (int j = node.start; j < node.start + node.count; j++)
                    {
                        pathSb.Append(j).Append(",");
                    }
                    pathSb.Append("]");
                    int parentIndex = node.parent;
                    while (parentIndex != -1)
                    {
                        pathSb.Insert(0, $"{parentIndex}-->");
                        parentIndex = nodes[parentIndex].parent;
                    }
                    sb.AppendLine(pathSb.ToString());
                }
            }
            return sb.ToString();
        }
    }
}

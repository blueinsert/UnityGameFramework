using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.DataStruct
{
    public class BIHSpheres
    {
        public BIHNode[] nodes = null;
        public Sphere[] spheres = null;

        public BIHSpheres()
        {

        }

        public Aabb AABB { get { return nodes[0].m_aabb; } }

        public BIHSpheres(BIHNode[] nodes, Sphere[] spheres)
        {
            this.nodes = nodes;
            this.spheres = spheres;
        }

        /// <summary>
        /// node是叶子节点
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static float DistanceToSpheresInLeaf(Sphere[] spheres,
                                              in BIHNode node,
                                              in Vector3 point, ref float minDist, ref int minIndex)
        {
            float minDistance = float.MaxValue;
            for (int i = node.start; i < node.start + node.count; ++i)
            {
                Sphere s = spheres[i];
                var dist = (point - s.c).magnitude - s.r;
                if(dist < minDistance)
                {
                    minDistance = dist;
                }
                if(dist < minDist)
                {
                    minDist = dist;
                    minIndex = i;
                }
            }
            return minDistance;
        }

        private static float DistanceToSurface(BIHNode[] nodes,
                                             Sphere[] spheres,
                                             in BIHNode node,
                                             in Vector3 point,ref float minDist, ref int minIndex)
        {
            float MinDistance(float d1, float d2)
            {
                return (d1) < (d2) ? d1 : d2;
            }

            if (node.firstChild >= 0)
            {
                float si = float.MaxValue;
                float p = point[node.axis];

                // child nodes overlap:
                if (node.leftSplitPlane > node.rightSplitPlane)
                {
                    // CASE 1: we are in the overlapping zone: recurse down both.
                    if (p <= node.leftSplitPlane && p >= node.rightSplitPlane)
                    {
                        si = MinDistance(DistanceToSurface(nodes, spheres, in nodes[node.firstChild], in point, ref minDist, ref minIndex),
                                               DistanceToSurface(nodes, spheres, in nodes[node.firstChild + 1], in point, ref minDist, ref minIndex));
                    }
                    // CASE 2: to the right of left pivot, that is: in the right child only.
                    else if (p > node.leftSplitPlane)
                    {
                        si = DistanceToSurface(nodes, spheres, in nodes[node.firstChild + 1], in point, ref minDist, ref minIndex);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (si > Mathf.Abs(p - node.leftSplitPlane))
                            si = MinDistance(si, DistanceToSurface(nodes, spheres, in nodes[node.firstChild], in point, ref minDist, ref minIndex));
                    }
                    // CASE 3: to the left of right pivot, that is: in the left child only.
                    else
                    {
                        si = DistanceToSurface(nodes, spheres, nodes[node.firstChild], point, ref minDist, ref minIndex);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (si > Mathf.Abs(node.rightSplitPlane - p))
                            si = MinDistance(si, DistanceToSurface(nodes, spheres, in nodes[node.firstChild + 1], in point, ref minDist, ref minIndex));
                    }
                }
                // child nodes do not overlap
                else
                {
                    // CASE 4: we are in the middle. just pick up one child (I chose right), get minimum, and if the other child pivot is nearer, recurse down it too.
                    // Just like case 2.
                    if (p > node.leftSplitPlane && p < node.rightSplitPlane)
                    {
                        si = DistanceToSurface(nodes, spheres, in nodes[node.firstChild + 1], in point, ref minDist, ref minIndex);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (si > Mathf.Abs(p - node.leftSplitPlane))
                            si = MinDistance(si, DistanceToSurface(nodes, spheres, in nodes[node.firstChild], in point, ref minDist, ref minIndex));
                    }
                    // CASE 5: in the left child. Just like case 3.
                    else if (p <= node.leftSplitPlane)
                    {
                        si = DistanceToSurface(nodes, spheres, in nodes[node.firstChild], in point, ref minDist, ref minIndex);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (si > Mathf.Abs(node.rightSplitPlane - p))
                            si = MinDistance(si, DistanceToSurface(nodes, spheres, in nodes[node.firstChild + 1], in point, ref minDist, ref minIndex));
                    }
                    // CASE 6: in the right child. Just like case 2
                    else if (p >= node.rightSplitPlane)
                    {
                        si = DistanceToSurface(nodes, spheres, in nodes[node.firstChild + 1], in point, ref minDist, ref minIndex);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (si > Mathf.Abs(p - node.leftSplitPlane))
                            si = MinDistance(si, DistanceToSurface(nodes, spheres, in nodes[node.firstChild], in point, ref minDist, ref minIndex));
                    }
                }

                return si;
            }
            else
                return DistanceToSpheresInLeaf(spheres, in node, point,ref minDist, ref minIndex);
        }

        public static float DistanceToSurface(BIHNode[] nodes,
                                       Sphere[] spheres,
                                       in Vector3 point, out int minIndex)
        {
            minIndex = -1;
            float minDist = float.MaxValue;

            if (nodes.Length > 0)
            {
                var res = DistanceToSurface(nodes, spheres, in nodes[0], in point, ref minDist, ref minIndex);
                return res;
            }

            return float.MaxValue;
        }
    }
}

using bluebean.UGFramework.DataStruct;
using bluebean.UGFramework.Geometry;
using bluebean.UGFramework.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.DataStruct
{
    public class BIHMesh
    {
        /// <summary>
        /// node是叶子节点，遍历所有三角面查找最近距离
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="node"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static float DistanceToSurface(Triangle[] triangles,
                                              Vector3[] vertices,
                                              Vector3[] normals,
                                              in BIHNode node,
                                              in Vector3 point)
        {
            float minDistance = float.MaxValue;
            int sign = 1;

            Vector3 pointOnTri;
            Vector3 interpolatedNormal;

            for (int i = node.start; i < node.start + node.count; ++i)
            {
                Triangle t = triangles[i];

                GeometryUtil.NearestPointOnTri(in vertices[t.i1],
                                           in vertices[t.i2],
                                           in vertices[t.i3],
                                           in point,
                                           out pointOnTri);

                Vector3 pointToTri = point - pointOnTri;
                float sqrDistance = pointToTri.sqrMagnitude;

                if (sqrDistance < minDistance)
                {
                    Vector3 bary = Vector3.zero;
                    GeometryUtil.BarycentricCoordinates(in vertices[t.i1], in vertices[t.i2], in vertices[t.i3], in pointOnTri, ref bary);
                    GeometryUtil.BarycentricInterpolation(in normals[t.i1],
                                                      in normals[t.i2],
                                                      in normals[t.i3],
                                                      in bary,
                                                      out interpolatedNormal);

                    sign = PhysicsUtil.PureSign(pointToTri.x * interpolatedNormal.x +
                                             pointToTri.y * interpolatedNormal.y +
                                             pointToTri.z * interpolatedNormal.z);

                    minDistance = sqrDistance;
                }
            }

            return Mathf.Sqrt(minDistance) * sign;
        }

        private static float DistanceToSurface(BIHNode[] nodes,
                                              Triangle[] triangles,
                                              Vector3[] vertices,
                                              Vector3[] normals,
                                              in BIHNode node,
                                              in Vector3 point)
        {

            float MinSignedDistance(float d1, float d2)
            {
                return (Mathf.Abs(d1) < Mathf.Abs(d2)) ? d1 : d2;
            }

            if (node.firstChild >= 0)
            {
                /**
                 * If the current node is not a leaf, figure out which side of the split plane that contains the query point, and recurse down that side.
                 * You will get the index and distance to the closest triangle in that subtree.
                 * Then, check if the distance to the nearest triangle is closer to the query point than the distance between the query point and the split plane.
                 * If it is closer, there is no need to recurse down the other side of the KD tree and you can just return.
                 * Otherwise, you will need to recurse down the other way too, and return whichever result is closer.
                 */

                float si = float.MaxValue;
                float p = point[node.axis];

                // child nodes overlap:
                if (node.leftSplitPlane > node.rightSplitPlane)
                {
                    // CASE 1: we are in the overlapping zone: recurse down both.
                    if (p <= node.leftSplitPlane && p >= node.rightSplitPlane)
                    {
                        si = MinSignedDistance(DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild], in point),
                                               DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild + 1], in point));
                    }
                    // CASE 2: to the right of left pivot, that is: in the right child only.
                    else if (p > node.leftSplitPlane)
                    {
                        si = DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild + 1], in point);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (Mathf.Abs(si) > Mathf.Abs(p - node.leftSplitPlane))
                            si = MinSignedDistance(si, DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild], in point));
                    }
                    // CASE 3: to the left of right pivot, that is: in the left child only.
                    else
                    {
                        si = DistanceToSurface(nodes, triangles, vertices, normals, nodes[node.firstChild], point);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (Mathf.Abs(si) > Mathf.Abs(node.rightSplitPlane - p))
                            si = MinSignedDistance(si, DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild + 1], in point));
                    }
                }
                // child nodes do not overlap
                else
                {
                    // CASE 4: we are in the middle. just pick up one child (I chose right), get minimum, and if the other child pivot is nearer, recurse down it too.
                    // Just like case 2.
                    if (p > node.leftSplitPlane && p < node.rightSplitPlane)
                    {
                        si = DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild + 1], in point);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (Mathf.Abs(si) > Mathf.Abs(p - node.leftSplitPlane))
                            si = MinSignedDistance(si, DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild], in point));
                    }
                    // CASE 5: in the left child. Just like case 3.
                    else if (p <= node.leftSplitPlane)
                    {
                        si = DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild], in point);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (Mathf.Abs(si) > Mathf.Abs(node.rightSplitPlane - p))
                            si = MinSignedDistance(si, DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild + 1], in point));
                    }
                    // CASE 6: in the right child. Just like case 2
                    else if (p >= node.rightSplitPlane)
                    {
                        si = DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild + 1], in point);

                        // only recurse down left child if nearest surface in right child is furthest than left pivot.
                        if (Mathf.Abs(si) > Mathf.Abs(p - node.leftSplitPlane))
                            si = MinSignedDistance(si, DistanceToSurface(nodes, triangles, vertices, normals, in nodes[node.firstChild], in point));
                    }
                }

                return si;
            }
            else
                return DistanceToSurface(triangles, vertices, normals, in node, point);
        }

        public static float DistanceToSurface(BIHNode[] nodes,
                                      Triangle[] triangles,
                                      Vector3[] vertices,
                                      Vector3[] normals,
                                      in Vector3 point)
        {

            if (nodes.Length > 0)
                return DistanceToSurface(nodes, triangles, vertices, normals, in nodes[0], in point);

            return float.MaxValue;
        }
    }
}

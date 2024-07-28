using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.Geometry
{
    public static class GeometryUtil
    {
        public const float Epsilon = 0.001f;

        public static void NearestPointOnTri(in Vector3 p1,
                                                    in Vector3 p2,
                                                    in Vector3 p3,
                                                    in Vector3 p,
                                                    out Vector3 result)
        {
            float e0x = p2.x - p1.x;
            float e0y = p2.y - p1.y;
            float e0z = p2.z - p1.z;

            float e1x = p3.x - p1.x;
            float e1y = p3.y - p1.y;
            float e1z = p3.z - p1.z;

            float v0x = p1.x - p.x;
            float v0y = p1.y - p.y;
            float v0z = p1.z - p.z;

            float a00 = e0x * e0x + e0y * e0y + e0z * e0z;
            float a01 = e0x * e1x + e0y * e1y + e0z * e1z;
            float a11 = e1x * e1x + e1y * e1y + e1z * e1z;
            float b0 = e0x * v0x + e0y * v0y + e0z * v0z;
            float b1 = e1x * v0x + e1y * v0y + e1z * v0z;

            const float zero = 0;
            const float one = 1;

            float det = a00 * a11 - a01 * a01;
            float t0 = a01 * b1 - a11 * b0;
            float t1 = a01 * b0 - a00 * b1;

            if (t0 + t1 <= det)
            {
                if (t0 < zero)
                {
                    if (t1 < zero)  // region 4
                    {
                        if (b0 < zero)
                        {
                            t1 = zero;
                            if (-b0 >= a00)  // V0
                            {
                                t0 = one;
                            }
                            else  // E01
                            {
                                t0 = -b0 / a00;
                            }
                        }
                        else
                        {
                            t0 = zero;
                            if (b1 >= zero)  // V0
                            {
                                t1 = zero;
                            }
                            else if (-b1 >= a11)  // V2
                            {
                                t1 = one;
                            }
                            else  // E20
                            {
                                t1 = -b1 / a11;
                            }
                        }
                    }
                    else  // region 3
                    {
                        t0 = zero;
                        if (b1 >= zero)  // V0
                        {
                            t1 = zero;
                        }
                        else if (-b1 >= a11)  // V2
                        {
                            t1 = one;
                        }
                        else  // E20
                        {
                            t1 = -b1 / a11;
                        }
                    }
                }
                else if (t1 < zero)  // region 5
                {
                    t1 = zero;
                    if (b0 >= zero)  // V0
                    {
                        t0 = zero;
                    }
                    else if (-b0 >= a00)  // V1
                    {
                        t0 = one;
                    }
                    else  // E01
                    {
                        t0 = -b0 / a00;
                    }
                }
                else  // region 0, interior
                {
                    float invDet = one / det;
                    t0 *= invDet;
                    t1 *= invDet;
                }
            }
            else
            {
                float tmp0, tmp1, numer, denom;

                if (t0 < zero)  // region 2
                {
                    tmp0 = a01 + b0;
                    tmp1 = a11 + b1;
                    if (tmp1 > tmp0)
                    {
                        numer = tmp1 - tmp0;
                        denom = a00 - 2 * a01 + a11;
                        if (numer >= denom)  // V1
                        {
                            t0 = one;
                            t1 = zero;
                        }
                        else  // E12
                        {
                            t0 = numer / denom;
                            t1 = one - t0;
                        }
                    }
                    else
                    {
                        t0 = zero;
                        if (tmp1 <= zero)  // V2
                        {
                            t1 = one;
                        }
                        else if (b1 >= zero)  // V0
                        {
                            t1 = zero;
                        }
                        else  // E20
                        {
                            t1 = -b1 / a11;
                        }
                    }
                }
                else if (t1 < zero)  // region 6
                {
                    tmp0 = a01 + b1;
                    tmp1 = a00 + b0;
                    if (tmp1 > tmp0)
                    {
                        numer = tmp1 - tmp0;
                        denom = a00 - 2 * a01 + a11;
                        if (numer >= denom)  // V2
                        {
                            t1 = one;
                            t0 = zero;
                        }
                        else  // E12
                        {
                            t1 = numer / denom;
                            t0 = one - t1;
                        }
                    }
                    else
                    {
                        t1 = zero;
                        if (tmp1 <= zero)  // V1
                        {
                            t0 = one;
                        }
                        else if (b0 >= zero)  // V0
                        {
                            t0 = zero;
                        }
                        else  // E01
                        {
                            t0 = -b0 / a00;
                        }
                    }
                }
                else  // region 1
                {
                    numer = a11 + b1 - a01 - b0;
                    if (numer <= zero)  // V2
                    {
                        t0 = zero;
                        t1 = one;
                    }
                    else
                    {
                        denom = a00 - 2 * a01 + a11;
                        if (numer >= denom)  // V1
                        {
                            t0 = one;
                            t1 = zero;
                        }
                        else  // 12
                        {
                            t0 = numer / denom;
                            t1 = one - t0;
                        }
                    }
                }
            }

            result.x = p1.x + t0 * e0x + t1 * e1x;
            result.y = p1.y + t0 * e0y + t1 * e1y;
            result.z = p1.z + t0 * e0z + t1 * e1z;
        }

        public static void BarycentricCoordinates(in Vector3 A,
                                                  in Vector3 B,
                                                  in Vector3 C,
                                                  in Vector3 P,
                                                  ref Vector3 bary)
        {

            // Compute vectors
            Vector3 v0 = C - A;
            Vector3 v1 = B - A;
            Vector3 v2 = P - A;

            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float det = dot00 * dot11 - dot01 * dot01;
            if (Mathf.Abs(det) > 1E-38)
            {
                float u = (dot11 * dot02 - dot01 * dot12) / det;
                float v = (dot00 * dot12 - dot01 * dot02) / det;
                bary = new Vector3(1 - u - v, v, u);
            }

        }

        public static void BarycentricInterpolation(in Vector3 p1, in Vector3 p2, in Vector3 p3, in Vector3 coords, out Vector3 result)
        {
            result.x = coords.x * p1.x + coords.y * p2.x + coords.z * p3.x;
            result.y = coords.x * p1.y + coords.y * p2.y + coords.z * p3.y;
            result.z = coords.x * p1.z + coords.y * p2.z + coords.z * p3.z;
        }

        public static Vector3[] CalculateAngleWeightedNormals(Vector3[] vertices, int[] triangles)
        {
            Vector3[] normals = new Vector3[vertices.Length];
            var normalBuffer = new Dictionary<Vector3, Vector3>();

            Vector3 v1, v2, v3, e1, e2;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                v1 = vertices[triangles[i]];
                v2 = vertices[triangles[i + 1]];
                v3 = vertices[triangles[i + 2]];

                if (!normalBuffer.ContainsKey(v1))
                    normalBuffer[v1] = Vector3.zero;
                if (!normalBuffer.ContainsKey(v2))
                    normalBuffer[v2] = Vector3.zero;
                if (!normalBuffer.ContainsKey(v3))
                    normalBuffer[v3] = Vector3.zero;

                e1 = v2 - v1;
                e2 = v3 - v1;
                normalBuffer[v1] += Vector3.Cross(e1, e2).normalized * Mathf.Acos(Vector3.Dot(e1.normalized, e2.normalized));

                e1 = v3 - v2;
                e2 = v1 - v2;
                normalBuffer[v2] += Vector3.Cross(e1, e2).normalized * Mathf.Acos(Vector3.Dot(e1.normalized, e2.normalized));

                e1 = v1 - v3;
                e2 = v2 - v3;
                normalBuffer[v3] += Vector3.Cross(e1, e2).normalized * Mathf.Acos(Vector3.Dot(e1.normalized, e2.normalized));
            }

            for (int i = 0; i < vertices.Length; ++i)
                normals[i] = normalBuffer[vertices[i]].normalized;

            return normals;
        }

        public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd, out float mu, bool clampToSegment = true)
        {
            Vector3 ap = point - lineStart;
            Vector3 ab = lineEnd - lineStart;

            mu = Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab);

            if (clampToSegment)
                mu = Mathf.Clamp01(mu);

            return lineStart + ab * mu;
        }

        #region 形状相交测试
        static Vector3 GetPlaneNormalVector(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            Vector3 normal = Vector3.Cross(v1, v2);
            normal.Normalize();
            return normal;
        }

        /// <summary>
        /// 检测box之间是否相交
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool IsBoxBoxOverlap(BoxShape c1, BoxShape c2)
        {
            Vector3[] normalVectors = new Vector3[6];
            normalVectors[0] = GetPlaneNormalVector(c1[0], c1[3], c1[2]);
            normalVectors[1] = GetPlaneNormalVector(c1[0], c1[4], c1[7]);
            normalVectors[2] = GetPlaneNormalVector(c1[3], c1[2], c1[6]);

            normalVectors[3] = GetPlaneNormalVector(c2[0], c2[3], c2[2]);
            normalVectors[4] = GetPlaneNormalVector(c2[0], c2[4], c2[7]);
            normalVectors[5] = GetPlaneNormalVector(c2[3], c2[2], c2[6]);

            bool isIntersect = true;
            for (int i = 0; i < 6; i++)
            {
                Vector3 normal = normalVectors[i];
                //
                float c1Max = float.MinValue;
                float c1Min = float.MaxValue;
                foreach (var v in c1)
                {
                    float projectValue = Vector3.Dot(v, normal);
                    if (projectValue > c1Max)
                    {
                        c1Max = projectValue;
                    }
                    if (projectValue < c1Min)
                    {
                        c1Min = projectValue;
                    }
                }
                //
                float c2Max = float.MinValue;
                float c2Min = float.MaxValue;
                foreach (var v in c2)
                {
                    float projectValue = Vector3.Dot(v, normal);
                    if (projectValue > c2Max)
                    {
                        c2Max = projectValue;
                    }
                    if (projectValue < c2Min)
                    {
                        c2Min = projectValue;
                    }
                }
                //
                if (c2Min > c1Max || c1Min > c2Max)
                {
                    isIntersect = false;
                    //Debug.Log("c1Min:" + c1Min + " c1Max" + c1Max + " c2Min:" + c2Min + " c2Max" + c2Max);
                    break;
                }

            }//for six separate axis
            return isIntersect;
        }

        /// <summary>
        /// 检查球与box是否相交
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="box"></param>
        /// <returns></returns>
        public static bool IsSphereBoxOverlap(SphereShape sphere,BoxShape box)
        {
            var sphereAabb = sphere.WorldAabb;
            var boxAabb = box.WorldAabb;
            if(!sphereAabb.IntersectsAabb(boxAabb))
            {
                return false;
            }
            //转为到box的本地坐标系进行计算
            var s2b = box.m_local2WorldTransform.ToMatrix().inverse * sphere.m_local2WorldTransform.ToMatrix();
            var center = s2b.MultiplyPoint3x4(sphere.m_position);
            var radius = s2b.lossyScale[0] * sphere.m_radius;

            //获取点到box的最小距离
            var halfSize = new Vector3(0.5f, 0.5f, 0.5f);
            var vec = new Vector3(Mathf.Abs(center.x), Mathf.Abs(center.y), Mathf.Abs(center.z));
            var distVec = vec - halfSize;
            float minDist = float.MaxValue;
            if(distVec.x<0 && distVec.y<0 && distVec.z < 0)
            {
                //点在内部
                for(int i = 0; i < 3; i++)
                {
                    if (distVec[i] < minDist)
                    {
                        minDist = distVec[i];
                    }
                }
            }
            else
            {
                float distSqr = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (distVec[i] > 0)
                    {
                        distSqr += distVec[i] * distVec[i];
                    }
                }
                minDist = Mathf.Sqrt(distSqr);
            }
            if (minDist < 0)
                return true;
            else
            {
                if (minDist <= radius - Epsilon)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检测球与网格是否相交
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="meshShape"></param>
        /// <returns></returns>
        public static bool IsSphereMeshOverlap(SphereShape sphere,MeshShape meshShape)
        {
            var aabbSphere = sphere.WorldAabb;
            var aabbMesh = meshShape.WorldAabb;
            if (!aabbSphere.IntersectsAabb(aabbMesh))
            {
                return false;
            }

            var s2m = meshShape.m_local2WorldTransform.ToMatrix().inverse * sphere.m_local2WorldTransform.ToMatrix();
            var center = s2m.MultiplyPoint3x4(sphere.m_position);
            var radius = s2m.lossyScale[0] * sphere.m_radius;
            var dist = BIH.DistanceToSurface(meshShape.m_bihNodes, meshShape.m_triangles, meshShape.m_vertices, meshShape.m_normals, center);
            if (dist <= radius - Epsilon)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
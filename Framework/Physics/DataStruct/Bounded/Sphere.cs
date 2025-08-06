using bluebean.UGFramework.DataStruct;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.DataStruct
{
    public struct Sphere : IBounded
    {
        public Vector3 c;
        public float r;
        public int index;
        Aabb b;

        public Sphere(Vector3 c, float r)
        {
            index = -1;
            this.c = c;
            this.r = r;
            b = new Aabb(c);
            b.Encapsulate(c + new Vector3(r, r, r));
            b.Encapsulate(c + new Vector3(-r, -r, -r));
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public Aabb GetBounds()
        {
            return b;
        }
    }
}

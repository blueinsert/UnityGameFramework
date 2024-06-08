using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public interface IEntityManager
    {
        public List<Entity> GetAllEntities();
        public bool IsDirty();
    }
}

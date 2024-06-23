using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace bluebean.UGFramework.GamePlay
{
    public abstract class TheWorldBase : IEntityManager,IComponentManager,IcomponentFactory
    {
        private const int EntityCapcity = 100;

        protected ObjectPool<Entity> m_entityPool = new ObjectPool<Entity>(EntityCapcity);
        private List<Entity> m_toRemoveEntityList = new List<Entity>(10);
        /// <summary>
        /// 系统列表
        /// </summary>
        private readonly List<SystemBase> m_systemList = new List<SystemBase>();
        protected int m_maxEntityId = 0;
        protected int m_frameCount = -1;
        public bool m_isDirty = false;

        protected Dictionary<Type, WorldComponentBase> m_funcComponentDic = new Dictionary<Type, WorldComponentBase>();
        private CompManagerComponentBase m_componentManagerComponent = null;
        private CompFactoryComponentBase m_compCreater = null;

        public T CreateWorldComponent<T>() where T : WorldComponentBase
        {
            var instance = Activator.CreateInstance(typeof(T),this);
            m_funcComponentDic.Add(typeof(T), instance as T);
            return instance as T;
        }

        public T GetWorldComponent<T>() where T : WorldComponentBase
        {
            WorldComponentBase instance = null;
            if(m_funcComponentDic.TryGetValue(typeof(T), out instance))
            {
                return instance as T;
            }
            return null;
        }

        protected abstract CompManagerComponentBase CreateCompManagerComponent();
        protected abstract CompFactoryComponentBase CreateComponentCreaterComponent();

        protected virtual void ConstructFuncComponents()
        {
            m_componentManagerComponent = CreateCompManagerComponent();
            m_compCreater = CreateComponentCreaterComponent();
        }


        public void Initialize()
        {
            ConstructFuncComponents();
            foreach(var pair in m_funcComponentDic)
            {
                pair.Value.PreInitialize();
            }
            foreach (var pair in m_funcComponentDic)
            {
                pair.Value.Initialize();
            }
            foreach (var pair in m_funcComponentDic)
            {
                pair.Value.PostInitialize();
            }
        }


        /// <summary>
        /// 添加System
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddSystem<T>() where T : SystemBase
        {
            var ins = System.Activator.CreateInstance(typeof(T), new object[] { this }) as SystemBase;
            m_systemList.Add(ins);
        }

        /// <summary>
        /// 获取系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSystem<T>() where T : SystemBase
        {
            foreach (var system in m_systemList)
            {
                var res = system as T;
                if (res != null)
                {
                    return res;
                }
            }
            return null;
        }

        protected Entity AllocateEntity()
        {
            var entity = m_entityPool.Allocate();
            //entity.SetWorld(this);
            entity.SetId(AllocateEntityId());
            Debug.Log(string.Format("TheWorld:AddEntity entityId:{0}", entity.ID));
            Debug_ReportEnityPool();
            SetDirty();
            return entity;
        }

        public int AllocateEntityId()
        {
            return m_maxEntityId++;
        }

        public T AddComponent4Entity<T>(int entityId) where T:ComponentBase
        {
            var entity = GetEntityById(entityId);
            return AddComponent4Entity<T>(entity);
        }

        public T AddComponent4Entity<T>(Entity entity) where T : ComponentBase
        {
            var comp = CreateComponent<T>();
            comp.SetOwnEntityId(entity.ID);
            comp.Initialize();
            SetDirty();
            return comp;
        }

        public void RemoveEntity(Entity entity)
        {
            m_toRemoveEntityList.Add(entity);
        }

        public Entity GetEntityById(int id)
        {
            foreach (var entity in m_entityPool.m_array)
            {
                if (entity.IsInusing())
                {
                    if (entity.ID == id)
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public void Debug_ReportEnityPool()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < m_entityPool.m_array.Count; i++)
            {
                var e = m_entityPool.m_array[i];
                sb.Append("(");
                sb.Append(e.IsInusing()).Append(" ").Append(e.ID);
                sb.Append(")");
            }
            Debug.Log(string.Format("Debug_ReportEnityPool: {0}", sb.ToString()));
        }

        protected virtual void OnStep()
        {
            foreach (var system in m_systemList)
            {
                system.PreProcess();
            }
            if (m_isDirty)
                m_isDirty = false;
            foreach (var system in m_systemList)
            {
                system.Process();
            }
        }

        protected void DoRemoveEntity(Entity entity)
        {
            Debug.Log(string.Format("TheWorld:DoRemoveEntity entityId:{0}", entity.ID));
            entity.SetInusing(false);
            ComponentBase[] temp = null;
            var count = GetAllComponents4Entity(entity.ID, temp);
            for(int i = 0; i < count; i++)
            {
                var comp = temp[i];
                comp.SetInusing(false);
                comp.DeInitialize();
            }
        }

        public void Step()
        {
            m_frameCount++;
            OnStep();
            if (m_toRemoveEntityList.Count != 0)
            {
                foreach (var entity in m_toRemoveEntityList)
                {
                    DoRemoveEntity(entity);
                }
                m_toRemoveEntityList.Clear();
                SetDirty();
            }
        }

        /// <summary>
        /// 运行时添加实体后，设置为dirty
        /// </summary>
        /// <returns></returns>
        public void SetDirty()
        {
            m_isDirty = true;
        }

        public bool IsDirty()
        {
            return m_isDirty;
        }

        public T CreateComponent<T>() where T : ComponentBase
        {
            return m_compCreater.CreateComponent<T>();
        }

        #region IEntityManager
        public List<Entity> GetAllEntities()
        {
            var entityList = new List<Entity>();//todo 减少new
            foreach (var entity in m_entityPool.m_array)
            {
                if (entity.IsInusing())
                {
                    entityList.Add(entity);
                }
            }
            return entityList;
        }
        #endregion

        #region IComponentManager

        public int GetAllComponents4Entity(int entityId, ComponentBase[] array)
        {
            return m_componentManagerComponent.GetAllComponents4Entity(entityId, array);
        }

        public T GetComponent4Entity<T>(int entityId) where T : ComponentBase
        {
            return m_componentManagerComponent.GetComponent4Entity<T>(entityId);
        }

        public IcomponentFactory GetComponentCreater()
        {
            return ((IComponentManager)m_componentManagerComponent).GetComponentCreater();
        }
        #endregion
    }
}
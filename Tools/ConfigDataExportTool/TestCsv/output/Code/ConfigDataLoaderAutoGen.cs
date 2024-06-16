using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluebean.UGFramework.Asset;

namespace bluebean.UGFramework.ConfigData
{
    public partial class ConfigDataLoader
    {
	    #region 类内部变量
	    private Dictionary<int,ConfigDataPosition> m_configDataPosition = new Dictionary<int,ConfigDataPosition>();
	    private Dictionary<int,ConfigDataPositionGroup> m_configDataPositionGroup = new Dictionary<int,ConfigDataPositionGroup>();
		#endregion
		
		 partial void InitAllConfigDataTableName(){
			m_allConfigTableNames.Clear();
			m_allConfigTableNames.Add("Position");
			m_allConfigTableNames.Add("PositionGroup");
		}
		
		#region 访问函数
	    public Dictionary<int,ConfigDataPosition> GetAllConfigDataPosition () {
			return m_configDataPosition;
		}
	    public Dictionary<int,ConfigDataPositionGroup> GetAllConfigDataPositionGroup () {
			return m_configDataPositionGroup;
		}
        
	    public ConfigDataPosition GetConfigDataPosition (int id) {
			ConfigDataPosition data;
			(m_configDataPosition).TryGetValue(id, out data);
			return data;
		}
	    public ConfigDataPositionGroup GetConfigDataPositionGroup (int id) {
			ConfigDataPositionGroup data;
			(m_configDataPositionGroup).TryGetValue(id, out data);
			return data;
		}
        #endregion	
	
	    #region 反序列化
	    private void DeserializeConfigDataPosition (AssetObject scriptableObj) {
		    var data = scriptableObj.m_bytes;
		    MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
			List<ConfigDataPosition> list = (List<ConfigDataPosition>) obj;
			foreach(var configData in list){
				(m_configDataPosition).Add(configData.ID, configData);
			}
	    }
	    private void DeserializeConfigDataPositionGroup (AssetObject scriptableObj) {
		    var data = scriptableObj.m_bytes;
		    MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
			List<ConfigDataPositionGroup> list = (List<ConfigDataPositionGroup>) obj;
			foreach(var configData in list){
				(m_configDataPositionGroup).Add(configData.ID, configData);
			}
	    }
		
	    partial void InitAllDeserializeFuncs(){
			m_deserializeFuncDics.Clear();
		    m_deserializeFuncDics.Add("ConfigDataPosition", DeserializeConfigDataPosition);
		    m_deserializeFuncDics.Add("ConfigDataPositionGroup", DeserializeConfigDataPositionGroup);
	    }
		
		#endregion
	}
}
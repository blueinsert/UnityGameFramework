using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace bluebean.UGFramework.ConfigData
{
    public partial class ConfigDataLoader
    {
	    #region 类内部变量
	    private Dictionary<int,ConfigDataCourse> m_configDataCourse = new Dictionary<int,ConfigDataCourse>();
	    private Dictionary<int,ConfigDataInsertCourse> m_configDataInsertCourse = new Dictionary<int,ConfigDataInsertCourse>();
	    private Dictionary<int,ConfigDataPosition> m_configDataPosition = new Dictionary<int,ConfigDataPosition>();
	    private Dictionary<int,ConfigDataPositionGroup> m_configDataPositionGroup = new Dictionary<int,ConfigDataPositionGroup>();
	    private Dictionary<int,ConfigDataSound> m_configDataSound = new Dictionary<int,ConfigDataSound>();
		#endregion
		
		 partial void InitAllConfigDataTableName(){
			m_allConfigTableNames.Clear();
			m_allConfigTableNames.Add("Course");
			m_allConfigTableNames.Add("InsertCourse");
			m_allConfigTableNames.Add("Position");
			m_allConfigTableNames.Add("PositionGroup");
			m_allConfigTableNames.Add("Sound");
		}
		
		#region 访问函数
	    public Dictionary<int,ConfigDataCourse> GetAllConfigDataCourse () {
			return m_configDataCourse;
		}
	    public Dictionary<int,ConfigDataInsertCourse> GetAllConfigDataInsertCourse () {
			return m_configDataInsertCourse;
		}
	    public Dictionary<int,ConfigDataPosition> GetAllConfigDataPosition () {
			return m_configDataPosition;
		}
	    public Dictionary<int,ConfigDataPositionGroup> GetAllConfigDataPositionGroup () {
			return m_configDataPositionGroup;
		}
	    public Dictionary<int,ConfigDataSound> GetAllConfigDataSound () {
			return m_configDataSound;
		}
        
	    public ConfigDataCourse GetConfigDataCourse (int id) {
			ConfigDataCourse data;
			(m_configDataCourse).TryGetValue(id, out data);
			return data;
		}
	    public ConfigDataInsertCourse GetConfigDataInsertCourse (int id) {
			ConfigDataInsertCourse data;
			(m_configDataInsertCourse).TryGetValue(id, out data);
			return data;
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
	    public ConfigDataSound GetConfigDataSound (int id) {
			ConfigDataSound data;
			(m_configDataSound).TryGetValue(id, out data);
			return data;
		}
        #endregion	
	
	    #region 反序列化
	    private void DeserializeConfigDataCourse (AssetObject scriptableObj) {
		    var data = scriptableObj.m_bytes;
		    MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
			List<ConfigDataCourse> list = (List<ConfigDataCourse>) obj;
			foreach(var configData in list){
				(m_configDataCourse).Add(configData.ID, configData);
			}
	    }
	    private void DeserializeConfigDataInsertCourse (AssetObject scriptableObj) {
		    var data = scriptableObj.m_bytes;
		    MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
			List<ConfigDataInsertCourse> list = (List<ConfigDataInsertCourse>) obj;
			foreach(var configData in list){
				(m_configDataInsertCourse).Add(configData.ID, configData);
			}
	    }
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
	    private void DeserializeConfigDataSound (AssetObject scriptableObj) {
		    var data = scriptableObj.m_bytes;
		    MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
			List<ConfigDataSound> list = (List<ConfigDataSound>) obj;
			foreach(var configData in list){
				(m_configDataSound).Add(configData.ID, configData);
			}
	    }
		
	    partial void InitAllDeserializeFuncs(){
			m_deserializeFuncDics.Clear();
		    m_deserializeFuncDics.Add("ConfigDataCourse", DeserializeConfigDataCourse);
		    m_deserializeFuncDics.Add("ConfigDataInsertCourse", DeserializeConfigDataInsertCourse);
		    m_deserializeFuncDics.Add("ConfigDataPosition", DeserializeConfigDataPosition);
		    m_deserializeFuncDics.Add("ConfigDataPositionGroup", DeserializeConfigDataPositionGroup);
		    m_deserializeFuncDics.Add("ConfigDataSound", DeserializeConfigDataSound);
	    }
		
		#endregion
	}
}
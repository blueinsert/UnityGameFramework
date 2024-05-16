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
	    private Dictionary<int,ConfigDataConst> m_configDataConst = new Dictionary<int,ConfigDataConst>();
		#endregion
		
		 partial void InitAllConfigDataTableName(){
			m_allConfigTableNames.Clear();
			m_allConfigTableNames.Add("Const");
		}
		
		#region 访问函数
	    public Dictionary<int,ConfigDataConst> GetAllConfigDataConst () {
			return m_configDataConst;
		}
        
	    public ConfigDataConst GetConfigDataConst (int id) {
			ConfigDataConst data;
			(m_configDataConst).TryGetValue(id, out data);
			return data;
		}
        #endregion	
	
	    #region 反序列化
	    private void DeserializeConfigDataConst (AssetObject scriptableObj) {
		    var data = scriptableObj.m_bytes;
		    MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(stream);
			List<ConfigDataConst> list = (List<ConfigDataConst>) obj;
			foreach(var configData in list){
				(m_configDataConst).Add(configData.ID, configData);
			}
	    }
		
	    partial void InitAllDeserializeFuncs(){
			m_deserializeFuncDics.Clear();
		    m_deserializeFuncDics.Add("ConfigDataConst", DeserializeConfigDataConst);
	    }
		
		#endregion
	}
}
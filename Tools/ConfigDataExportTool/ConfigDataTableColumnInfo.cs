using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.ConfigDataExportTool
{
    public class ConfigDataTableColumnInfo
    {
        public ConfigDataTable m_configDataTable;
        public int m_index;
        public string m_exportType;//line 3
        public string m_name;//line 4
        public string m_desc;//line 5
        public string m_type;//line 6
        #region list类型有关
        public string[] m_subTypeParamNames;//line 7
        public string[] m_subTypeParamDescs;//line 8
        public string[] m_subTypeParamTypes;//line 9
        public string[] m_subTypeParamDefaultValues;//line 10
        #endregion
        public string[] m_foreignKeys;//line 11
        public string m_multiplyLanguage;

        //外键对应的主表列
        public ConfigDataTableColumnInfo[] m_mainColumnInfoArray;
        public bool m_hasForeignKey = false;

        public void Check()
        {
            //todo 检查数据填写有效性
        }

        /// <summary>
        /// 收集外键对应的主列信息
        /// </summary>
        /// <returns></returns>
        public void CollectForeignKeyMainColumnInfo()
        {
            m_hasForeignKey = false;
            if (m_configDataTable.HeadInfo.TableType == ConfigDataType.ENUM)
            {
                return;
            }
            if(m_foreignKeys==null)
            {
                return;
            }
            m_mainColumnInfoArray = new ConfigDataTableColumnInfo[m_foreignKeys.Length];
            for (int i = 0; i < m_mainColumnInfoArray.Length; i++)
            {
                m_mainColumnInfoArray[i] = null;
            }
            if (m_foreignKeys == null || m_foreignKeys.Length == 0)
                return;
            if (m_foreignKeys.Length == 1 && m_foreignKeys[0] == "NULL")
            {
                return;
            }
            if (!IsListType())
            {
                ConfigDataTableColumnInfo columnInfo;
                if (m_foreignKeys[0] != null && m_foreignKeys[0].ToLower() != "null")
                {
                    ConfigDataManager.Instance.FindMainColumnInfo(m_configDataTable.HeadInfo.m_name, m_name, 0, m_foreignKeys[0], out columnInfo);
                    if (columnInfo.m_type != m_type)
                    {
                        ConfigDataException except = new ConfigDataException(m_configDataTable.HeadInfo.m_name, m_name, 0, string.Format("外键{0}列类型与主表列不一致", m_foreignKeys[0]));
                        throw except;
                    }
                    m_mainColumnInfoArray[0] = columnInfo;
                    m_hasForeignKey = true;
                }
            }
            else
            {
                for(int i = 0; i < m_foreignKeys.Length; i++)
                {
                    ConfigDataTableColumnInfo columnInfo;
                    if(m_foreignKeys[i] != null && m_foreignKeys[i].ToLower() != "null")
                    {
                        ConfigDataManager.Instance.FindMainColumnInfo(m_configDataTable.HeadInfo.m_name, m_name, 0, m_foreignKeys[i], out columnInfo);
                        if (columnInfo.m_type != m_type)
                        {
                            ConfigDataException except = new ConfigDataException(m_configDataTable.HeadInfo.m_name, m_name, 0, string.Format("外键{0}列类型与主表列不一致", m_foreignKeys[i]));
                            throw except;
                        }
                        m_mainColumnInfoArray[i] = columnInfo;
                        m_hasForeignKey = true;
                    }
                    
                }
            }
        }

        public bool IsListType()
        {
            return m_type.StartsWith("LIST");
        }

        /// <summary>
        /// 获取类型字符串
        /// </summary>
        /// <returns></returns>
        public string GetTypeFullName()
        {
            if (!IsListType())
            {
                var typeStr = ConfigDataManager.Instance.TypeStr2Type(m_type).FullName;
                return typeStr;
            }
            else
            {
                var splits = m_type.Split(new char[] { ':' });
                if (splits.Length == 1)
                {
                    var paramType = ConfigDataManager.Instance.TypeStr2Type(m_subTypeParamTypes[0]).FullName;
                    var typeStr = string.Format("System.Collections.Generic.List<{0}>", paramType);
                    return typeStr;
                }else if (splits.Length == 2)
                {
                    var paramType = splits[1];
                    var typeStr = string.Format("System.Collections.Generic.List<{0}>", paramType);
                    return typeStr;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool HasSubTypeDef()
        {
            if (!IsListType())
                return false;
            var splits = m_type.Split(new char[] { ':' });
            if (splits.Length == 2)
            {
                return true;
            }
            return false;
        }

        public string GetSubTypeName()
        {
            var splits = m_type.Split(new char[] { ':' });
            return splits[1];
        }

        public CodeTypeDeclaration BuildSubTypeDeclaration(string typeName)
        {  
            CodeTypeDeclaration typeDefineClass = new CodeTypeDeclaration(typeName);

            typeDefineClass.CustomAttributes.Add(new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(SerializableAttribute))));
            typeDefineClass.IsClass = true;
            typeDefineClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
            for (int i = 0; i < m_subTypeParamTypes.Length; i++)
            {
                string paramName = m_subTypeParamNames[i];
                string paramTypeStr = m_subTypeParamTypes[i];
                string fieldName = "m_" + paramName;
                string typeFullName = ConfigDataManager.Instance.TypeStr2Type(paramTypeStr).FullName;
                CodeMemberField field = new CodeMemberField(typeFullName, fieldName);
                field.Attributes = MemberAttributes.Private;
                //添加field
                typeDefineClass.Members.Add(field);
                CodeMemberProperty property = new CodeMemberProperty();
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                property.Name = paramName;
                property.Type = new CodeTypeReference(typeFullName);
                property.HasGet = true;
                property.HasSet = true;
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertySetValueReferenceExpression()));
                //添加property
                typeDefineClass.Members.Add(property);
            }
            return typeDefineClass;
        }
    }
}

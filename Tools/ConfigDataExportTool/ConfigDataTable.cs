using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace bluebean.ConfigDataExportTool
{
    

    public class ConfigDataTable
    {
        #region 模板访问变量
        public string TableType
        {
            get
            {
                if(m_headInfo.TableType == ConfigDataType.DATA)
                {
                    return "DataTable";
                }
                else
                {
                    return "EnumTable";
                }
            }
        }

        public string TableName
        {
            get
            {
                return m_headInfo.Name;
            }
        }
        #endregion

        public string FilePath
        {
            get
            {
                return m_filePath;
            }
        }

        public ConfigDataTableHeadInfo HeadInfo
        {
            get
            {
                return m_headInfo;
            }
        }

        public DataGrid Data
        {
            get
            {
                return m_dataGrid;
            }
        }

        private string m_filePath;

        private ConfigDataTableHeadInfo m_headInfo;
        
        private DataGrid m_dataGrid;

        public ConfigDataTable()
        {
            m_headInfo = new ConfigDataTableHeadInfo();
            m_headInfo.SetOwner(this);
        }

        public void SetDataGrid(DataGrid dataGrid)
        {
            m_dataGrid = dataGrid;
        }

        public void SetFilePath(string path)
        {
            m_filePath = path;
        }

        public void ParseTableHeadInfo()
        {
             m_headInfo.Parse(m_dataGrid);
        }

        /// <summary>
        /// 构建一个表对应的类的CodeTypeDeclaration,用于产生代码
        /// </summary>
        /// <param name="configData"></param>
        /// <returns></returns>
        public CodeTypeDeclaration BuildCodeTypeDeclaration()
        {
            CodeTypeDeclaration typeDefineClass = new CodeTypeDeclaration("ConfigData" + HeadInfo.Name);
            if (HeadInfo.TableType == ConfigDataType.DATA)
            {
                typeDefineClass.CustomAttributes.Add(new CodeAttributeDeclaration(
                    new CodeTypeReference(typeof(SerializableAttribute))));
                typeDefineClass.IsClass = true;
                typeDefineClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
                foreach (var pair in HeadInfo.ColumnInfoDic)
                {
                    var columnInfo = pair.Value;
                    string fieldName = "m_" + columnInfo.m_name;
                    string columnTypeFullName = columnInfo.GetTypeFullName();
                    CodeMemberField field = new CodeMemberField(columnTypeFullName, fieldName);
                    field.Attributes = MemberAttributes.Private;
                    typeDefineClass.Members.Add(field);
                    CodeMemberProperty property = new CodeMemberProperty();
                    property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    property.Name = columnInfo.m_name;
                    property.Type = new CodeTypeReference(columnTypeFullName);
                    property.HasGet = true;
                    property.HasSet = true;
                    property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                    property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertySetValueReferenceExpression()));
                    typeDefineClass.Members.Add(property);
                }
            }
            else if (HeadInfo.TableType == ConfigDataType.ENUM)
            {
                typeDefineClass.IsEnum = true;
                typeDefineClass.TypeAttributes = System.Reflection.TypeAttributes.Public;
                foreach (var pair in HeadInfo.EnumDic)
                {
                    typeDefineClass.Members.Add(new CodeSnippetTypeMember(string.Format("{0} = {1},", pair.Key, pair.Value)));
                }

            }
            return typeDefineClass;
        }
    }
}

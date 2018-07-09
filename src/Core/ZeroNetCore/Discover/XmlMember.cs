using Agebull.ZeroNet.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 从程序集文档读取的注释信息
    /// </summary>
    internal class XmlMember : DocumentItem
    {
        /// <summary>
        /// 读取的帮助XML
        /// </summary>
        private static readonly List<XmlMember> HelpXml = new List<XmlMember>();
        
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static XmlMember Find(Type type)
        {
            var re = HelpXml.FirstOrDefault(p => /*p.Type == "T" &&*/ p.Name == type.FullName);
            if (re == null && !Assemblies.Contains(type.Assembly))
            {
                Load(type.Assembly);
            }
            return re;
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="subType">注释类型 T 类型 P 属性 M 方法 F字段</param>
        /// <param name="sub">子级名称</param>
        /// <returns></returns>
        public static XmlMember Find(Type type, string sub,string subType = "P")
        {
            var name = $"{type.FullName}.{sub}";
            return HelpXml.FirstOrDefault(p => /*p.Type == subType &&*/ p.Name == name);
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="type">注释类型 T 类型 P 属性 M 方法 F字段</param>
        /// <returns></returns>
        public static XmlMember Find(string name, string type = "T")
        {
            return HelpXml.FirstOrDefault(p => /*p.Type == type &&*/ p.Name == name);
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="type">注释类型 T 类型 P 属性 M 方法 F字段</param>
        /// <param name="sub">子级名称</param>
        /// <returns></returns>
        public static XmlMember Find(string name, string sub, string type = "P")
        {
            name = $"{name}.{sub}";
            return HelpXml.FirstOrDefault(p => /*p.Type == type &&*/ p.Name == name);
        }

        /// <summary>
        /// 读取的帮助XML
        /// </summary>
        private static readonly List<Assembly> Assemblies = new List<Assembly>();

        /// <summary>
        /// 载入
        /// </summary>
        /// <returns></returns>
        public static void Load(Assembly assembly)
        {
            if (Assemblies.Contains(assembly))
                return;
            Assemblies.Add(assembly);
            Load(Path.Combine(Path.GetDirectoryName(assembly.Location), Path.GetFileNameWithoutExtension(assembly.Location) + ".xml"));
        }

        /// <summary>
        /// 载入
        /// </summary>
        /// <returns></returns>
        public static void Load(string path)
        {
            ZeroTrace.WriteInfo("DOC", path);
            if (!File.Exists(path))
                return;
            XElement xRoot = XElement.Load(path);
            var xElement = xRoot.Element("members");
            if (xElement == null)
            {
                return;
            }
            var chars = new char[] { ':', '(', '`' };
            var members = (from p in xElement.Elements("member")
                           let name = p.Attribute("name")
                           where !string.IsNullOrEmpty(name?.Value)
                           let summary = p.Element("summary")
                           let remarks = p.Element("remarks")
                           let seealso = p.Element("seealso")
                           let value = p.Element("value")
                           let example = p.Element("example")
                           let returns = p.Element("returns")
                           let paramss = p.Elements("param")
                           let np = name.Value.Split(chars)
                           select new XmlMember
                           {
                               Type = np[0],
                               Name = np[1],
                               Caption = summary?.Value.ConverToAscii(),
                               Description = remarks?.Value.ConverToAscii(),
                               Seealso = seealso?.Value,
                               Value = value?.Value,
                               Example = example?.Value,
                               Returns = returns?.Value,
                               XArguments = paramss
                           });

            HelpXml.AddRange(members);
        }

        /// <summary>
        /// 类名
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// 返回
        /// </summary>
        public string Returns
        {
            get;
            set;
        }

        /// <summary>
        /// 参数(XML对象)
        /// </summary>
        internal IEnumerable <XElement> XArguments
        {
            get;
            set;
        }
        Dictionary<string, string> _arguments;
        /// <summary>
        /// 参数字典
        /// </summary>
        public Dictionary<string,string> Arguments
        {
            get
            {
                if (_arguments != null)
                    return _arguments;
                _arguments = new Dictionary<string, string>();
                if (XArguments == null)
                    return _arguments;
                foreach(var el in XArguments)
                {
                    _arguments.Add(el.Attribute("name").Value, el.Value);
                }
                return _arguments;
            }
        }
    }
}
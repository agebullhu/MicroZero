/*design by:agebull designer date:2017/5/14 17:18:59*/
using System;
using System.Collections.Generic;
using System.Text;
using Agebull.Common.DataModel;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 文本参数
    /// </summary>
    internal partial class StringArgumentData : DataObjectBase, ITson
    {
        #region 构造
        
        /// <summary>
        /// 构造
        /// </summary>
        public StringArgumentData()
        {
        }
        
        #endregion


        #region 属性字义

        /// <summary>
        /// 参数:参数的实时记录顺序
        /// </summary>
        internal const int Real_Argument = 1;

        /// <summary>
        /// 参数:参数
        /// </summary>
        internal string _argument;

        partial void OnArgumentGet();

        partial void OnArgumentSet(ref string value);

        partial void OnArgumentSeted();

        /// <summary>
        /// 参数:参数
        /// </summary>
        /// <remarks>
        /// 参数
        /// </remarks>
        public  string Argument
        {
            get
            {
                OnArgumentGet();
                return this._argument;
            }
            set
            {
                if(this._argument == value)
                    return;
                OnArgumentSet(ref value);
                this._argument = value;
                OnArgumentSeted();
                OnPropertyChanged(nameof(Argument));
                
            }
        }
        #endregion

        #region 属性扩展

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(string property, object value)
        {
            switch(property.Trim().ToLower())
            {
            case "argument":
                this.Argument = value?.ToString();
                return;
            }

            //System.Diagnostics.Trace.WriteLine(property + @"=>" + value);

        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="property"></param>
        protected override object GetValueInner(string property)
        {
            switch(property)
            {
            case "Argument":
                return this.Argument;
            }

            return null;
        }

        protected override void SetValueInner(int property, object value)
        {
            this.Argument = value?.ToString();
        }

        protected override object GetValueInner(int property)
        {
            return this.Argument;
        }

        #endregion

        #region 复制


        /// <summary>
        /// 自定义复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        partial void CopyExtendValue(StringArgumentData source);

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected override void CopyValueInner(DataObjectBase source)
        {
            var sourceEntity = source as StringArgumentData;
            if(sourceEntity == null)
                return;
            this._argument = sourceEntity._argument;
            CopyExtendValue(sourceEntity);
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void Copy(StringArgumentData source)
        {
            this.Argument = source.Argument;
        }
        #endregion

        #region 文本


        /// <summary>
        /// 显示为文本
        /// </summary>
        /// <returns>文本</returns>
        public override string ToString()
        {
            return $@"
[参数]:{Argument}";
        }
        #endregion

        #region 序列化

        #region 数据常量

        /// <summary>
        /// StringArgument类型代号
        /// </summary>
        public const int TYPE_INDEX_STRINGARGUMENT = 0x120004;

        /// <summary>
        /// StringArgument类型代号
        /// </summary>
        public const int EntityId = 0x120004;

        /// <summary>
        /// 文本的参数-参数 的字段索引
        /// </summary>
        public const byte FIELD_INDEX_STRINGARGUMENT_ARGUMENT = 0x1;

        /// <summary>
        /// 文本的参数的合理序列化长度
        /// </summary>
        public const int TSON_BUFFER_LEN_STRINGARGUMENT = 1120;


        #endregion 数据常量


        #region ITson
        
        /// <summary>
        /// 类型ID
        /// </summary>
        public int TypeId { get { return TYPE_INDEX_STRINGARGUMENT; } }
        
        /// <summary>
        /// 安全的缓存长度
        /// </summary>
        public int SafeBufferLength {get { return TSON_BUFFER_LEN_STRINGARGUMENT; } }

        /// <summary>
        /// 从TSON反序列化
        /// </summary>
        public void Deserialize(TsonDeserializer reader)
        {
            reader.Begin();
            while (!reader.IsEof)
            {
                int idx = reader.ReadIndex();
                switch (idx)
                {
                case FIELD_INDEX_STRINGARGUMENT_ARGUMENT://参数
                    reader.Read(ref this._argument);
                    continue;
                }
                break;//错误发生时中止
            }
            reader.End();
        }

        /// <summary>
        /// 序列化到Tson
        /// </summary>
        public void Serialize(TsonSerializer writer)
        {
            writer.Begin(TYPE_INDEX_STRINGARGUMENT, 1);
            
            //参数
            if(!writer.IsEmpty(this._argument))
            {
                writer.WriteIndex(FIELD_INDEX_STRINGARGUMENT_ARGUMENT);
                writer.Write(this._argument);
            }

            writer.End();
        }
        #endregion
        #endregion 序列化
    }

}
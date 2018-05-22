using System;
using System.Collections.Generic;
using System.Text;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;
using WebMonitor;

namespace Agebull.ZeroNet.LogService
{
    /// <inheritdoc />
    /// <summary>
    /// 日志服务
    /// </summary>
    public class LogSubscribe : SubStation
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static LogSubscribe Station { get; set; }


        /// <summary>
        /// 构造
        /// </summary>
        public LogSubscribe()
        {
            StationName = "RemoteLog";
            Subscribe = "";
            Station = this;
            IsRealModel = true;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            try
            {
                List<RecordInfo> items = JsonConvert.DeserializeObject<List<RecordInfo>>(args.Content);
                foreach (var info in items)
                {
                    StringBuilder code = new StringBuilder();

                    code.AppendLine("<table style='width:99%'><col style='width:120px'/><col/><col style='width:120px'/><col/>");
                    code.AppendLine($"<tr style='border-bottom: silver 1px solid;padding: 4px'><td>类型</td><td>{info.TypeName}</td><td>序号</td><td>{info.Index}</td></tr>");
                    code.AppendLine($"<tr style='border-bottom: silver 1px solid;padding: 4px'><td>标识</td><td>{info.RequestID}</td><td>时间</td><td>{DateTime.Now}({info.Time})</td></tr>");
                    code.AppendLine($"<tr style='border-bottom: silver 1px solid;padding: 4px'><td>机器</td><td>{info.Machine}</td><td>用户</td><td>{info.User}</td></tr>");
                    code.AppendLine("</table>");
                    var lines = info.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        code.AppendLine("<table><tr style='border-bottom: silver 1px solid;padding: 4px'>");
                        var line = lines[0];
                        var words = line.Split(new char[] { '|', '│' },5);
                        for (var index = 0; index < words.Length && index < 4; index++)
                        {
                            var word = words[index];
                            code.AppendLine($"<td>{word.Trim()}</td>");
                        }

                        for (var index = 1; index < lines.Length; index++)
                        {
                            line = lines[index].Substring(1);//.TrimStart(new char[] { '|', '│', '┌', '└', '├' });
                            words = line.Split(new char[] { '|', '│' });
                            code.AppendLine("<tr style='border-bottom: silver 1px solid;padding: 4px'>");
                            var word = words[0];
                            code.Append("<td");
                            if (words.Length == 1)
                            {
                                code.AppendLine(" colspan='4'>");
                            }
                            else
                            {
                                code.Append('>');
                            }
                            foreach (var ch in word)
                            {
                                switch (ch)
                                {
                                    default:
                                        code.Append(ch);
                                        break;
                                    case '┌':
                                    case '│':
                                    case '└':
                                    case '├':
                                    case '┴':
                                    case '─':
                                        code.Append("　");
                                        break;
                                }
                            }
                            code.AppendLine("</td>");
                            for (var i = 1; i < words.Length; i++)
                            {
                                word = words[i];
                                code.AppendLine($"<td>{word.Trim()}</td>");
                            }

                            code.AppendLine("</tr>");
                        }

                        code.AppendLine("</table>");
                    }
                    //var lines = info.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (var line in lines)
                    //{
                    //    code.Append(line.Replace(' ', '　'));
                    //    code.AppendLine("</td></tr>");
                    //}
                    WebSocketPooler.Instance?.Publish(info.Type.ToString(), code.ToString());
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}
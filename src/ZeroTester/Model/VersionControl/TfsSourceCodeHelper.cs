using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Agebull.Common.Base;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// TFS源代码控制帮助类
    /// </summary>
    public class TfsSourceCodeHelper : ScopeBase
    {
        private string _codeFile;
        /// <param name="file">文件名</param>
        public TfsSourceCodeHelper(string file)
        {
            _codeFile = file;

            //string path = Path.GetDirectoryName(file);

            ////设置服务器文件夹
            //if (path == null)
            //    return;
            //string serverFolder = "$/GBS_Trade_1202/" + path.ToLower().Replace(@"c:\work\", "").Replace('\\', '/');
            ////设置本地映射文件
            //string localFolder = path;



            //_projectCollection = new TfsTeamProjectCollection(new Uri("http://192.168.1.65:8080/tfs/"));
            ////设置版本控制Server
            
            //var versionControl = _projectCollection.GetService<VersionControlServer>();

            ////设置工作空间名称
            //var workspaceName = Environment.MachineName;
            
            //_workspace = versionControl.GetWorkspace(workspaceName, versionControl.AuthorizedUser);
            // 创建工作空间的本地映射地址
            //workspace.Map(serverFolder, localFolder);
        }

        private TfsTeamProjectCollection _projectCollection;
        private readonly Workspace _workspace;
        /// <summary>
        /// 更新文件
        /// </summary>
        public void Update()
        {
            _workspace.PendEdit(_codeFile);
            _workspace.PendAdd(_codeFile);
            var pendingAdds = new List<PendingChange>(_workspace.GetPendingChanges());
            var array = pendingAdds.ToArray();
            _workspace.CheckIn(array, "设计器生成代码后自动提交");
        }

        /// <summary>
        /// 签出代码
        /// </summary>
        public void CheckOut()
        {
            //_workspace.PendEdit(_codeFile);
        }

        /// <summary>
        /// 签入代码
        /// </summary>
        public void CheckIn()
        {
            int changesetForAdd;
            //将这个文件排队等待迁入TFS管理
            _workspace.PendAdd(_codeFile);

            //  创建等待添加的文件项集合
            var pendingAdds = new List<PendingChange>(_workspace.GetPendingChanges(_codeFile));
            var array = pendingAdds.ToArray();
            if (array.Length <= 0 ||
                !array.Any(p => string.Equals(p.FileName, _codeFile, StringComparison.InvariantCultureIgnoreCase)))
            {
                changesetForAdd = _workspace.Undo(_codeFile);
                Debug.WriteLine(changesetForAdd);
                return;
            }
            var a2 =
                array.Where(p => string.Equals(p.FileName, _codeFile, StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
            // 将工作项CheckIn系统中
            changesetForAdd = _workspace.CheckIn(a2, "设计器自动提交");
            Debug.WriteLine(changesetForAdd);
        }

        #region Overrides of ScopeBase

        protected override void OnDispose()
        {
            _projectCollection?.Dispose();
            _projectCollection = null;
        }

        #endregion
    }
}
# ZmqNet
基于ZMQ的Rpc框架，实现分布式远程调用、消息队列、自动扩容等网络功能。

## 系统模块
### 1 ZeroCenter
基于Libzmq的C++编写的中心控制类,实现以下几个功能:
1. SystemMangement
- 工作站配置管理:安装\卸载\获取
- 工作站状态管理:启动\关闭\暂停\恢复
- 工作站心跳管理:加入\就绪\退出\健康分析
- 计划任务管理:加入\执行\退出\修改
2. SystemMonitor
- 系统状态广播
- 工作站状态广播
- 运行计数定时广播
3. 站点类型
- Pub:实现发布订阅功能
- Api:实现Api站点
- Vote:实现多路并发的投票模式
 
### 2 Clrzmq.Core

基于Clrzmq重构的C#使用Libzmq的类库,支持.Net Core2.0

### 3 ZeroNet.Core
.NetCore2.0下ZeroNet站点的核心实现,实现以下几个功能
1. 包括ZMQ使用在内的与ZeroCenter交互的基础封装
2. ZeroStation:所有工作站的基类
3. ApiStation:Api工作站的实现类,特点如下:
- 实现自动ApiControler发现,保持与MVC方式90%的相似度,使用原使用WebApi的简单移植.
- 实现跨机器的上下文保持
- 标准的请求与返回格式定义
- 自动安装与服务注册,使用方通过与ZeroCenter通讯即可透明调用.
3. SubStation:消息订阅处理基类,重载时只需要关心业务逻辑,无需关心通讯问题
4. PubStation:消息发布的处理基类,重载时只需简单调用即可
5. ZeroPublisher:通用的简单消息发布类,根据名称即可发布,无需关心更多,也不需要重载.
6. 其它


### 4 HttpRouteer
实现Http请求与ZeroCenter路由,用于保证原有对外接口通讯方式不变,同时提供基础安全功能.

### 5 WebMonitor
可视化系统监控网站.通过WebSocket实时推送消息.

### 6 RemoteLog
实现日志广播与基础处理功能

## Demo
[demo]: http://118.31.13.240  "http://118.31.13.240"



# 计划任务

## 1. 数据格式

### 1.1 对应配置
- redis_addr : Redis地址
- redis_defdb : Redis存储DB
- plan_exec_timeout : 任务执行超时(秒)

### 1.2 计划类型
```c++
/**
* \brief 计划类型
*/
enum class plan_date_type
{
	/**
	* \brief 无计划，立即发送
	*/
	none,
	/**
	* \brief 在指定的时间发送
	*/
	time,
	/**
	* \brief 秒间隔后发送
	*/
	second,
	/**
	* \brief 分钟间隔后发送
	*/
	minute,
	/**
	* \brief 小时间隔后发送
	*/
	hour,
	/**
	* \brief 日间隔后发送
	*/
	day,
	/**
	* \brief 每周几
	*/
	week,
	/**
	* \brief 每月几号
	*/
	month
};
```
### 1.3 计划
```c++
/**
* \brief 消息
*/
class plan_message
{
	public:
	/**
	* \brief 消息标识
	*/
	int64_t plan_id;

	/**
	* \brief 发起者提供的标识
	*/
	shared_char request_id;

	/**
	* \brief 站点
	*/
	shared_char station;

	/**
	* \brief 原始请求者
	*/
	shared_char caller;

	/**
	* \brief 计划说明
	*/
	shared_char description;

	/**
	* \brief 计划类型
	*/
	plan_date_type plan_type;

	/**
	* \brief 类型值
	*/
	int plan_value;

	/**
	* \brief 重复次数,0不重复 >0重复次数,-1永久重复
	*/
	int plan_repet;

	/**
	* \brief 执行次数
	*/
	int real_repet;
	/**
	* \brief 是否空跳
	*/
	bool no_keep;

	/**
	* \brief 最后一次执行状态
	*/
	int last_state;

	/**
	* \brief 计划时间
	*/
	time_t plan_time;

	/**
	* \brief 执行时间
	*/
	time_t exec_time;

	/**
	* \brief 消息内容
	*/
	vector<shared_char> messages;
};
```
## 2. 数据存储
使用Redis存储
### 2.1. 时间队列
使用SortSet,每次执行取出 小等于当前时间的Name执行

- **Key**
 plan:time:set
- **Name** 
 plan:**msg**:[_station_]:[_plan_id_]
 _station_  : 任务执行目标站点，
 _plan_id_  : 十六 进制格式化的任务标识
- **score**
 Unix时间戳(1970-1-1到计划时间的秒数)

### 2.2. 消息存储
使用String , Json存储

- **Key**
 plan:**msg**:[station]:[plan_id]
 _station_ : 任务执行目标站点，
 _plan_id_ : 十六 进制格式化的任务标识
- **Value**
 Json格式的消息体(见数据格式)
### 2.3. 任务参与者与执行结果
> 普通任务,参与者在执行完成才登记(后续版本可能修改为提前登记)

- **Key**
 plan:**work**:[station]:[plan_id]
 _station_ : 任务执行目标站点，
 _plan_id_ : 十六 进制格式化的任务标识

- **Value**
执行结果的原始数据(说明帧 + 内容帧)的Json数组

## 3.   任务流程

### 3.1 任务发布
- 说明帧命令状态符设置为 ZERO_BYTE_COMMAND_PLAN(2)
- 必须存在帧类型为 ZERO_FRAME_PLAN 的数据帧,有效字段如下:

``` c++
/**
* \brief 计划说明
*/
char* description;

/**
* \brief 计划类型
*/
plan_date_type plan_type;

/**
* \brief 类型值
*/
int plan_value;

/**
* \brief 重复次数,0不重复 >0重复次数,-1永久重复
*/
int plan_repet;

/**
* \brief 是否空跳
*/
bool no_keep;

/**
* \brief 计划时间
*/
time_t plan_time;
``` 
- 必须有命令内容
 1 API或Vote:必须有命令帧
 2 Pub: 必须完整

### 3.2 任务保存
- plan_id : 如有global_id帧,使用此值,如无系统生成一个global_id
- 计算第一次执行时间,如数据错误，任务将不能正确保存
- 保存到执行队列
- 保存消息内容

### 3.3 任务执行时间计算
#### 3.3.1 明确执行时间
- plan_type time
- plan_time  有效时间
> 如过时 : 且设置no_keep则计划会执行,否则直接跳过

- plan_repet 无效,只执行一次
- no_keep 如时间已过,设置为true则照样执行

#### 3.3.2 延时处理
- plan_type : second minute hour day
- plan_time  有效时间或0,如为0,起始时间为服务器接收任务的当前时间
- plan_repet : 不等于0
- plan_value : 大等于0 且 小于32767
- no_keep=false : 当设置为false时,所有小于计算时当前时间的都会跳过(包括因执行时间过长而耽误的时间)
> 特别说明:如到最后一次执行时间也小于当前时间,则任务自动删除,可能一次也未执行过.

- no_keep=true : 每次计划都延时一次且执行完成后再进入下一次时间

#### 3.3.3 每周循环
- plan_type : week
- plan_repet : 不等于0
- plan_value : 0-6,周日为0
- plan_time : 仅时间部分有效,当天执行时间
> 所计划时间均为未来时间

#### 3.3.4 每月循环
- plan_type : month
- plan_repet : 不等于0
- plan_value(1~31) : 指定号数,如当月无此号,则为当月最后一天
- plan_value(0) : 当月最后一天 
- plan_value(-1~-31) : 当月最后一天加此数字(因为是负数,其实时减法)得出的号数,如小于1,则为当月1号.
- plan_time : 仅时间部分有效,当天执行时间
> 所计划时间均为未来时间

### 3.3 执行计划
> 所有可执行的计划,都必须存在于执行队列中,否则将永不执行.不在执行队列中的计划,可删除重新走计划流程,或手工修改执行计划(非到万不得已不要使用)

1. 系统取出小于当前时间的所有键,循环执行
2. 如当前计划已在执行(ZERO_STATUS_RUNING_ID),如到达超时,则继续执行;否则跳过.
3. 如计划状态设置ZERO_STATUS_FRAME_PLANERROR_ID将不执行且移出执行队列.
> 如Station为空,或帧数量不足5个(中途被异常修改)也将设置状态为ZERO_STATUS_FRAME_PLANERROR_ID

> 计划状态为ZERO_STATUS_FRAME_PLANERROR_ID等同于剥夺计划执行权

4. 发送参数会包含的任务消息体,执行者可以使用此中提供的信息,特别有用的字段为plan_time,可作为处理数据的时间区间

5. 发送者名称为虚拟名称 \*[plan_id],具体执行站点依据此名称(\*为第一字节)回发到任务调度站点
6. 帧状态位设置为ZERO_BYTE_COMMAND_PROXY,以此证明为任务计划代理执行,如无此状态位,将收不到ZERO_STATUS_RUNING_ID状态.
7. 如命令发送失败或未收到执行状态,仅设置计划状态(即下次循环继续执行)
> 原因可能是处理超时,可尝试加大收发超时设置

8. 如果执行状态为ZERO_STATUS_NOT_WORKER_ID,会发出异常状态广播.
9. 如果执行状态为ZERO_STATUS_RUNING_ID,将会在对应状态下等等回发.
> 如果执行站点为API且执行正常总是返回此状态

10. 如果执行状态为ZERO_STATUS_OK_ID,将会进入执行结果处理流程.
> 计划执行站点为PUB且执行正常总是返回此状态

11. 非以上执行状态,任务将设置为错误状态并保存执行结果帧,然后将任务移出任务队列.

### 3.4 执行结果
> 处理站点会依据请求者名称(mail box)标识(\*)将执行结果回发到任务设计站点.

1. 根据请求者名称还原任务标识
2. 取出任务,如任务无法取出,将不再做任何处理(如为人为破坏,系统不无法负责,如为BUG则需要报告)
3. 设置计划状态,并保存之
4. 如执行结果状态为错误(> ZERO_STATUS_ERROR_FLAG),将不重新计算执行时间,而直接回到执行队列
5. 如执行结果状态为正确(< ZERO_STATUS_ERROR_FLAG),将正常计算下一次执行,如已完成执行计划,则正常删除.
> 删除数据的前提为任务下发者已记录执行结果.同时也为保证Redis数据不至于暴增

6. 执行结果将进行广播

### 3.5 结果广播
1. 主题为任务标题
2. 副题为任务标识
3. 计划内容为ZERO_FRAME_CONTENT
> 后续帧中如果有ZERO_FRAME_CONTENT,处理站点会因为已接收内容而放在Values中.


## 4. 任务发布站点
- 应负责任务日志或历史记录,如不能,应该有特定站点记录,否则过程数据将丢失
- 任务发布成功与否应进行处理,否则将导致计划不可控
- 发布任务应认真阅读本文档,以保证发布成功.

## 5. 任务处理站点
> 任何普通站点都可能成为任务处理站点且透明处理

1. API:数据帧的第一帧为原始请求者,原封不动返回

2. PUB:不关心原始发布者

3. 如果需要计划时间作为参数,可取出ZERO_FRAME_PLAN_TIME帧进行处理

4.如执行时间过长,可回发等待信号(Sustus=ZERO_BYTE_COMMAND_WAITING),以防止超时重发















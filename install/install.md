# Ubuntu部署ZeroCenter

## 1 系统更新

sudo apt-get update

## 2 Net Core
> https://www.microsoft.com/net/learn/get-started/linux/ubuntu16-04

wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https

sudo apt-get update

sudo apt-get install dotnet-sdk-2.1

## 3 安装 supervisor

sudo apt-get install supervisor
修改配置为HTTP
> supervisord -c /etc/supervisor/supervisord.conf
### supervisorctl命令
supervisorctl status

supervisorctl stop tomcat

supervisorctl start tomcat

supervisorctl restart tomcat

supervisorctl reread

supervisorctl update

# 4 安装redis
> https://www.cnblogs.com/zongfa/p/7808807.html

sudo apt-get install redis-server

> 注意：使用了16以外的库ID所以要修改Redis.Conf(/etc/redis/Redis.Conf) 

databases 16 => 32
重启
> sudo /etc/init.d/redis-server restart 


# 5 防火墙
sudo ufw status

sudo ufw enable|disable

sudo ufw disable

# 5 复制文件
1. 建立/root/zero目录并复制install\zero目录下的内容到此目录
2. 在/root/zero下建立logs与datas目录
2. 权限都设置为777
3. 将install\lib目录下的文件复制到 /usr/local/lib目录下

# 6 运行ZeroCenter
进入Shell，输入 ：/root/zero/center/zero_center.out

# 7 启用守护进程
1. 复制守护进程
> Insall\supervisor目录下的所有文件到install\supervisor

2. 更新守护进程

> supervisorctl reread

> supervisorctl update

# 8 完成




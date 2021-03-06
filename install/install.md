# Ubuntu16.4 部署ZeroCenter

## 1 系统更新

sudo apt-get update

## 2 Net Core
> https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/install

wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https

sudo apt-get update

sudo apt-get install dotnet-sdk-2.1

## 3 安装 supervisor

sudo apt-get install supervisor

> 运行命令为：supervisord -c /etc/supervisor/supervisord.conf
### supervisorctl命令(备用）
supervisorctl status

supervisorctl stop tomcat

supervisorctl start tomcat

supervisorctl restart tomcat

supervisorctl reread

supervisorctl update


## 4 安装redis
> https://www.cnblogs.com/zongfa/p/7808807.html

1 安装 sudo apt-get install redis-server
2 重启 sudo /etc/init.d/redis-server restart 


## 5 复制文件
1. 建立/root/zero目录并复制install\zero目录下的内容到此目录
2. 在/root/zero下建立logs与datas目录
2. 权限都设置为777
3. 将install\lib目录下的文件复制到 /usr/local/lib目录下
> 有些情况，/usr/local/lib目录下运行会找不到对应的库，那么就将文件复制到/usr/lib目录下
## 6 运行ZeroCenter
> /root/zero/center/zero_center.out
查看是否运行正常，运行完成关闭（ctr+c)

### 如有外网无法访问，请配置防火墙或暂时关闭它
sudo ufw status

sudo ufw enable|disable

sudo ufw disable

## 7 启用守护进程
1. 复制守护进程
> Insall\supervisor目录下的所有文件到install\supervisor

2. 更新守护进程

 supervisorctl reread

 supervisorctl update


## 8 完成
1 在浏览器打开你的站点的 81端口，看三个项目是否正确运行
2 在浏览器打开你的站点的5000端口，看监控后台是否正确运行



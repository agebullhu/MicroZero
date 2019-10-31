####Ubuntu部署####
#1 FTP
> https://www.linuxidc.com/Linux/2017-04/142493.htm

sudo apt-get update
sudo apt-get install vsftpd
systemctl start vsftpd
systemctl enable vsftpd


# 2 Net Core
> https://www.microsoft.com/net/download/linux-package-manager/ubuntu16-04/sdk-2.0.3
wget -q packages-microsoft-prod.deb https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.0.3 

# 3 安装GCC

sudo apt-get  build-dep  gcc
sudo apt install cmake

# 4 安装GIT

sudo apt-get git

# 5 编译BOOST库
> Github下载的执行出错
> sudo apt-get install libboost-all-dev #直接安装
> https://blog.csdn.net/u011573853/article/details/52682256

wget https://dl.bintray.com/boostorg/release/1.67.0/source/boost_1_67_0.tar.gz 
tar zxf boost_1_67_0.tar.gz
cd boost-1.67.0
./bootstrap.sh –with-libraries=all –with-toolset=gcc 
./b2 toolset=gcc
./b2 install
ln -s /usr/local/include/boost /usr/include/boost


# 6 编译ZMQ
> https://zhuanlan.zhihu.com/p/27971993

wget https://github.com/agebullhu/libzmq/archive/v4.2.5.tar.gz
mkdir libzmq
tar zxf v4.3.2.tar.gz -C /root/libzmq
cd libzmq/libzmq-4.2.5/builds

cmake ..

sudo make -j4 install
# 7 安装UUID
sudo apt-get install uuid-dev

ldconfig

# 8 编译ACL
wget https://github.com/zhengshuxin/acl/archive/acl.3.3.1.rc1.tar.gz
tar zxf acl.3.3.1.rc1.tar.gz


# 9 安装redis
> https://www.cnblogs.com/zongfa/p/7808807.html

sudo apt-get install redis-server

注意：使用了16以外的库ID所以要修改Redis.Conf(/etc/redis/Redis.Conf) 
databases 16 => 32
sudo /etc/init.d/redis-server restart #重启

ZRANGEBYSCORE plan:time:set +inf -inf  WITHSCORES #命令

# 10 安装 supervisor

sudo apt-get install supervisor
修改配置为HTTP
supervisord -c /etc/supervisor/supervisord.conf
> 失败请先杀死进程
## 其它命令
supervisorctl status
supervisorctl stop tomcat
supervisorctl start tomcat
supervisorctl restart tomcat
supervisorctl reread
supervisorctl update

关闭防火墙的方法为：
 查看防火墙状态

sudo ufw status
​

开启/关闭防火墙 (默认设置是’disable’)

sudo ufw enable|disable
sudo ufw disable
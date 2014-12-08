iveely
==========


Iveely Search Engine 是由纯Java实现，依靠Iveely.Framework和Iveely.Computing实现的分布式搜索（知识）引擎。<br/>
当前最新版本：0.7.0 。<br/>
###### 主要包含以下功能模块：<br/>
> A. 文本检索(Web & Mobile)。<br/>
> B. 图片检索(Web only)。<br/>
> C. 百科检索(Web only)。<br/>
> D. 问答检索(Web & Mobile)。<br/>

###### 主要包含技术模块：<br/>
> A. Iveely.Framework：基础模块。<br/>
> B. Iveely.Computing：程序运行平台。<br/>
> C. Iveely.Search:    搜索技术模块。<br/>
> D. Iveely.Search.UI: 网页搜索模块。<br/>
> E. Iveely.Search.Mobile: 移动客户端搜索模块。<br/>

#### 1.示例截图  
> 1.1 文本检索
  ![image](http://www.iveely.com/0.7.0/wenben1.png) <br/>
> 1.2 图片检索
  ![image](http://www.iveely.com/0.7.0/image1.png) <br/>
> 1.3 百科检索
  ![image](http://www.iveely.com/0.7.0/baike1.png) <br/>
> 1.4 问答检索
  ![image](http://www.iveely.com/0.7.0/wenda1.png) <br/>
  
  ![image](http://www.iveely.com/0.7.0/wenda2png.png) <br/>

#### 2. 编译部署
> 2.1 编译工具 <br/>
  编译工具可以采用任何编译器，但是上传的文件中是按照Netbeans开发上传，且JDK要求为1.8及其以上。

> 2.2 部署（含多机器和单机部署-windows，linux类似）
  您可以在Iveely.Search.Release中下载对应版本的示例程序（Iveely Computing 0.7.0 Release demo.rar），下周解压之后：
  ![image](http://www.iveely.com/0.7.0/001.png) <br/>
  上图中各个文件夹的含义如下：<br/>
  Console：   用户控制台。<br/>
  Master：    Iveely的Master结点。<br/>
  Slave-7001：该机器上的7001端口服务结点。<br/>
  Slave-7002：该机器上的7002端口服务结点。<br/>
  这是一个单机运行的情况，如果是多机器情况，则Slave-7001和Slave-7002可拷贝到更多的机器中。其余操作方法均如下：<br/>
  第一步，启动Master。<br/>
          运行参数：master 127.0.0.1 9010 <br/>
          “master”表示是master结点，127.0.0.1表示当前Master地址，9010表示Master服务端口。windows下可以双击“RunMaster.bat”.<br/>
          效果如下：<br/>
          ![image](http://www.iveely.com/0.7.0/002.png) <br/>
  第二步，启动Slave。<br/>
          运行参数：slave 127.0.0.1 9010 7001 <br/>
          “slave”表示是slave结点，127.0.0.1表示Master地址，9010是Master服务端口，7001是slave服务短裤。<br/>
          效果如下：<br/>
          ![image](http://www.iveely.com/0.7.0/003.png) <br/>
  第三步，启动Console。<br/>
          当当前机器或更多机器的master和slave结点均已经启动的情况下，运行Console，运行参数:console 127.0.0.1 9010 <br/>
          "console"表示当前是用户控制台，127.0.0.1是master结点地址，9010是master端口。<br/>
  

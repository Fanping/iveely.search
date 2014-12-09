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
  
> 1.5 移动客户端 <br/>
  ![image](http://www.iveely.com/0.7.0/mobile.png) <br/>

#### 2. 编译部署  <br/>
##### 2.1 编译工具 <br/>
> 编译工具可以采用任何编译器，但是上传的文件中是按照Netbeans开发上传，且JDK要求为1.8及其以上。<br/>

##### 2.2 部署（含多机器和单机部署-windows，linux类似） <br/>
> 您可以在 https://onedrive.live.com/?cid=46338B55F029D384&id=46338B55F029D384%21140 中下载对应版本的示例程序（Iveely Computing 0.7.0 Release demo.rar），下周解压之后：<br/>
> ![image](http://www.iveely.com/0.7.0/001.png) <br/>
> 上图中各个文件夹的含义如下：<br/>
> Console：   用户控制台。<br/>
> Master：    Iveely的Master结点。<br/>
> Slave-7001：该机器上的7001端口服务结点。<br/>
> Slave-7002：该机器上的7002端口服务结点。<br/>
> 这是一个单机运行的情况，如果是多机器情况，则Slave-7001和Slave-7002可拷贝到更多的机器中。其余操作方法均如下：<br/>
> 第一步，启动Master。<br/>
>        运行参数：master 127.0.0.1 9010 <br/>
>         “master”表示是master结点，127.0.0.1表示当前Master地址，9010表示Master服务端口。windows下可以双击“RunMaster.bat”.<br />
>         效果如下：<br/>
>         ![image](http://www.iveely.com/0.7.0/002.png) <br/>
> 第二步，启动Slave。<br/>
>         运行参数：slave 127.0.0.1 9010 7001 <br/>
>         “slave”表示是slave结点，127.0.0.1表示Master地址，9010是Master服务端口，7001是slave服务短裤。<br/>
>         效果如下：<br/>
>         ![image](http://www.iveely.com/0.7.0/003.png) <br/>
> 第三步，启动Console。<br/>
>         当当前机器或更多机器的master和slave结点均已经启动的情况下，运行Console，运行参数:console 127.0.0.1 9010 <br/>
>         "console"表示当前是用户控制台，127.0.0.1是master结点地址，9010是master端口。<br/>
  
##### 2.3 有效的Console指令 <br/>
> 既然有用户控制台，那么一定有有效的指令。<br/>
###### 2.3.1 提交app到Iveely.computing。<br/>
> format:upload [your app folder]<br/>
> 示例：<br/>
> ![image](http://www.iveely.com/0.7.0/004.png) <br/>

###### 2.3.2 在Iveely.Computing上执行App。<br/>
> format:run [your app name] [count]<br/>
> 第三个参数 count 表示在几个机器结点上执行 示例：<br/>
> ![image](http://www.iveely.com/0.7.0/005.png) <br/>
> 运行结果，在每一个slave上都会打印： <br/>
> ![image](http://www.iveely.com/0.7.0/006.png) <br/>

###### 2.3.3 在Iveely.Computing上其它指令。<br/>
> format:list ,表示查看所有app。<br/>
> format:slaves ,表示查看所有Iveely.Computing上的运行机器结点。<br/>
> 当然这是常用的指令，还有更多指令，期待您在代码中发现。

##### 2.4 运行示例中的搜索引擎 <br/>
> 当Iveely.Computing的master和salve均启动起来之后，运行：<br/>
> 第一步：run Iveely.Search.DataService <br/>
> 第二步: run Iveely.Search.UIService 1 <br/>
> 对于第二步，一定要注意后面有一个1，只需要在一个结点上运行。
> 第三步：修改 https://github.com/Fanping/iveely/blob/master/Iveely.Search.WebUI/JS/query.js 中最顶层的地址，端口不变，服务器地址改为UIService运行结点的IP，怎么看？用list命令即可看到。<br/>
> 如果从头运行，不需要示例中的准备数据，请删除文件夹下的Service_Text_Data和Service_Image_Data，然后部署好Iveely.Computing，运行命令：run Iveely.Search.Backstage.其次再运行上面的第一步、第二步、第三步。这点很重要。

#### 3.怎么写基于Iveely.Computing的app？<br/>
> 难以想象的简单：<br/>
> 写一个类,添加一个函数：  public String invoke(String arg) { ... } 并打包成jar。<br/>
> 程序被Iveely.Computing执行的时候会调此方法，但是如何让Iveely.Computing识别到它？<br/>
> 在jar同在目录下，新建一个文件app.run这是程序的配置文件，配置信息大致如下：<br/>
> " <br/>
> jar:HelloWorld.jar <br/>
> class:helloworld.MyHelloWorld <br/>
> params:NULL <br/>
> cycle:daily <br/>
> " <br/>
> jar表示指定运行的jar，class是invoke方法所在的class，params是你想给他指定的参数，会是invoke的输入参数，cycle是运行周期，这里标识是每天运行一次。还有hourly,always,weekly。<br/>
> 至此，您已经了解了最基本的信息，如果有疑问，欢迎您联系我liufanping@iveely.com,或是发现bug，请您直接在issue中描述，谢谢！



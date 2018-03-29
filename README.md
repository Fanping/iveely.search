## iveely search engine [![Build Status](https://travis-ci.org/Fanping/iveely.search.svg?branch=master)](https://travis-ci.org/Fanping/iveely.search)

Iveely is an abbreviation from 'I void everything, except loving you'.
Iveely search engine tries to directly hit the user's search for answers， which is implemented with pure java.

Contact me if you have any questions, [liufanping@iveely.com](mailto:liufanping@iveely.com).
Auto build [here](https://travis-ci.org/Fanping/iveely.search).

### (1) Quick Start in five minutes.
#### 1. Download and build.
Download iveely.crawler & iveely.search.api,and build to executable jar,you can use maven build them quickly.
#### 2. Config and run.
Copy "example conf/for crawler/conf" to the same parent directory with 'iveely.crawler.jar'.
Use 'java -jar iveely.search.api.jar' & 'java -jar iveely.crawler.jar' to execute them,please run iveely.search.api.jar first.



		1. First run: java -jar iveely.search.api.jar
		2. Next  run: java -jar iveely.crawler.jar
		



#### 3. Let's search.
Query with keyword 'java', you can open browser with [http://127.0.0.1:8080/api/0.9.1/query?queryField=title&respFields=title%2Ccontent%2Curl&keywords=java&totalHits=10](http://127.0.0.1:8080/api/0.9.1/query?queryField=title&respFields=title%2Ccontent%2Curl&keywords=java&totalHits=10) to get the result.<br/>
If you get the response json,congratulations, you've successfully run.<br/>
Moreover API information were described using Swagger-UI. So you can visit [http://127.0.0.1:8080/swagger-ui.html](http://127.0.0.1:8080/swagger-ui.html) to get more api.
### (2) Maven support
Iveely also was submitted to the maven central repositry. visit [iveely@maven](http://search.maven.org/#search%7Cga%7C1%7Civeely) to get more.
### (3) How to make search engine more smarter?
With only document search is not the goal,to build more intelligent search engine is very important,so we have added a project named 'iveely.brain'.<br/>
iveely.brain has two mode, local debug & Remote network calls.
To run iveely.brain,you should do as follow:
#### 1. Download and build.
Download iveely.brain and use maven to build,you also can run code by main class Progam.java. <br/>
Local operation does not require any arguments,but you need copy folder 'example conf/for brain/ai' to the same parent directory with 'iveely.brain.jar'. <br/>
#### 2. Test is successful start.
When run local mode,you can enter a question on console like 'Which city is the capital of the United States?' <br/>
If console write 'Washington.',congratulations, you've successfully run.<br/>



		Q:Which city is the capital of the United States?
		A:Washington.
		


For more information see [Distributed Artificial Intelligence Markup Language](http://www.cnblogs.com/liufanping/p/5189678.html).
#### 3. Configuration for remote calls.
Modify file 'ai/property/branches.xml', configure the port number and offer to provide network services, so that the external system can access the service, which is important for distributed search engine.<br/>
### (4) What's is the iveely computing used for,and how to use it?
In 2015, my friend and I began to research in the field of artificial intelligence, we need a lightweight computing framework to help us build data model in fast. Rapid deployment, rapid results, simple to fit is our original intention at that time. Even hope that any program can be distributed, such as crawler program of search engine. In the process of last year, Iveely Computing has given us a lot of help, so we decided to open source to more developers.<br/>
It is a very lightweight distributed real-time computing framework like storm,it has four very important components:<br/>
#### 1. IInput(The basic data input interface)
It is the input source data, can be gained by reading the file system data source, also can be achieved by other ways. It is also the place where the whole cluster program execution started
#### 2. IOutput(The basic data output interface)
IOutput data sources can be from IOutput and IInput, but the output must IOutput or no output, cannot be directly output to a file.It is the middle of the data processing unit.
#### 3. IInputReader
IInputReader is an input with the function of file read, any IReader implementation can be used in IInputReader, including Windows file system,Unix file system, Hadoop file system, etc.
#### 4. IOutputWriter
It is a subclass of IOuput, used for writing data to a local file system or other file system.
<br/>
Example can be found [here](https://github.com/Fanping/iveely.search/blob/master/iveely.computing/src/com/iveely/computing/example/WordCount.java).

### (5) How to use iveely database?
iveely database is a mini data storage system,it's has two mode, as follow:<br/>
#### 1. Local mode.
Local mode is easy to use, example code =>



		final String houseName = "example_house";
		final String tableName = "example_table";
		Warehouse warehouse = LocalStore.getWarehouse(houseName);
		warehouse.createTable(tableName);
		int id = warehouse.insert(tableName,new Object[]{"1", "this is example"});
        Object[] data = warehouse.selectById(tableName,id);
        System.out.print(data);
        warehouse.dropTable(tableName);
		

#### 2. Remote mode.
Use remote mode you can build a database server,and every application can access the database. example code =>



		final String houseName = "example_house";
        final String tableName = "example_table";
        DbConnector connector = new DbConnector(houseName, "localhost", 4321);
        final int id = connector.insert(tableName, new Object[]{"1", "this is example"});
        Object[] data = connector.selectOne(tableName, id);
        System.out.print(data);
        connector.dropTable(tableName);
		

 
### (6) Can I known more about iveely search engine?
For a better understanding of the next generation of modern search engines, I wrote a book named "Big Data Search Engine Principle Analysis and Implementation", you can get this book on  [amazon.com](https://www.amazon.cn/%E5%A4%A7%E6%95%B0%E6%8D%AE%E6%90%9C%E7%B4%A2%E5%BC%95%E6%93%8E%E5%8E%9F%E7%90%86%E5%88%86%E6%9E%90%E5%8F%8A%E7%BC%96%E7%A8%8B%E5%AE%9E%E7%8E%B0-%E5%88%98%E5%87%A1%E5%B9%B3/dp/B01HYCX288/ref=sr_1_1?ie=UTF8&qid=1468111657&sr=8-1&keywords=%E5%88%98%E5%87%A1%E5%B9%B3) or [jd.com](http://item.jd.com/11981242.html).
### (7) Why you did iveely search engine?
From 2009, I began to think search engine not just a simple search tool. I offer keywords to search engine, the search engine returns some documents, I think this is not smart enough. <br/>
I expect I'll give the search engines a question, it gives me an answer,this is my original goal.<br/>

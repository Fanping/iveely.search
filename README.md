## iveely search engine
Welcome here,Iveely search engine is a pure java realize search engine, try to directly hit the user's search for answers.
Iveely is an abbreviation from 'I void everything,except love you'.<br/>
<br/>
Contact me if you have any questions, [liufanping@iveely.com](mailto:liufanping@iveely.com).
### (1) Quick Start in five minutes.
#### 1. Download and build.
Download iveely.crawler & iveely.search.api,and build to executable jar,you can use maven build them quickly.
#### 2. Config and run.
Copy "example conf/for crawler/conf" to the same parent directory with 'iveely.crawler.jar'.
Use 'java -jar iveely.search.api.jar' & 'java -jar iveely.crawler.jar' to execute them,please run iveely.search.api.jar first.
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
For more information see [Distributed Artificial Intelligence Markup Language](http://www.cnblogs.com/liufanping/p/5189678.html).
#### 3. Configuration for remote calls.
Modify file 'ai/property/branches.xml', configure the port number and offer to provide network services, so that the external system can access the service, which is important for distributed search engine.<br/>
### (4) What's is the iveely computing used for,and how to use it?
In 2015, my friend and I began to research in the field of artificial intelligence, we need a lightweight computing framework to help us build data model in fast. Rapid deployment, rapid results, simple to fit is our original intention at that time. Even hope that any program can be distributed, such as crawler program of search engine. In the process of last year, Iveely Computing has given us a lot of help, so we decided to open source to more developers.<br/>
### (5) How to use iveely database?
iveely database is a mini data storage system.<br/>
### (6) Can I known more about iveely search engine?
For a better understanding of the next generation of modern search engines, I wrote a book named "Big Data Search Engine Principle Analysis and Implementation", you can get this book on  [amazon.com](https://www.amazon.cn/%E5%A4%A7%E6%95%B0%E6%8D%AE%E6%90%9C%E7%B4%A2%E5%BC%95%E6%93%8E%E5%8E%9F%E7%90%86%E5%88%86%E6%9E%90%E5%8F%8A%E7%BC%96%E7%A8%8B%E5%AE%9E%E7%8E%B0-%E5%88%98%E5%87%A1%E5%B9%B3/dp/B01HYCX288/ref=sr_1_1?ie=UTF8&qid=1468111657&sr=8-1&keywords=%E5%88%98%E5%87%A1%E5%B9%B3) or [jd.com](http://item.jd.com/11981242.html).
### (7) Why you did iveely search engine?
From 2009, I began to think search engine not just a simple search tool. I offer keywords to search engine, the search engine returns some documents, I think this is not smart enough. <br/>
I expect I'll give the search engines a question, it gives me an answer,this is my original goal.<br/>

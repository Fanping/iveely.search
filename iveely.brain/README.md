# iveely.brain
Let the program with logical reasoning ability of intelligent conversational services.

### (1) Local Mode.
Local mode means you can use console to test your AIML.You can execute the jar
without any arguments,that would be local mode.
### (2) Remote Mode.
Remote mode means we can use TCP to visit the service as product enviroment
.the default service port is 8001,also you can modify it in branches.xml.You
can execute the jar with argument "-remote".
### (3) Example Case.
#### 1.Direct exact match mode.
```xml
<category>
        <pattern>Hello</pattern>
        <template>
            Hello!
        </template>
</category>
```
When you say 'Hello',it would be response with 'Hello!'.
#### 2.Match with *.
```xml
<category>
        <pattern>My name is *</pattern>
        <template>
            Hello,<star index="1" />.
        </template>
</category>
```
When you say 'Hello JIM',it would be response with 'Hello,JIM'.
#### 3.Random answers.
```xml
<category>
        <pattern>How are you?</pattern>
        <template>
            <random>
                <li>Fine.</li>
                <li>Fine,thanks!</li>
                <li>Fine,and you?</li>
            </random>
        </template>
</category>
```
When you say 'How are you?',it would be response by random.
#### 4.Recursive answer.
```xml
<category>
        <pattern>How are you? JIM</pattern>
        <template>
            <srai>
               How are you?
            </srai>
        </template>
</category>
```
When you say 'How are you?JIM',the answer would be same with question 'How
are you?'
#### 5.Pattern matching with constraints.
```xml
<category>
        <pattern that="Hello">Hi</pattern>
        <template>
            <random>
                <li>
                    You just said 'hello'.
                </li>
                <li>
                    Hi!
                </li>
            </random>
        </template>
</category>
```
When you say 'Hello',and say 'Hi',it would be response with random.


# IPManager
This is an ip manager about  network interface  that run on the windows

The project worked on windows. if you are a windows administrator, it will helpful.

first it read network interface config whether dhcp or static, and then you can modify the config to change network interface config what are you choosed.

you can study read/write network interface config when network interface is not connect network (what it read/write registry) from this project.

这个项目运行在windows上，用于更改IP，工作原理大概是读取网卡配置信息，然后可以进行更改。

C#没有API可以直接读写断网状态的网卡信息，读取的方法可以是通过注册表或者ipconfig(这里采用注册表),写则是注册表。

本项目很好的运行在联网或者断网的windows上，目前测试下，没有不良反应。

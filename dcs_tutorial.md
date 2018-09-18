DCS使用手册
================

# DCS 简介
- Drone Control System是我在前人的基础上整合开发的一套无人机控制脚本。主要参考了Easy lidar homing script. Vector Thruster 2. Player2k's Vector Thruster. Whip's Rotor Thruster Manager. 等
- 该脚本主要用于单母舰控制多架无人机同时进行起降, 攻击等动作.

# 概要
- 本教程会分为以下章节, 循序渐近. 让用户逐步掌握该脚本的使用方法.
  - 基本使用
  - 起降
  - 矢量喷口
  - 与FCS的配合
  -  特性说明

# 基本使用
- 在本章中, 我们会学习如何做一个最简单指挥模型, 体会一下基础的无人机制作.

## 母舰设置
- 首先, 准备一艘母舰, 除了必备的飞行功能外, 还需要4个组件
- 1, 一个飞行座椅, 放在中线上即可.
- 2, 一个天线. 位置随意.
- 3, 一个编程块. 位置随意.
- 4, 一块LCD面板. 放在座椅能看到的位置

之后, 打开编程块的菜鸟, 找到Edit按钮, 点开后, 再点击Browse Script按钮, 找到之前订阅的Kaien's Drone Control System脚本, 点击后选择OK. 加载该脚本到编程块中.

之后, 注意显示出来的脚本正文, 在前面找到 custom data sample for mother ship 这一句话, 将其下面一段:
motherCode=1st
到
macAV=0.005
复制出来.

点击OK, 关闭脚本编辑界面.

点击Custom Data按钮, 打开自定义数据输入框. 将刚才Copy出来的一段, 粘贴到其中.

在Custom Data中, 找到 stAName=Antenna fcs 这一句, 将=号后面的 Antenna fcs 复制出来. 点击OK关闭该界面.

在组件列表中, 找到天线. 将其重命名为 Antenna fcs . 注意不要有多余字符.

回到编程块的Custom Data编辑界面, 找到 CockpitNameTag=Flight Seat 3 这一行, 将=号后面的 Flight Seat 3 复制出来. 关闭该界面, 在组件列表中找到刚放下的飞行座椅, 将其重命名为 Flight Seat 3.

后续我们将上述过程, 简述为, 根据编程块的Custom Data, 重命名XX组件. 下面, 根据编程块的Custom Data, DCSLCDNameTag=DCS_LCD, 重命名刚放下的LCD面板.

在组件列表中找到LCD面板, 将Show text on screen置为On.

在组件列表中找到编程块, 点击Run启动.

如果上述步骤一切正常. 你将看到编程块的信息输出区域, 打印了如下字样.
[ERROR]: No remote or control seat with name tag 'Reference' was found.

同时, LCD面板显示蓝色的几排字母, 类似如下:
  Auto:      
 M: 1st 
 Location 
 Relative Position: (l u f h t)
 Wing: 100 # 0 # 0 # f # l
 Prepare: 50 # 50 # 50 # f # u
 Dock: 0 # -100 # -100 # f # u

那么恭喜你, 母舰设置一切正常.

FAQ:
1, 编程块关于Reference的报错. 这个是后续安装矢量喷口时需要的部分, 我们可以暂时不用管这个报错, 不影响基本使用.
2, 编程块报 XXX 未找到. Catch Exception: ... 这表示有部分组件的名字没有和编程块的Custom Data中设置的名字对应起来. 用户可以修改上述组件的名字, 但注意Custom Data中的名字需要同步修改.
3, 组件放在子网格上是否可以? 飞行座椅和编程块必须在主网格上, LCD面板和天线可以在子网格上.
4, 飞行座椅是否可以用别的东西替代? 可以用两种驾驶仓, 控制台, 或者远程控制块代替.
5, LCD面板是否可以用上下角面板, 宽屏 或者Text panel代替? 可以, 所有能显示文字的面板都可以用.
6, 可编程块的信息区域没有输出? 初次设置后需要点击RUN
7, LCD面板上只显示online字样? 需要手工将LCD面板设置为Show text on the screen 为 On


# 起降

# 矢量喷口

# 与FCS的配合

# 特性说明
## 大气内姿态
## 加减速
## 同网络问题
## 避撞
## 攻击模式的路径
## 环绕模式
## 靶机模式
## 跟人模式
## 打印模式
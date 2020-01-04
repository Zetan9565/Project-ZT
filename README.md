# ProjectZT
业余时间个人启动的第一个未定名的单机独立游戏，主类型为2D开放世界ARPG，副类型为经营、养成。一直断断续续的做着，有时候一天敲十个钟代码，有时候一天敲十分钟，有时候一周内天天敲代码，有时候一个月才打开两三次项目，所以截至2019年12月15日，只开发了任务框架、对话框架、部分道具框架、建造系统、制作系统、地图系统、时间系统，底层类多数利用ScriptableObject实现，还自定义了友好的Inspector面板。道具框架暂时止步，剧情框架做了一半不到就暂停了，考虑到Timeline的使用，正在考虑重建。种田系统开发了一半，这个系统没有美术资源真的进行不下去。
# 暂定游戏大纲
## 1  道具系统 ■
### 1.1 装备（设计耐久度）
#### 1.1.1 武器（工程量极大Orz)
武器特性：按武器类型分招式，不同武器模式下，某些招式共用，但造成的伤害类型不一样
##### ①单手短兵
    剑模式：主打穿刺伤害
    刀模式：主打切割伤害
##### ②双手短兵
    双剑模式：主打穿刺伤害
    双鞭模式：主打钝击伤害
##### ③长兵
    棍模式：主打钝击伤害
    长枪模式：主打穿刺伤害
    关刀模式：主打切割伤害
##### ④弓：主打伤害类型视箭矢类型而定
    远距狙击模式
    近距连发模式
    近距散射模式
##### ⑤视开发能力精力而定
#### 1.1.2 防具
防具特性：分四个部位，每个部位有两个部件可以强化，不同部件对不同伤害类型的防御力不同
##### 头部
    头盔
    面罩
##### 躯干
    盔甲
    衣服
##### 手部
    护腕
    手套
##### 脚部
    护膝
    鞋子
#### 1.1.3 饰品
   戒指
   项链
#### 1.1.4 工具
   镰刀
   锄头
   钓竿
   斧头
   铲子
   等等
### 1.2 图纸 ■
  制造图纸 √
  技能书
### 1.3 药品 ■
  血药 √
  蓝药
  耐力药
### 1.4 料理
### 1.5 材料
### 1.6 等等
## 2  剧情及任务系统
### 2.1 剧情 ■
 <br>对话系统 √
 即时演算
### 2.2 任务目标类型 ■
 对话 √
 收集、提交道具 √
 按数杀怪 √
 地点到达 √
 触发器 √
 等等
## 3  农牧系统 ■
### 3.1 种子
### 3.2 开垦
  单个格子？
  多个格子的田地？ √
### 3.3 种菜
### 3.4 养殖
### 3.5 钓鱼
## 4 建造系统 ■
### 4.1 建造系统特性
整体建造，不支持单部件组合（臣妾做不到啊~）
### 4.2 建筑物类型
#### 4.2.1 住房
#### 4.2.2 仓库 √
#### 4.2.3 畜舍
#### 4.2.4 禽舍
#### 4.2.5 制作设施
   锻冶炉（熔炼）
   臼子（捣碎、研磨）
   打铁台（打造）
   手工台
   晾晒台
   等等
#### 4.2.6 等等
## 5 工人系统
### 5.1 工人特性
  需携带材料至工作地点，优先使用工作地点最近仓库中的材料。
### 5.2 工人功能
  雇佣系统
  开垦农田
  建造设施
## 6 坐骑系统
  坐骑技能
  坐骑培养
  乘骑作战
## 7 纪年系统 ■
### 7.1 1年 = 360天 = 12月，1月 = 30天，现实1秒 = 游戏1分 √
### 7.2 纪念日系统
   节日
   生日
### 7.3 日出而作，日落而息
## 8 天气系统
## 9 地图系统 √
### 9.1 小地图模式 √
#### 9.1.1 旋转 √
#### 9.1.2 缩放 √
### 9.2 大地图模式 √
#### 9.2.1 拖拽 √
#### 9.2.2 复位（重定位） √
#### 9.2.3 缩放 √
## 10 角色属性
### 10.1 防御属性
体力值（HP)
抗热能力（过热、过热都会使耐力消耗速度激增）
抗寒能力
切割防御力
穿刺防御力
钝击防御力
闪避力
移动速度
### 10.2 攻击属性
#### 10.2.1气力值
所有战斗动作（攻击、格挡、冲刺、闪躲等）都会消耗，无战斗动作迅速恢复，类似怪物猎人黄条但不同等
#### 10.2.2 耐力值
影响气力上限，只能通过休息或吃喝恢复。值过低时角色进入疲劳，无法战斗，移速降低，所谓“饿得没力气了”“累得没力气了”；值耗尽时，角色开始随时间损失体力值
#### 10.2.3 攻击伤害值
切割伤害
穿刺伤害
钝击伤害
#### 10.2.4 命中力
#### 10.2.5 其它
暴击力
攻击速度
移动速度
### 10.3 附加属性
眩晕抗性
击倒抗性
浮空抗性
击退抗性
硬直抗性

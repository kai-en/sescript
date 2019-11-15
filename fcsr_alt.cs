/*
 ======= [ MEA ] 全自动转子基座炮台控制程序 FCS-R v1.3 by MEA群主 QQ群530683714 =======
 【介绍】
	这是一套几乎全自动的转子基座炮台的控制程序。不需要陀螺仪，不需要复杂的安装。
	特点只有一个 ———————— "准"
	它的转子控制采用PID控制修正，几乎消除了常规控制的延迟和震荡，效率极高
	它的预瞄算法采用了FCS的弹道预瞄算法，指哪打哪。
 【说明】
	1、转子基座炮台是一种使用转子固定在飞船上的炮台，活动部分通常安装固定加特林机枪。威力比传统自动武器大大增加。基础结构是一个水平活动的X转子、一个垂直活动的Y转子，然后在最终活动部安装一组固定加特林机枪
	2、这个程序可以控制同一网格（包括垮转子、连接器、活塞）里的所有转子基座炮台。所以并不需要每个炮台安装一个程序。
	3、程序索敌采用3种方式：
		a.指定一个FCS编程块（FCS是我组推出的知名火控程序，采用摄像头索敌，http://www.spacemea.com//forum.php?mod=viewthread&tid=2），程序会自动读取FCS编程块锁定的目标。
		b.程序会自动获取网格内所有的自动武器（包括自动火箭弹炮塔、自动加特林机枪、室内机枪），当其中任何武器发现目标并开始自动攻击，程序就会指令所有可以攻击的转子基座炮台一起攻击这个目标。多目标情况下，程序会自动分配
	4、程序在弹道预瞄算法上优于游戏原版自动武器的算法，在双方高速无规律机动的情况下命中率比原版武器提升50%以上。但这个算法仅限于FCS锁定和自主锁定的情况下才生效。
	5、程序在转子旋转控制上采用PID控制，对不同类型的炮塔指定不同的PID参数可以最大化转子的瞄准效率
	6、程序允许在转子基座炮塔上安装若干个探测器，这个（这些）探测器用来防止这个转子基座炮台命中自己。请手动设定这些探测器的范围，调整到刚好覆盖该炮塔的火力前方最大距离，并且设为可以探测自己阵营、不能探测敌方阵营。
	当探测器被激活（探测到设定的物体），程序就会下令这个炮台立刻停止攻击。
	
 【安装】
	1、如果你做好了一套转子基座炮塔，请把上面的所有相关块放进一个编组里。并且挑选一个方向块（这个方向块用来给程序识别自己的指向，可以是任何K表中的方块，注意前后左右上下一定要正确），修改这个方块的名字，
	让它的名字包含程序顶部的AimBlockKey设定的关键字。
	2、相关方块包括：
		======= 重要 ======
		a.一个瞄准块，可以是任意K表中可以看到的方块，必须设定它的名字，让其中[包含]AimBlockKey设定的关键字。【必须】
		b.若干转子，程序会自动识别转子安装方向。支持多个转子共同控制一个轴（对向安装法）。但必须有x轴转子和y轴转子至少各一个，否则无法正常瞄准。【必须】
		c.若干武器，理论上所有武器类型都可以，建议使用固定加特林机枪或固定火箭弹发射器。【必须】
		d.若干探测器，用于攻击保护。【可选】
		e.若干摄像头，用于自主索敌。【可选】
	3、这里建议使用远程控制块（或驾驶舱）作为瞄准块。使用远程控制块作为瞄准块时，当你控制这个远程控制块，程序会识别到你的控制，并把你的鼠标操作传给转子。让你可以向控制飞船一样直接用鼠标控制这个炮台的指向。
	4、多套炮台怎么办？把各个炮台属于自己的方块都放在自己的编组里。即：A炮台的瞄准块、武器、转子等方块都放进编组A中，B炮台的瞄准块、武器、转子等方块都放进编组B中。不允许一个方块同时存在多个炮台的编组里。
	5、飞船上还有其他方块编组怎么办？没关系，程序会读取所有编组，但只有编组中存在一个名字包含了AimBlockKey的方块时，这个编组才会被认为是转子基座炮台。程序不会扰乱其他编组里的任何方块。
	6、特别注意，炮塔在待命模式【指向前方】的时候，会在待命时指向这个炮塔X轴转子的方块前方。所以安装炮台的时候请注意X转子的方向。同时在待命模式【指向舰外】时，炮台会指向编程块与该炮台的连线方向（不考虑相对高度），所以尽量把编程块放在舰船的中心位置
	
  【指令】
	输入指令时注意中英文和大小写
	1、你可以把代码调试参数中的DebugMode设为true来调试，或者使用参数[Debug]运行本编程块。调试状态下会在编程块右下角显示出各个编组以及它不能被作为炮塔的原因。
	2、OnOff —— 总开关，关闭后武器不会主动瞄准和开火。但不影响获取目标和手动控制
	3、FireMode  ——  开关精确瞄准后开火，精确瞄准是指在指向目标的过程中，只有指向几乎准确了才会开火。这个准确与否的判定可以在下方【瞄准控制系统设置】中修改
	4、Attention ——  切换待命瞄准模式，可选：无、瞄准自己前方、随机警戒
	
  【更新说明】
	v0.9 优化了手动控制时的细节
	v1.0 新增了瞄准后才开火的判定，节省子弹。
		新增了只攻击上半平面的设定（测试功能）
		新增了主LCD动态，方便查看是否程序在运行
	v1.1 新增了待命时的瞄准模式切换
		新增了攻击功能总开关
	v1.3 新增了转子反转标签
		新增了自定义开火定时块
*/

// =============== 基础设置 =============
//再次强调，每个转子炮台的所有零件必须在同一个编组中，并且这个编组中必须有一个名字包含 AimBlockKey关键字的方块用于瞄准。多个转子炮台的方块应该各自放在自己的编组，不允许冲突。
const string FCSComputerNameTag = "Programmable block fcs"; //FCS编程块名字，用于读取其中目标，非必须。
const string AimBlockKey = "FCS#R"; //瞄准块名字关键字，可以写在名字中，只要包含这个关键字即可。
const string LCDNameTag = "FCSR_LCD"; //LCD名字，用来显示各个目标和炮台信息，非必须，也可以是多个。
const string RotorNagtiveTag = "[-]"; //转子负转标签。当转子名字里完全包含这个标签的时候，它会被强制认为是反向控制的转子。用来解决某些特殊结构的转子问题

string CockpitNameTag = "Reference";

// ============== 战斗设置 ==============
const double AttackDistance = 910; //默认武器的自动开火距离
static bool OnlyAttackUpPlane = false; //自动选择目标时是否只选择转子基座炮塔的上半球面里的目标（当目标低于该炮塔时不选择这个目标）

// ============== 进阶开火机制 ============
//程序会自动检测原版武器并在有目标时触发武器开火。同时你也可以自己使用定时块来触发开火。你需要在需要触发自定义开火的转子基座炮台上加入一个或多个定时块，必须放进这个炮台的编组。
//同时，你需要让这个开火定时块的名字里完全包含下方的FireTimerNameTag设置的标签，并且在这个定时块的自定义参数里写上触发开火的距离（纯数字），如果不设置距离，将采用默认武器的开火距离
//程序会在目标进入开火距离内的时候，以每秒60次的速度对这个定时块执行“立即触发”动作。
const string FireTimerNameTag = "[Fire_Timer]";  //自动开火定时块名字标签，定时块名字需要完全包含这个标签。

// ================ 武器子弹参数设置 =============
const double BulletInitialSpeed = 400; //子弹初速度
const double BulletAcceleration = 0; //子弹加速度
const double BulletMaxSpeed = 400; //子弹最大速度
const double ShootDistance = 910; //开火距离
const double BulletInitialSpeed2 = 170; //子弹初速度
const double BulletAcceleration2 = 10; //子弹加速度
const double BulletMaxSpeed2 = 190; //子弹最大速度
const double ShootDistance2 = 910; //开火距离

// ============== 其他设置 ==============
static int AttentionRandomTime = 180; //随机警戒模式的切换间隔，60是1秒，120是2秒，这个数必须是整数

// ================= 瞄准控制系统设置 ===============
// PID控制
const double RotorMaxSpeed = 30; //自动状态下转子最大角速度，单位 圈/分钟。标准值30
const double AimRatio = 5; //瞄准精度，单位：度。用来判断炮台是否瞄准，以便其他动作判断。不影响瞄准的效率。当瞄准块的正前方向量与瞄准块和目标的连线向量夹角小于这个值时，整个系统判定瞄准了目标。
const double Aim_PID_P = 0.9; //比例系数：可以理解为整个PID控制的总力度，建议范围0到1.2，1是完全出力。
const double Aim_PID_I = 8; //积分系数：增加这个系数会让静态误差增加（即高速环绕误差），但会减少瞄准的震荡。反之同理
const double Aim_PID_D = 1; //微分系数：增加这个系数会减少瞄准的震荡幅度，但会加剧在小角度偏差时的震荡幅度。反之同理
const int Aim_PID_T = 5; //PID 采样周期（单位：帧），周期越小效果越好，但太小的周期会让积分系数难以发挥效果
// 手动控制
const double PlayerAimRatio = 1; // 玩家手动控制的瞄准灵敏度，只有当瞄准块是远程控制块或驾驶舱才存在这个功能。

// =============== 代码调试参数 ====================
const int ProgramUpdateRatio = 60; //程序每秒循环的次数，可在GetBlocks()函数中修改程序循环频率，可以降帧使用。修改后同步修改这个参数即可
const string ShootActionName = "ShootOnce"; //武器的开火指令（默认的武器类型不需要修改，Mod武器也部分可用）

bool DebugMode = false; //调试模式开关，设为true开启，false关闭。开启调试后程序不会执行正常功能，会在编程块右下角输出所有获取到的方块编组以及错误原因。

bool init;
static int t;
static Random R_D = new Random();
static bool isOnOff = true; //是否开机
static bool isFireWhenAimOK = true; //是否当炮台瞄准了预瞄点才开火，是否瞄准的判定由【瞄准控制系统设置】中的AimRatio决定。开启这个选项可以节省子弹，也能让一部分由于角度限制无法瞄准到目标的炮台不会浪费子弹开火。
static int AttentionMode = 2; //待命模式，1 自由 2指向X转子前方 3随机模式
List<RotorBase> FCSR = new List<RotorBase>(); //炮台集合
List<Target> TargetList = new List<Target>(); //目标集合
static Vector3D PBPosition;

IMyShipController Cockpit;
string HeadNameTag = "VF_HEAD";
IMyShipController Head;
static bool isAeroDynamic = false;
static float aeroTRM = 0f;// transport rotor margin 0.2
static float aeroFM = 0f;// front margin 0.05
static int aeroACS = -180;
static int aeroAMP = 0;

Program()
{
Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

void Main(string arguments)
{
	//接收指令
	if(arguments == "Debug"){DebugMode = true;}
	if(arguments == "FireMode"){isFireWhenAimOK = !isFireWhenAimOK;}
	if(arguments == "OnOff"){isOnOff = !isOnOff;}
	if(arguments == "Attention"){
		AttentionMode += 1;
		if(AttentionMode > 3){AttentionMode = 1;}
	}
	
	PBPosition = Me.GetPosition();
	
	//这里获取了自动武器和所有的转子基座炮塔
	if(!init){GetBlocks(); return;}
	
	t ++;//时钟
	
	//生成目标，每个自动武器一个目标，每个转子基座炮台自动索敌一个目标，加一个FCS编程块目标
	TargetList = new List<Target>();
	//获取FCS目标
	Target FCS_T = new Target();
	FCS_T.GetTarget(GridTerminalSystem.GetBlockWithName(FCSComputerNameTag) as IMyProgrammableBlock);
	if(FCS_T.EntityId != 0){
		TargetList.Add(FCS_T);
	}
	//获取自动武器目标
	for(int i = 0; i < AutoWeapons.Count; i ++){
		Target ATWP_T = new Target();
		ATWP_T.GetTarget(AutoWeapons[i]);
		if(ATWP_T.EntityId != 0){
			TargetList.Add(ATWP_T);
		}
	}

	//指令每个炮台选择最近的目标
	int mode = 0; // no aero
	if (isAeroDynamic && Cockpit != null) {
	var MeVelocity = Cockpit.GetShipVelocities().LinearVelocity;
	MatrixD refWorldMatrix = Cockpit.WorldMatrix;
	MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(0,0,0), refWorldMatrix.Forward, refWorldMatrix.Up);
	Vector3D mySpeedToMe = Vector3D.TransformNormal(MeVelocity, refLookAtMatrix);
	bool controlByHead = Head!=null && Head.IsUnderControl;
	var naturalGravity = Cockpit.GetNaturalGravity();
	if (Math.Abs(mySpeedToMe.Z)>18 && !controlByHead && naturalGravity.Length() > 0.01) {
	if(aeroAMP != 1) {
	aeroACS = t;
	aeroAMP = 1;
	}
	mode = 1; // aero high speed
	} else {
	if(aeroAMP != 2) {
	aeroACS = t;
	aeroAMP = 2;
	}
	mode = 2; // aero low speed
	}
	}

	if(isOnOff && mode != 1){
		foreach(RotorBase R in FCSR){
			R.UpdateMotionInfo();//更新运动信息
			if(TargetList.Count > 0){
				R.AttackCloestTarget(TargetList);
			}
			else{
				R.Attention(AttentionMode, mode);//归位
			}
			R.CheckPlayerControl();//检测玩家控制
			R.ShowLCD();//LCD显示状态信息
			Echo("debugInfo " + R.isRocket + " " + R.debugInfo);
			
		}
	}else{
		foreach(RotorBase R in FCSR){
			R.Attention(1, mode);
			Echo("debugInfo " + R.isRocket + " " + R.debugInfo);
		}
	}
	
	ShowMainLCD();
}

// ========== 显示主要LCD内容 =========
public void ShowMainLCD()
{
	List<IMyTextPanel> Lcds = new List<IMyTextPanel>();
	GridTerminalSystem.GetBlocksOfType(Lcds, b => b.CustomName == LCDNameTag);
	string info = "";
	string br = "\n";
	info += " =========== [ MEA ] FCS-R ========== " + br;
	info += "  攻击开关 : " + (isOnOff ? " 开 " : " - ") + "         " + "精确开火 : " + (isFireWhenAimOK ? " 开 " : " - ") + br;
	info += "  炮台总数 : " +  FCSR.Count + "          " + "待命模式 : " + (AttentionMode == 1 ? "  自由  " : (AttentionMode == 2 ? "瞄准前方" : "随机警戒")) + br;
	int dot_count = t%30;
	if(dot_count <= 5) info += "  .";
	else if(dot_count <= 10) info += "  ..";
	else if(dot_count <= 15) info += "  ...";
	else if(dot_count <= 20) info += "  ....";
	else if(dot_count <= 25) info += "  .....";
	else if(dot_count <= 30) info += "  ......";
	info += br + " =========== 目标列表 ========== " + br;
	info += "  名字         距离         直径" + br;
	for(int i = 0; i < TargetList.Count; i ++){
		if(TargetList[i].EntityId != 0){
			info += "  " + TargetList[i].Name + "        " + Math.Round(Vector3D.Distance(TargetList[i].Position, Me.GetPosition()),0) + "      " + Math.Round(TargetList[i].Diameter,0) + br;
			
		}
	}
	info += " --------------------- 炮台组 ----------------------- " + br;
	info += "  组名      武器数量       完整度" + br;
	foreach(RotorBase R in FCSR){
		int goodcount = 0;
		foreach(IMyTerminalBlock wp in R.Weapons){
			if(wp.IsFunctional){goodcount += 1;}
		}
		double FunctionalPersent = 0;
		if (R.Weapons.Count > 0) {
		    FunctionalPersent = 100*goodcount/R.Weapons.Count;
		}
		info += "  " + R.Name + "            " + R.Weapons.Count + "                  " + FunctionalPersent + "%" + br;
	}
	foreach(IMyTextPanel lcd in Lcds){
		lcd.ShowPublicTextOnScreen();
		lcd.WritePublicText(info);
	}
}

List<IMyLargeTurretBase> AutoWeapons = new List<IMyLargeTurretBase>();
public void GetBlocks()
{
	List<IMyTerminalBlock> tmpBlocks = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType(tmpBlocks, b => b.CustomName.Contains(CockpitNameTag) );
	if (tmpBlocks.Count>0) Cockpit = (IMyShipController)tmpBlocks[0];
	else Cockpit = null;
	
	GridTerminalSystem.GetBlocksOfType(tmpBlocks, b => b.CustomName.Contains(HeadNameTag) );
	if (tmpBlocks.Count>0) Head = (IMyShipController)tmpBlocks[0];

	GridTerminalSystem.GetBlocksOfType(AutoWeapons, b => true);
	
	List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
	GridTerminalSystem.GetBlockGroups(groups);
	
	List<RotorBase> FCSR_temp = new List<RotorBase>();
	foreach(IMyBlockGroup grou in groups){
		FCSR_temp.Add(new RotorBase(grou));
	}
	
	for(int i = 0; i < FCSR_temp.Count; i++){
		if(FCSR_temp[i].ThisRotorBaseRight){
			FCSR.Add(FCSR_temp[i]);
		}
	}

	if(DebugMode){
		foreach(RotorBase rt in FCSR_temp){
			string info = rt.Name + " -- " + rt.ErrorReport;
			Echo(info);
		}
	}
	else{
	init = true;
	}
}

// =========== 目标基类 ============
public class Target
{
	public string Name;
	public long EntityId;
	public double Diameter;
	public int TimeStamp;
	public MyDetectedEntityType Type;
	public Vector3D Position;
	public Vector3D Velocity;
	public Vector3D Acceleration;
	public MatrixD Orientation;
	
	// --------- 初始化方法 -------
	public Target()
	{
		this.Name = "";
		this.EntityId = 0;
	}
	
	// -------- 通过不同方式获取目标 -------
	//Fire Control System$OnOff@Aim@Weapon@ExactLock@Fire@Speed$79432486378773108@大型网格 3108@62.6996810199223@{X:910.617573761076 Y:-767.260930602875 Z:-641.466380670639}@{X:0 Y:0 Z:0}@{X:0 Y:0 Z:0}@{X:0 Y:0 Z:-1}@{X:0 Y:1 Z:0}@0$
	public void GetTarget(IMyProgrammableBlock computer)
	{
		if(computer != null && computer.CustomData.Split('\n').Length >= 1){
CustomConfiguration cfgTarget = new CustomConfiguration(computer);
cfgTarget.Load();

string tmpS = "";
double tmpD = 0D;
long tmpL = 0L;

cfgTarget.Get("EntityId", ref tmpL);
this.EntityId = tmpL;

if(this.EntityId != 0){
cfgTarget.Get("Diameter", ref tmpD);
this.Diameter = tmpD;

cfgTarget.Get("Position", ref tmpS);
Vector3D.TryParse(tmpS, out this.Position);

cfgTarget.Get("Velocity", ref tmpS);
Vector3D.TryParse(tmpS, out this.Velocity);

cfgTarget.Get("Acceleration", ref tmpS);
Vector3D.TryParse(tmpS, out this.Acceleration);

this.TimeStamp = t;
}

		}
	}
	
	public void GetTarget(IMyLargeTurretBase autoWeapon)
	{
		MyDetectedEntityInfo thisEntity = autoWeapon.GetTargetedEntity();
		if(!thisEntity.IsEmpty()){
			this.Name = thisEntity.Name;
			this.EntityId = thisEntity.EntityId;
			this.Diameter = Vector3D.Distance(thisEntity.BoundingBox.Max, thisEntity.BoundingBox.Min);
			Vector3D.TryParse(thisEntity.Position.ToString(), out this.Position);
			Vector3D.TryParse(thisEntity.Velocity.ToString(), out this.Velocity);
			this.TimeStamp = t;
		}
	}
	
	public void Clear()
	{
		this.Name = "";
		this.EntityId = 0;
		this.TimeStamp = 0;
		this.Diameter = 0;
		this.Position = this.Velocity = this.Acceleration = new Vector3D();
	}
}

// =========== 转子炮塔组件基类 ===========
public class RotorBase
{
	public string Name;
	public string ErrorReport = "Normal";
	
	public Target MyTarget;
	
	public Vector3D Position;
	public Vector3D Velocity;
	public Vector3D Acceleration;
	
	public IMyTerminalBlock AimBlock;
	public List<IMyMotorStator> RotorXs = new List<IMyMotorStator>();
	public List<int> RotorXField = new List<int>();
	public List<IMyMotorStator> RotorYs = new List<IMyMotorStator>();
	public List<int> RotorYField = new List<int>();
	public List<IMyTerminalBlock> Weapons = new List<IMyTerminalBlock>();
	public List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();
	public List<IMySensorBlock> Sensors = new List<IMySensorBlock>();
	public List<IMyTextPanel> LCDs = new List<IMyTextPanel>();
	public List<IMyTerminalBlock> FireTimers = new List<IMyTerminalBlock>();
	
	public bool ThisRotorBaseRight;
	public string debugInfo;
	public bool isRocket = true;
	public CustomConfiguration cfg;
	public double hori;
	public double horiD;
	public double vert;
	public double vertD;
	static float pp=20F,pi=1F,pd=0F, pim=0.1F;
	public PIDController pidX = new PIDController(pp, pi, pd,pim,-pim,12);
	public PIDController pidY = new PIDController(pp, pi, pd,pim,-pim,12);
	
	// -------- 初始化方法 ---------
	public RotorBase(IMyBlockGroup thisgroup)
	{
		Name = thisgroup.Name;
		
		List<IMyTerminalBlock> blocks_temp = new List<IMyTerminalBlock>();
		thisgroup.GetBlocks(blocks_temp);
		
		//获得瞄准块
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block.CustomName.Contains(AimBlockKey)) {AimBlock = block; break;}
		}
		if(AimBlock == null) {ErrorReport = "AimBlock Not Found!"; return;}
		cfg = new CustomConfiguration(AimBlock);
		cfg.Load();
		cfg.Get("vert", ref vert);
		cfg.Get("vertD", ref vertD);
		cfg.Get("hori", ref hori);
		cfg.Get("horiD", ref horiD);
		
		//获得转子
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block is IMyMotorStator){
				int NagtiveRotor = 1;
				if(block.CustomName.Contains(RotorNagtiveTag)){NagtiveRotor = -1;}
				Base6Directions.Direction rotor_Up = block.WorldMatrix.GetClosestDirection(AimBlock.WorldMatrix.Up);
				Base6Directions.Direction rotor_Right = block.WorldMatrix.GetClosestDirection(AimBlock.WorldMatrix.Right);
				switch(rotor_Up){
					case Base6Directions.Direction.Up:
						RotorXs.Add(block as IMyMotorStator);
						RotorXField.Add(1*NagtiveRotor);
					break;
					case Base6Directions.Direction.Down:
						RotorXs.Add(block as IMyMotorStator);
						RotorXField.Add(-1*NagtiveRotor);
					break;
				}
				switch(rotor_Right){
					case Base6Directions.Direction.Up:
						RotorYs.Add(block as IMyMotorStator);
						RotorYField.Add(-1*NagtiveRotor);
					break;
					case Base6Directions.Direction.Down:
						RotorYs.Add(block as IMyMotorStator);
						RotorYField.Add(1*NagtiveRotor);
					break;
				}
			}
		}
		if(RotorXs.Count < 1 || RotorYs.Count < 1) {ErrorReport = "Rotors Not Complete!"; return;}
		
		//获得武器
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block is IMyUserControllableGun){
				Weapons.Add(block);
			}
			if(block is IMySmallGatlingGun) {
				 this.isRocket = false;
			}
		}
		
		//获取自定义开火定时块
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block.CustomName.Contains(FireTimerNameTag)){
				FireTimers.Add(block);
			}
		}
		
		//初始化这个炮台模板成功
		ThisRotorBaseRight = true;
		
		//获得摄像头、探测器等辅助方块，非必须
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block is IMyCameraBlock){
				Cameras.Add(block as IMyCameraBlock);
			}
			else if(block is IMySensorBlock){
				Sensors.Add(block as IMySensorBlock);
			}
			if(block is IMyTextPanel){
				LCDs.Add(block as IMyTextPanel);
			}
		}
	}

	// ------- 更新运动信息 ---------
	public void UpdateMotionInfo()
	{
		this.Acceleration = (((this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio) - this.Velocity)*ProgramUpdateRatio;
		this.Velocity = (this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio;
		this.Position = this.AimBlock.GetPosition();
	}
	
	// ------- 待命归位 --------
	public void Attention(int ATMode, int aeroMode)
	{
		if (this.Weapons.Count == 0) return;
		this.debugInfo = "aeromode " + aeroMode;
		// Vector3D aimPoint = new Vector3D();
		// MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.RotorXs[0].WorldMatrix.Forward, this.RotorXs[0].WorldMatrix.Up);
		float xt = 0F, yt=0F;
		switch(aeroMode) {
		case 0:
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidX.Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			             var a = this.RotorYs[i].Angle;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidY.Filter(-a,2);
			}
		break;
		case 1:
			if (this.RotorXs[0].Position.X < 0) {
			// left rotor
			yt = (float)(aeroTRM * Math.PI);
			} else {
			yt = (float)(-aeroTRM * Math.PI);
			}
			if (t > aeroACS + 180) yt = 0;

			if (t < aeroACS + 60) {
			if (this.RotorXs[0].Position.X < 0) {
			// left rotor
			xt = (float)((1 + aeroFM) * Math.PI);
			} else {
			xt = (float)((1 - aeroFM) * Math.PI);
			}
			} else {
			xt = 0;
			}

			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle - xt;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidX.Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			            var a = this.RotorYs[i].Angle - yt;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidY.Filter(-a,2);
			}
		break;
		case 2:
			if (t > aeroACS + 180 || t < 180) yt = 0;
			else {
			if (this.RotorXs[0].Position.X < 0) {
			// left rotor
			yt = (float)(aeroTRM * Math.PI);
			} else {
			yt = (float)(-aeroTRM * Math.PI);
			}			
			}

			if (t < aeroACS + 60 && t > 60) xt = 0;
			else {
			if (this.RotorXs[0].Position.X < 0) {
			// left rotor
			xt = (float)((1+aeroFM) * Math.PI);
			} else {
			xt = (float)((1 - aeroFM) * Math.PI);
			}
			}
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle - xt;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidX.Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			            var a = this.RotorYs[i].Angle - yt;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidY.Filter(-a,2);
			}

		break;
		}
		// switch(ATMode){
		// 	case 1: //自由模式，关闭所有转子运动
		// 	for(int i = 0; i < this.RotorXs.Count; i ++){
		// 		this.RotorXs[i].TargetVelocityRPM = 0;
		// 	}
		// 	for(int i = 0; i < this.RotorYs.Count; i ++){
		// 		this.RotorYs[i].TargetVelocityRPM = 0;
		// 	}
		// 	break;
		// 	case 2: //rotor清零
		// 	for(int i = 0; i < this.RotorXs.Count; i ++){
		// 	             var a = this.RotorXs[i].Angle;
		// 		if (a > Math.PI) a = a - MathHelper.TwoPi;
		// 		this.RotorXs[i].TargetVelocityRPM = -a;
		// 	}
		// 	for(int i = 0; i < this.RotorYs.Count; i ++){
		// 	             var a = this.RotorYs[i].Angle;
		// 		if (a > Math.PI) a = a - MathHelper.TwoPi;
		// 		this.RotorYs[i].TargetVelocityRPM = -a;
		// 	}
		// 	break;
		// 	case 3: //随机指向，仅指向炮塔的上半球
		// 	if(t%AttentionRandomTime == 0){
		// 		aimPoint = this.RotorXs[0].GetPosition() + this.RotorXs[0].WorldMatrix.Forward*(R_D.Next(-500,500)) + this.RotorXs[0].WorldMatrix.Right*(R_D.Next(-500,500)) + this.RotorXs[0].WorldMatrix.Up*(R_D.Next(100,500));
		// 		this.AimAtTarget(aimPoint);
		// 	}
		// 	break;
		// }
		
	}
	
	// ------ 检测和执行玩家控制 --------
	public void CheckPlayerControl()
	{
		if(this.AimBlock is IMyShipController && (this.AimBlock as IMyShipController).IsUnderControl){
			Vector2 MouseInput = (this.AimBlock as IMyShipController).RotationIndicator;
			for(int i = 0; i < this.RotorXs.Count; i ++){
				this.RotorXs[i].TargetVelocityRPM = (float)(MouseInput.Y * RotorXField[i] * PlayerAimRatio);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
				this.RotorYs[i].TargetVelocityRPM = -(float)(MouseInput.X * RotorYField[i] * PlayerAimRatio);
			}
		}
	}

	public double angleDeltaAbs (double a, double b) {
	       var t = Math.Abs(a - b);
	       if ( t > Math.PI) {
	       	  t = Math.Abs(t - MathHelper.TwoPi);
	       }
	       debugInfo += t + "\n";
	       return t;
	}
	
	// ------- 攻击并搜索最近的目标 -------
	public void AttackCloestTarget(List<Target> targetList)
	{
		double currentDis = double.MaxValue;
		Target MyAttackTarget = new Target();
		for(int i = 0; i < targetList.Count; i ++){
			double distance = Vector3D.Distance(targetList[i].Position, this.Position);
			if(targetList[i].EntityId != 0 && distance <= currentDis){
				if(OnlyAttackUpPlane){
					MatrixD RotortXLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.RotorXs[0].WorldMatrix.Forward, this.RotorXs[0].WorldMatrix.Up);
					Vector3D TargetPositionToRotorX = Vector3D.TransformNormal(targetList[i].Position - this.RotorXs[0].GetPosition(), RotortXLookAtMatrix);
					if(TargetPositionToRotorX.Y >= 0){
						MyAttackTarget = targetList[i];
						currentDis = distance;
					}
				}
				else{
					MyAttackTarget = targetList[i];
					currentDis = distance;
				}
			}
		}
		
		if(MyAttackTarget.EntityId != 0){
			debugInfo = "";
			bool isAimedOK = this.AimAtTarget(MyAttackTarget, false);
			bool sensorActive = false;
			foreach(IMySensorBlock sensor in this.Sensors){
				if(sensor.IsActive){sensorActive = true;}
			}
			bool rx = false;
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle;
				if (this.RotorXs.Count == 2 && angleDeltaAbs(a , vert*Math.PI) < vertD*Math.PI) {
					rx = true;
				}else
				if (this.RotorXs.Count == 1 && angleDeltaAbs(a , hori*Math.PI) < horiD*Math.PI) {
					rx = true;
				}
			}
			bool ry = false;
			for(int i = 0; i < this.RotorYs.Count; i ++){
			            var a = this.RotorYs[i].Angle;
				if (this.RotorYs.Count == 2 && angleDeltaAbs(a , vert*Math.PI) < vertD*Math.PI) {
					ry = true;
				}else
				if (this.RotorYs.Count == 1 && angleDeltaAbs(a , hori*Math.PI) < horiD*Math.PI) {
					ry = true;
				}
			}
			debugInfo += ";";
			sensorActive = sensorActive || (rx&&ry);
			MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.AimBlock.WorldMatrix.Forward, this.AimBlock.WorldMatrix.Up);
			Vector3D TargetPositionToMe = Vector3D.TransformNormal(MyAttackTarget.Position - this.Position, refLookAtMatrix);
			//存在自定义开火定时块就执行自定义开火
//			if(FireTimer.Count() > 0){
			if(false){
//				foreach(IMyTerminalBlock fire_timer in FireTimers){
//					int fire_distance = 0;
//					if(!int.TryParse(fire_timer.CustomData, out fire_distance)){fire_distance = AttackDistance;}
//					if(Vector3D.Distance(MyAttackTarget.Position, this.Position) <= AttackDistance && !sensorActive){
//						fire_timer.ApplyAction("TriggerNow");
//					}
//				}
			}
			else if(Vector3D.Distance(MyAttackTarget.Position, this.Position) <= AttackDistance && !sensorActive){
				if(isFireWhenAimOK){
					if(isAimedOK){
						this.Fire();
					}
				}
				else{
					this.Fire();
				}
			}
			this.MyTarget = MyAttackTarget;
		}
		else{
			this.MyTarget = null;
		}
		
	}
	
	// ---------- 瞄准目标 -----------
	private List<Vector3D> Aim_PID_Data = new List<Vector3D>();
	bool AimAtTarget(Vector3D Position)
	{
		return AimAtTarget(Position, new Vector3D(), new Vector3D(), true);
	}
	
	bool AimAtTarget(Vector3D Position, Vector3D Velocity, Vector3D Acceleration)
	{
		return AimAtTarget(Position, Velocity, Acceleration, true);
	}
	
	bool AimAtTarget(Target aimTarget, bool isStraight)
	{
		return AimAtTarget(aimTarget.Position, aimTarget.Velocity, aimTarget.Acceleration, isStraight);
	}

	Vector3D maxTo(Vector3D ori, Double l) {
		 if (ori.Length() > l) {
		 	return ori * (l/ori.Length());
		 }else {
		 	return ori;
		 }
	}
	
	bool AimAtTarget(Vector3D Position, Vector3D Velocity, Vector3D Acceleration, bool isStraight)
	{
		if (this.Weapons.Count == 0) return false;
		bool isRocket = this.isRocket;
		MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.AimBlock.WorldMatrix.Forward, this.AimBlock.WorldMatrix.Up);
		Vector3D thisV;
		Vector3D thisA;
		double bs, ba, bm;
		if (isRocket == true) {
			thisV = this.Velocity * 0.3;
			thisA = new Vector3D(0,0,0);
			bs = BulletInitialSpeed2;
			ba = BulletAcceleration2;
			bm = BulletMaxSpeed2;
		} else {
			thisV = this.Velocity;
			thisA = this.Acceleration;
			bs = BulletInitialSpeed;
			ba = BulletAcceleration;
			bm = BulletMaxSpeed;
		}
		if (isStraight){
		bs = 100000;
		ba = 0;
		bm = 100000;
		}
		
		Vector3D HitPoint = HitPointCaculate(this.Position, thisV, thisA, Position, Velocity, Acceleration, bs, ba, bm);
		Vector3D TargetPositionToMe = Vector3D.Normalize(Vector3D.TransformNormal(HitPoint - this.AimBlock.GetPosition(), refLookAtMatrix));
		Vector3D aimDir = CalcAim(this.Position, thisV, Position, Velocity, bs, ba, bm);
		var aimDirToMe = Vector3D.TransformNormal(aimDir,  refLookAtMatrix);
		debugInfo += Vector3D.Dot(aimDirToMe, TargetPositionToMe) + "\n";
		//储存采样点
		if(Aim_PID_Data.Count < Aim_PID_T){
			for(int i = 0; i < Aim_PID_T; i ++){
				Aim_PID_Data.Add(new Vector3D());
			}
		}
		else{Aim_PID_Data.Remove(Aim_PID_Data[0]); Aim_PID_Data.Add(TargetPositionToMe);}
		
		//获得采样点积分
		double X_I = 0;
		double Y_I = 0;
		foreach(Vector3D datapoint in Aim_PID_Data){
			X_I += datapoint.X;
			Y_I += datapoint.Y;
		}
		
		//计算输出
		double YawValue = Aim_PID_P*(TargetPositionToMe.X + (1/Aim_PID_I)*X_I + Aim_PID_D*(Aim_PID_Data[Aim_PID_T-1].X - Aim_PID_Data[0].X)/Aim_PID_T);

		//double aa=0, ea=0;
		//Vector3D.GetAzimuthAndElevation(TargetPositionToMe, out aa, out ea);

		double PitchValue = Aim_PID_P*(TargetPositionToMe.Y + (1/Aim_PID_I)*Y_I + Aim_PID_D*(Aim_PID_Data[Aim_PID_T-1].Y - Aim_PID_Data[0].Y)/Aim_PID_T);

		bool isCeaseZoon = angleDeltaAbs(this.RotorXs[0].Angle, hori*Math.PI) < horiD*Math.PI;
		if (isCeaseZoon && isAeroDynamic) {
		PitchValue = this.RotorYs[0].Angle; //CeaseElevator
		debugInfo += this.RotorYs[0].Angle + " Cease\n";
		}


		for(int i = 0; i < this.RotorXs.Count; i ++){
			this.RotorXs[i].TargetVelocityRPM = (float)(YawValue * this.RotorXField[i] * RotorMaxSpeed);
		}
		if(TargetPositionToMe.Z < 0){
			for(int i = 0; i < this.RotorYs.Count; i ++){
				this.RotorYs[i].TargetVelocityRPM = (float)(PitchValue * this.RotorYField[i] * RotorMaxSpeed);
			}
		}
		
		// 计算当前与预期瞄准点的瞄准夹角
		Vector3D V_A = HitPoint - this.Position;
		Vector3D V_B = this.AimBlock.WorldMatrix.Forward;
		double Angle = Math.Acos(Vector3D.Dot(V_A,V_B)/(V_A.Length() * V_B.Length())) * 180 / Math.PI;
		if(Angle <= AimRatio) return true;
		return false;
	}
	
	// ------ 开火 ---------
	public void Fire()
	{
		foreach(IMyTerminalBlock gun in this.Weapons){
			gun.ApplyAction(ShootActionName);
		}
	}
	
	// ------ 显示LCD --------
	public void ShowLCD()
	{
		string info = "";
		string br = "\n";
		info += " =========== [ MEA ] FCS-R ========== " + br;
		if(this.MyTarget != null){
			info += " 目标: " + this.MyTarget.Name + br;
			info += " 距离: " + Math.Round(Vector3D.Distance(this.Position, this.MyTarget.Position),0) + br;
		}
		else{
			info += " 待命中..." + br + br;
		}
		info += " --------------------------------------------------------- " + br;
		info += " 武器数量: " + this.Weapons.Count + br;
		info += " 摄像头数: " + this.Cameras.Count + br;
		foreach(IMyTextPanel lcd in this.LCDs){
			lcd.ShowPublicTextOnScreen();
			lcd.WritePublicText(info);
		}
	}
}

// ============ 辅助函数 =============
static Vector3D HitPointCaculate(Vector3D Me_Position, Vector3D Me_Velocity, Vector3D Me_Acceleration, Vector3D Target_Position, Vector3D Target_Velocity, Vector3D Target_Acceleration,    
							double Bullet_InitialSpeed, double Bullet_Acceleration, double Bullet_MaxSpeed)   
{   
	//迭代算法   
	Vector3D HitPoint = new Vector3D();   
	Vector3D Smt = Target_Position - Me_Position;//发射点指向目标的矢量   
	Vector3D Velocity = Target_Velocity - Me_Velocity; //目标飞船和自己飞船总速度   
	Vector3D Acceleration = Target_Acceleration; //目标飞船和自己飞船总加速度   
	   
	double AccTime = (Bullet_Acceleration == 0 ? 0 : (Bullet_MaxSpeed - Bullet_InitialSpeed)/Bullet_Acceleration);//子弹加速到最大速度所需时间   
	double AccDistance = Bullet_InitialSpeed*AccTime + 0.5*Bullet_Acceleration*AccTime*AccTime;//子弹加速到最大速度经过的路程   
	   
	double HitTime = 0;   
	   
	if(AccDistance < Smt.Length())//目标在炮弹加速过程外   
	{   
		HitTime = (Smt.Length() - Bullet_InitialSpeed*AccTime - 0.5*Bullet_Acceleration*AccTime*AccTime + Bullet_MaxSpeed*AccTime)/Bullet_MaxSpeed;   
		HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
	}   
	else//目标在炮弹加速过程内 
	{   
		double HitTime_Z = (-Bullet_InitialSpeed + Math.Pow((Bullet_InitialSpeed*Bullet_InitialSpeed + 2*Bullet_Acceleration*Smt.Length()),0.5))/Bullet_Acceleration;   
		double HitTime_F = (-Bullet_InitialSpeed - Math.Pow((Bullet_InitialSpeed*Bullet_InitialSpeed + 2*Bullet_Acceleration*Smt.Length()),0.5))/Bullet_Acceleration;   
		HitTime = (HitTime_Z > 0 ? (HitTime_F > 0 ? (HitTime_Z < HitTime_F ? HitTime_Z : HitTime_F) : HitTime_Z) : HitTime_F);   
		HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
	}   
	//迭代，仅迭代更新碰撞时间，每次迭代更新右5位数量级   
	for(int i = 0; i < 3; i ++)   
	{   
		if(AccDistance < Vector3D.Distance(HitPoint,Me_Position))//目标在炮弹加速过程外   
		{   
			HitTime = (Vector3D.Distance(HitPoint,Me_Position) - Bullet_InitialSpeed*AccTime - 0.5*Bullet_Acceleration*AccTime*AccTime + Bullet_MaxSpeed*AccTime)/Bullet_MaxSpeed;   
			HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
		}   
		else//目标在炮弹加速过程内   
		{   
			double HitTime_Z = (-Bullet_InitialSpeed + Math.Pow((Bullet_InitialSpeed*Bullet_InitialSpeed + 2*Bullet_Acceleration*Vector3D.Distance(HitPoint,Me_Position)),0.5))/Bullet_Acceleration;   
			double HitTime_F = (-Bullet_InitialSpeed - Math.Pow((Bullet_InitialSpeed*Bullet_InitialSpeed + 2*Bullet_Acceleration*Vector3D.Distance(HitPoint,Me_Position)),0.5))/Bullet_Acceleration;   
			HitTime = (HitTime_Z > 0 ? (HitTime_F > 0 ? (HitTime_Z < HitTime_F ? HitTime_Z : HitTime_F) : HitTime_Z) : HitTime_F);   
			HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
		}   
	}   
	return HitPoint;   
}

public class CustomConfiguration
{
public IMyTerminalBlock configBlock;
public Dictionary<string, string> config;

public CustomConfiguration(IMyTerminalBlock block)
{
configBlock = block;
config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public void Load()
{
ParseCustomData(configBlock, config);
}

public void Save()
{
WriteCustomData(configBlock, config);
}

public string Get(string key, string defVal = null)
{
return config.GetValueOrDefault(key.Trim(), defVal);
}

public void Get(string key, ref string res)
{
string val;
if (config.TryGetValue(key.Trim(), out val))
{
res = val;
}
}

public void Get(string key, ref int res)
{
int val;
if (int.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref long res)
{
long val;
if (long.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref float res)
{
float val;
if (float.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref double res)
{
double val;
if (double.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref bool res)
{
bool val;
if (bool.TryParse(Get(key), out val))
{
res = val;
}
}
public void Get(string key, ref bool? res)
{
bool val;
if (bool.TryParse(Get(key), out val))
{
res = val;
}
}

public void Set(string key, string value)
{
config[key.Trim()] = value;
}

public static void ParseCustomData(IMyTerminalBlock block, Dictionary<string, string> cfg, bool clr = true)
{
if (clr)
{
cfg.Clear();
}

string[] arr = block.CustomData.Split(new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
for (int i = 0; i < arr.Length; i++)
{
string ln = arr[i];
string va;

int p = ln.IndexOf('=');
if (p > -1)
{
va = ln.Substring(p + 1);
ln = ln.Substring(0, p);
}
else
{
va = "";
}
cfg[ln.Trim()] = va.Trim();
}
}

public static void WriteCustomData(IMyTerminalBlock block, Dictionary<string, string> cfg)
{
StringBuilder sb = new StringBuilder(cfg.Count * 100);
foreach (KeyValuePair<string, string> va in cfg)
{
sb.Append(va.Key).Append('=').Append(va.Value).Append('\n');
}
block.CustomData = sb.ToString();
}
}

public class PIDController
{
public static double DEF_SMALL_GRID_P = 31.42;
public static double DEF_SMALL_GRID_I = 0;
public static double DEF_SMALL_GRID_D = 10.48;

public static double DEF_BIG_GRID_P = 15.71;
public static double DEF_BIG_GRID_I = 0;
public static double DEF_BIG_GRID_D = 7.05;

double integral;
double lastInput;

double gain_p;
double gain_i;
double gain_d;
double upperLimit_i;
double lowerLimit_i;
double second;

public PIDController(double pGain, double iGain, double dGain, double iUpperLimit = 0, double iLowerLimit = 0, float stepsPerSecond = 60f)
{
gain_p = pGain;
gain_i = iGain;
gain_d = dGain;
upperLimit_i = iUpperLimit;
lowerLimit_i = iLowerLimit;
second = stepsPerSecond;
}

public double Filter(double input, int round_d_digits)
{
double roundedInput = Math.Round(input, round_d_digits);

integral = integral + (input / second);
integral = (upperLimit_i > 0 && integral > upperLimit_i ? upperLimit_i : integral);
integral = (lowerLimit_i < 0 && integral < lowerLimit_i ? lowerLimit_i : integral);

double derivative = (roundedInput - lastInput) * second;
lastInput = roundedInput;

return (gain_p * input) + (gain_i * integral) + (gain_d * derivative);
}

public void Reset()
{
integral = lastInput = 0;
}
}

static Vector3D CalcAim(Vector3D mp, Vector3D mv, Vector3D tp, Vector3D tv, double bs, double ba, double bm) {

var los = Vector3D.Normalize(tp - mp);
var rv = Vector3D.Reject(tv-mv, los);
var ov = (tv-mv) - rv; // 假设
var ovl = ov.Length();
if (Vector3D.Dot(ov, los) < 0) ovl = -ovl;
var rvd = Vector3D.Normalize(rv);
var rvl = rv.Length();
if (bs == bm) {
var lvl = Math.Sqrt(bm*bm - rvl*rvl);
var aim = Vector3D.Normalize( los + rvd*(rvl/lvl));
return aim;
} else {
// 假设一定超过加速阶段
var accSpeed = (bs + bm)/2;
var accTime = (bm - bs) / ba;
// 设剩余追及时间为x, 夹角cos为c
// 等式1直线距离  (tp - mp).Length() + ovl*(accTime + x)  = accTime * ( accSpeed * c) + bm*c*x;
// 等式2侧向速度 rv.Length() = (accSpeed * (1-c*c) * accTime + bm * (1 - c*c) * x ) / (accTime + x);
// 方程太复杂
return Vector3D.Zero;
}

}

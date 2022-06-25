﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace fcsr_p
{
    partial class Program : MyGridProgram
    {
        #region ingamescript

// start

static bool usingTurretAsSrc = false; // if using radar output, set to false
static bool isHoming = false; // 超出射程是否归位
static bool isOnOff = true; //是否开机 舰船true 可变战机手臂 false
static bool OnlyAttackUpPlane = false; //自动选择目标时是否只选择转子基座炮塔的上半球面里的目标（当目标低于该炮塔时不选择这个目标） 舰船一般true 可变战机一般false
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
const string FCSComputerNameTag = "Programmable Block fcs"; //FCS编程块名字，用于读取其中目标，非必须。
const string AimBlockKey = "FCS#R"; //瞄准块名字关键字，可以写在名字中，只要包含这个关键字即可。
const string LCDNameTag = "FCSR_LCD"; //LCD名字，用来显示各个目标和炮台信息，非必须，也可以是多个。
const string RotorNagtiveTag = "[-]"; //转子负转标签。当转子名字里完全包含这个标签的时候，它会被强制认为是反向控制的转子。用来解决某些特殊结构的转子问题

string CockpitNameTag = "Reference";

// ============== 战斗设置 ==============
const double AttackDistance = 910; //默认武器的自动开火距离

// ============== 进阶开火机制 ============
//程序会自动检测原版武器并在有目标时触发武器开火。同时你也可以自己使用定时块来触发开火。你需要在需要触发自定义开火的转子基座炮台上加入一个或多个定时块，必须放进这个炮台的编组。
//同时，你需要让这个开火定时块的名字里完全包含下方的FireTimerNameTag设置的标签，并且在这个定时块的自定义参数里写上触发开火的距离（纯数字），如果不设置距离，将采用默认武器的开火距离
//程序会在目标进入开火距离内的时候，以每秒60次的速度对这个定时块执行“立即触发”动作。
const string FireTimerNameTag = "PG-";  //自动开火定时块名字标签，定时块名字需要完全包含这个标签。

// ================ 武器子弹参数设置 =============
const double BulletInitialSpeed = 400; //子弹初速度
const double BulletAcceleration = 0; //子弹加速度
const double BulletMaxSpeed = 400; //子弹最大速度
const double ShootDistance = 870; //开火距离
const double BulletInitialSpeed2 = 170; //子弹初速度
const double BulletAcceleration2 = 10; //子弹加速度
const double BulletMaxSpeed2 = 190; //子弹最大速度
const double ShootDistance2 = 910; //开火距离
const double BulletInitialSpeed3 = 267.5; //子弹初速度
const double BulletAcceleration3 = 0; //子弹加速度
const double BulletMaxSpeed3 = 267.5; //子弹最大速度
const double ShootDistance3 = 3000; //开火距离
const float xdegree = -2.0F;// 活塞炮Y轴（围绕X轴）上抬

// ============== 其他设置 ==============
static int AttentionRandomTime = 180; //随机警戒模式的切换间隔，60是1秒，120是2秒，这个数必须是整数

// ================= 瞄准控制系统设置 ===============
// PID控制
const double RotorMaxSpeed = 30; //自动状态下转子最大角速度，单位 圈/分钟。标准值30
const double AimRatio = 5; //瞄准精度，单位：度。用来判断炮台是否瞄准，以便其他动作判断。不影响瞄准的效率。当瞄准块的正前方向量与瞄准块和目标的连线向量夹角小于这个值时，整个系统判定瞄准了目标。
const double Aim_PID_P = 20; //比例系数：可以理解为整个PID控制的总力度，建议范围0到1.2，1是完全出力。
const double Aim_PID_I = 1; //积分系数：增加这个系数会让静态误差增加（即高速环绕误差），但会减少瞄准的震荡。反之同理
const double Aim_PID_D = 0; //微分系数：增加这个系数会减少瞄准的震荡幅度，但会加剧在小角度偏差时的震荡幅度。反之同理

const int Aim_PID_T = 5; //PID 采样周期（单位：帧），周期越小效果越好，但太小的周期会让积分系数难以发挥效果
// 手动控制
const double PlayerAimRatio = 0.1; // 玩家手动控制的瞄准灵敏度，只有当瞄准块是远程控制块或驾驶舱才存在这个功能。

// =============== 代码调试参数 ====================
const int ProgramUpdateRatio = 60; //程序每秒循环的次数，可在GetBlocks()函数中修改程序循环频率，可以降帧使用。修改后同步修改这个参数即可
const string ShootActionName = "ShootOnce"; //武器的开火指令（默认的武器类型不需要修改，Mod武器也部分可用）

bool DebugMode = false; //调试模式开关，设为true开启，false关闭。开启调试后程序不会执行正常功能，会在编程块右下角输出所有获取到的方块编组以及错误原因。

bool init;
static int t;
static Random R_D = new Random();
static bool isFireWhenAimOK = true; //是否当炮台瞄准了预瞄点才开火，是否瞄准的判定由【瞄准控制系统设置】中的AimRatio决定。开启这个选项可以节省子弹，也能让一部分由于角度限制无法瞄准到目标的炮台不会浪费子弹开火。
static int AttentionMode = 2; //待命模式，1 自由 2指向X转子前方 3随机模式
List<RotorBase> FCSR = new List<RotorBase>(); //炮台集合
static List<Target> TargetList = new List<Target>(); //目标集合
static List<Target> LastTargetList = new List<Target>(); //目标集合
static CustomConfiguration meCfg;
double axisBs = 400;
double axisBr = 800;
double axisGr = 2;
double axisYOffset = -0.5;
double axisCr = 1;
class RadarTarget
{
    public long id;
    public bool isSelected;
    public bool isHighThreaten;
    public Vector3D position;
    public Vector3D velocity;
}
static Dictionary<long, RadarTarget> radarTargets;
static string RADAR_COMPUTER_NAME = "Programmable Block radar";
static IMyTextSurface radarSurface;
static Vector3D PBPosition;

static IMyShipController Cockpit;
string HeadNameTag = "VF_HEAD";
static IMyShipController Head;
static IMyShipController msc;
static bool isAeroDynamic = false;
static float aeroTRM = 0f;// transport rotor margin 0.2
static float aeroFM = 0f;// front margin 0.05
static int aeroACS = -180;
static int aeroAMP = 0;
static string debugInfo = "";
static string initInfo = "";
static List<Vector3D> ignorePosList = new List<Vector3D>();
static List<long> ignoreList = new List<long>();
const double ignoreDistance = 10;


//List<IMyTerminalBlock> TURRETS = new List<IMyTerminalBlock>();

static IMyRemoteControl remoteBlock = null;
static Vector3D piratePosition = Vector3D.Zero;

static Dictionary<string, List<IMyTerminalBlock>> weaponDic = new Dictionary<string, List<IMyTerminalBlock>>();
static Dictionary<string, int> weaponSeqDic = new Dictionary<string, int>();

static IMyUserControllableGun axisGun;

static bool isNoManual=false;
static bool usingSearchStable=true;
static int frameInterval = 15;
static float tvnToRpm = (float)(1.0 / Math.Sin(1.0 / 60));

Program()
{
Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

void Main(string arguments)
{
	t ++;//时钟
	debugInfo = "";

	//接收指令
	if(arguments == "Debug"){DebugMode = true;}
	else if(arguments == "FireMode"){isFireWhenAimOK = !isFireWhenAimOK;}
	else if(arguments == "OnOff"){isOnOff = !isOnOff;}
	else if(arguments == "On"){isOnOff = true;
            }
	else if(arguments == "Off"){isOnOff = false;}
	else if(arguments == "Attention"){
		AttentionMode += 1;
		if(AttentionMode > 3){AttentionMode = 1;}
	}
	else if(arguments == "IgnoreAll"){
		     // coding
		     foreach(var t in TargetList){
		     		 ignoreList.Add(t.EntityId);
				 ignorePosList.Add(t.Position);
		     }
	}
	else if(arguments == "ClearIgnore"){
		     // coding
		     ignoreList = new List<long>();
		     ignorePosList = new List<Vector3D>();
	}
	else if("ResetOffset".Equals(arguments)) { 
		// FEATURE 0220
		FCSR.ForEach(x=>x.AimOffset=Vector2.Zero);
    }
	else if ("NoManual".Equals(arguments)) { 
		isNoManual = true;
	}

	
	try {
	var al = arguments.Split(':');	
	if (al.Length <= 1) throw new Exception();
	switch(al[0]) {
	case "FireOne":
	List<IMyTerminalBlock> wl = null;
	int idx = 0;
	if (!weaponDic.ContainsKey(al[1])) {
	var group = GridTerminalSystem.GetBlockGroupWithName(al[1]);
	if (group == null) break;
	wl = new List<IMyTerminalBlock>();
	group.GetBlocks(wl);

	var mscLookAt = MatrixD.CreateLookAt(Vector3D.Zero, msc.WorldMatrix.Forward, msc.WorldMatrix.Up);
	wl.Sort((a,b)=>(Vector3D.TransformNormal(a.GetPosition()-msc.GetPosition(), mscLookAt).Z.CompareTo( Vector3D.TransformNormal(b.GetPosition()-msc.GetPosition(), mscLookAt).Z)
	));
	weaponDic[al[1]] = wl;
	weaponSeqDic[al[1]] = 0;
	} else {
	wl = weaponDic[al[1]];
	idx = weaponSeqDic[al[1]];
	}
	weaponSeqDic[al[1]] = (idx + 1) % wl.Count;
	wl[idx].ApplyAction(ShootActionName);
	break;
	default:
	break;
	}
	}catch(Exception e){}
	
	PBPosition = Me.GetPosition();
	
	//这里获取了自动武器和所有的转子基座炮塔
	if(!init){GetBlocks(); return;}
	
            debugInfo += "\nFCSR Count: " + FCSR.Count;
	foreach ( var r in FCSR ) {
		debugInfo += "\n" + r.debugInfo + "\n" + r.debugInfoInter;
	}
	
	//生成目标，每个自动武器一个目标，每个转子基座炮台自动索敌一个目标，加一个FCS编程块目标
	TargetList = new List<Target>();
	//获取FCS目标
	TargetList.AddRange(GetFcsTargetList());
	bool haveRadar = checkRadarTarget(out radarTargets);
	if(haveRadar) { 
	    TargetList.AddRange(radarTargets.Values.Select(delegate(RadarTarget rt) {
			Target ret = new Target();
			ret.EntityId = rt.id;
			ret.Position = rt.position;
			ret.Velocity = rt.velocity;
			ret.isFCS = rt.isSelected;
			ret.Diameter = 1;
			ret.Acceleration = Vector3D.Zero;
			ret.TimeStamp = t;
			return ret;
		}).ToList());
	}
	// Target FCS_T = new Target();
	// FCS_T.GetTarget(GridTerminalSystem.GetBlockWithName(FCSComputerNameTag) as IMyProgrammableBlock);
	// if(FCS_T.EntityId != 0){
	// 	debugInfo += "\nGet fcs_t"; 
        //                 FCS_T.isFCS=true;
	// 	TargetList.Add(FCS_T);
	// }
	//获取自动武器目标
	bool hav = false;
	for(int i = 0; i < AutoWeapons.Count; i ++){
		Target ATWP_T = new Target();
		ATWP_T.GetTarget(AutoWeapons[i]);
                        if (ATWP_T.EntityId == 0) continue;
                        hav = false;
                        foreach (var t in TargetList) {
                            if (t.EntityId == ATWP_T.EntityId) hav = true;
                        }
		if(!hav){
			TargetList.Add(ATWP_T);
		}
	}
	LastTargetList = TargetList;
    debugInfo += "\nTargetCount: "+TargetList.Count;

	//指令每个炮台选择最近的目标
	bool controlByHead = Head!=null && Head.IsUnderControl;
	if (controlByHead) msc = Head;
	else msc = Cockpit;

	int mode = 0; // no aero
	if (isAeroDynamic && msc != null) {
	var MeVelocity = msc.GetShipVelocities().LinearVelocity;
	MatrixD refWorldMatrix = msc.WorldMatrix;
	MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(0,0,0), refWorldMatrix.Forward, refWorldMatrix.Up);
	Vector3D mySpeedToMe = Vector3D.TransformNormal(MeVelocity, refLookAtMatrix);
	var naturalGravity = msc.GetNaturalGravity();
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
		R.debugInfo = "";
		if(t%frameInterval==R.refreshFrame)R.debugInfoInter="";
			R.UpdateMotionInfo();//更新运动信息
			R.CheckPlayerControl();//检测玩家控制
			if(TargetList.Count > 0){
				R.AttackCloestTarget(TargetList, mode);
			}
			else{
				R.Attention(AttentionMode, mode);//归位
			}
			R.ShowLCD();//LCD显示状态信息
		}
	}else{
		foreach(RotorBase R in FCSR){
					R.debugInfo = "";
					if (t % frameInterval == R.refreshFrame) R.debugInfoInter = "";
					if (isOnOff) R.Attention(1, mode);
                                    else R.Attention(1, 99);
		}
	}

	// axis cannon
	axisCannon();
	
            Echo("initInfo "+ initInfo);
            Echo("debugInfo "+ debugInfo);
	ShowMainLCD();
}

private void axisCannon()
{
	string output = "";
	while(true) { 
		// check target
		var tarList = TargetList.Where(x=>x.isFCS);
		if (!tarList.Any()) break;
		var tar = tarList.First();
		
		Vector3D HitPoint = HitPointCaculate(msc.GetPosition(), msc.GetShipVelocities().LinearVelocity, Vector3D.Zero, tar.Position + msc.WorldMatrix.Up * axisYOffset, tar.Velocity, Vector3D.Zero, axisBs, 0, axisBs, (float)axisGr, msc.GetNaturalGravity(), axisBr, axisCr);
        Vector3D tarN = Vector3D.Normalize(HitPoint - msc.GetPosition());
		tarN = Vector3D.Transform(tarN, MatrixD.CreateLookAt(Vector3D.Zero, msc.WorldMatrix.Forward, msc.WorldMatrix.Up));
		output = tarN.X + ":" + tarN.Y + ":" + tarN.Z;
		break;
	}


    while (true) { 
		if (axisGun == null) break;
		string detail = axisGun.DetailedInfo;
		var lines = detail.Split('\n').Where(l => l.Contains("recharged"));
		if (!lines.Any()) break;
		var line = lines.First();
		var sec = line.Split(':')[1].Trim().Split(' ')[0].Trim();
		int secInt;
		int.TryParse(sec, out secInt);
		output += "\n" + secInt;
		break;
	}

	if (Me is IMyTextSurfaceProvider) { 
		var panel = ((IMyTextSurfaceProvider)Me).GetSurface(0);
		panel.WriteText(output);
	}

}

private bool checkRadarTarget(out Dictionary<long, RadarTarget> radarTargets)
{
    radarTargets = new Dictionary<long, RadarTarget>();
    if (radarSurface == null) return false;
    string text = radarSurface.GetText();
    if (text == null || text.Length == 0) return false;
    string[] lines = text.Split('\n');
    foreach (var l in lines)
    {
        if (l == null || l.Length == 0) continue;
        string[] fields = l.Split(':');
        if (fields.Count() < 9) continue;
        RadarTarget radarTarget = new RadarTarget();
        double x, y, z, vx, vy, vz;
        bool allRead = true;
        allRead &= long.TryParse(fields[0], out radarTarget.id);
        if (fields[1].Equals("Y")) radarTarget.isSelected = true;
        else if (fields[1].Equals("N")) radarTarget.isSelected = false;
        else allRead = false;
        if (fields[2].Equals("Y")) radarTarget.isHighThreaten = true;
        else if (fields[2].Equals("N")) radarTarget.isHighThreaten = false;
        else allRead = false;
        //if(!radarTargets.Any())radarTarget.isHighThreaten = true; // debugMode2
        allRead &= double.TryParse(fields[3], out x);
        allRead &= double.TryParse(fields[4], out y);
        allRead &= double.TryParse(fields[5], out z);
        allRead &= double.TryParse(fields[6], out vx);
        allRead &= double.TryParse(fields[7], out vy);
        allRead &= double.TryParse(fields[8], out vz);
        if (!allRead) continue;
        radarTarget.position = new Vector3D(x, y, z);
        radarTarget.velocity = new Vector3D(vx, vy, vz);
        radarTargets.Add(radarTarget.id, radarTarget);
    }
    return true;
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

	if (usingTurretAsSrc) GridTerminalSystem.GetBlocksOfType(AutoWeapons, b => true);
	
	List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
	GridTerminalSystem.GetBlockGroups(groups);
	
	List<RotorBase> FCSR_temp = new List<RotorBase>();
	foreach(IMyBlockGroup grou in groups){
		FCSR_temp.Add(new RotorBase(grou, this));
	}
	
	for(int i = 0; i < FCSR_temp.Count; i++){
		if(FCSR_temp[i].ThisRotorBaseRight){
			FCSR.Add(FCSR_temp[i]);
		}
	}
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl> (tmpBlocks);
            if (tmpBlocks.Count>0) remoteBlock = (IMyRemoteControl)tmpBlocks[0];


  fcsComputer = GridTerminalSystem.GetBlockWithName(FCSComputerNameTag) as IMyProgrammableBlock;
    List<IMyProgrammableBlock> tmpProgBlockList = new List<IMyProgrammableBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(tmpProgBlockList, b => b.CustomName.Contains(RADAR_COMPUTER_NAME));
    if (tmpProgBlockList.Count > 0)
    {
        radarSurface = ((IMyTextSurfaceProvider)tmpProgBlockList[0]).GetSurface(0);
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

	string nameAxisGun = "Main";
	List<IMyUserControllableGun> tmpGuns = new List<IMyUserControllableGun>();
	GridTerminalSystem.GetBlocksOfType(tmpGuns, b => b.CustomName.Contains(nameAxisGun) );
	if (tmpGuns.Count > 0) axisGun = tmpGuns[0];

	meCfg = new CustomConfiguration(Me);
	meCfg.Load();
	double.TryParse(meCfg.Get("MAIN_CANNON_BS", axisBs + ""), out axisBs);
	double.TryParse(meCfg.Get("MAIN_CANNON_BR", axisBr + ""), out axisBr);
	double.TryParse(meCfg.Get("MAIN_CANNON_GR", axisGr + ""), out axisGr);
	double.TryParse(meCfg.Get("MAIN_CANNON_YOFF", axisYOffset + ""), out axisYOffset);
    double.TryParse(meCfg.Get("MAIN_CANNON_CR", axisCr + ""), out axisCr);
	meCfg.Set("MAIN_CANNON_BS", axisBs+"");
	meCfg.Set("MAIN_CANNON_BR", axisBr+"");
	meCfg.Set("MAIN_CANNON_GR", axisGr+"");
	meCfg.Set("MAIN_CANNON_YOFF", axisYOffset+ "");
	meCfg.Set("MAIN_CANNON_CR", axisCr+ "");
	meCfg.Save();
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
            public bool isFCS = false;
	public string type = "Auto";
	
	// --------- 初始化方法 -------
	public Target()
	{
		this.Name = "";
		this.EntityId = 0;
	}
	
	// -------- 通过不同方式获取目标 -------
	
	public void GetTarget(IMyLargeTurretBase autoWeapon)
	{
		MyDetectedEntityInfo thisEntity = autoWeapon.GetTargetedEntity();
		if(!thisEntity.IsEmpty()){
			// Target lt = new Target();
			// foreach ( Target ti in LastTargetList ) {
			// 	if (ti.EntityId == thisEntity.EntityId) {
			// 		lt = ti;
			// 		break;
			// 	}
			// }
			// coding
			this.Name = thisEntity.Name;
			this.EntityId = thisEntity.EntityId;
			this.Diameter = Vector3D.Distance(thisEntity.BoundingBox.Max, thisEntity.BoundingBox.Min);
                                    this.Position = thisEntity.Position;
			//Vector3D.TryParse(thisEntity.Position.ToString(), out this.Position);
			//if (t == lt.TimeStamp) {
			//	this.Velocity = lt.Velocity;
			//} else {
			//	this.Velocity = (this.Position - lt.Position) / ((t - lt.TimeStamp)/60D);
			//}
			// Vector3D.TryParse(thisEntity.Velocity.ToString(), out this.Velocity);
                                    this.Velocity = thisEntity.Velocity;
			this.TimeStamp = t;
		}
	}
                string displayVector3D(Vector3D tar) {
                       return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
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
	public MyGridProgram program;
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
	public IMyShipController controller;
	
	public bool ThisRotorBaseRight;
	public string debugInfo;
	public string debugInfoInter;
	public bool isRocket = true;
	public CustomConfiguration cfg;
	public float hori; // 禁止开火旋转指向1 角度（左右） - 90
	public float horiD;// 禁止开火旋转范围1 角度 - 45
	public float hori2;// 禁止开火旋转指向2 角度（后） - 180
	public float horiD2;// 禁止开火旋转范围2 角度 - 45
	public float vert; // 禁止开火俯仰的指向1 角度 (左右) - 0
	public float vertD; // 禁止开火俯仰下限1 角度 - -30
	public float vert2; // 禁止开火俯仰的指向2 角度（后） - 0
	public float vertD2; // 禁止开火的俯仰下限2 角度 - 30
	public float offX = 0; // 关机时炮台旋转指向
	public float offY = 1F; // 关机时炮台俯仰指向
	public float onX = 0; // 开机时炮台旋转默认
	public float onY = 0; // 开机时炮台俯仰默认
	public float ra=0; // 优先攻击角度 （基于船）
	public float raD=-1F; // 不瞄准下限，比这个还低的相似度就不瞄准了，回待机位置
	public double bulletMaxSpeedConf = 0;
	public double bulletMaxRange = 900;
	public float gravityRate = 0;
	public float eaRate = 1F;
	public double curvationRate = 0.2;
	public bool isAutoFire = true;
	public double PID_P = Aim_PID_P;
	public double PID_I = Aim_PID_I;
	public double PID_D = Aim_PID_D;
	public string FACE_TO = "Forward";
	public string TARGET_TYPE = "All";
	public double OFFSET_Y = 0;
	public double RANDOM_Y = 0;
	public long lastFireFrame = 0;
	public long fireIntervalFrame = 1;
	public int fireIntervalIdx = 0;
	
	static float pp=20F,pi=1F,pd=0F, pim=0.01F;
	public List<PIDController> pidXL = new List<PIDController>();
	public List<PIDController> pidYL = new List<PIDController>();
	public Vector2 AimOffset = Vector2.Zero;
    private long lastEntityId = 0;
    private IMyProjector aimSightBlock = null;
	public bool lastSearchStable = false;
	public Vector3D searchStableDir = Vector3D.Zero;
	public String aimUp = "Up";

	public int refreshFrame = new Random().Next(frameInterval);
	public bool isAimOk = false;

	public Vector3D getDirAim(String need) { 
		return getDir(this.AimBlock, need, this.aimUp);
	}
	public Vector3D getDir(IMyTerminalBlock b, String need, String refUp) { 
		int clockWiseStep = toDirStep(refUp);
		int needStep = toDirStep(need);
		int resultStep = (clockWiseStep + needStep) % 4;
		var m = b.WorldMatrix;
		switch(resultStep) { 
		case 0:
			return m.Up;
		case 1:
			return m.Right;
		case 2:
			return m.Down;
		case 3:
		    return m.Left;
		}
		return m.Up;
	}
	public int toDirStep(String dir) { 
		switch(dir) { 
		case "Left":
			return 3;
			break;
		case "Right":
			return 1;
			break;
		case "Down":
			return 2;
			break;
		}
		return 0;
	}
	
	// -------- 初始化方法 ---------
	public RotorBase(IMyBlockGroup thisgroup, MyGridProgram program)
	{
		this.program = program;
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
		vert = toRa(vert);
		cfg.Get("vertD", ref vertD);
		vertD = toRa(vertD);
		cfg.Get("vert2", ref vert2);
		vert2 = toRa(vert2);
		cfg.Get("vertD2", ref vertD2);
		vertD2 = toRa(vertD2);
		cfg.Get("hori", ref hori);
		hori = toRa(hori);
		cfg.Get("horiD", ref horiD);
		horiD = toRa(horiD);
		cfg.Get("hori2", ref hori2);
		hori2 = toRa(hori2);
		cfg.Get("horiD2", ref horiD2);
		horiD2 = toRa(horiD2);
		cfg.Get("offX", ref offX);
		offX = toRa(offX);
		cfg.Get("offY", ref offY);
		offY = toRa(offY);
		cfg.Get("onX", ref onX);
		onX = toRa(onX);
		cfg.Get("onY", ref onY);
		onY = toRa(onY);
		cfg.Get("ra", ref ra);
		ra = toRa(ra);
		cfg.Get("raD", ref raD);

		cfg.Get("bulletMaxSpeedConf", ref bulletMaxSpeedConf);
		cfg.Get("bulletMaxRange", ref bulletMaxRange);
		cfg.Get("gravityRate", ref gravityRate);
		cfg.Get("curvationRate", ref curvationRate);
		cfg.Get("isAutoFire", ref isAutoFire);
		cfg.Get("PID_P", ref PID_P);
		cfg.Get("PID_I", ref PID_I);
		cfg.Get("PID_D", ref PID_D);
		cfg.Get("FACE_TO", ref FACE_TO);
		cfg.Get("TARGET_TYPE", ref TARGET_TYPE);
		cfg.Get("OFFSET_Y", ref OFFSET_Y);
		cfg.Get("RANDOM_Y", ref RANDOM_Y);
		cfg.Get("EA_RATE", ref eaRate);
		cfg.Get("FIRE_INTERVAL", ref fireIntervalFrame);
		cfg.Get("AIM_UP", ref aimUp);

		//获得转子
		bool haveHinge = false;
		foreach(IMyTerminalBlock b in blocks_temp) {
			if (b.CustomName.Contains("Hinge")) {
				haveHinge = true;
			}
		}
		if (haveHinge) {
			foreach(IMyTerminalBlock block in blocks_temp) {
				int NagtiveRotor = 1;
				if (!(block is IMyMotorStator)) continue;
				if (block.CustomName.Contains("Hinge")) {
					var dot = Vector3D.Dot(getDirAim("Left"), block.WorldMatrix.Up);
					if (Math.Abs(dot) > 0.9) {
						RotorYs.Add(block as IMyMotorStator);
						if (dot > 0)
						RotorYField.Add(1*NagtiveRotor);
						else 
						RotorYField.Add(-1*NagtiveRotor);
						//debugInfo = block.CustomName;
					} else {
						RotorXs.Add(block as IMyMotorStator);
						RotorXField.Add(1*NagtiveRotor);
					}
				} else {
					RotorXs.Add(block as IMyMotorStator);
					if (Vector3D.Dot(getDirAim("Up"), block.WorldMatrix.Up)>0)
						RotorXField.Add(1*NagtiveRotor);
					else RotorXField.Add(-1*NagtiveRotor);
				}
			}
		} else {
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block is IMyMotorStator){
				int NagtiveRotor = 1;
				if(block.CustomName.Contains(RotorNagtiveTag)){NagtiveRotor = -1;}
				var dot = Vector3D.Dot(getDirAim("Left"), block.WorldMatrix.Up);
				if (Math.Abs(dot)>0.9) {
					RotorYs.Add(block as IMyMotorStator);
					if (dot > 0) RotorYField.Add(1*NagtiveRotor);
					else RotorYField.Add(-1*NagtiveRotor);
				} else {
					RotorXs.Add(block as IMyMotorStator);
					if (Vector3D.Dot(getDirAim("Up"), block.WorldMatrix.Up)>0)
						RotorXField.Add(1*NagtiveRotor);
					else RotorXField.Add(-1*NagtiveRotor);
				}
			}
		}
		if (RotorYs.Count > 0) {
		foreach(IMyTerminalBlock block in blocks_temp){
			if (!(block is IMyMotorStator)) continue;
				int NagtiveRotor = 1;
				if(block.CustomName.Contains(RotorNagtiveTag)){NagtiveRotor = -1;}
				var dot = Vector3D.Dot(getDirAim("Left"), block.WorldMatrix.Up);
				if (Math.Abs(dot)>0.9) continue;
				var dot2 = Vector3D.Dot(getDirAim("Up"), block.WorldMatrix.Up);
				RotorXs.Add(block as IMyMotorStator);
				if (dot2 >0  || block.CustomName.Contains("[Arm]")) 
					RotorXField.Add(1*NagtiveRotor);
				else RotorXField.Add(-1*NagtiveRotor);
		}
		}
		}
		if(RotorXs.Count < 1 || RotorYs.Count < 1) {ErrorReport = "Rotors Not Complete!"; return;}

		for(int i = 0; i < RotorXs.Count; i++) {
		    pidXL.Add(new PIDController(PID_P, PID_I, PID_D, pim,-pim,60/frameInterval));
		}
		for(int i = 0; i < RotorYs.Count; i++) {
		    pidYL.Add(new PIDController(PID_P, PID_I, PID_D, pim,-pim,60/frameInterval));
		}

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
			else if(block is IMyTextPanel){
				LCDs.Add(block as IMyTextPanel);
			}
			else if (block is IMyProjector) { 
				this.aimSightBlock = (IMyProjector)block; // only one
			}
			else if (block is IMyShipController) { 
				this.controller = (IMyShipController) block;
			}
		}
	}

	// ------- 更新运动信息 ---------
	public void UpdateMotionInfo()
	{
		this.Acceleration = (((this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio) - this.Velocity)*ProgramUpdateRatio;
		if (msc != null) this.Velocity = msc.GetShipVelocities().LinearVelocity;
		else this.Velocity = (this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio;

		this.Position = this.AimBlock.GetPosition();
	}
	
	// ------- 待命归位 --------
	public void Attention(int ATMode, int aeroMode)
	{
		if (this.Weapons.Count == 0 && this.controller != null && this.controller.IsUnderControl) {
			this.AimAtTarget(this.AimBlock.GetPosition() + searchStableDir * 100000);
		    return;
		}
		if(!isNoManual && this.controller != null && this.controller.IsUnderControl) {
			this.AimAtTarget(this.AimBlock.GetPosition() + searchStableDir * 100000);
			return;
		}
		if (this.Weapons.Count == 0 && this.controller != null && remoteBlock!=null && remoteBlock.GetNearestPlayer(out piratePosition)) {
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle - ((t % 300) * MathHelper.TwoPi/300);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			             var a = this.RotorYs[i].Angle;
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(-a,2);
			}
			return;
		}
		// Vector3D aimPoint = new Vector3D();
		// MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.RotorXs[0].WorldMatrix.Forward, this.RotorXs[0].WorldMatrix.Up);
		float xt = 0F, yt=0F;
		//this.debugInfo+="attention mode:"+aeroMode+"\n";
		switch(aeroMode) {
		case 0:
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle - (float)(onX);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			             var a = this.RotorYs[i].Angle - (float)(onY);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(-a,2);
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
			            var a = this.RotorXs[i].Angle - xt - (float)(onX);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			            var a = this.RotorYs[i].Angle - yt - (float)(onY);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(-a,2);
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
			            var a = this.RotorXs[i].Angle - xt - (float)(onX);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
			            var a = this.RotorYs[i].Angle - yt - (float)(onY);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(-a,2);
			}

		break;
                        default:
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle - (float)(offX);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(-a,2);
			}
			for(int i = 0; i < this.RotorYs.Count; i ++){
				var a = this.RotorYs[i].Angle - (float)(offY);
				if (a > Math.PI) a = a - MathHelper.TwoPi;
				if (a < -Math.PI) a = a + MathHelper.TwoPi;
				this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(-a,2);
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
        if (this.Weapons.Count == 0 ) { 
			if(TargetList.Any(x=>x.isFCS)) {
				if(this.aimSightBlock!=null)this.aimSightBlock.Enabled=true;
				return;
			} else { 
				if(this.aimSightBlock!=null)this.aimSightBlock.Enabled=false;
			}
		} // 非正常炮台（即瞄准台），在有目标的情况下不接受一般的玩家指示 FEATURE 0220
		if (this.controller!=null && lastSearchStable != this.controller.IsUnderControl) { 
			if(this.controller.IsUnderControl) { 
				// reset searchStableDir
				searchStableDir = this.AimBlock.WorldMatrix.Forward;
			} else { 
			}
			lastSearchStable = this.controller.IsUnderControl;
		}
		if(this.controller != null && this.controller.IsUnderControl){
			Vector2 MouseInput = this.controller.RotationIndicator;
			if(MouseInput.Length() > 0.1) {
				if(isNoManual) { 
                    searchStableDir = this.AimBlock.WorldMatrix.Forward;
				}
			    isNoManual = false;
			}
			if(!isNoManual && MouseInput.Length()!=0) { 
				Vector3D need = getDirAim("Right") * MouseInput.X + getDirAim("Up") * MouseInput.Y;
				var lM = MatrixD.CreateFromDir(this.AimBlock.WorldMatrix.Forward, Vector3D.Normalize(need));
				var axis = lM.Down; //Right wrong
				float ROTATE_RATIO = 0.001F;
				searchStableDir = Vector3D.Transform(searchStableDir, Quaternion.CreateFromAxisAngle(axis, (float)MouseInput.Length()*ROTATE_RATIO));
			//for(int i = 0; i < this.RotorXs.Count; i ++){
			//	this.RotorXs[i].TargetVelocityRPM = (float)(MouseInput.Y * RotorXField[i] * PlayerAimRatio);
			//}
			//for(int i = 0; i < this.RotorYs.Count; i ++){
			//	this.RotorYs[i].TargetVelocityRPM = -(float)(MouseInput.X * RotorYField[i] * PlayerAimRatio);
			//}
			}
		}
	}

	public double angleDeltaAbs (double a, double b) {
	       if (a > Math.PI) {
	       	  a  -= MathHelper.TwoPi;
	       }
	       if (a < -Math.PI) {
	       	  a += MathHelper.TwoPi;
	       }
	       var t = Math.Abs(a - b);
                   if (t > Math.PI) t = Math.Abs(t - MathHelper.TwoPi);
	       return t;
	}
	
	// ------- 攻击并搜索最近的目标 -------
	public void AttackCloestTarget(List<Target> targetList, int mode)
	{
                if (this.Weapons.Count == 0 && this.controller != null && this.FireTimers.Count == 0) {
                   Target FCS_T = null;
                   foreach(var t in targetList) {
                       if (t.isFCS) FCS_T = t;
                   }
                   if (FCS_T == null) {
				      if (!isNoManual && this.controller != null && this.controller.IsUnderControl) { 
						this.AimAtTarget(this.AimBlock.GetPosition() + searchStableDir * 100000);
					  } else {
							//this.debugInfo += "call att 3\n";
							this.Attention(1,0);
					  }
                      return;
                   }
                   //debugInfo += "\n"+FCS_T.Position;
				   // FEATURE 0220
				   if (FCS_T.EntityId != this.lastEntityId) this.AimOffset = Vector2.Zero;
				   this.lastEntityId = FCS_T.EntityId;
				   Vector2 MouseInput = this.controller.RotationIndicator;
					float ControlRatio = 0.01F;
					this.AimOffset += MouseInput * ControlRatio;
                   this.AimAtTarget(FCS_T.Position, FCS_T.Velocity, Vector3D.Zero);
                   return;
                }
		if(!isNoManual && this.controller!=null && this.controller.IsUnderControl) { 
			this.AimAtTarget(this.AimBlock.GetPosition() + searchStableDir * 100000);
			return;
		}
		double currentDis = double.MaxValue;
		Target MyAttackTarget = new Target();
				if (t % frameInterval == this.refreshFrame)
				{
					this.debugInfoInter += "\ntlc:" + targetList.Count;
					for (int i = 0; i < targetList.Count; i++)
					{
						if (ignoreList.Contains(targetList[i].EntityId)) continue;
						bool isIgnorePos = false;
						foreach (var ig in ignorePosList)
						{
							if ((ig - targetList[i].Position).Length() < ignoreDistance) isIgnorePos = true;
						}
						if (isIgnorePos) continue;
						if (this.TARGET_TYPE == "Focus")
						{
							if (!targetList[i].isFCS && targetList[i].type != "Focus") continue;
						}
						double distance = Vector3D.Distance(targetList[i].Position, this.Position);
						if (distance > bulletMaxRange * 1.1) continue;
						this.debugInfoInter += "\ndistanceOK";
						double dot = 1;
						// a b k filter direction use ra and raD
						if (msc != null && raD != -1)
						{
							//this.debugInfo = "\nra: " + this.ra;
							Vector3D mid = msc.WorldMatrix.Right * Math.Sin(this.ra) + msc.WorldMatrix.Forward * Math.Cos(this.ra);
							Vector3D tar = Vector3D.Normalize(targetList[i].Position - this.Position);
							//this.debugInfo += "\n dirYN: " + (Vector3D.Dot(mid, tar));
							dot = Vector3D.Dot(mid, tar);
							if (dot < this.raD) continue;
						}
						else
						{
							//this.debugInfo = "\n\n";
						}
						double compose = distance / 900 + (1 - dot);
						if (targetList[i].EntityId != 0 && compose <= currentDis)
						{
							if (OnlyAttackUpPlane)
							{
								MatrixD RotortXLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.RotorXs[0].WorldMatrix.Forward, this.RotorXs[0].WorldMatrix.Up);
								Vector3D TargetPositionToRotorX = Vector3D.TransformNormal(targetList[i].Position - this.RotorXs[0].GetPosition(), RotortXLookAtMatrix);
								if (TargetPositionToRotorX.Y >= 0)
								{
									MyAttackTarget = targetList[i];
									currentDis = compose;
								}
							}
							else
							{
								MyAttackTarget = targetList[i];
								currentDis = compose;
							}
						}
					}
					if(MyAttackTarget.EntityId == 0)
                    {
						this.debugInfoInter += "\nno target";
                    }
					this.MyTarget = MyAttackTarget;
				}
				if (this.MyTarget != null && this.MyTarget.EntityId != 0)
				{
					if (t % frameInterval == this.refreshFrame)
						this.isAimOk = this.AimAtTarget(MyAttackTarget, false);
					bool sensorActive = false;
					foreach (IMySensorBlock sensor in this.Sensors)
					{
						if (sensor.IsActive) { sensorActive = true; }
					}
					//debugInfo += "\nsa:" + sensorActive;
					MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.AimBlock.WorldMatrix.Forward, this.AimBlock.WorldMatrix.Up);
					Vector3D TargetPositionToMe = Vector3D.TransformNormal(MyAttackTarget.Position - this.Position, refLookAtMatrix);
					//存在自定义开火定时块就执行自定义开火
					if (FireTimers.Count() > 0)
					{
						foreach (IMyTerminalBlock fire_timer in FireTimers)
						{
							int fire_distance = (int)ShootDistance3;//CODING
																	//if(Vector3D.Distance(MyAttackTarget.Position, this.Position) <= fire_distance && !sensorActive && this.isAimOk){
																	//fire_timer.ApplyAction("TriggerNow");
																	//}
						}
					}
					//debugInfo += $"\nin range: {Vector3D.Distance(MyAttackTarget.Position, this.Position)} {sensorActive} {isAimedOK}";
					if (Vector3D.Distance(MyAttackTarget.Position, this.Position) <= this.bulletMaxRange && !sensorActive)
					{
						if (isFireWhenAimOK)
						{
							if (this.isAimOk)
							{
								this.Fire();
							}
						}
						else
						{
							this.Fire();
						}
					}
				}
				else
				{
					this.debugInfo += "\nattention";
					this.Attention(AttentionMode, mode);
				}
		
	}

string displayVector3D(Vector3D tar) {
return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
}

	// ---------- 瞄准目标 -----------
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
		//if (this.Weapons.Count == 0) return false;
		bool isRocket = this.isRocket;
		MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.AimBlock.WorldMatrix.Forward, this.AimBlock.WorldMatrix.Up);
		Vector3D thisV;
		Vector3D thisA;
		double bs, ba, bm;
		if (FireTimers.Count > 0) {
			thisV = this.Velocity;
			thisA = this.Acceleration;
			bs = BulletInitialSpeed3;
			ba = BulletAcceleration3;
			bm = BulletMaxSpeed3;
		} else if (bulletMaxSpeedConf != 0) {
			thisV = this.Velocity;
			thisA = this.Acceleration;
			bs = bulletMaxSpeedConf;
			ba = 0;
			bm = bulletMaxSpeedConf;
		} else if (isRocket == true) {
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
		Vector3D ng = Vector3D.Zero;
		if (msc != null) ng = msc.GetNaturalGravity();
		//debugInfo += "\nbs: " + bs;
		Vector3D HitPoint = HitPointCaculate(this.Position, thisV, thisA, Position + msc.WorldMatrix.Up * (OFFSET_Y + RANDOM_Y * 0.001 * R_D.Next(-1000, 1000)), Velocity, Acceleration, bs, ba, bm, FireTimers.Count > 0 ? 1F : gravityRate, ng, bulletMaxRange, curvationRate);
                        Vector3D tp2me = Position - this.Position;
		var tp = HitPoint - this.Position;
		Vector3D TargetPositionToMe = new Vector3D(0,0,-1);
		if (HitPoint != Vector3D.Zero) {
		TargetPositionToMe = Vector3D.Normalize(Vector3D.TransformNormal(tp, refLookAtMatrix));
		}
//		Vector3D aimDir = CalcAim(this.Position, thisV, Position, Velocity, bs, ba, bm);
//		var aimDirToMe = Vector3D.TransformNormal(aimDir,  refLookAtMatrix);
		if (FireTimers.Count>0) {
			// ABK piston gun aim angle correction
			// https://www.andre-gaschler.com/rotationconverter/
			TargetPositionToMe = Vector3D.Transform(TargetPositionToMe, new Quaternion((float)Math.Sin(toRa(xdegree * 0.5F)), 0, 0, (float)Math.Cos(toRa(xdegree * 0.5F))));
		}

		// FEATURE 0220
		if (this.AimOffset.Length() > 0) { 
		    // 先x再y?
			TargetPositionToMe = Vector3D.Transform(TargetPositionToMe, new Quaternion(0, (float)Math.Sin(toRa(-this.AimOffset.Y * 0.5F)), 0, (float)Math.Cos(toRa(-this.AimOffset.Y * 0.5F))));
			TargetPositionToMe = Vector3D.Transform(TargetPositionToMe, new Quaternion((float)Math.Sin(toRa(-this.AimOffset.X * 0.5F)), 0, 0, (float)Math.Cos(toRa(-this.AimOffset.X * 0.5F))));
        }

		
		//计算输出
		var targetPositionToReal = Vector3D.TransformNormal(TargetPositionToMe, this.AimBlock.WorldMatrix);//
		var faceDir = RotorXs[0].WorldMatrix.Forward;
		if (FACE_TO == "Right") faceDir = RotorXs[0].WorldMatrix.Right;
		else if (FACE_TO == "Left") faceDir = RotorXs[0].WorldMatrix.Left;
		else if (FACE_TO == "Backward") faceDir = RotorXs[0].WorldMatrix.Backward;
		if (RotorXs[0].CustomName.Contains("Hinge")) faceDir = RotorXs[0].WorldMatrix.Left;
		var upDir = RotorXs[0].WorldMatrix.Up;
		if (RotorXField[0] < 0) upDir = RotorXs[0].WorldMatrix.Down;
		var rcLookAt = MatrixD.CreateLookAt(Vector3D.Zero, faceDir, upDir);
		var tpToRc = Vector3D.TransformNormal(targetPositionToReal, rcLookAt);
		var tvToRcNml = Vector3D.TransformNormal(Velocity - this.Velocity, rcLookAt) / tp.Length();
		double aa=0, ea=0;
		Vector3D.GetAzimuthAndElevation(tpToRc, out aa, out ea);
		ea*=eaRate;
		this.debugInfoInter += $"\n{this.Name}";
		//debugInfo += "\naaea: " + Math.Round(aa,2) + " " + Math.Round(ea,2);
		if (this.AimBlock.CustomName.Contains("[Arm]")) ea += Math.PI * 0.5;

			bool rx = false;
			bool ry = false;
			rx = angleDeltaAbs(aa, -hori) < horiD; // aa left +, right -
			rx = rx || angleDeltaAbs(aa + Math.PI, -hori) < horiD;
			bool rx2 = angleDeltaAbs(aa, -hori2) < horiD2;
                                    bool ry2 = ea-vert2 < -vertD2;
			ry = ea-vert < -vertD;
			//debugInfo += "\nrxry: " + rx + " " + ry;
			//debugInfo += "\nrx2ryx: " + rx2 + " " + ry2;
		bool isFireZone = !(rx && ry) && !(rx2 && ry2);

		double fireRange = ShootDistance;
		if (isRocket) fireRange = ShootDistance2;
		if(FireTimers.Count>0) fireRange = ShootDistance3;
		if (bulletMaxRange != 0) fireRange = bulletMaxRange;
                        isFireZone = isFireZone && tp2me.Length() < fireRange;
                        if (isStraight) isFireZone = true;
		if (!isFireZone && isHoming) {
		aa = (float)onX * this.RotorXField[0];
		ea = (float)onY * this.RotorYField[0];
		}
		//debugInfo+= "\nazineed : " + aa;

		for(int i = 0; i < this.RotorXs.Count; i ++){
			var a = (float)(-aa*this.RotorXField[i]) - this.RotorXs[i].Angle;
			if (a > Math.PI) a = a - MathHelper.TwoPi;
			if (a < -Math.PI) a = a + MathHelper.TwoPi;
			this.debugInfoInter+=$"\nazierror:{a}";
			//debugInfo+=$"\nazir: {(float)tvToRcNml.X * tvnToRpm}";
			this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(a,2) + (float)tvToRcNml.X * tvnToRpm * 0.225F;
			this.debugInfoInter += "\n" + this.RotorXs[i].TargetVelocityRPM;
		}
			for(int i = 0; i < this.RotorYs.Count; i ++){
				//this.RotorYs[i].TargetVelocityRPM = (float)(PitchValue * this.RotorYField[i] * RotorMaxSpeed);
			var a = (float)(ea*this.RotorYField[i]) - this.RotorYs[i].Angle;
			if (a > Math.PI) a = a - MathHelper.TwoPi;
			if (a < -Math.PI) a = a + MathHelper.TwoPi;
			// (float)tvToRcNml.Y*tvnToRpm
			this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(a,2) + (float)tvToRcNml.Y * tvnToRpm * 0.225F;
			}
		
		// 计算当前与预期瞄准点的瞄准夹角
		if (HitPoint == Vector3D.Zero) return false;
		Vector3D V_A = HitPoint - this.Position;
		Vector3D V_B = this.AimBlock.WorldMatrix.Forward;
		// 更换算法，按1米半径计算误差，距离越大，要求精度越高
		var tarN = Vector3D.Normalize(V_A);
		var tarL = V_A.Length();
		double error = tarL / Math.Sqrt(4 + tarL * tarL);
		//double Angle = Math.Acos(Vector3D.Dot(V_A,V_B)/(V_A.Length() * V_B.Length())) * 180 / Math.PI;
		//if(Angle <= AimRatio && isFireZone) return true;
		//debugInfo += "\nisFireZone: " + isFireZone;
		isFireZone &= Math.Round(Vector3D.Dot(tarN, V_B),2) >= Math.Round(error,2);
		//debugInfo += $"\naimerror: {Math.Round(error,2)} {Math.Round(Vector3D.Dot(tarN, V_B),2)} {isFireZone}";
		return isFireZone;
	}
	
	// ------ 开火 ---------
	public void Fire()
	{
		foreach(var w in this.Weapons) { 
			debugInfo += "\n" + w.DetailedInfo;
		}
		if (!isAutoFire) return;
		if (t < lastFireFrame + fireIntervalFrame) return;
		lastFireFrame = t;
		if (this.Weapons.Count == 0) return;

		fireIntervalIdx = (fireIntervalIdx+1) % this.Weapons.Count;
		this.Weapons[fireIntervalIdx].ApplyAction(ShootActionName);
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
							double Bullet_InitialSpeed, double Bullet_Acceleration, double Bullet_MaxSpeed,
float gravityRate, Vector3D ng, double bulletMaxRange, double curvationRate)   
{
	string debugString = "";
	//GravityHitPointCaculate(new Vector3D(1, 1, 0), new Vector3D(0,0,-1), new Vector3D(0,-1,0), 3D, out debugString);
	//debugInfo += "\nghpc\n" + debugString + "\n";
	if (gravityRate > 0 && ng.Length() != 0) {
		var ret = GravityHitPointCaculate(Target_Position - Me_Position, Target_Velocity - Me_Velocity, ng * gravityRate, Bullet_InitialSpeed, bulletMaxRange, curvationRate, out debugString);
		if (ret == Vector3D.Zero) return Vector3D.Zero;
		ret += Me_Position;
		// debugInfo += "\nghpc\n" + debugString + "\n";
		return ret;
	}
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

public double Filter(double input, int round_d_digits, bool debug = false)
{
double roundedInput = Math.Round(input, round_d_digits);

integral = integral + (input / second);
integral = (upperLimit_i > 0 && integral > upperLimit_i ? upperLimit_i : integral);
integral = (lowerLimit_i < 0 && integral < lowerLimit_i ? lowerLimit_i : integral);

double derivative = (roundedInput - lastInput) * second;
lastInput = roundedInput;

if(debug) { 
debugInfo += $"\n{input}";
debugInfo += $"\n{integral}";
debugInfo += $"\n{derivative}";
}

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

static double modAngle (double a) {
var r = a;
if ( r > Math.PI) {
r = r - MathHelper.TwoPi;
}
if ( r < -Math.PI) {
r = r + MathHelper.TwoPi;
}
return r;
}


static float toRa(float i) {
return (i / 180F) * (float)Math.PI;
}

static string displayVector3D(Vector3D tar) {
return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
}

static Vector3D GravityHitPointCaculate(Vector3D tp, Vector3D tv, Vector3D g, double aV, double bulletMaxRange, double curvationRate, out string debugString) {
debugString = "";
// 问题5 怀疑星球曲率原因，对g采取近小远大处理
var gd = Vector3D.Normalize(g);
var ngtpr = Vector3D.Reject(tp, gd).Length();
g = g * (1 + ((ngtpr - bulletMaxRange*0.5)*2 / bulletMaxRange) * curvationRate);

/*
目标速度为  tvx, tvy, tvz. 目标位置  tpx, tpy, tpz.
重力加速度 gax, gay, gaz.
我方位置 mpx, mpy, mpz, 我方速度 mvx, mvy, mvz
炮弹速度标量 aV, 
假设炮弹速度为 avx, avy, avz
命中时间为 n

则有
tpx + tvx * n = mpx + mvx * n + avx * n +  0.5 * gax * n * n 方程1
tpy + tvy * n = mpy + mvy * n + avy * n + 0.5 * gay * n * n 方程2
tpz + tvz * n = mpz + mvz * n + avz * n + 0.5 * gaz * n * n 方程3
avx * avx + avy * avy + avz * avz = aV * aV 方程4

以重力方向向下, 面朝目标方向建立座标系, 则有 gax = gaz = 0 gay = -重力加速度
且 tpx = mpx = 0
由于接受参数时, 已改用本机位置和速度作为位置和速度基准, 所以还有mp = 0和mv=0

步骤1, 由方程1
则可以直接求出avx = tvx - mvx

由方程3
tpz - mpz = (avz + mvz - tvz) * n
可知 n大 则 avz 小, n小, 则avz大

由方程4
avz * avz + avy * avy = aV * aV  - avx * avx 
由于avx已知, 可知 avz 和 avy 形成一个圆形

假设gay = 0 先求一个n出来, 再把gay代入, 重新计算avy avz n 迭代多次逼近正确的n值

*/

if (tp == Vector3D.Zero) return Vector3D.Zero;
// 1 建立座标系
// 1.1 检查 tp方向 与 g 方向是否 完全同向/异向 算法无法处理这种情况 , 不攻击 (缺陷1)
var dot = Vector3D.Dot(Vector3D.Normalize(tp), gd);
if (dot == 1 || dot == -1) return Vector3D.Zero;
// 1.2 构建座标转换矩阵
var forward = Vector3D.Normalize(Vector3D.Reject(tp, gd));
var tranmt = MatrixD.CreateLookAt(new Vector3D(), forward, -gd);

// 1.3 转换座标
var tp2 = Vector3D.TransformNormal(tp, tranmt);
var tv2 = Vector3D.TransformNormal(tv, tranmt);
//debugString = displayVector3D(tp2);
//debugString += "\n" + displayVector3D(tv2);

// 2 求 avx
double avx = tv2.X;
if (Math.Abs(avx) > aV) return Vector3D.Zero; // 炮速赶不上tv2.X 无法追踪
//debugString += "\navx: " + avx;

// 3 无视g 先算一个avy avz
// 3.1 假设减掉tvYZ, Z轴剩余速度有avzd, 则Y轴需要的剩余速度为 (tpy/tpz)*avzd
// 有 (tvz+avzd)2 + (tvy + (tpy/tpz)avzd)2 = aVyz2
double aVyz = Math.Sqrt(aV*aV - avx*avx);
if (tv2.Z*tv2.Z + tv2.Y*tv2.Y > aVyz*aVyz) return Vector3D.Zero; // yz面目标速度大于炮弹速度 追不上, 不考虑利用重力加速度追 (缺陷2)
// tpz 不可能为0 因为能建座标系就表示有向前的分量, 即z方向分量)
// 采用一元二次方程 ax2 +bx + c = 0方式, 整理a , b, c
double fa = 1 + ((tp2.Y*tp2.Y) / (tp2.Z*tp2.Z));
double fb = 2 * tv2.Z + 2 * tv2.Y * (tp2.Y/tp2.Z);
double fc = tv2.Z * tv2.Z + tv2.Y * tv2.Y - aVyz * aVyz;
if (fb*fb - 4*fa*fc < 0) return Vector3D.Zero; // 无解, 返回
// 根据 一元二次方程求根公式, 计算x (即avzd)
double x = (-fb + Math.Sqrt(fb*fb - 4*fa*fc)) / (2*fa);
double avz = 0;
if (tv2.Z + x < 0) {
avz = tv2.Z + x;
} else {
x =  (-fb - Math.Sqrt(fb*fb - 4*fa*fc)) / (2*fa);
avz = tv2.Z + x;
}
double avy = tv2.Y + (tp2.Y/tp2.Z) * x;

// 3.2 根据z轴, 算追及时间 n
double zdelta = avz - tv2.Z;
double n = tp2.Z / zdelta;
if (n < 0) return Vector3D.Zero; // Z轴追及时间为负, 无法追踪
//debugString += "\naVyz: " + aVyz;
//debugString += "\navy: " + avy;
//debugString += "\navz: " + avz;
//debugString += "\nn: " + n;

// 4 循环逼近多次(这个方程应该有解, 但这里采取逼近法简化)(缺陷3) 加速度的问题 主要是计算avyg分量应该取多少
double avyg = 0;
double avyp = avy;
for (int i = 0; i < 4; i++) {
// 4.1 按当前n 计算avyg, 取avyg = 0.5 * (-g) * n ; 举例, g为 向下10, 时间1秒, 我们需要向上5, 这样前0.5秒为上升期, 后0.5秒为下降期, 上升距离=下降距离, 不影响瞄准
avyg = 0.5 * g.Length() * n;
// 4.2 由于时间n变长了, 需要重新计算avy
avyp = tv2.Y + (tp2.Y / n);
// 4.3 加上avyg
avyp = avyp + avyg;
if (Math.Abs(avyp) > aVyz) return Vector3D.Zero;
double avzL = Math.Sqrt(aVyz*aVyz - avyp*avyp);
avz = - avzL;
zdelta = avz - tv2.Z;
double nn = tp2.Z / zdelta;
if (nn > n) n = nn;
else n = (nn + n) /2;
if (n < 0) return Vector3D.Zero; // Z轴追及时间为负, 无法追踪

//debugString += "\navy: " + avyp;
//debugString += "\navz: " + avz;
//debugString += "\nn: " + n;

}

// 5 将avx avy avz 转回绝对座标系 并输出(注意本来这里应该输出碰撞点位置 , 但这里以发射速度代替, 所以还要加上本机位置, 调用者处理)
avyp *= 0.98;
Vector3D av2m = new Vector3D(avx, avyp, avz);
Vector3D av = Vector3.Transform(av2m, Matrix.Transpose(tranmt));
//debugString += "\nav: " + displayVector3D(av);
return av;
}

static IMyProgrammableBlock fcsComputer = null;
static List<Target> GetFcsTargetList() {
debugInfo+="\nstartfcs";
List<Target> ret = new List<Target>();
if (fcsComputer == null) return ret;
		if(fcsComputer.CustomData.Split('\n').Length >= 1){
CustomConfiguration cfgTarget = new CustomConfiguration(fcsComputer);
cfgTarget.Load();

string tmpS = "";
double tmpD = 0D;
long tmpL = 0L;

long mainTargetId = 0;
cfgTarget.Get("EntityId", ref tmpL);
mainTargetId = tmpL;

cfgTarget.Get("TargetCount", ref tmpL);

int targetCount = (int)tmpL;
debugInfo += "\ntarget count: " + targetCount;
for (int i = 0; i < targetCount; i++) {
Target nt = new Target();
cfgTarget.Get("EntityId" + i, ref tmpL);
nt.EntityId = tmpL;

if (nt.EntityId == mainTargetId) nt.isFCS = true;

if(nt.EntityId != 0){
cfgTarget.Get("Diameter"+i, ref tmpD);
nt.Diameter = tmpD;

cfgTarget.Get("Position"+i, ref tmpS);
Vector3D.TryParse(tmpS, out nt.Position);

cfgTarget.Get("Velocity"+i, ref tmpS);
Vector3D.TryParse(tmpS, out nt.Velocity);

cfgTarget.Get("Acceleration"+i, ref tmpS);
Vector3D.TryParse(tmpS, out nt.Acceleration);

nt.TimeStamp = t;

ret.Add(nt);
}

}
		}

return ret;
}





        #endregion
    }
}

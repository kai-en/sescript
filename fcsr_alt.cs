/*
 ======= [ MEA ] ȫ�Զ�ת�ӻ�����̨���Ƴ��� FCS-R v1.3 by MEAȺ�� QQȺ530683714 =======
 �����ܡ�
	����һ�׼���ȫ�Զ���ת�ӻ�����̨�Ŀ��Ƴ��򡣲���Ҫ�����ǣ�����Ҫ���ӵİ�װ��
	�ص�ֻ��һ�� ���������������� "׼"
	����ת�ӿ��Ʋ���PID�������������������˳�����Ƶ��ӳٺ��𵴣�Ч�ʼ���
	����Ԥ���㷨������FCS�ĵ���Ԥ���㷨��ָ�Ĵ��ġ�
 ��˵����
	1��ת�ӻ�����̨��һ��ʹ��ת�ӹ̶��ڷɴ��ϵ���̨�������ͨ����װ�̶������ֻ�ǹ�������ȴ�ͳ�Զ�����������ӡ������ṹ��һ��ˮƽ���Xת�ӡ�һ����ֱ���Yת�ӣ�Ȼ�������ջ����װһ��̶������ֻ�ǹ
	2�����������Կ���ͬһ���񣨰�����ת�ӡ����������������������ת�ӻ�����̨�����Բ�����Ҫÿ����̨��װһ������
	3���������в���3�ַ�ʽ��
		a.ָ��һ��FCS��̿飨FCS�������Ƴ���֪����س��򣬲�������ͷ���У�http://www.spacemea.com//forum.php?mod=viewthread&tid=2����������Զ���ȡFCS��̿�������Ŀ�ꡣ
		b.������Զ���ȡ���������е��Զ������������Զ�������������Զ������ֻ�ǹ�����ڻ�ǹ�����������κ���������Ŀ�겢��ʼ�Զ�����������ͻ�ָ�����п��Թ�����ת�ӻ�����̨һ�𹥻����Ŀ�ꡣ��Ŀ������£�������Զ�����
	4�������ڵ���Ԥ���㷨��������Ϸԭ���Զ��������㷨����˫�������޹��ɻ���������������ʱ�ԭ����������50%���ϡ�������㷨������FCS��������������������²���Ч��
	5��������ת����ת�����ϲ���PID���ƣ��Բ�ͬ���͵�����ָ����ͬ��PID�����������ת�ӵ���׼Ч��
	6������������ת�ӻ��������ϰ�װ���ɸ�̽�������������Щ��̽����������ֹ���ת�ӻ�����̨�����Լ������ֶ��趨��Щ̽�����ķ�Χ���������պø��Ǹ������Ļ���ǰ�������룬������Ϊ����̽���Լ���Ӫ������̽��з���Ӫ��
	��̽���������̽�⵽�趨�����壩������ͻ����������̨����ֹͣ������
	
 ����װ��
	1�������������һ��ת�ӻ�����������������������ؿ�Ž�һ�������������ѡһ������飨������������������ʶ���Լ���ָ�򣬿������κ�K���еķ��飬ע��ǰ����������һ��Ҫ��ȷ�����޸������������֣�
	���������ְ������򶥲���AimBlockKey�趨�Ĺؼ��֡�
	2����ط��������
		======= ��Ҫ ======
		a.һ����׼�飬����������K���п��Կ����ķ��飬�����趨�������֣�������[����]AimBlockKey�趨�Ĺؼ��֡������롿
		b.����ת�ӣ�������Զ�ʶ��ת�Ӱ�װ����֧�ֶ��ת�ӹ�ͬ����һ���ᣨ����װ��������������x��ת�Ӻ�y��ת�����ٸ�һ���������޷�������׼�������롿
		c.���������������������������Ͷ����ԣ�����ʹ�ù̶������ֻ�ǹ��̶�������������������롿
		d.����̽���������ڹ�������������ѡ��
		e.��������ͷ�������������С�����ѡ��
	3�����ｨ��ʹ��Զ�̿��ƿ飨���ʻ�գ���Ϊ��׼�顣ʹ��Զ�̿��ƿ���Ϊ��׼��ʱ������������Զ�̿��ƿ飬�����ʶ����Ŀ��ƣ������������������ת�ӡ������������Ʒɴ�һ��ֱ���������������̨��ָ��
	4��������̨��ô�죿�Ѹ�����̨�����Լ��ķ��鶼�����Լ��ı��������A��̨����׼�顢������ת�ӵȷ��鶼�Ž�����A�У�B��̨����׼�顢������ת�ӵȷ��鶼�Ž�����B�С�������һ������ͬʱ���ڶ����̨�ı����
	5���ɴ��ϻ����������������ô�죿û��ϵ��������ȡ���б��飬��ֻ�б����д���һ�����ְ�����AimBlockKey�ķ���ʱ���������Żᱻ��Ϊ��ת�ӻ�����̨�����򲻻�����������������κη��顣
	6���ر�ע�⣬�����ڴ���ģʽ��ָ��ǰ������ʱ�򣬻��ڴ���ʱָ���������X��ת�ӵķ���ǰ�������԰�װ��̨��ʱ����ע��Xת�ӵķ���ͬʱ�ڴ���ģʽ��ָ���⡿ʱ����̨��ָ���̿������̨�����߷��򣨲�������Ը߶ȣ������Ծ����ѱ�̿���ڽ���������λ��
	
  ��ָ�
	����ָ��ʱע����Ӣ�ĺʹ�Сд
	1������԰Ѵ�����Բ����е�DebugMode��Ϊtrue�����ԣ�����ʹ�ò���[Debug]���б���̿顣����״̬�»��ڱ�̿����½���ʾ�����������Լ������ܱ���Ϊ������ԭ��
	2��OnOff ���� �ܿ��أ��رպ���������������׼�Ϳ��𡣵���Ӱ���ȡĿ����ֶ�����
	3��FireMode  ����  ���ؾ�ȷ��׼�󿪻𣬾�ȷ��׼��ָ��ָ��Ŀ��Ĺ����У�ֻ��ָ�򼸺�׼ȷ�˲ŻῪ�����׼ȷ�����ж��������·�����׼����ϵͳ���á����޸�
	4��Attention ����  �л�������׼ģʽ����ѡ���ޡ���׼�Լ�ǰ�����������
	
  ������˵����
	v0.9 �Ż����ֶ�����ʱ��ϸ��
	v1.0 ��������׼��ſ�����ж�����ʡ�ӵ���
		������ֻ�����ϰ�ƽ����趨�����Թ��ܣ�
		��������LCD��̬������鿴�Ƿ����������
	v1.1 �����˴���ʱ����׼ģʽ�л�
		�����˹��������ܿ���
	v1.3 ������ת�ӷ�ת��ǩ
		�������Զ��忪��ʱ��
*/

// =============== �������� =============
//�ٴ�ǿ����ÿ��ת����̨���������������ͬһ�������У�������������б�����һ�����ְ��� AimBlockKey�ؼ��ֵķ���������׼�����ת����̨�ķ���Ӧ�ø��Է����Լ��ı��飬�������ͻ��
const string FCSComputerNameTag = "Programmable block fcs"; //FCS��̿����֣����ڶ�ȡ����Ŀ�꣬�Ǳ��롣
const string AimBlockKey = "FCS#R"; //��׼�����ֹؼ��֣�����д�������У�ֻҪ��������ؼ��ּ��ɡ�
const string LCDNameTag = "FCSR_LCD"; //LCD���֣�������ʾ����Ŀ�����̨��Ϣ���Ǳ��룬Ҳ�����Ƕ����
const string RotorNagtiveTag = "[-]"; //ת�Ӹ�ת��ǩ����ת����������ȫ���������ǩ��ʱ�����ᱻǿ����Ϊ�Ƿ�����Ƶ�ת�ӡ��������ĳЩ����ṹ��ת������

string CockpitNameTag = "Reference";

// ============== ս������ ==============
const double AttackDistance = 910; //Ĭ���������Զ��������
static bool OnlyAttackUpPlane = false; //�Զ�ѡ��Ŀ��ʱ�Ƿ�ֻѡ��ת�ӻ����������ϰ��������Ŀ�꣨��Ŀ����ڸ�����ʱ��ѡ�����Ŀ�꣩

// ============== ���׿������ ============
//������Զ����ԭ������������Ŀ��ʱ������������ͬʱ��Ҳ�����Լ�ʹ�ö�ʱ����������������Ҫ����Ҫ�����Զ��忪���ת�ӻ�����̨�ϼ���һ��������ʱ�飬����Ž������̨�ı��顣
//ͬʱ������Ҫ���������ʱ�����������ȫ�����·���FireTimerNameTag���õı�ǩ�������������ʱ����Զ��������д�ϴ�������ľ��루�����֣�����������þ��룬������Ĭ�������Ŀ������
//�������Ŀ����뿪������ڵ�ʱ����ÿ��60�ε��ٶȶ������ʱ��ִ�С�����������������
const string FireTimerNameTag = "[Fire_Timer]";  //�Զ�����ʱ�����ֱ�ǩ����ʱ��������Ҫ��ȫ���������ǩ��

// ================ �����ӵ��������� =============
const double BulletInitialSpeed = 400; //�ӵ����ٶ�
const double BulletAcceleration = 0; //�ӵ����ٶ�
const double BulletMaxSpeed = 400; //�ӵ�����ٶ�
const double ShootDistance = 910; //�������
const double BulletInitialSpeed2 = 170; //�ӵ����ٶ�
const double BulletAcceleration2 = 10; //�ӵ����ٶ�
const double BulletMaxSpeed2 = 190; //�ӵ�����ٶ�
const double ShootDistance2 = 910; //�������

// ============== �������� ==============
static int AttentionRandomTime = 180; //�������ģʽ���л������60��1�룬120��2�룬���������������

// ================= ��׼����ϵͳ���� ===============
// PID����
const double RotorMaxSpeed = 30; //�Զ�״̬��ת�������ٶȣ���λ Ȧ/���ӡ���׼ֵ30
const double AimRatio = 5; //��׼���ȣ���λ���ȡ������ж���̨�Ƿ���׼���Ա����������жϡ���Ӱ����׼��Ч�ʡ�����׼�����ǰ����������׼���Ŀ������������н�С�����ֵʱ������ϵͳ�ж���׼��Ŀ�ꡣ
const double Aim_PID_P = 0.9; //����ϵ�����������Ϊ����PID���Ƶ������ȣ����鷶Χ0��1.2��1����ȫ������
const double Aim_PID_I = 8; //����ϵ�����������ϵ�����þ�̬������ӣ������ٻ����������������׼���𵴡���֮ͬ��
const double Aim_PID_D = 1; //΢��ϵ�����������ϵ���������׼���𵴷��ȣ�����Ӿ���С�Ƕ�ƫ��ʱ���𵴷��ȡ���֮ͬ��
const int Aim_PID_T = 5; //PID �������ڣ���λ��֡��������ԽСЧ��Խ�ã���̫С�����ڻ��û���ϵ�����Է���Ч��
// �ֶ�����
const double PlayerAimRatio = 1; // ����ֶ����Ƶ���׼�����ȣ�ֻ�е���׼����Զ�̿��ƿ���ʻ�ղŴ���������ܡ�

// =============== ������Բ��� ====================
const int ProgramUpdateRatio = 60; //����ÿ��ѭ���Ĵ���������GetBlocks()�������޸ĳ���ѭ��Ƶ�ʣ����Խ�֡ʹ�á��޸ĺ�ͬ���޸������������
const string ShootActionName = "ShootOnce"; //�����Ŀ���ָ�Ĭ�ϵ��������Ͳ���Ҫ�޸ģ�Mod����Ҳ���ֿ��ã�

bool DebugMode = false; //����ģʽ���أ���Ϊtrue������false�رա��������Ժ���򲻻�ִ���������ܣ����ڱ�̿����½�������л�ȡ���ķ�������Լ�����ԭ��

bool init;
static int t;
static Random R_D = new Random();
static bool isOnOff = true; //�Ƿ񿪻�
static bool isFireWhenAimOK = true; //�Ƿ���̨��׼��Ԥ���ſ����Ƿ���׼���ж��ɡ���׼����ϵͳ���á��е�AimRatio�������������ѡ����Խ�ʡ�ӵ���Ҳ����һ�������ڽǶ������޷���׼��Ŀ�����̨�����˷��ӵ�����
static int AttentionMode = 2; //����ģʽ��1 ���� 2ָ��Xת��ǰ�� 3���ģʽ
List<RotorBase> FCSR = new List<RotorBase>(); //��̨����
List<Target> TargetList = new List<Target>(); //Ŀ�꼯��
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
	//����ָ��
	if(arguments == "Debug"){DebugMode = true;}
	if(arguments == "FireMode"){isFireWhenAimOK = !isFireWhenAimOK;}
	if(arguments == "OnOff"){isOnOff = !isOnOff;}
	if(arguments == "Attention"){
		AttentionMode += 1;
		if(AttentionMode > 3){AttentionMode = 1;}
	}
	
	PBPosition = Me.GetPosition();
	
	//�����ȡ���Զ����������е�ת�ӻ�������
	if(!init){GetBlocks(); return;}
	
	t ++;//ʱ��
	
	//����Ŀ�꣬ÿ���Զ�����һ��Ŀ�꣬ÿ��ת�ӻ�����̨�Զ�����һ��Ŀ�꣬��һ��FCS��̿�Ŀ��
	TargetList = new List<Target>();
	//��ȡFCSĿ��
	Target FCS_T = new Target();
	FCS_T.GetTarget(GridTerminalSystem.GetBlockWithName(FCSComputerNameTag) as IMyProgrammableBlock);
	if(FCS_T.EntityId != 0){
		TargetList.Add(FCS_T);
	}
	//��ȡ�Զ�����Ŀ��
	for(int i = 0; i < AutoWeapons.Count; i ++){
		Target ATWP_T = new Target();
		ATWP_T.GetTarget(AutoWeapons[i]);
		if(ATWP_T.EntityId != 0){
			TargetList.Add(ATWP_T);
		}
	}

	//ָ��ÿ����̨ѡ�������Ŀ��
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
			R.UpdateMotionInfo();//�����˶���Ϣ
			if(TargetList.Count > 0){
				R.AttackCloestTarget(TargetList);
			}
			else{
				R.Attention(AttentionMode, mode);//��λ
			}
			R.CheckPlayerControl();//�����ҿ���
			R.ShowLCD();//LCD��ʾ״̬��Ϣ
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

// ========== ��ʾ��ҪLCD���� =========
public void ShowMainLCD()
{
	List<IMyTextPanel> Lcds = new List<IMyTextPanel>();
	GridTerminalSystem.GetBlocksOfType(Lcds, b => b.CustomName == LCDNameTag);
	string info = "";
	string br = "\n";
	info += " =========== [ MEA ] FCS-R ========== " + br;
	info += "  �������� : " + (isOnOff ? " �� " : " - ") + "         " + "��ȷ���� : " + (isFireWhenAimOK ? " �� " : " - ") + br;
	info += "  ��̨���� : " +  FCSR.Count + "          " + "����ģʽ : " + (AttentionMode == 1 ? "  ����  " : (AttentionMode == 2 ? "��׼ǰ��" : "�������")) + br;
	int dot_count = t%30;
	if(dot_count <= 5) info += "  .";
	else if(dot_count <= 10) info += "  ..";
	else if(dot_count <= 15) info += "  ...";
	else if(dot_count <= 20) info += "  ....";
	else if(dot_count <= 25) info += "  .....";
	else if(dot_count <= 30) info += "  ......";
	info += br + " =========== Ŀ���б� ========== " + br;
	info += "  ����         ����         ֱ��" + br;
	for(int i = 0; i < TargetList.Count; i ++){
		if(TargetList[i].EntityId != 0){
			info += "  " + TargetList[i].Name + "        " + Math.Round(Vector3D.Distance(TargetList[i].Position, Me.GetPosition()),0) + "      " + Math.Round(TargetList[i].Diameter,0) + br;
			
		}
	}
	info += " --------------------- ��̨�� ----------------------- " + br;
	info += "  ����      ��������       ������" + br;
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

// =========== Ŀ����� ============
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
	
	// --------- ��ʼ������ -------
	public Target()
	{
		this.Name = "";
		this.EntityId = 0;
	}
	
	// -------- ͨ����ͬ��ʽ��ȡĿ�� -------
	//Fire Control System$OnOff@Aim@Weapon@ExactLock@Fire@Speed$79432486378773108@�������� 3108@62.6996810199223@{X:910.617573761076 Y:-767.260930602875 Z:-641.466380670639}@{X:0 Y:0 Z:0}@{X:0 Y:0 Z:0}@{X:0 Y:0 Z:-1}@{X:0 Y:1 Z:0}@0$
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

// =========== ת������������� ===========
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
	
	// -------- ��ʼ������ ---------
	public RotorBase(IMyBlockGroup thisgroup)
	{
		Name = thisgroup.Name;
		
		List<IMyTerminalBlock> blocks_temp = new List<IMyTerminalBlock>();
		thisgroup.GetBlocks(blocks_temp);
		
		//�����׼��
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
		
		//���ת��
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
		
		//�������
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block is IMyUserControllableGun){
				Weapons.Add(block);
			}
			if(block is IMySmallGatlingGun) {
				 this.isRocket = false;
			}
		}
		
		//��ȡ�Զ��忪��ʱ��
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block.CustomName.Contains(FireTimerNameTag)){
				FireTimers.Add(block);
			}
		}
		
		//��ʼ�������̨ģ��ɹ�
		ThisRotorBaseRight = true;
		
		//�������ͷ��̽�����ȸ������飬�Ǳ���
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

	// ------- �����˶���Ϣ ---------
	public void UpdateMotionInfo()
	{
		this.Acceleration = (((this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio) - this.Velocity)*ProgramUpdateRatio;
		this.Velocity = (this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio;
		this.Position = this.AimBlock.GetPosition();
	}
	
	// ------- ������λ --------
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
		// 	case 1: //����ģʽ���ر�����ת���˶�
		// 	for(int i = 0; i < this.RotorXs.Count; i ++){
		// 		this.RotorXs[i].TargetVelocityRPM = 0;
		// 	}
		// 	for(int i = 0; i < this.RotorYs.Count; i ++){
		// 		this.RotorYs[i].TargetVelocityRPM = 0;
		// 	}
		// 	break;
		// 	case 2: //rotor����
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
		// 	case 3: //���ָ�򣬽�ָ���������ϰ���
		// 	if(t%AttentionRandomTime == 0){
		// 		aimPoint = this.RotorXs[0].GetPosition() + this.RotorXs[0].WorldMatrix.Forward*(R_D.Next(-500,500)) + this.RotorXs[0].WorldMatrix.Right*(R_D.Next(-500,500)) + this.RotorXs[0].WorldMatrix.Up*(R_D.Next(100,500));
		// 		this.AimAtTarget(aimPoint);
		// 	}
		// 	break;
		// }
		
	}
	
	// ------ ����ִ����ҿ��� --------
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
	
	// ------- ���������������Ŀ�� -------
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
			//�����Զ��忪��ʱ���ִ���Զ��忪��
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
	
	// ---------- ��׼Ŀ�� -----------
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
		//���������
		if(Aim_PID_Data.Count < Aim_PID_T){
			for(int i = 0; i < Aim_PID_T; i ++){
				Aim_PID_Data.Add(new Vector3D());
			}
		}
		else{Aim_PID_Data.Remove(Aim_PID_Data[0]); Aim_PID_Data.Add(TargetPositionToMe);}
		
		//��ò��������
		double X_I = 0;
		double Y_I = 0;
		foreach(Vector3D datapoint in Aim_PID_Data){
			X_I += datapoint.X;
			Y_I += datapoint.Y;
		}
		
		//�������
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
		
		// ���㵱ǰ��Ԥ����׼�����׼�н�
		Vector3D V_A = HitPoint - this.Position;
		Vector3D V_B = this.AimBlock.WorldMatrix.Forward;
		double Angle = Math.Acos(Vector3D.Dot(V_A,V_B)/(V_A.Length() * V_B.Length())) * 180 / Math.PI;
		if(Angle <= AimRatio) return true;
		return false;
	}
	
	// ------ ���� ---------
	public void Fire()
	{
		foreach(IMyTerminalBlock gun in this.Weapons){
			gun.ApplyAction(ShootActionName);
		}
	}
	
	// ------ ��ʾLCD --------
	public void ShowLCD()
	{
		string info = "";
		string br = "\n";
		info += " =========== [ MEA ] FCS-R ========== " + br;
		if(this.MyTarget != null){
			info += " Ŀ��: " + this.MyTarget.Name + br;
			info += " ����: " + Math.Round(Vector3D.Distance(this.Position, this.MyTarget.Position),0) + br;
		}
		else{
			info += " ������..." + br + br;
		}
		info += " --------------------------------------------------------- " + br;
		info += " ��������: " + this.Weapons.Count + br;
		info += " ����ͷ��: " + this.Cameras.Count + br;
		foreach(IMyTextPanel lcd in this.LCDs){
			lcd.ShowPublicTextOnScreen();
			lcd.WritePublicText(info);
		}
	}
}

// ============ �������� =============
static Vector3D HitPointCaculate(Vector3D Me_Position, Vector3D Me_Velocity, Vector3D Me_Acceleration, Vector3D Target_Position, Vector3D Target_Velocity, Vector3D Target_Acceleration,    
							double Bullet_InitialSpeed, double Bullet_Acceleration, double Bullet_MaxSpeed)   
{   
	//�����㷨   
	Vector3D HitPoint = new Vector3D();   
	Vector3D Smt = Target_Position - Me_Position;//�����ָ��Ŀ���ʸ��   
	Vector3D Velocity = Target_Velocity - Me_Velocity; //Ŀ��ɴ����Լ��ɴ����ٶ�   
	Vector3D Acceleration = Target_Acceleration; //Ŀ��ɴ����Լ��ɴ��ܼ��ٶ�   
	   
	double AccTime = (Bullet_Acceleration == 0 ? 0 : (Bullet_MaxSpeed - Bullet_InitialSpeed)/Bullet_Acceleration);//�ӵ����ٵ�����ٶ�����ʱ��   
	double AccDistance = Bullet_InitialSpeed*AccTime + 0.5*Bullet_Acceleration*AccTime*AccTime;//�ӵ����ٵ�����ٶȾ�����·��   
	   
	double HitTime = 0;   
	   
	if(AccDistance < Smt.Length())//Ŀ�����ڵ����ٹ�����   
	{   
		HitTime = (Smt.Length() - Bullet_InitialSpeed*AccTime - 0.5*Bullet_Acceleration*AccTime*AccTime + Bullet_MaxSpeed*AccTime)/Bullet_MaxSpeed;   
		HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
	}   
	else//Ŀ�����ڵ����ٹ����� 
	{   
		double HitTime_Z = (-Bullet_InitialSpeed + Math.Pow((Bullet_InitialSpeed*Bullet_InitialSpeed + 2*Bullet_Acceleration*Smt.Length()),0.5))/Bullet_Acceleration;   
		double HitTime_F = (-Bullet_InitialSpeed - Math.Pow((Bullet_InitialSpeed*Bullet_InitialSpeed + 2*Bullet_Acceleration*Smt.Length()),0.5))/Bullet_Acceleration;   
		HitTime = (HitTime_Z > 0 ? (HitTime_F > 0 ? (HitTime_Z < HitTime_F ? HitTime_Z : HitTime_F) : HitTime_Z) : HitTime_F);   
		HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
	}   
	//������������������ײʱ�䣬ÿ�ε���������5λ������   
	for(int i = 0; i < 3; i ++)   
	{   
		if(AccDistance < Vector3D.Distance(HitPoint,Me_Position))//Ŀ�����ڵ����ٹ�����   
		{   
			HitTime = (Vector3D.Distance(HitPoint,Me_Position) - Bullet_InitialSpeed*AccTime - 0.5*Bullet_Acceleration*AccTime*AccTime + Bullet_MaxSpeed*AccTime)/Bullet_MaxSpeed;   
			HitPoint = Target_Position + Velocity*HitTime + 0.5*Acceleration*HitTime*HitTime;   
		}   
		else//Ŀ�����ڵ����ٹ�����   
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
var ov = (tv-mv) - rv; // ����
var ovl = ov.Length();
if (Vector3D.Dot(ov, los) < 0) ovl = -ovl;
var rvd = Vector3D.Normalize(rv);
var rvl = rv.Length();
if (bs == bm) {
var lvl = Math.Sqrt(bm*bm - rvl*rvl);
var aim = Vector3D.Normalize( los + rvd*(rvl/lvl));
return aim;
} else {
// ����һ���������ٽ׶�
var accSpeed = (bs + bm)/2;
var accTime = (bm - bs) / ba;
// ��ʣ��׷��ʱ��Ϊx, �н�cosΪc
// ��ʽ1ֱ�߾���  (tp - mp).Length() + ovl*(accTime + x)  = accTime * ( accSpeed * c) + bm*c*x;
// ��ʽ2�����ٶ� rv.Length() = (accSpeed * (1-c*c) * accTime + bm * (1 - c*c) * x ) / (accTime + x);
// ����̫����
return Vector3D.Zero;
}

}

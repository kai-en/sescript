static bool isOnOff = true; //�Ƿ񿪻� ����true �ɱ�ս���ֱ� false
static bool OnlyAttackUpPlane = false; //�Զ�ѡ��Ŀ��ʱ�Ƿ�ֻѡ��ת�ӻ����������ϰ��������Ŀ�꣨��Ŀ����ڸ�����ʱ��ѡ�����Ŀ�꣩ ����һ��true �ɱ�ս��һ��false
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

// ============== ���׿������ ============
//������Զ����ԭ������������Ŀ��ʱ������������ͬʱ��Ҳ�����Լ�ʹ�ö�ʱ����������������Ҫ����Ҫ�����Զ��忪���ת�ӻ�����̨�ϼ���һ��������ʱ�飬����Ž������̨�ı��顣
//ͬʱ������Ҫ���������ʱ�����������ȫ�����·���FireTimerNameTag���õı�ǩ�������������ʱ����Զ��������д�ϴ�������ľ��루�����֣�����������þ��룬������Ĭ�������Ŀ������
//�������Ŀ����뿪������ڵ�ʱ����ÿ��60�ε��ٶȶ������ʱ��ִ�С�����������������
const string FireTimerNameTag = "PG-";  //�Զ�����ʱ�����ֱ�ǩ����ʱ��������Ҫ��ȫ���������ǩ��

// ================ �����ӵ��������� =============
const double BulletInitialSpeed = 400; //�ӵ����ٶ�
const double BulletAcceleration = 0; //�ӵ����ٶ�
const double BulletMaxSpeed = 400; //�ӵ�����ٶ�
const double ShootDistance = 870; //�������
const double BulletInitialSpeed2 = 170; //�ӵ����ٶ�
const double BulletAcceleration2 = 10; //�ӵ����ٶ�
const double BulletMaxSpeed2 = 190; //�ӵ�����ٶ�
const double ShootDistance2 = 910; //�������
const double BulletInitialSpeed3 = 267.5; //�ӵ����ٶ�
const double BulletAcceleration3 = 0; //�ӵ����ٶ�
const double BulletMaxSpeed3 = 267.5; //�ӵ�����ٶ�
const double ShootDistance3 = 3000; //�������
const float xdegree = -2.0F;

// ============== �������� ==============
static int AttentionRandomTime = 180; //�������ģʽ���л������60��1�룬120��2�룬���������������

// ================= ��׼����ϵͳ���� ===============
// PID����
const double RotorMaxSpeed = 30; //�Զ�״̬��ת�������ٶȣ���λ Ȧ/���ӡ���׼ֵ30
const double AimRatio = 5; //��׼���ȣ���λ���ȡ������ж���̨�Ƿ���׼���Ա����������жϡ���Ӱ����׼��Ч�ʡ�����׼�����ǰ����������׼���Ŀ������������н�С�����ֵʱ������ϵͳ�ж���׼��Ŀ�ꡣ
const double Aim_PID_P = 20; //����ϵ�����������Ϊ����PID���Ƶ������ȣ����鷶Χ0��1.2��1����ȫ������
const double Aim_PID_I = 1; //����ϵ�����������ϵ�����þ�̬������ӣ������ٻ����������������׼���𵴡���֮ͬ��
const double Aim_PID_D = 0; //΢��ϵ�����������ϵ���������׼���𵴷��ȣ�����Ӿ���С�Ƕ�ƫ��ʱ���𵴷��ȡ���֮ͬ��

const int Aim_PID_T = 5; //PID �������ڣ���λ��֡��������ԽСЧ��Խ�ã���̫С�����ڻ��û���ϵ�����Է���Ч��
// �ֶ�����
const double PlayerAimRatio = 0.1; // ����ֶ����Ƶ���׼�����ȣ�ֻ�е���׼����Զ�̿��ƿ���ʻ�ղŴ���������ܡ�

// =============== ������Բ��� ====================
const int ProgramUpdateRatio = 60; //����ÿ��ѭ���Ĵ���������GetBlocks()�������޸ĳ���ѭ��Ƶ�ʣ����Խ�֡ʹ�á��޸ĺ�ͬ���޸������������
const string ShootActionName = "ShootOnce"; //�����Ŀ���ָ�Ĭ�ϵ��������Ͳ���Ҫ�޸ģ�Mod����Ҳ���ֿ��ã�

bool DebugMode = false; //����ģʽ���أ���Ϊtrue������false�رա��������Ժ���򲻻�ִ���������ܣ����ڱ�̿����½�������л�ȡ���ķ�������Լ�����ԭ��

bool init;
static int t;
static Random R_D = new Random();
static bool isFireWhenAimOK = true; //�Ƿ���̨��׼��Ԥ���ſ����Ƿ���׼���ж��ɡ���׼����ϵͳ���á��е�AimRatio�������������ѡ����Խ�ʡ�ӵ���Ҳ����һ�������ڽǶ������޷���׼��Ŀ�����̨�����˷��ӵ�����
static int AttentionMode = 2; //����ģʽ��1 ���� 2ָ��Xת��ǰ�� 3���ģʽ
List<RotorBase> FCSR = new List<RotorBase>(); //��̨����
static List<Target> TargetList = new List<Target>(); //Ŀ�꼯��
static List<Target> LastTargetList = new List<Target>(); //Ŀ�꼯��
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


//List<IMyTerminalBlock> TURRETS = new List<IMyTerminalBlock>();

static IMyRemoteControl remoteBlock = null;
static Vector3D piratePosition = Vector3D.Zero;

static Dictionary<string, List<IMyTerminalBlock>> weaponDic = new Dictionary<string, List<IMyTerminalBlock>>();
static Dictionary<string, int> weaponSeqDic = new Dictionary<string, int>();

Program()
{
Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

void Main(string arguments)
{
	t ++;//ʱ��
	debugInfo = "";

	//����ָ��
	if(arguments == "Debug"){DebugMode = true;}
	if(arguments == "FireMode"){isFireWhenAimOK = !isFireWhenAimOK;}
	if(arguments == "OnOff"){isOnOff = !isOnOff;}
	if(arguments == "On"){isOnOff = true;
            }
	if(arguments == "Off"){isOnOff = false;}
	if(arguments == "Attention"){
		AttentionMode += 1;
		if(AttentionMode > 3){AttentionMode = 1;}
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
	
	//�����ȡ���Զ����������е�ת�ӻ�������
	if(!init){GetBlocks(); return;}
	
            debugInfo += "\nFCSR Count: " + FCSR.Count;
	foreach ( var r in FCSR ) {
		debugInfo += "\n" + r.debugInfo;
	}
	
	//����Ŀ�꣬ÿ���Զ�����һ��Ŀ�꣬ÿ��ת�ӻ�����̨�Զ�����һ��Ŀ�꣬��һ��FCS��̿�Ŀ��
	TargetList = new List<Target>();
	//��ȡFCSĿ��
	TargetList.AddRange(GetFcsTargetList());
	// Target FCS_T = new Target();
	// FCS_T.GetTarget(GridTerminalSystem.GetBlockWithName(FCSComputerNameTag) as IMyProgrammableBlock);
	// if(FCS_T.EntityId != 0){
	// 	debugInfo += "\nGet fcs_t"; 
        //                 FCS_T.isFCS=true;
	// 	TargetList.Add(FCS_T);
	// }
	//��ȡ�Զ�����Ŀ��
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

	//ָ��ÿ����̨ѡ�������Ŀ��
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
			R.UpdateMotionInfo();//�����˶���Ϣ
			if(TargetList.Count > 0){
				R.AttackCloestTarget(TargetList, mode);
			}
			else{
				R.Attention(AttentionMode, mode);//��λ
			}
			R.CheckPlayerControl();//�����ҿ���
			R.ShowLCD();//LCD��ʾ״̬��Ϣ
		}
	}else{
		foreach(RotorBase R in FCSR){
                                    if (isOnOff) R.Attention(1, mode);
                                    else R.Attention(1, 99);
		}
	}
	
            Echo("initInfo "+ initInfo);
            Echo("debugInfo "+ debugInfo);
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

	if(DebugMode){
		foreach(RotorBase rt in FCSR_temp){
			string info = rt.Name + " -- " + rt.ErrorReport;
			Echo(info);
		}
	}
	else{
	if(FCSR.Count>0)init = true;
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
            public bool isFCS = false;
	public string type = "Auto";
	
	// --------- ��ʼ������ -------
	public Target()
	{
		this.Name = "";
		this.EntityId = 0;
	}
	
	// -------- ͨ����ͬ��ʽ��ȡĿ�� -------
	
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

// =========== ת������������� ===========
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
	
	public bool ThisRotorBaseRight;
	public string debugInfo;
	public bool isRocket = true;
	public CustomConfiguration cfg;
	public float hori;
	public float horiD;
	public float vert;
	public float vertD;
	public float offX = 0;
	public float offY = 1F;
	public float onX = 0;
	public float onY = 0;
	public float ra=0;
	public float raD=1F;
	public double bulletMaxSpeedConf = 0;
	public double bulletMaxRange = 900;
	public float gravityRate = 0;
	public double curvationRate = 0.2;
	public bool isAutoFire = true;
	public double PID_P = Aim_PID_P;
	public double PID_I = Aim_PID_I;
	public double PID_D = Aim_PID_D;
	public string FACE_TO = "Forward";
	public string TARGET_TYPE = "All";
	public double OFFSET_Y = 0;
	public double RANDOM_Y = 0;
	
	static float pp=20F,pi=1F,pd=0F, pim=0.1F;
	public List<PIDController> pidXL = new List<PIDController>();
	public List<PIDController> pidYL = new List<PIDController>();
	
	// -------- ��ʼ������ ---------
	public RotorBase(IMyBlockGroup thisgroup, MyGridProgram program)
	{
		this.program = program;
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
		vert = toRa(vert);
		cfg.Get("vertD", ref vertD);
		vertD = toRa(vertD);
		cfg.Get("hori", ref hori);
		hori = toRa(hori);
		cfg.Get("horiD", ref horiD);
		horiD = toRa(horiD);
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

		//���ת��
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
					var dot = Vector3D.Dot(AimBlock.WorldMatrix.Left, block.WorldMatrix.Up);
					if (Math.Abs(dot) > 0.9) {
						RotorYs.Add(block as IMyMotorStator);
						RotorYField.Add(1*NagtiveRotor);
						debugInfo = block.CustomName;
					} else {
						RotorXs.Add(block as IMyMotorStator);
						RotorXField.Add(1*NagtiveRotor);
					}
				} else {
					RotorXs.Add(block as IMyMotorStator);
					if (Vector3D.Dot(AimBlock.WorldMatrix.Up, block.WorldMatrix.Up)>0)
						RotorXField.Add(1*NagtiveRotor);
					else RotorXField.Add(-1*NagtiveRotor);
				}
			}
		} else {
		foreach(IMyTerminalBlock block in blocks_temp){
			if(block is IMyMotorStator){
				int NagtiveRotor = 1;
				if(block.CustomName.Contains(RotorNagtiveTag)){NagtiveRotor = -1;}
				var dot = Vector3D.Dot(AimBlock.WorldMatrix.Left, block.WorldMatrix.Up);
				if (Math.Abs(dot)>0.9) {
					RotorYs.Add(block as IMyMotorStator);
					if (dot > 0) RotorYField.Add(1*NagtiveRotor);
					else RotorYField.Add(-1*NagtiveRotor);
				} else {
					RotorXs.Add(block as IMyMotorStator);
					if (Vector3D.Dot(AimBlock.WorldMatrix.Up, block.WorldMatrix.Up)>0)
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
				var dot = Vector3D.Dot(AimBlock.WorldMatrix.Left, block.WorldMatrix.Up);
				if (Math.Abs(dot)>0.9) continue;
				var dot2 = Vector3D.Dot(AimBlock.WorldMatrix.Up, block.WorldMatrix.Up);
				RotorXs.Add(block as IMyMotorStator);
				if (dot2 >0  || block.CustomName.Contains("[Arm]")) 
					RotorXField.Add(1*NagtiveRotor);
				else RotorXField.Add(-1*NagtiveRotor);
		}
		}
		}
		if(RotorXs.Count < 1 || RotorYs.Count < 1) {ErrorReport = "Rotors Not Complete!"; return;}

		for(int i = 0; i < RotorXs.Count; i++) {
		    pidXL.Add(new PIDController(PID_P, PID_I, pd,pim,-pim,12));
		}
		for(int i = 0; i < RotorYs.Count; i++) {
		    pidYL.Add(new PIDController(PID_P, PID_I, pd,pim,-pim,12));
		}

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
		if (msc != null) this.Velocity = msc.GetShipVelocities().LinearVelocity;
		else this.Velocity = (this.AimBlock.GetPosition() - this.Position)*ProgramUpdateRatio;

		this.Position = this.AimBlock.GetPosition();
	}
	
	// ------- ������λ --------
	public void Attention(int ATMode, int aeroMode)
	{
		if (this.Weapons.Count == 0 && this.AimBlock is IMyShipController && (this.AimBlock as IMyShipController).IsUnderControl) return;
		if (this.Weapons.Count == 0 && this.AimBlock is IMyShipController && remoteBlock!=null && remoteBlock.GetNearestPlayer(out piratePosition)) {
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
	       return t;
	}
	
	// ------- ���������������Ŀ�� -------
	public void AttackCloestTarget(List<Target> targetList, int mode)
	{
		//debugInfo = "act";
                if (this.Weapons.Count == 0 && this.AimBlock is IMyShipController && !((this.AimBlock as IMyShipController).IsUnderControl) && this.FireTimers.Count == 0) {
                   Target FCS_T = null;
                   foreach(var t in targetList) {
                       if (t.isFCS) FCS_T = t;
                   }
                   if (FCS_T == null) {
                      this.Attention(1,0);
                      return;
                   }
                   //debugInfo += "\n"+FCS_T.Position;
                   this.AimAtTarget(FCS_T.Position);
                   //CODING
                   return;
                }

		double currentDis = double.MaxValue;
		Target MyAttackTarget = new Target();
		for(int i = 0; i < targetList.Count; i ++){
			if (this.TARGET_TYPE == "Focus") {
			   if (!targetList[i].isFCS && targetList[i].type != "Focus") continue;
			}
			double distance = Vector3D.Distance(targetList[i].Position, this.Position);
double dot = 1;
// a b k filter direction use ra and raD
if (msc != null && raD != 1) {
//this.debugInfo = "\nra: " + this.ra;
Vector3D mid = msc.WorldMatrix.Right * Math.Sin(this.ra) + msc.WorldMatrix.Forward * Math.Cos(this.ra);
Vector3D tar = Vector3D.Normalize(targetList[i].Position - this.Position);
//this.debugInfo += "\n dirYN: " + (Vector3D.Dot(mid, tar));
dot = Vector3D.Dot(mid, tar);
if ( dot < this.raD) continue;
}else {
//this.debugInfo = "\n\n";
}
double compose = distance / 900 + (1- dot);
			if(targetList[i].EntityId != 0 && compose <= currentDis){
				if(OnlyAttackUpPlane){
					MatrixD RotortXLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.RotorXs[0].WorldMatrix.Forward, this.RotorXs[0].WorldMatrix.Up);
					Vector3D TargetPositionToRotorX = Vector3D.TransformNormal(targetList[i].Position - this.RotorXs[0].GetPosition(), RotortXLookAtMatrix);
					if(TargetPositionToRotorX.Y >= 0){
						MyAttackTarget = targetList[i];
						currentDis = compose;
					}
				}
				else{
					MyAttackTarget = targetList[i];
					currentDis = compose;
				}
			}
		}
		
		if(MyAttackTarget.EntityId != 0){
			bool isAimedOK = this.AimAtTarget(MyAttackTarget, false);
			bool sensorActive = false;
			foreach(IMySensorBlock sensor in this.Sensors){
				if(sensor.IsActive){sensorActive = true;}
			}
			bool rx = false;
			for(int i = 0; i < this.RotorXs.Count; i ++){
			            var a = this.RotorXs[i].Angle;
				if (this.RotorXs.Count == 2 && angleDeltaAbs(a , vert) < vertD) {
					rx = true;
				}else
				if (this.RotorXs.Count == 1 && angleDeltaAbs(a , hori) < horiD) {
					rx = true;
				}
			}
			bool ry = false;
			for(int i = 0; i < this.RotorYs.Count; i ++){
			            var a = this.RotorYs[i].Angle;
				if (this.RotorYs.Count == 2 && angleDeltaAbs(a , vert) < vertD) {
					ry = true;
				}else
				if (this.RotorYs.Count == 1 && angleDeltaAbs(a , hori) < horiD) {
					ry = true;
				}
			}
			sensorActive = sensorActive || (rx&&ry);
			MatrixD refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), this.AimBlock.WorldMatrix.Forward, this.AimBlock.WorldMatrix.Up);
			Vector3D TargetPositionToMe = Vector3D.TransformNormal(MyAttackTarget.Position - this.Position, refLookAtMatrix);
			//�����Զ��忪��ʱ���ִ���Զ��忪��
			if(FireTimers.Count() > 0){
				foreach(IMyTerminalBlock fire_timer in FireTimers){
					int fire_distance = (int)ShootDistance3;//CODING
					if(Vector3D.Distance(MyAttackTarget.Position, this.Position) <= fire_distance && !sensorActive && isAimedOK){
						//fire_timer.ApplyAction("TriggerNow");
					}
				}
			}
			if(Vector3D.Distance(MyAttackTarget.Position, this.Position) <= this.bulletMaxRange && !sensorActive){
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
                                    this.Attention(AttentionMode, mode);
		}
		
	}

string displayVector3D(Vector3D tar) {
return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
}

	// ---------- ��׼Ŀ�� -----------
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
		Vector3D HitPoint = HitPointCaculate(this.Position, thisV, thisA, Position + this.AimBlock.WorldMatrix.Up * (OFFSET_Y + RANDOM_Y * 0.001 * R_D.Next(-1000, 1000)), Velocity, Acceleration, bs, ba, bm, FireTimers.Count > 0 ? 1F : gravityRate, ng, bulletMaxRange, curvationRate);
                        Vector3D tp2me = Position - this.Position;
		Vector3D TargetPositionToMe = new Vector3D(0,0,-1);
		if (HitPoint != Vector3D.Zero) {
		TargetPositionToMe = Vector3D.Normalize(Vector3D.TransformNormal(HitPoint - this.Position, refLookAtMatrix));
		}
//		Vector3D aimDir = CalcAim(this.Position, thisV, Position, Velocity, bs, ba, bm);
//		var aimDirToMe = Vector3D.TransformNormal(aimDir,  refLookAtMatrix);
		if (FireTimers.Count>0) {
			// ABK piston gun aim angle correction
			// https://www.andre-gaschler.com/rotationconverter/
			TargetPositionToMe = Vector3D.Transform(TargetPositionToMe, new Quaternion((float)Math.Sin(toRa(xdegree * 0.5F)), 0, 0, (float)Math.Cos(toRa(xdegree * 0.5F))));
		}
		
		//�������
		var targetPositionToReal = Vector3D.TransformNormal(TargetPositionToMe, this.AimBlock.WorldMatrix);//
		var faceDir = RotorXs[0].WorldMatrix.Forward;
		if (FACE_TO == "Right") faceDir = RotorXs[0].WorldMatrix.Right;
		else if (FACE_TO == "Left") faceDir = RotorXs[0].WorldMatrix.Left;
		else if (FACE_TO == "Backward") faceDir = RotorXs[0].WorldMatrix.Backward;
		var upDir = RotorXs[0].WorldMatrix.Up;
		if (RotorXField[0] < 0) upDir = RotorXs[0].WorldMatrix.Down;
		var rcLookAt = MatrixD.CreateLookAt(Vector3D.Zero, faceDir, upDir);
		var tpToRc = Vector3D.TransformNormal(targetPositionToReal, rcLookAt);
		double aa=0, ea=0;
		Vector3D.GetAzimuthAndElevation(tpToRc, out aa, out ea);
		debugInfo = "\n" + aa + " " + ea;
		if (this.AimBlock.CustomName.Contains("[Arm]")) ea += Math.PI * 0.5;

		bool isFireZone = angleDeltaAbs(this.RotorXs[0].Angle, hori) > horiD;
		double fireRange = ShootDistance;
		if (isRocket) fireRange = ShootDistance2;
		if(FireTimers.Count>0) fireRange = ShootDistance3;
		if (bulletMaxRange != 0) fireRange = bulletMaxRange;
                        isFireZone = isFireZone && tp2me.Length() < fireRange;
                        if (isStraight) isFireZone = true;
		if (!isFireZone) {
		aa = (float)onX * this.RotorXField[0];
		ea = (float)onY * this.RotorYField[0];
		}

		for(int i = 0; i < this.RotorXs.Count; i ++){
			var a = (float)(-aa*this.RotorXField[i]) - this.RotorXs[i].Angle;
			if (a > Math.PI) a = a - MathHelper.TwoPi;
			if (a < -Math.PI) a = a + MathHelper.TwoPi;
			this.RotorXs[i].TargetVelocityRPM = (float)pidXL[i].Filter(a,2);
		}
			for(int i = 0; i < this.RotorYs.Count; i ++){
				//this.RotorYs[i].TargetVelocityRPM = (float)(PitchValue * this.RotorYField[i] * RotorMaxSpeed);
			var a = (float)(ea*this.RotorYField[i]) - this.RotorYs[i].Angle;
			if (a > Math.PI) a = a - MathHelper.TwoPi;
			if (a < -Math.PI) a = a + MathHelper.TwoPi;
			this.RotorYs[i].TargetVelocityRPM = (float)pidYL[i].Filter(a,2);
			}
		
		// ���㵱ǰ��Ԥ����׼�����׼�н�
		if (HitPoint == Vector3D.Zero) return false;
		Vector3D V_A = HitPoint - this.Position;
		Vector3D V_B = this.AimBlock.WorldMatrix.Forward;
		double Angle = Math.Acos(Vector3D.Dot(V_A,V_B)/(V_A.Length() * V_B.Length())) * 180 / Math.PI;
		if(Angle <= AimRatio) return true;
		return false;
	}
	
	// ------ ���� ---------
	public void Fire()
	{
		if (!isAutoFire) return;
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
		debugInfo += "\nghpc\n" + debugString + "\n";
		return ret;
	}
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

// ����5 ������������ԭ�򣬶�g��ȡ��СԶ����
var gd = Vector3D.Normalize(g);
var ngtpr = Vector3D.Reject(tp, gd).Length();
g = g * (1 + ((ngtpr - bulletMaxRange*0.5)*2 / bulletMaxRange) * curvationRate);

/*
Ŀ���ٶ�Ϊ  tvx, tvy, tvz. Ŀ��λ��  tpx, tpy, tpz.
�������ٶ� gax, gay, gaz.
�ҷ�λ�� mpx, mpy, mpz, �ҷ��ٶ� mvx, mvy, mvz
�ڵ��ٶȱ��� aV, 
�����ڵ��ٶ�Ϊ avx, avy, avz
����ʱ��Ϊ n

����
tpx + tvx * n = mpx + mvx * n + avx * n +  0.5 * gax * n * n ����1
tpy + tvy * n = mpy + mvy * n + avy * n + 0.5 * gay * n * n ����2
tpz + tvz * n = mpz + mvz * n + avz * n + 0.5 * gaz * n * n ����3
avx * avx + avy * avy + avz * avz = aV * aV ����4

��������������, �泯Ŀ�귽��������ϵ, ���� gax = gaz = 0 gay = -�������ٶ�
�� tpx = mpx = 0
���ڽ��ܲ���ʱ, �Ѹ��ñ���λ�ú��ٶ���Ϊλ�ú��ٶȻ�׼, ���Ի���mp = 0��mv=0

����1, �ɷ���1
�����ֱ�����avx = tvx - mvx

�ɷ���3
tpz - mpz = (avz + mvz - tvz) * n
��֪ n�� �� avz С, nС, ��avz��

�ɷ���4
avz * avz + avy * avy = aV * aV  - avx * avx 
����avx��֪, ��֪ avz �� avy �γ�һ��Բ��

����gay = 0 ����һ��n����, �ٰ�gay����, ���¼���avy avz n ������αƽ���ȷ��nֵ

*/

debugString = "";
if (tp == Vector3D.Zero) return Vector3D.Zero;
// 1 ��������ϵ
// 1.1 ��� tp���� �� g �����Ƿ� ��ȫͬ��/���� �㷨�޷������������ , ������ (ȱ��1)
var dot = Vector3D.Dot(Vector3D.Normalize(tp), gd);
if (dot == 1 || dot == -1) return Vector3D.Zero;
// 1.2 ��������ת������
var forward = Vector3D.Normalize(Vector3D.Reject(tp, gd));
var tranmt = MatrixD.CreateLookAt(new Vector3D(), forward, -gd);

// 1.3 ת������
var tp2 = Vector3D.TransformNormal(tp, tranmt);
var tv2 = Vector3D.TransformNormal(tv, tranmt);
//debugString = displayVector3D(tp2);
//debugString += "\n" + displayVector3D(tv2);

// 2 �� avx
double avx = tv2.X;
if (Math.Abs(avx) > aV) return Vector3D.Zero; // ���ٸϲ���tv2.X �޷�׷��
//debugString += "\navx: " + avx;

// 3 ����g ����һ��avy avz
// 3.1 �������tvYZ, Z��ʣ���ٶ���avzd, ��Y����Ҫ��ʣ���ٶ�Ϊ (tpy/tpz)*avzd
// �� (tvz+avzd)2 + (tvy + (tpy/tpz)avzd)2 = aVyz2
double aVyz = Math.Sqrt(aV*aV - avx*avx);
if (tv2.Z*tv2.Z + tv2.Y*tv2.Y > aVyz*aVyz) return Vector3D.Zero; // yz��Ŀ���ٶȴ����ڵ��ٶ� ׷����, �����������������ٶ�׷ (ȱ��2)
// tpz ������Ϊ0 ��Ϊ�ܽ�����ϵ�ͱ�ʾ����ǰ�ķ���, ��z�������)
// ����һԪ���η��� ax2 +bx + c = 0��ʽ, ����a , b, c
double fa = 1 + ((tp2.Y*tp2.Y) / (tp2.Z*tp2.Z));
double fb = 2 * tv2.Z + 2 * tv2.Y * (tp2.Y/tp2.Z);
double fc = tv2.Z * tv2.Z + tv2.Y * tv2.Y - aVyz * aVyz;
if (fb*fb - 4*fa*fc < 0) return Vector3D.Zero; // �޽�, ����
// ���� һԪ���η��������ʽ, ����x (��avzd)
double x = (-fb + Math.Sqrt(fb*fb - 4*fa*fc)) / (2*fa);
double avz = 0;
if (tv2.Z + x < 0) {
avz = tv2.Z + x;
} else {
x =  (-fb - Math.Sqrt(fb*fb - 4*fa*fc)) / (2*fa);
avz = tv2.Z + x;
}
double avy = tv2.Y + (tp2.Y/tp2.Z) * x;

// 3.2 ����z��, ��׷��ʱ�� n
double zdelta = avz - tv2.Z;
double n = tp2.Z / zdelta;
if (n < 0) return Vector3D.Zero; // Z��׷��ʱ��Ϊ��, �޷�׷��
//debugString += "\naVyz: " + aVyz;
//debugString += "\navy: " + avy;
//debugString += "\navz: " + avz;
//debugString += "\nn: " + n;

// 4 ѭ���ƽ����(�������Ӧ���н�, �������ȡ�ƽ�����)(ȱ��3) ���ٶȵ����� ��Ҫ�Ǽ���avyg����Ӧ��ȡ����
double avyg = 0;
double avyp = avy;
for (int i = 0; i < 4; i++) {
// 4.1 ����ǰn ����avyg, ȡavyg = 0.5 * (-g) * n ; ����, gΪ ����10, ʱ��1��, ������Ҫ����5, ����ǰ0.5��Ϊ������, ��0.5��Ϊ�½���, ��������=�½�����, ��Ӱ����׼
avyg = 0.5 * g.Length() * n;
// 4.2 ����ʱ��n�䳤��, ��Ҫ���¼���avy
avyp = tv2.Y + (tp2.Y / n);
// 4.3 ����avyg
avyp = avyp + avyg;
if (Math.Abs(avyp) > aVyz) return Vector3D.Zero;
double avzL = Math.Sqrt(aVyz*aVyz - avyp*avyp);
avz = - avzL;
zdelta = avz - tv2.Z;
double nn = tp2.Z / zdelta;
if (nn > n) n = nn;
else n = (nn + n) /2;
if (n < 0) return Vector3D.Zero; // Z��׷��ʱ��Ϊ��, �޷�׷��

//debugString += "\navy: " + avyp;
//debugString += "\navz: " + avz;
//debugString += "\nn: " + n;

}

// 5 ��avx avy avz ת�ؾ�������ϵ �����(ע�Ȿ������Ӧ�������ײ��λ�� , �������Է����ٶȴ���, ���Ի�Ҫ���ϱ���λ��, �����ߴ���)
avyp *= 0.98;
Vector3D av2m = new Vector3D(avx, avyp, avz);
Vector3D av = Vector3.Transform(av2m, Matrix.Transpose(tranmt));
//debugString += "\nav: " + displayVector3D(av);
return av;
}

// (* -1.64 0.60) (- 2.3 0.6) 1.7
// ����1, ����n����, avyҲ��Ҫͬʱ����, ��ԭ�㷨û�����������, �ⷨ, ����n���¼���avy
// (* 1.72 0.58) (- 2.24 0.58) (/ (+ 2.24 1.66) 2) (* 1.95 0.58)
// ����2, ���㷨����, ��2.25 �� 2 ����ֵ ֮��, 10����û������ 100��Ҳ��0.03�����, ������ÿ֡����, ���10��
// �ⷨ n���������� n = (nn + n) / 2, 4�������0.01����
// (* 1.86 0.537) (- 2.13 0.537) (/ (+ 2.13 1.593) 2)
// ����3 fcsrֹͣ����? ��ʱ������ָ������ ɾ��
// ����4 ����, ����hinge������, ���Ǹ߻�ը? �޶�����-20, СĿ���ը? subgrid-damage ��ը?

// (- 2.12 0.53) (/ (+ 2.12 1.59) 2) (* 1.855 0.53)
// (* 1.86 0.53)
// (/ 1300 2.72)


static IMyProgrammableBlock fcsComputer = null;
static List<Target> GetFcsTargetList() {
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
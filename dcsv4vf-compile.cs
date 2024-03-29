﻿using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace kradar_p
{
    partial class Program : MyGridProgram
    {
#region ingamescript

/*
Kaien's drone control system V5.6
?id=1406061291

last update:
v5.6.1 202203
-----------------
RADAR:STANDBYOFF
...

custom data sample for son ship:

CockpitNameTag=R_FORWARD
DCSLCDNameTag=DCS_LCD
fighterFcsName=Programmable block fcs
LCDNameTag=Text panel fcs
dockingList=0,0,-250;0,50.5,-16.14;0,-3.78,-15.6;0,-14.89,-16.14
upMode=true
flyByOffsetDirection=LEFT
dockingForward=FORWARD
dockingUp=UP
dockingApproach=DOWN
soundBlockNameTag=Sound Block fcs
commandAllTic = 100
commandWaitTic = 0
ScanRange=3000

custom data sample for mother ship

CockpitNameTag=Flight Seat 3
dockingList=0,0,-250;0,50.5,-16.14;0,-3.78,-15.6;0,-14.89,-16.14
DCSLCDNameTag=DCS_LCD
fighterFcsName=Programmable block fcs
LCDNameTag=LCD Panel fcs
homingTurretName=R_FORWARD_OC
useTurretAsAimer=true
macAV=0.005
isBase=true
maxTargetCount=1
ScanRange=6000
limitInnerRotor=false

mother ship commands

FLYBYON
  :take off
DOCKINGON
  :landing
ATTACKON
  :attack enemy locked
ATTACKOFF
  :withdraw
WEAPON1
  :switch to gatlin guns
WEAPON2
  :switch to rocket launcher
*/

float VTOL_PID_P = 5F; 
string CockpitNameTag = "Cockpit";
string HeadNameTag = "VF_HEAD";
string LCDNameTag = "FCS_LCD";
string DCSLCDNameTag = "DCS_LCD";
string GyroscopesNameTag = "";
string soundBlockNameTag = "FCSFoundSound";
string homingTurretName = "R_FORWARD_OC";
double ScanRange = 2000;
string fighterFcsName = "Programmable block fcs";

bool useTurretAsAimer = false;

Vector3D flyByAimPosition = new Vector3D(0,0,0);
Vector3D flyByAttackPosition = new Vector3D(0,0,0);
Vector3D shipPosition = new Vector3D(0,0,0);
long shipPositionTime = 0;
Vector3D estShipPosition() {
if (shipPositionTime == 0) return shipPosition;
return shipPosition + (t-shipPositionTime)*(1D/60) * shipSpeed;
}
Vector3D targetPosition = new Vector3D(0,0,0);
Vector3D shipSpeed = new Vector3D(0,0,0);
Vector3D flyByAimSpeed = new Vector3D(0,0,0);
bool needFlyByAim = false;
IMyTextPanel gcTargetPanel = null;
List<IMyTextPanel> gcTargetPanelList = new List<IMyTextPanel>();
int maxTargetCount = 1;
Vector3D naturalGravity;
double naturalGravityLength;
bool isDown = true;
bool isStandBy = true;
bool isME = false;
bool isLaunch = false;
bool isBase = false;
bool isBig = false;
bool dampenersOn = true;
double droneAttackRange = 0;
bool isPrinted = false;
bool isPtdTurnOn = false;
long ptdSeparatedStart = 0;
bool limitInnerRotor = false;
bool isAeroDynamic=false;
float angleWhenDown= 0F; //default 0 , vf -0.65F
float angleWhenWalk= 0F; //default 0.1F
int aeroSpeedLevel=0;
int MAX_SPEED = 1000;
int sl_fs = 0;
int sl_bs = 0;
static int refreshInterval = 15;
int refreshFrame = new Random().Next(15);
void aeroSLp(bool a){

if (a) {
// acc
if (sl_fs == 0) {
sl_fs = t;
sl_bs = 0;
}
} else {
if (sl_bs == 0) {
sl_bs = t;
sl_fs = 0;
}
}

var nf = -mySpeedToMe.Z;
aeroSpeedLevel = (int)nf / 10;
}

void aeroSLz() {
if (sl_fs != 0 && t - sl_fs > 180) aeroSpeedLevel = MAX_SPEED / 10;
if (sl_bs != 0 && t - sl_bs > 180) aeroSpeedLevel = 0;
sl_fs = 0;
sl_bs = 0;
}
bool isSteeringAccumulate = false;
Vector3D steeringAccumulation = Vector3D.Zero;
long saIgnoreStart = -1;

float ft = 0f;
Vector3D mySpeedToMe = Vector3D.Zero;

List<IMySensorBlock> sensors = new List<IMySensorBlock>();
long lastMotherSignalTime=0;

bool init = false;
bool configFinish = false;
IMyShipController Cockpit;
IMyShipController msc;
IMyTerminalBlock fighterFcs = null;
List<IMyTextSurface> LCD = new List<IMyTextSurface>();
List<IMyTerminalBlock> Gyroscopes;
string[] gyroYawField = null;
string[] gyroPitchField = null;
string[] gyroRollField = null;
string[] gyroYawFieldHead = null;
string[] gyroPitchFieldHead = null;
string[] gyroRollFieldHead = null;
float[] gyroYawFactor = null;
float[] gyroPitchFactor = null;
float[] gyroRollFactor = null;
float[] gyroYawFactorHead = null;
float[] gyroPitchFactorHead = null;
float[] gyroRollFactorHead = null;
const float GYRO_FACTOR = 1f;
static int t = 1;
Vector3D MePosition = Vector3D.Zero;
Vector3D myToShipPosition = Vector3D.Zero;
double AimRatio = 0.5;
double maxAV = 1;
double diffX, diffZ;
Vector3D MeVelocity;
Vector3D MeLastVelocity;
Vector3D inputVec;
Vector3D inputVec_RT = Vector3D.Zero;
Vector3D needA_G = Vector3D.Zero;
float inputRoll;
Vector2 inputRota;
bool isUnderControl = false;
MyShipMass myShipMass;
float shipMass;
MatrixD refLookAtMatrix;
MatrixD refMatrix;

Vector3D LockTargetPosition = Vector3D.Zero;
bool LockTargetAiming = false;
Vector3D LockTargetVelocity = Vector3D.Zero;
Vector3D radarHighThreatPosition = Vector3D.Zero;
Vector3D asteroidPosition = Vector3D.Zero;
bool avoidOn = false;
List<Vector3D> LTPs = new List<Vector3D>();
List<Vector3D> LTVs = new List<Vector3D>();

int lastSendingTime = 0;
int lastReceivingTime = 0;

Vector3D maintainSpeedToMeAA = Vector3D.Zero;

// vf mode
bool isVF=true;
IMyShipController Head = null;
List<IMyTerminalBlock> rotorShoulder = new List<IMyTerminalBlock>();
PIDController[] shoulderPIDList = new PIDController[2];
static int legMode = 0; // default = 0, vf = 1
float legMargin = -0.05f * (float)Math.PI;
string FCSRNameTag = "Programmable block FCSR";
IMyTerminalBlock fighterFcsr = null;

Vector3D downStartPos = Vector3D.Zero;
int lastDownT = 0;
int lastUpT = 0;

bool ignoreTag(string n) {
return n.Contains("#A#") || n.Contains("#B#") || n.Contains("RCS") || n.Contains("L2T");
}

// sm mode
bool isSM = false;

// auto height hold
bool ahhOn = false;
bool continueAhhOn = false;
bool isNear = false;
void setAhhOn(bool v) {
// new ahh
if(!v && ahhOn)resetThrusters();
ahhOn = continueAhhOn = v;
return;

}
double ahh = 0;

// suspect mode
bool susMode = false;

// smoke
List<IMyUserControllableGun> smokeList = new List<IMyUserControllableGun>();
int smokeInterval = -1;

// input
static double INPUT_RATE = 5;

// ctnNA
int ctnNA = 0;
bool useCtn = false;

// axis
bool isAxisAim=false;
List<IMyTerminalBlock> axisLight=new List<IMyTerminalBlock>();
Color axisN = new Color(101,153,255),
axisG = new Color(78,255,130),
axisY = new Color(255,238,139),
axisR = new Color(255,130,78);
long axisBs=0;
PIDController pidaxx = new PIDController(1,0.4,0,0.001,-0.001,60/refreshInterval),
pidaxy = new PIDController(1,0.4,0,0.001,-0.001,60/refreshInterval);

// s light
List<IMyTerminalBlock> sLights = new List<IMyTerminalBlock>();
Color SLow = new Color(255,112,148),
SHight = new Color(255, 217, 165);

// 
bool wingForce = false;

bool hsPosi = true;

Program()
{

}

void Main(string arguments, UpdateType updateType)
{
if(!debugFix && t % refreshInterval == refreshFrame) debugInfo = "";
Runtime.UpdateFrequency = UpdateFrequency.Update1;
if ((updateType & UpdateType.Update1) != 0) {
arguments = "";
}

if (!configFinish) {
ProcessCustomConfiguration();
configFinish = true;
}

if(!init)
{
GetBlocks(arguments);
t++;
return;
}
if ((updateType & UpdateType.Update1) !=0)
  NavigationInfoUpdata(true);
List<IMyTerminalBlock> welderList;
switch (arguments)
{
case ("AVOID"):
avoidOn = !avoidOn;
break;
case ("CONTROL"):
attackMode=false;
flyByOn=false;
dockingOn=false;
isDkMv=false;
resetThrusters();
approachIdx = 0;
PlayActionList(connectors, "OnOff_On");
PlayActionList(landingGears, "OnOff_On");
SetGyroOverride(false);
break;
case ("FLYBY"):
if (shipPosition == Vector3D.Zero) break;
isDkMv=false;
attackMode=false;
flyByOn = !flyByOn;
if (flyByOn) {
dockingOn=false;
resetApproachIdx();
setDampenersOverride(Cockpit, false);
PlayActionList(thrusters, "OnOff_On");
setAhhOn(false);
PlayActionList(connectors, "OnOff_Off");
PlayActionList(landingGears, "OnOff_Off");
}
if (!flyByOn) {
resetThrusters();
resetApproachIdx();
PlayActionList(connectors, "OnOff_On");
PlayActionList(landingGears, "OnOff_On");
SetGyroOverride(false);
}
break;
case ("FLYBYON"):

break;
case ("DOCKING"):
if (shipPosition == Vector3D.Zero) break;
isDkMv=false;
attackMode=false;
dockingOn = !dockingOn;
if (dockingOn) {
flyByOn=false;
setDampenersOverride(Cockpit, false);
PlayActionList(thrusters, "OnOff_On");
setAhhOn(false);
PlayActionList(connectors, "OnOff_On");
PlayActionList(landingGears, "OnOff_On");
}
if (!dockingOn) {
resetThrusters();
PlayActionList(connectors, "OnOff_On");
PlayActionList(landingGears, "OnOff_On");
SetGyroOverride(false);
}
break;
case ("DOCKINGON"):
break;
case ("ATTACKON"):
break;
case ("ATTACKOFF"):
break;
case ("WEAPON1"):
break;
case ("WEAPON2"):
break;
case ("LOADMISSILE"):
// TODO
break;
case "DOWN":
isDown = !isDown;
if (isDown) {
isLaunch=false;
downStartPos = MePosition;
} else {
SetGyroOverride(false);
}
resetThrusters();
break;
case "LAUNCH":
isLaunch = !isLaunch;
if (isLaunch) {
isDown=false;
} else {
SetGyroOverride(false);
}
resetThrusters();
break;
case "RESET":
init = false;
break;
case "POINT":
motherPointerMode = !motherPointerMode;
break;
case "ATTACK":
if (flyByOn) {
attackMode = !attackMode;
}
break;
case "WALKMODE":
if (legMode<2) break;
if (legMode == 2) {
legMode = 3;
PlayActionList(thrusters, "OnOff_Off");
PlayActionList(spotlights, "OnOff_Off");
setDampenersOverride(Cockpit, false);
} else {
legMode = 2;
PlayActionList(thrusters, "OnOff_On");
PlayActionList(spotlights, "OnOff_On");
setDampenersOverride(Cockpit, true);
}
break;
case "VFTUP":
VFTransformNew(true);
break;
case "VFTDOWN":
VFTransformNew(false);
break;
case "SM":
isSM = !isSM;
break;
case "BST":
useBst = !useBst;
break;
case "STEER":
isSteeringAccumulate = !isSteeringAccumulate;
steeringAccumulation = Vector3D.Zero;
saIgnoreStart = -1;
break;
case "AXIS":
isAxisAim=!isAxisAim;
break;
default:
if(arguments!=null && arguments != "") {
ParseMaintainSpeed(arguments);
lastReceivingTime=t;
}
break;
}

if ((updateType & UpdateType.Update1) == 0) {
//ShowLCD();
return;
}

switch(commandCache) {
case "FLYBYON":
if(shipPosition==Vector3D.Zero) break;
if ( t > commandStart + commandWaitTic) {
startFlyBy();
setAhhOn(false);
commandCache=null;
}
break;
case "DOCKINGON":
if ( t > commandStart + (commandAllTic-commandWaitTic)) {
startDocking();
setAhhOn(false);
commandCache=null;
}
break;
default:
break;
}

t++;
if(isStandBy) {ShowLCD();return;}
userControl();
if(t%refreshInterval != refreshFrame) return;
if (t > lastMotherSignalTime + 120) {
ParseSensor();
}

if (gcTargetPanel != null) {
if (!LockTargetPosition.Equals( Vector3D.Zero)) {
gcTargetPanel.WritePublicTitle("[T:" + LockTargetPosition.X + ":" + LockTargetPosition.Y + ":" + LockTargetPosition.Z + ":"
+ LockTargetVelocity.X + ":" + LockTargetVelocity.Y + ":" + LockTargetVelocity.Z);

for (int i = 0; i < maxTargetCount; i++ ){
if (i >= gcTargetPanelList.Count) break;
var cp = gcTargetPanelList[i];
int vi = i % LTPs.Count;
cp.WritePublicTitle("[T:" + LTPs[vi].X + ":" + LTPs[vi].Y + ":" + LTPs[vi].Z + ":"
+ LTVs[vi].X + ":" + LTVs[vi].Y + ":" + LTVs[vi].Z);
}

} else {
gcTargetPanel.WritePublicTitle("");
for (int i = 0; i < maxTargetCount; i++ ){
if (i >= gcTargetPanelList.Count) break;
var cp = gcTargetPanelList[i];
cp.WritePublicTitle("");
}


}
}
NavigationInfoUpdata(false);
inputVec_RT = inputVec;
inputVec_RT *= INPUT_RATE;

if (!(flyByOn || dockingOn) && avoidOn) {
Vector3D avoidSpeed = calcSpeedToMeAndAvoid(MeVelocity, true);
inputVec_RT += avoidSpeed * 0.1;
}

if(notDocked()) {
if ( dockingOn && dockable() ) {
PlayActionList(landingGears, "Lock");
ShowLCD();
return;
}
} else {
if (dockingOn ) {
PlayActionList(thrusters, "OnOff_Off");
offGridThrustLL.ForEach(l=>l.ForEach(t=>t.Enabled=false));
SetGyroOverride(false);
PlayActionList(spotlights, "OnOff_Off");
}
}

bool needGyroOverride = false;
if(notDocked()) MaintainSpeed();
bool isFcsAiming = false;
// axis cannon
while(true){
if (fighterFcsr == null) break;
string[] lines = ((IMyTextSurfaceProvider)fighterFcsr).GetSurface(0).GetText().Split('\n');
if(isAxisAim){
string[] tarS = lines[0].Split(':');
if (tarS.Length<3) { isAxisAim = false; break;}
var tarN = new Vector3D();
double.TryParse(tarS[0],out tarN.X);
double.TryParse(tarS[1],out tarN.Y);
double.TryParse(tarS[2],out tarN.Z);
SetGyroYaw(pidaxx.Filter(tarN.X,2));
SetGyroPitch(pidaxy.Filter(tarN.Y, 2));
needGyroOverride=true;
isFcsAiming=true;
}
if (lines.Length<2) break;
setAxisColor(lines[1], isAxisAim);
break;
}
// speed light
updateSLight();
// a b g
var ng = naturalGravity;
bool isAssist = isDown || isLaunch || isSM || continueAhhOn;
bool suc = isUnderControl;
bool tuc = homingTurret != null && homingTurret.IsUnderControl;
if (isAssist) {


isFcsAiming |= LockTargetAiming && LockTargetPosition!=Vector3D.Zero;

if(ng.Length() > 0.001 && !isFcsAiming) {
var diff = diffX;
if (diff != 0) {
SetGyroRoll(diff * AimRatio * -1);
}
diff = diffZ;
if(isDown && legMode == 1) {
diff += angleWhenDown;
}
if(isDown && legMode == 3) {
diff += angleWhenWalk;
}
if (!isFcsAiming && !isSM && !tuc) {
SetGyroPitch(diff * AimRatio);
if(!isSteeringAccumulate)SetGyroYaw((inputRota.Y));
} else {
// rix = -Y riy = X
SetGyroPitch((-inputRota.X) * 0.5);
if(!isSteeringAccumulate)SetGyroYaw((inputRota.Y));
}

needGyroOverride = true;

}

bool sma = LockTargetPosition!=Vector3D.Zero && isSM;
if ( isDown || isLaunch || sma){
Vector3D maintainSpeedToMe = Vector3D.Zero;

Vector3D nowSpeedToMe = Vector3D.TransformNormal(Cockpit.GetShipVelocities().LinearVelocity, refLookAtMatrix);
if (isDown) {
var ds = -1.5;
Vector3D mstm = Vector3D.Zero;
if(naturalGravityLength > 0.01) {
mstm = -1 * Vector3D.Normalize(naturalGravity) * ds;
Vector3D downAim = MePosition - downStartPos;
downAim = Vector3D.Reject(downAim, Vector3D.Normalize(naturalGravity));
mstm = Vector3D.TransformNormal(mstm, refLookAtMatrix);
} else {
mstm = new Vector3D(0, ds, 0);
}
maintainSpeedToMe = mstm - nowSpeedToMe;

}else if (isLaunch){
maintainSpeedToMe = new Vector3D(0,90,0);
}else if (sma){
maintainSpeedToMe = Vector3D.TransformNormal(LockTargetVelocity, refLookAtMatrix) - nowSpeedToMe;
maintainSpeedToMe += inputVec * 10;
}
forwardMoveIndicator=0;//auto balance
if(dampenersOn) {
DimSpeedAll(maintainSpeedToMe, refLookAtMatrix);
} else {
resetThrusters(false);
}
inputVec_RT = maintainSpeedToMe * 1f; //
}

}

if (ahhOn && !isDown && !isLaunch && dampenersOn) {
double h = 0;
bool getted = msc.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out h);
if (getted) {
if (ahh == 0) ahh = h;
var needD = ahh - h;
needD = needD / (Vector3D.Dot(-Vector3D.Normalize(naturalGravity), msc.WorldMatrix.Up));  
var needV = needD;
var m2m = mySpeedToMe;
var needA = (needV - m2m.Y) ;  
inputVec_RT.Y = needA;
var nud = Cockpit.WorldMatrix.Up.Dot(naturalGravity);
string dsdr = DimSpeed(needA, upThrusters, downThrusters, ref thrusterPercentY, m2m.Y, lastSpeedY, "UP".Equals(dockingApproach)||"DOWN".Equals(dockingApproach), nud,1);
lastSpeedY = m2m.Y;
}
}
if(!(flyByOn||dockingOn||isSM) && dampenersOn){
// auto forward
if(Math.Abs(inputVec.Z) < 0.1 && Math.Abs(aeroSpeedLevel)>0 ){
ft = (float)MathHelper.Clamp(((-10*aeroSpeedLevel) - mySpeedToMe.Z) * 0.1, -10, 10);
inputVec_RT.Z = ft;
var needForce = (Vector3D.Dot(-naturalGravity, msc.WorldMatrix.Backward) + ft) * shipMass;
if(needForce < 0){
double allForce = 0;
foreach(var t in forwardThrusters) {
  allForce += ((IMyThrust)t).MaxEffectiveThrust;
}
float percent = 0;
if(allForce > 0) percent=(float) (-needForce/allForce);

SetThrusterListOverride(forwardThrusters, percent);
SetThrusterListOverride(backwardThrusters, 0);
}else {
if (naturalGravityLength>0.01){ 
double allForce = 0;
foreach(var t in backwardThrusters) {
  allForce += ((IMyThrust)t).MaxEffectiveThrust;
}
float percent = 0;
if(allForce > 0) percent=(float) (needForce/allForce);
SetThrusterListOverride(forwardThrusters, 0);
SetThrusterListOverride(backwardThrusters, percent);
}
}
} else {
SetThrusterListOverride(forwardThrusters, -1f);
SetThrusterListOverride(backwardThrusters, -1f);
}
}

// start dummy
if (isDummy) {
if (!isDummyInit) {
dummyP = MePosition;
dummyL = msc.WorldMatrix.Left;
dummyF = msc.WorldMatrix.Forward;
isDummyInit=true;
if(naturalGravityLength>0) {
dummyT = dummyP + (rnd.Next(10,1000) - 500) * dummyL + (rnd.Next(10,1000) - 500) * dummyF;
} else {
dummyT = dummyP + new Vector3D(rnd.Next(10,1000)-500,rnd.Next(10,1000)-500,rnd.Next(10,1000)-500);
}
}
if (t%600 == 0) {
if(naturalGravityLength>0) {
dummyT = dummyP + (rnd.Next(10,1000) - 500) * dummyL + (rnd.Next(10,1000) - 500) * dummyF;
} else {
dummyT = dummyP + new Vector3D(rnd.Next(10,1000)-500,rnd.Next(10,1000)-500,rnd.Next(10,1000)-500);
}
}
Vector3D dummySpeed = (dummyT - MePosition)/10D;

Vector3D maintainSpeedToMe = Vector3D.TransformNormal(dummySpeed - Cockpit.GetShipVelocities().LinearVelocity, refLookAtMatrix);
inputVec_RT = maintainSpeedToMe;
 if (maintainSpeedToMe.Length() > 50){
// high speed dir
Vector3D d = Vector3D.Normalize(maintainSpeedToMe);
if (d.Z > 0.5) {
d = new Vector3D(1,0,0);
}
SetGyroYaw(AimRatio*d.X);
if (naturalGravityLength>0){
} else {
SetGyroPitch(AimRatio*d.Y);
}
d.Z=0;
needGyroOverride = true;

}
DimSpeedAll(maintainSpeedToMe,refLookAtMatrix);
}

if(LockTargetAiming && LockTargetPosition!=Vector3D.Zero) needGyroOverride = true;

if((flyByOn || dockingOn) && notDocked()) needGyroOverride = true;

if(isSteeringAccumulate) needGyroOverride = true;
if (needGyroOverride == false && Math.Abs(inputRoll) < 0.001 && inputRota.Length() < 0.001)
{
SetGyroPitch(0);
SetGyroYaw(0);
SetGyroRoll(0);
needGyroOverride = true;
}
SetGyroOverride(needGyroOverride);
Main_RT(arguments, updateType); // in front of main_vt

Main_VT();

if (smokeInterval > 0 && t % smokeInterval == 0) {
PlayActionList(smokeList.OfType<IMyTerminalBlock>().ToList(),"ShootOnce");
}

ShowLCD();
// End Main
}

void updateSLight() { 
var fs = Vector3D.Dot(MeVelocity, msc.WorldMatrix.Forward);
var pe = MathHelper.Clamp(fs * 0.01, 0, 1);
var nc = new Color((int)((SHight.R - SLow.R)*pe + SLow.R), (int)((SHight.G - SLow.G) * pe + SLow.G), (int)((SHight.B - SLow.B) * pe + SLow.B));
SetBlocksValueColor(sLights, "Color", nc);
SetBlocksValueFloat(sLights, "Radius", (float)((160-10)*pe+10));  
SetBlocksValueFloat(sLights, "Intensity", (float)((5-0.5)*pe+0.5));
}
private void setAxisColor(string l, bool a)
{
var cn = "Color";
int axisSec = 0;
int.TryParse(l,out axisSec);
if (axisSec == 0) {axisBs=0; if (a) SetBlocksValueColor(axisLight, cn, axisG); else SetBlocksValueColor(axisLight, cn, axisN); }
else if(axisSec < 10) {
if(axisBs==0)axisBs=t;
if(t/60 %2==0) SetBlocksValueColor(axisLight, cn, axisY);
else SetBlocksValueColor(axisLight, cn, Color.Black);
} else SetBlocksValueColor(axisLight, cn, axisR);
}
Vector3D dummyP;
Vector3D dummyL;
Vector3D dummyF;
bool isDummyInit = false;
bool isDummy = false;
Vector3D dummyT;
Random rnd = new Random();

class VFTRotor {
IMyMotorStator r;
float[] pl = new float[3];
PIDController pid = new PIDController(8f, 0.1f, 0f, 0.01f, -0.01f, 60/refreshInterval);
long delay = 0L;
long sTime = 0L;
bool pg = false;
int lastMode = 0;
int tarMode = 0;
IMyPistonBase p;
bool isR;
float[] tapsl = new float[3]{0,0,0};
float[] tapel = new float[3]{0,0,0};
float[] tansl = new float[3]{0,0,0};
float[] tanel = new float[3]{0,0,0};
bool isTaReverse;
bool isVTA;
int iWTA;
bool inited = false;

static int DELAY_ALL = 360;

public VFTRotor(IMyTerminalBlock r, float p0, float p1, float p2, long d
,float ps0=0, float pe0=0, float ps1=0, float pe1=0, float ps2=0, float pe2=0, float ns0=0, float ne0=0, float ns1=0, float ne1=0, float ns2=0, float ne2=0, bool tr = false, int wta = -1) {
pl[0] = p0;
pl[1] = p1;
pl[2] = p2;
delay = d;
if (r is IMyMotorStator) {
this.r = (IMyMotorStator)r;
isR = true;
} else if (r is IMyPistonBase){
this.p = (IMyPistonBase)r;
isR = false;
pid = new PIDController(20f, 0.1f, 0f, 1f, -1f, 60/refreshInterval);
} else {
throw new Exception("e");
}
tapsl[0] = ps0;
tapsl[1] = ps1;
tapsl[2] = ps2;
tapel[0] = pe0;
tapel[1] = pe1;
tapel[2] = pe2;
tansl[0] = ns0;
tansl[1] = ns1;
tansl[2] = ns2;
tanel[0] = ne0;
tanel[1] = ne1;
tanel[2] = ne2;
isTaReverse = tr;
isVTA = !(pe0==0 && pe1==0 && pe2==0 && ne0==0 && ne1==0 && ne2==0);
iWTA = wta;
}

public void setMode(int mode) {
if(!inited) { 
lastMode = tarMode = mode;
inited=true;
}
if (!pg && mode != tarMode)
{
pg = true;
sTime = t;
tarMode = mode;
}

long d = delay;
if (tarMode < lastMode) d = (DELAY_ALL - delay);
int nm;
if (t < sTime + d) nm = lastMode;
else nm = tarMode;
if (t > sTime + DELAY_ALL * 2) { sTime = 0; pg = false; lastMode = tarMode; }
if (!isR)
{
p.SetValueFloat("Velocity", 0.6F*(float)pid.Filter(pl[nm] - p.CurrentPosition, 2, p.Velocity));
return;
}
var targetA = pl[nm];
var need = 0F;
if (isVTA && legMode!=3 ) {
if (VTA >= 0) {
need = MathHelper.Clamp(VTA - tapsl[nm], 0, tapel[nm] - tapsl[nm]);
} else {
need = MathHelper.Clamp(VTA - tansl[nm], tanel[nm] - tansl[nm], 0);
}
}

if (legMode == 3 && iWTA >= 0 && iWTA < 6) {
need += WTA[iWTA];
}

if (isTaReverse) need = - need;

r.SetValueFloat("Velocity", (float)pid.Filter(modangle(pl[nm] + need - r.Angle),2, r.TargetVelocityRPM));
}

public bool isNose() {
if (!isR) return false;
return r.CustomName.Contains("nose");
}

}

List<VFTRotor> vftrList = new List<VFTRotor>();
List<VFTRotor> wingrList = new List<VFTRotor>();
int landrMode = 0, wingrMode = 0;
List<VFTRotor> landrList = new List<VFTRotor>();
static float VTA = 0; //global vector angle left;

void tryAddVftr(String name, float p0, float p1, float p2, long d, List<VFTRotor> li
, float ps0=0, float pe0=0, float ns0=0, float ne0=0, float ps1=0, float pe1=0, float ns1=0, float ne1=0, float ps2=0, float pe2=0, float ns2=0, float ne2=0, bool tr = false, int wta = -1
) {
List<IMyTerminalBlock> tmprs = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(tmprs, b => ((IMyTerminalBlock)b).CustomName.Contains(name));
if (tmprs.Count == 0) return;
if (tmprs[0] is IMyMotorStator)
foreach(var t in tmprs) li.Add(new VFTRotor((IMyMotorStator)t, p0, p1, p2, d, ps0, pe0, ps1, pe1, ps2, pe2, ns0, ne0, ns1, ne1, ns2, ne2, tr, wta));
else if (tmprs[0] is IMyPistonBase)
foreach(var t in tmprs) li.Add(new VFTRotor((IMyPistonBase)t, p0, p1, p2, d));

}

int vftrMode = 0; // 0 = F 1 = G 2 = B
static long vftrStart = 0;
void VFTransformNew(bool up) {
if (legMode == 3 ) return;
if (t < vftrStart + 360) return;
vftrStart = t;
if (up) {
if (vftrMode == 2) return;
else vftrMode ++;
if(vftrMode >= 1){
legMode = 2;
//legMode = vftrMode;
}
if(vftrMode == 1){
//callComputer(fighterFcsr, "On");
}
} else {
if (vftrMode == 0) return;
else vftrMode --;
if(vftrMode == 1) {
legMode = 1;
}else if (vftrMode == 0) {
legMode = 2;
//legMode = 1;
}
if(vftrMode == 0){
callComputer(fighterFcsr, "Off");
}
}
}

void VFTransformLoop() {
if (t > vftrStart + 350) {
if (vftrMode == 2)  {
if (legMode < 2) legMode = 2;
} else if (vftrMode == 1) {
legMode = 1;
} else if(vftrMode == 0) {
legMode = 1;
}
}
if (t > vftrStart + 450 && t < vftrStart + 460) {
if (vftrMode >= 1) callComputer(fighterFcsr, "On");
}
foreach( VFTRotor r in vftrList) {
if (r.isNose() && ((shipPosition != Vector3D.Zero && (MePosition - estShipPosition()).Length() < 200) )) {
r.setMode(2);
continue;
}
r.setMode(vftrMode);
}
if (mySpeedToMe.Z < -60)
wingrMode = 1;
else if (mySpeedToMe.Z > -30)
wingrMode = 0;

if (isDown)wingrMode = 1;

landrMode = isDown?1:0;
if (shipPosition != Vector3D.Zero && (MePosition - estShipPosition()).Length() < 200) {
wingrMode = 1;
landrMode = 1;
}
if(wingForce) wingrMode = 0;

foreach(VFTRotor w in wingrList) {
w.setMode(wingrMode);
}
foreach(VFTRotor w in landrList) {
w.setMode(landrMode);
}
}

void VFTransform(int mode) {
if(rotorShoulder.Count ==0) return;
float tsl;
if (mode == 1) {
tsl = 0;
} else if (mode ==2) {
tsl = 0.5f * (float)Math.PI;
} else {
tsl = 0.8f * mpi;
}

for (int i = 0; i < rotorShoulder.Count; i++) {
IMyMotorStator r = (IMyMotorStator) rotorShoulder[i];
if ( i == 0) {
r.SetValueFloat("Velocity", (float)shoulderPIDList[0].Filter(tsl - modangle(r.Angle),2));
} else {
r.SetValueFloat("Velocity", (float)shoulderPIDList[0].Filter(-tsl - modangle(r.Angle),2));
}
}
}

void NavigationInfoUpdata(bool before)
{
if (Head != null && vftrMode == 2) {
msc = Head;
upThrusters = backwardThrustersR;
downThrusters = forwardThrustersR;
forwardThrusters = upThrustersR;
backwardThrusters = downThrustersR;

} else {
msc = Cockpit;
upThrusters = upThrustersR;
downThrusters = downThrustersR;
forwardThrusters = forwardThrustersR;
backwardThrusters = backwardThrustersR;
}
refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), msc.WorldMatrix.Forward, msc.WorldMatrix.Up);
refMatrix = msc.WorldMatrix;
if(before)
{
MePosition = msc.CenterOfMass;

if (shipPosition != Vector3D.Zero) {
Vector3D myPosition = MePosition;
MatrixD shipLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(0,0,0), shipMatrix.Forward, shipMatrix.Up);
myToShipPosition = Vector3D.TransformNormal(MePosition - estShipPosition(), shipLookAtMatrix);
}

MeVelocity = msc.GetShipVelocities().LinearVelocity;
if (MeVelocity.Length() < 40){
smokeInterval = -1;
} else {
var tmpddd = Vector3D.Dot(Vector3D.Normalize(MeVelocity), msc.WorldMatrix.Forward);
tmpddd = 1 - tmpddd;
if (tmpddd <= 0) tmpddd = 0.00001;
smokeInterval = (int)((MeVelocity.Length()) / (tmpddd * 40));
}
mySpeedToMe = Vector3D.TransformNormal(MeVelocity, refLookAtMatrix);

myShipMass = msc.CalculateShipMass();
shipMass = myShipMass.PhysicalMass;
naturalGravity = Cockpit.GetNaturalGravity();
naturalGravityLength = naturalGravity.Length();
dampenersOn = msc.DampenersOverride;

if(Head != null ) {
if (Head.IsUnderControl) {
if (legMode == 1) {
legMode = 2;
}
} else {
if (legMode == 2) {
legMode = 1;
}
}
VFTransform(legMode);
}
VFTransformLoop();

isUnderControl = false;
inputVec = Vector3D.Zero;
inputRoll = 0;
inputRota = Vector2.Zero;
if (Head != null && Head.IsUnderControl) {
inputVec = Head.MoveIndicator;
inputRoll = Head.RollIndicator;
inputRota = Head.RotationIndicator;
isUnderControl = true;
} else if (Cockpit.IsUnderControl){
inputVec = Cockpit.MoveIndicator;
inputRoll = Cockpit.RollIndicator;
inputRota = Cockpit.RotationIndicator;
isUnderControl = true;
}

// parse locktarget from fcs
if (fighterFcs != null) {
CustomConfiguration cfgTarget = new CustomConfiguration(fighterFcs);
cfgTarget.Load();

string tmpS = "";
cfgTarget.Get("Position", ref tmpS);
Vector3D.TryParse(tmpS, out LockTargetPosition);
if(LockTargetPosition==Vector3D.Zero) isSM=false;
cfgTarget.Get("Aiming", ref tmpS);
LockTargetAiming = tmpS == "True";

cfgTarget.Get("Velocity", ref tmpS);
Vector3D.TryParse(tmpS, out LockTargetVelocity);

cfgTarget.Get("Asteroid", ref tmpS);
Vector3D.TryParse(tmpS, out asteroidPosition);

cfgTarget.Get("radarHighThreatPosition", ref tmpS);
Vector3D.TryParse(tmpS, out radarHighThreatPosition);

int tmpI = 0;
cfgTarget.Get("TargetCount", ref tmpI);

int targetCount = tmpI;
LTPs.Clear();
LTVs.Clear();
for (int i = 0; i < targetCount; i++) {
Vector3D tmpP, tmpV;
cfgTarget.Get("Position"+i, ref tmpS);
Vector3D.TryParse(tmpS, out tmpP);
LTPs.Add(tmpP);
cfgTarget.Get("Velocity"+i, ref tmpS);
Vector3D.TryParse(tmpS, out tmpV);
LTVs.Add(tmpV);
}
}
var left = msc.WorldMatrix.Left;
var forward = msc.WorldMatrix.Forward;
diffX = diffGravity(left, naturalGravity, forward);
diffZ = diffGravity(forward, naturalGravity, -left);

// add before end
}
else
{
MeLastVelocity = MeVelocity;
}


}

void userControl() { 
if(isUnderControl) {

if (isSteeringAccumulate) {
double r2sRate = 0.002;
steeringAccumulation += new Vector3D(0, inputRota.Y * r2sRate, 0);
if(saIgnoreStart == -1 && steeringAccumulation.Length() < 0.5 && inputRota.Length()<0.5) {
saIgnoreStart = t;
}
if (steeringAccumulation.Length() > 0.5 || inputRota.Length() > 0.5) {
saIgnoreStart = -1;
}
if (saIgnoreStart != -1 && t > saIgnoreStart + 90) {
isSteeringAccumulate = false;
}

Vector3D nowA = msc.GetShipVelocities().AngularVelocity;
SetGyroYaw((steeringAccumulation.Y + nowA.Y * 100) * 0.01);
SetGyroPitch(-inputRota.X);
SetGyroRoll(-msc.RollIndicator);
}

if(inputVec.Z>0.1) {
aeroSLp(false);
} else if(inputVec.Z<-0.1) {
aeroSLp(true);
} else {
aeroSLz();
}
// ahh
Vector3D inputVecWorld = Vector3D.TransformNormal(inputVec, msc.WorldMatrix);
bool wantUD = false;
double udi = 0.1, uda = 0.2;
if (isAeroDynamic) {
udi = 0.5; uda = 0.7;
}
if (naturalGravity.Length() > 0.1 && inputVec.Length() >0.1) {
  wantUD = Math.Abs(inputVecWorld.Dot(Vector3D.Normalize(naturalGravity))) > udi;
}
if (!wantUD) {
if (Math.Abs(diffZ) > uda) wantUD = true;
}
if (!wantUD && isUnderControl) {
if (inputRota.Length() > 0.1
|| Math.Abs(inputRoll) > 0.1
) {
wantUD = true;
}
}

if (wantUD) {
setAhhOn(false);
}

}


if ( inputVec.Y > 0.5) {
if (lastUpT == 0 && Math.Abs(diffX)<0.2 && Math.Abs(diffZ)<0.2) {
lastUpT = t;
}
setAhhOn(false);
} else if (Math.Abs(inputVec.Y) > -0.1 && lastUpT != 0) {
if( t - lastUpT < 10 && !isNear) {
double h = 0;
if (msc.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out h)) {
ahh = h;
setAhhOn(true);
}
}
lastUpT = 0;
} else if (inputVec.Y < -0.5) {
setAhhOn(false);
}

if ( inputVec.Y < -0.5) {
if (lastDownT == 0 && Math.Abs(diffX)<0.2 && Math.Abs(diffZ)<0.2 && MeVelocity.Length() < 30) {
lastDownT = t;
}
if(isDown)resetThrusters();
isDown=false;
wingForce=false;
} else if (Math.Abs(inputVec.Y) < 0.1 && lastDownT != 0) {
if( t - lastDownT < 10) {
if(!isNear) { 
isDown = true;
downStartPos = MePosition;
isLaunch=false;
}else { 
wingForce = true;
}
}
lastDownT = 0;
} else if (inputVec.Y > 0.5) {
if(isDown) resetThrusters();
isDown = false;
wingForce = false;
}
}

void resetThrusters(bool needD = true){
if(needD)setDampenersOverride(msc, true);
for (int i = 0; i < thrusters.Count; i++) {
PlayAction(thrusters[i], "OnOff_On");
thrusters[i].SetValue("Override", 0f);
}
}

Vector3D CalculateTurretViewVector(IMyLargeTurretBase turret)
{
Vector3D direction;
Vector3D.CreateFromAzimuthAndElevation(turret.Azimuth, turret.Elevation, out direction);

return Vector3D.TransformNormal(direction, turret.WorldMatrix);
}

double diffGravity(Vector3D dir, Vector3D ng, Vector3D axis) {
if (ng.Length() == 0) return 0;
var naturalGravityLength = ng.Length();
var ngDir = Vector3D.Normalize(ng);
var vertialPlaneLaw = Vector3D.Normalize(new Vector3D(ngDir.Y * axis.Z - ngDir.Z * axis.Y,
ngDir.Z * axis.X - ngDir.X * axis.Z,
ngDir.X * axis.Y - ngDir.Y * axis.X));
var angle = Math.Asin(dir.Dot(vertialPlaneLaw));
var diff = Math.PI / 2 + angle;
var leftOrRight = Math.Acos(dir.Dot(ngDir));
if (leftOrRight > Math.PI / 2) {
diff = -diff;
}

if (Math.Abs(diff) > 0.00001f) {
return diff;
}else {
return 0;
}

}

void PlayAction(IMyTerminalBlock block, String action) {
if (block != null) {
var a = block.GetActionWithName(action);
if (a!=null) a.Apply(block);
}
}

void PlayAction(IMyTerminalBlock block, String action, List<TerminalActionParameter> args = null) {
if (block != null) {
block.GetActionWithName(action).Apply(block, args);
}
}

void PlayActionList(List<IMyTerminalBlock> blocks, String action) {
if(blocks == null) return;
for(int i = 0; i < blocks.Count; i ++)
{
blocks[i].GetActionWithName(action).Apply(blocks[i]);
}
}


void ShowLCD()
{
// Echo status:
Echo("DCS v5.6");
Echo(debugInfo);
Echo("FC:" + Cockpit.CustomName);
if (Head != null) {
Echo(" Head:" + Head.CustomName + "\n");
}
Echo("WMI Rotor Thruster\nManager... " + RunningSymbol() + "\n");
Echo($"Gyros :{Gyroscopes.Count}");
Echo($"Off-Grid Thrusters: {offGridThrustLL.Sum(l=>l.Count)} {thrusters.Count}");
string dampenerStatus = dampenersOn ? "Enabled" : "Disabled";
Echo($"Dampeners: {dampenerStatus}");
Echo($"Location: {distanceInfo}");

string br = "\n";

string info = "";
info += " Auto:  " + (flyByOn ? "Wing" : "") + (dockingOn ? "Landing" : "") + (attackMode ? "Attack" : "") + (approachIdx > 0 ? "Near" : "") + "    " + (isDown?"Down":"") + (isLaunch?"Launching":"") +  (isSM?"Speed Match":"") + (avoidOn?"Avoid":"") + (isStandBy ? "StandBy" : "") + br;
info += (motherPointerMode?"Pointing":"") + " " + (t - lastSendingTime<120 && t%60<30 ?"=>":"") + (t - lastReceivingTime<120 && t%60<30?"<=":"") + br;
info += " Speed level: " + aeroSpeedLevel + " AHH: " + ahhOn + " " + Math.Round(ahh,1) + " Steer: " + (isSteeringAccumulate?"On":"Off") +  '\n';
info += " Boosters: " + (useBst?"On":"Off") + br;
info += " Location " + distanceInfo + br;
if (gcTargetPanel != null) {
info += " Target: " + gcTargetPanel.GetPublicTitle() + "\n";
}

info += " Relative Position: (l u f h t)\n";
if (dockingList.Count > 0)
info += " Wing: " + op2npd(flyByOffsetDirection, upMode, displayVector3D(dockingList[0])) + "\n";
info += " Prepare: " + dockingForward + " " + dd(dockingUp) + " " + displayVector3D(dockingList[approachIdx]) + "\n";
if (dockingList.Count > 1)
info += " Dock: " + dockingForward + " " + dd(dockingUp) + " " + displayVector3D(dockingList[dockingList.Count - 1]) + "\n";

info += " VTA: " + VTA + "\n";
info += debugInfo + br;

for(int i = 0; i < LCD.Count; i ++)
{
IMyTextSurface lcd = LCD[i] as IMyTextSurface;
lcd.WriteText(info);
if(flyByOn || dockingOn) {
if(approachIdx > 0) lcd.FontColor = new Color(76,255,0);
else lcd.FontColor = new Color(255,76,0);
} else if (isStandBy){
lcd.FontColor = new Color(150,150,150);
} else {
if(avoidOn) lcd.FontColor = new Color(0,255,255);
else lcd.FontColor = new Color(0,76,255);
}
}

}

void recordNp(out double l, out double u , out double f, out string h, out string t) {
l = Math.Round(-myToShipPosition.X,2);
u = Math.Round(myToShipPosition.Y,2);
f = Math.Round(-myToShipPosition.Z,2);
Base6Directions.Direction s2mh = shipMatrix.GetClosestDirection(Cockpit.WorldMatrix.Forward);
switch (s2mh)
{
case Base6Directions.Direction.Forward:
h="FORWARD";
break;
case Base6Directions.Direction.Backward:
h="BACKWARD";
break;
case Base6Directions.Direction.Up:
h="UP";
break;
case Base6Directions.Direction.Down:
h="DOWN";
break;
case Base6Directions.Direction.Left:
h="LEFT";
break;
case Base6Directions.Direction.Right:
h="RIGHT";
break;
default:
h="";
break;
}
Base6Directions.Direction s2mt = shipMatrix.GetClosestDirection(Cockpit.WorldMatrix.Up);
switch (s2mt)
{
case Base6Directions.Direction.Forward:
t="FORWARD";
break;
case Base6Directions.Direction.Backward:
t="BACKWARD";
break;
case Base6Directions.Direction.Up:
t="UP";
break;
case Base6Directions.Direction.Down:
t="DOWN";
break;
case Base6Directions.Direction.Left:
t="LEFT";
break;
case Base6Directions.Direction.Right:
t="RIGHT";
break;
default:
t="";
break;
}
}

string op2npd(string dir, bool isUp, string p) {
string r="";
r += dir + " ";
r += isUp ? "U" : "D";
r += " " + p;
return r;
}

string r2 (double d) {
return "" + Math.Round(d, 2);
}

string dd(string dir) {
string r = "";
switch(dir) {
case("FORWARD"):
r = "f";
break;
case("BACKWARD"):
r = "f";
break;
case("UP"):
r = "u";
break;
case("DOWN"):
r = "d";
break;
case("LEFT"):
r = "l";
break;
case("RIGHT"):
r = "r";
break;
}
return r;
}

void SetBlocksValueColor(List<IMyTerminalBlock> blocks, String name, Color v) {
for(int i = 0; i < blocks.Count; i ++)
{
blocks[i].SetValue(name, v);
}
}

// utils
Vector3D trimSpeed(Vector3D s) {
double max = 100;
if (s.Length() > max) {
return max * Vector3D.Normalize(s);
}
return s;
}

IMyTerminalBlock getBlockByName(string name, bool sameGrid = true, bool sameName = false) {
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.SearchBlocksOfName(name, blocks);
if (sameGrid) FilterSameGrid(Me.CubeGrid, ref blocks);
if (sameName) FilterSameName(name, ref blocks);
if (blocks.Count > 0) return blocks[0];
return null;
}

List<IMyTerminalBlock> getBlockListByName(string name) {
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.SearchBlocksOfName(name, blocks);
FilterSameGrid(Me.CubeGrid, ref blocks);
return blocks;
}

void GetBlocks(String args)
{
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(blocks);
FilterSameGrid(Me.CubeGrid, ref blocks);
landingGears = blocks;
if (ptdSeparatedStart == 0 && !TestNotConnected(connectors) && !isBase) {
approachIdx=dockingList.Count - 1;
PlayActionList(connectors, "Unlock");
return;
}
if (isPrinted) {
Echo("PTD1");
if (args.Equals("PTD")) {
isPtdTurnOn = true;
cfg.Set("isPrinted","false");
cfg.Save();
}
if (!isPtdTurnOn) {
return;
}
if (ptdSeparatedStart == 0) {
blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(blocks, b => b.CubeGrid == Me.CubeGrid);
PlayActionList(blocks, "OnOff_Off");
ptdSeparatedStart = t;
}

if (ptdSeparatedStart > 0 && t > ptdSeparatedStart && t < ptdSeparatedStart + 182) {
PlayActionList(connectors, "Lock");
}

if (ptdSeparatedStart > 0 && t > ptdSeparatedStart + 182 && t < ptdSeparatedStart + 200) {
PlayActionList(connectors, "Unlock");
flyByOn = true;
approachIdx = dockingList.Count - 1;
}

if (ptdSeparatedStart == 0 || t < ptdSeparatedStart + 200) {
return;
}

}

if ((!notDocked() || dockable()) && !isBase){
//dockingOn=true;
approachIdx = dockingList.Count - 1;
}
Cockpit = getBlockByName(CockpitNameTag) as IMyShipController;
if(Cockpit == null)
{Echo(CockpitNameTag  + "RC not found"); return;}
//setDampenersOverride(Cockpit, true);
refLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(), Cockpit.WorldMatrix.Forward, Cockpit.WorldMatrix.Up);

Head = getBlockByName(HeadNameTag) as IMyShipController;

//FA86
//tryAddVftr("Hinge Land", toRa(0F), toRa(25F), toRa(25F), 0, landrList);

// ffx9
//tryAddVftr("Hinge Land 1-1", toRa(0F), toRa(80F), toRa(80F), 0, landrList);
//tryAddVftr("Hinge Land 1-2", toRa(-89F), toRa(90F), toRa(90F), 360, landrList);
//tryAddVftr("Hinge Land 2-1", toRa(0F), toRa(45F), toRa(45F), 0, landrList);
//tryAddVftr("Piston Land 2-2", 0, 4, 4, 360, landrList);
//tryAddVftr("Piston Connect", 0, 10, 10, 360, landrList);

// U5
tryAddVftr("Hinge Land", toRa(90F), toRa(0F), toRa(0F), 0, landrList);
//tryAddVftr("Leg-L1", toRa(90F), toRa(90F), 0, 0, vftrList, 0,0,0,0, 0,0,0,0, 0,0,0,0, false, 0);
tryAddVftr("Leg-L2", toRa(-88F), toRa(-88F), 0, 0, vftrList, 0,0,0,0, 0,0,0,0, toRa(60), toRa(90), toRa(-60), toRa(-90), false, 2);
tryAddVftr("Leg-L3", toRa(88F), toRa(-2F), toRa(-90F), 0, vftrList, 0,0,0,0, toRa(0), toRa(90), toRa(0), toRa(-90), toRa(0), toRa(60), toRa(0), toRa(-60), true, 4);
//tryAddVftr("Leg-R1", toRa(-90F), toRa(-90F), 0, 0, vftrList, 0,0,0,0, 0,0,0,0, 0,0,0,0, true, 1);
tryAddVftr("Leg-R2", toRa(-88F), toRa(-88F), 0, 0, vftrList, 0,0,0,0, 0,0,0,0, toRa(60), toRa(90), toRa(-60), toRa(-90), false, 3);
tryAddVftr("Leg-R3", toRa(88F), toRa(-2F), toRa(-90F), 0, vftrList, 0,0,0,0, toRa(0), toRa(90), toRa(0), toRa(-90), toRa(0), toRa(60), toRa(0), toRa(-60), true, 5);

//tryAddVftr("Arm-L1", 0, 0, toRa(-90), 0, vftrList);
//tryAddVftr("Arm-L2", 0, toRa(130F), toRa(180F), 0, vftrList);
//tryAddVftr("Arm-R1", 0, 0, toRa(90), 0, vftrList);
//tryAddVftr("Arm-R2", 0, toRa(-130F), toRa(-180F), 0, vftrList);

//tryAddVftr("Hinge Nose", toRa(90F), toRa(90F), 0, 0, vftrList);
//tryAddVftr("Piston Nose", 1.52F, 1.52F, 0, 200, vftrList);
var tbl = getBlockListByName(DCSLCDNameTag);
LCD = tbl.Select(b => { if (b is IMyTextSurfaceProvider) { return ((IMyTextSurfaceProvider)b).GetSurface(2); } else return (IMyTextSurface)b; }).ToList();

if(LCD.Count < 1)
{errorInfo = DCSLCDNameTag  + " not found";}

Gyroscopes = new List<IMyTerminalBlock>();
if(GyroscopesNameTag != "")
{
Gyroscopes = getBlockListByName(GyroscopesNameTag);
}
if(Gyroscopes.Count < 1)
{
GridTerminalSystem.GetBlocksOfType<IMyGyro> (Gyroscopes, b=>!ignoreTag(b.CustomName));
FilterSameGrid(Me.CubeGrid, ref Gyroscopes);
}
if(Gyroscopes.Count < 1)
{
errorInfo = "No Gyro";
}

setupGyroField(Gyroscopes, ref gyroYawField, ref gyroYawFactor, ref gyroPitchField, ref gyroPitchFactor, ref gyroRollField, ref gyroRollFactor, Cockpit);
if (Head!=null)
  setupGyroField(Gyroscopes, ref gyroYawFieldHead, ref gyroYawFactorHead, ref gyroPitchFieldHead, ref gyroPitchFactorHead, ref gyroRollFieldHead, ref gyroRollFactorHead, Head);
SetGyroOverride(false);

fighterFcs = getBlockByName(fighterFcsName,false,true);
if (fighterFcs != null)
{
    String cmd = "REINIT:"+CockpitNameTag+","+LCDNameTag+","+ScanRange+","+homingTurretName+","+useTurretAsAimer+","+maxTargetCount;
    TerminalActionParameter tap = TerminalActionParameter.Deserialize(cmd, cmd.GetTypeCode());
    List<TerminalActionParameter> argumentList = new List<TerminalActionParameter>();
    argumentList.Add(tap);
    PlayAction(fighterFcs, "Run", argumentList);
}

fighterFcsr = getBlockByName(FCSRNameTag,false,true);

InitSpeedControl();

GridTerminalSystem.GetBlocksOfType<IMySensorBlock> (sensors, b=>b.CubeGrid==Me.CubeGrid);

if (isBase){
downStartPos = Cockpit.GetPosition();
isStandBy=false;
}

initL2T();

GridTerminalSystem.GetBlocksOfType<IMyUserControllableGun> (smokeList, b=>b.CustomName.Contains("Smoke") && (!ignoreTag(b.CustomName)));

axisLight = getBlockListByName("Axis Light");
sLights = getBlockListByName("Speed Light");
// init new

init = true;
}

void setupGyroField(List<IMyTerminalBlock> Gyroscopes,
ref string[] gyroYawField,
ref float[] gyroYawFactor,
ref string[] gyroPitchField,
ref float[] gyroPitchFactor,
ref string[] gyroRollField,
ref float[] gyroRollFactor,
IMyShipController fc
) {
if(Gyroscopes.Count > 0)
{
gyroYawField = new string[Gyroscopes.Count];
gyroPitchField = new string[Gyroscopes.Count];
gyroYawFactor = new float[Gyroscopes.Count];
gyroPitchFactor = new float[Gyroscopes.Count];
gyroRollField = new string[Gyroscopes.Count];
gyroRollFactor = new float[Gyroscopes.Count];
for (int i = 0; i < Gyroscopes.Count; i++)
{
Base6Directions.Direction gyroUp = Gyroscopes[i].WorldMatrix.GetClosestDirection(fc.WorldMatrix.Up);
Base6Directions.Direction gyroLeft = Gyroscopes[i].WorldMatrix.GetClosestDirection(fc.WorldMatrix.Left);
Base6Directions.Direction gyroForward = Gyroscopes[i].WorldMatrix.GetClosestDirection(fc.WorldMatrix.Forward);

switch (gyroUp)
{
case Base6Directions.Direction.Up:
gyroYawField[i] = "Yaw";
gyroYawFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Down:
gyroYawField[i] = "Yaw";
gyroYawFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Left:
gyroYawField[i] = "Pitch";
gyroYawFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Right:
gyroYawField[i] = "Pitch";
gyroYawFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Forward:
gyroYawField[i] = "Roll";
gyroYawFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Backward:
gyroYawField[i] = "Roll";
gyroYawFactor[i] = GYRO_FACTOR;
break;
}

switch (gyroLeft)
{
case Base6Directions.Direction.Up:
gyroPitchField[i] = "Yaw";
gyroPitchFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Down:
gyroPitchField[i] = "Yaw";
gyroPitchFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Left:
gyroPitchField[i] = "Pitch";
gyroPitchFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Right:
gyroPitchField[i] = "Pitch";
gyroPitchFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Forward:
gyroPitchField[i] = "Roll";
gyroPitchFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Backward:
gyroPitchField[i] = "Roll";
gyroPitchFactor[i] = GYRO_FACTOR;
break;
}

switch (gyroForward)
{
case Base6Directions.Direction.Up:
gyroRollField[i] = "Yaw";
gyroRollFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Down:
gyroRollField[i] = "Yaw";
gyroRollFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Left:
gyroRollField[i] = "Pitch";
gyroRollFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Right:
gyroRollField[i] = "Pitch";
gyroRollFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Forward:
gyroRollField[i] = "Roll";
gyroRollFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Backward:
gyroRollField[i] = "Roll";
gyroRollFactor[i] = GYRO_FACTOR;
break;
}
Gyroscopes[i].SetValue("Yaw", 0f);
Gyroscopes[i].SetValue("Pitch", 0f);
Gyroscopes[i].SetValue("Roll", 0f);
Gyroscopes[i].ApplyAction("OnOff_On");
}
}
}

List<List<IMyTerminalBlock>> sortByRelativePosition(List<IMyTerminalBlock> blocks, string dir, bool isAsc) {

  IEnumerable<IGrouping<int, IMyTerminalBlock>> grouped;

  switch (dir) {
  case "X":
    grouped = blocks.GroupBy(b=>(int)Math.Round((Vector3D.TransformNormal(b.GetPosition() - Cockpit.GetPosition(), refLookAtMatrix).X * 10 )));
  break;
  case "Y":
    grouped = blocks.GroupBy(b=>(int)Math.Round((Vector3D.TransformNormal(b.GetPosition() - Cockpit.GetPosition(), refLookAtMatrix).Y * 10 )));
  break;
  case "Z":
    grouped = blocks.GroupBy(b=>(int)Math.Round((Vector3D.TransformNormal(b.GetPosition() - Cockpit.GetPosition(), refLookAtMatrix).Z * 10 )));
  break;
  case "ZX":
    grouped = blocks.GroupBy(b=>((int)Math.Round((Vector3D.TransformNormal(b.GetPosition() - Cockpit.GetPosition(), refLookAtMatrix).Z * 10 ))) * 1000 + (int)Math.Round((Vector3D.TransformNormal(b.GetPosition() - Cockpit.GetPosition(), refLookAtMatrix).X * 10 )));
  break;
  default:
  return null;
  }

  if(isAsc)
    return grouped.OrderBy(g=>g.Key).Select(g=>g.ToList()).ToList();
  else
    return grouped.OrderByDescending(g=>g.Key).Select(g=>g.ToList()).ToList();
}


void callComputer(IMyTerminalBlock computer, string cmd) {
if (computer == null) return;
    TerminalActionParameter tap = TerminalActionParameter.Deserialize(cmd, cmd.GetTypeCode());
    List<TerminalActionParameter> argumentList = new List<TerminalActionParameter>();
    argumentList.Add(tap);
    PlayAction(computer, "Run", argumentList);

}

void SetGyroOverride(bool bOverride)
{
for (int i = 0; i < Gyroscopes.Count; i++)
{
if (((IMyGyro)Gyroscopes[i]).GyroOverride != bOverride)
{
Gyroscopes[i].ApplyAction("Override");
}
}
}

float gyroDZ = 0.01F;
double rateAdjust(double r) {
return r*r*r*(60/(gyroDZ*gyroDZ));
}

void SetGyroYaw(double yawRate)
{
if (Math.Abs(yawRate) > gyroDZ) yawRate *=60;
else yawRate = rateAdjust(yawRate);
if (legMode < 2 || Head == null) {
for (int i = 0; i < Gyroscopes.Count; i++)
{
Gyroscopes[i].SetValue(gyroYawField[i], (float)yawRate * gyroYawFactor[i]);
}
} else {
for (int i = 0; i < Gyroscopes.Count; i++)
{
Gyroscopes[i].SetValue(gyroYawFieldHead[i], (float)yawRate * gyroYawFactorHead[i]);
}
}
}

void SetGyroPitch(double pitchRate)
{
if (Math.Abs(pitchRate) > gyroDZ) pitchRate *=60;
else pitchRate = rateAdjust(pitchRate);
if (legMode < 2 || Head == null) {
for (int i = 0; i < Gyroscopes.Count; i++)
{
Gyroscopes[i].SetValue(gyroPitchField[i], (float)pitchRate * gyroPitchFactor[i]);
}
} else {
for (int i = 0; i < Gyroscopes.Count; i++)
{
Gyroscopes[i].SetValue(gyroPitchFieldHead[i], (float)pitchRate * gyroPitchFactorHead[i]);
}
}
}

void SetGyroRoll(double rollRate)
{
if (Math.Abs(rollRate) > gyroDZ) rollRate *=60;
else rollRate = rateAdjust(rollRate);
if (legMode < 2 || Head == null) {
for (int i = 0; i < Gyroscopes.Count; i++)
{
Gyroscopes[i].SetValue(gyroRollField[i], (float)rollRate * gyroRollFactor[i]);
}
} else {
for (int i = 0; i < Gyroscopes.Count; i++)
{
Gyroscopes[i].SetValue(gyroRollFieldHead[i], (float)rollRate * gyroRollFactorHead[i]);
}
}
}


// speed control start
List<IMyTerminalBlock> thrusters = null;
List<IMyTerminalBlock> forwardThrustersR = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> upThrustersR = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> downThrustersR = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> backwardThrustersR = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> forwardThrusters = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> leftThrusters = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> rightThrusters = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> upThrusters = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> downThrusters = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> backwardThrusters = new List<IMyTerminalBlock>();
IMyLargeTurretBase homingTurret = null;
Vector3D maintainSpeed = Vector3D.Zero;
MatrixD shipMatrix = MatrixD.Zero;
static string debugInfo="";
static string errorInfo="";
static bool debugFix = false;
string distanceInfo="";
int i = 0;

bool flyByOn=false;
string flyByOffsetDirection="LEFT";
double flyByDistance=100;
String motherCode=null;
String sonCode="RADAR";
bool upMode=false;
string dockingForward="FORWARD";
string dockingUp="UP";
bool dockingOn=false;
string dockingApproach="DOWN";
List<Vector3D> dockingList = new List<Vector3D>();
List<Vector3D> lndList = new List<Vector3D>();
float dtp=0F;
int approachIdx = 0;
void resetApproachIdx() {
approachIdx = 0;
if (shipMatrix == MatrixD.Zero) return;
if (myToShipPosition.Length() > 200) return;
double n = 99999;
for(int i = 0; i < dockingList.Count; i++) {
var v = dockingList[i];
var l = (myToShipPosition - v).Length();
if ( l < n) {
approachIdx = i;
n = l;
}
}
if (approachIdx == dockingList.Count -1 && dockingList.Count > 1) approachIdx--;
}
string commandCache=null;
int commandStart=0;
int commandWaitTic=10;
int commandAllTic=100;

long pointDis = 1500;
bool motherPointerMode = false;
bool attackMode = false;

List<IMyTerminalBlock> connectors = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> landingGears = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> spotlights = new List<IMyTerminalBlock>();



void GetThrusters() {
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks, b => b.CubeGrid == Me.CubeGrid && !ignoreTag(b.CustomName)
&& !b.CustomName.Contains("Hydrogen")
);
if (blocks.Count == 0) {
Echo("Warning: Missing Thrusters.");
}
thrusters = blocks;
MatrixD refWorldMatrix = Cockpit.WorldMatrix;
for (int i = 0; i < thrusters.Count; i++){
Base6Directions.Direction thrusterDirection = refWorldMatrix.GetClosestDirection(thrusters[i].WorldMatrix.Backward);
switch (thrusterDirection){
case Base6Directions.Direction.Forward:
forwardThrustersR.Add(thrusters[i]);
break;
case Base6Directions.Direction.Left:
leftThrusters.Add(thrusters[i]);
break;
case Base6Directions.Direction.Right:
rightThrusters.Add(thrusters[i]);
break;
case Base6Directions.Direction.Up:
upThrustersR.Add(thrusters[i]);
break;
case Base6Directions.Direction.Down:
downThrustersR.Add(thrusters[i]);
break;
case Base6Directions.Direction.Backward:
backwardThrustersR.Add(thrusters[i]);
break;
}
}
blocks = new List<IMyTerminalBlock>();

}

// util
void setDampenersOverride(IMyTerminalBlock controller, bool onOff) {
bool nowOnOff = controller.GetValue<bool>("DampenersOverride");
if (nowOnOff != onOff) {
PlayAction(controller, "DampenersOverride");
}
}

void FilterSameGrid<T>(IMyCubeGrid grid, ref List<T> blocks) where T: class, IMyTerminalBlock
{
List<T> filtered = new List<T>();
for (int i = 0; i < blocks.Count; i++)
{
if (blocks[i].CubeGrid == grid && !blocks[i].CustomName.Contains("GE_"))
{
filtered.Add(blocks[i]);
}
}
if(filtered.Count == 0) return;
blocks = filtered;
}

void FilterSameName(String name, ref List<IMyTerminalBlock> blocks)
{
List<IMyTerminalBlock> filtered = new List<IMyTerminalBlock>();
for (int i = 0; i < blocks.Count; i++)
{
if (blocks[i].CustomName.Equals(name))
{
filtered.Add(blocks[i]);
}
}
if(filtered.Count == 0) return;
blocks = filtered;
}

public bool TestBoolValueForBlockList(List<IMyTerminalBlock> list, String name, bool value) {
for (var i=0;i<list.Count;i++){
if (name.Equals("IsConnected")) {
IMyShipConnector obj = (IMyShipConnector)list[i];
if (obj.Status.ToString().Equals("Connected")) return false;
}
if (name.Equals("IsLocked")) {
IMyLandingGear obj = (IMyLandingGear)list[i];
var builder = new StringBuilder();
obj.GetActionWithName("SwitchLock").WriteValue(obj, builder);

if (builder.ToString() == "Locked") return false;
}
}
return true;
}

public bool TestNotLocked() {
foreach ( IMyLandingGear obj in landingGears) {
var builder = new StringBuilder();
obj.GetActionWithName("SwitchLock").WriteValue(obj, builder);
if (builder.ToString().Equals("Locked")) return false;
}

return true;
}

public bool TestNotConnected(List<IMyTerminalBlock> list) {
for (var i=0;i<list.Count;i++){
IMyShipConnector obj = (IMyShipConnector)list[i];
if (obj.Status.ToString().Equals("Connected")) return false;
}
return true;
}


bool IsNotFriendly(MyDetectedEntityInfo FoundObjectInfo)
{
string ot = FoundObjectInfo.Type.ToString();
if (ot == "Planet" || ot == "Asteroid") return false;
var relationship = FoundObjectInfo.Relationship;
return (relationship != VRage.Game.MyRelationsBetweenPlayerAndBlock.FactionShare && relationship != VRage.Game.MyRelationsBetweenPlayerAndBlock.Owner);
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
CustomConfiguration cfg;
void ProcessCustomConfiguration(){
cfg = new CustomConfiguration(Me);
cfg.Load();
cfg.Get("flyByOffsetDirection", ref flyByOffsetDirection);
cfg.Get("flyByDistance", ref flyByDistance);
cfg.Get("CockpitNameTag", ref CockpitNameTag);
cfg.Get("fighterFcsName", ref fighterFcsName);
cfg.Get("LCDNameTag", ref LCDNameTag);
cfg.Get("DCSLCDNameTag", ref DCSLCDNameTag);
cfg.Get("soundBlockNameTag", ref soundBlockNameTag);
cfg.Get("upMode", ref upMode);
cfg.Get("homingTurretName", ref homingTurretName);
cfg.Get("useTurretAsAimer", ref useTurretAsAimer);
cfg.Get("ScanRange", ref ScanRange);
cfg.Get("maxAV", ref maxAV);
cfg.Get("commandWaitTic", ref commandWaitTic);
cfg.Get("commandAllTic", ref commandAllTic);
isBig = Me.CubeGrid.GridSizeEnum == MyCubeSize.Large;
cfg.Get("isDummy", ref isDummy);
cfg.Get("isBase", ref isBase);
cfg.Get("isDown", ref isDown);
cfg.Get("isME", ref isME);
cfg.Get("droneAttackRange", ref droneAttackRange);
cfg.Get("isPrinted", ref isPrinted);
cfg.Get("limitInnerRotor", ref limitInnerRotor);
cfg.Get("isAeroDynamic", ref isAeroDynamic);
cfg.Get("angleWhenDown", ref angleWhenDown);
cfg.Get("maxTargetCount", ref maxTargetCount);
cfg.Get("susMode", ref susMode);
cfg.Get("legMode", ref legMode);
cfg.Get("dockingForward", ref dockingForward);
cfg.Get("dockingUp", ref dockingUp);
cfg.Get("legMargin", ref legMargin);
// new para here

string tmps = "";
cfg.Get("dockingList", ref tmps);
parseV3L(tmps, dockingList);
cfg.Get("lndList", ref tmps);
parseV3L(tmps, lndList);
}

void parseV3L (string tmps, List<Vector3D> l) {
string[] ps = tmps.Split(';');
foreach (var s in ps) {
string[] ss = s.Split(',');
if (ss.Length < 3) continue;
Vector3D v;
try{
v = new Vector3D(Convert.ToDouble(ss[0]),Convert.ToDouble(ss[1]),Convert.ToDouble(ss[2]));
} catch {
continue;
}
l.Add(v);
}

}

void InitSpeedControl() {
GetThrusters();

List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
homingTurret = getBlockByName(homingTurretName) as IMyLargeTurretBase;
if (homingTurret == null) {
GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(blocks);
FilterSameGrid(Me.CubeGrid, ref blocks);
homingTurret = (blocks.Count > 0 ? blocks[0] as IMyLargeTurretBase : null);
}

blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(blocks, b => !b.CustomName.Contains("COMBINE"));
FilterSameGrid(Me.CubeGrid, ref blocks);
connectors = blocks;

blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyReflectorLight>(blocks);
FilterSameGrid(Me.CubeGrid, ref blocks);
spotlights = blocks;

}

double thrusterPercentX = 0;
double thrusterPercentY = 0;
double thrusterPercentZ = 0;
double lastSpeedX = 0;
double lastSpeedY = 0;
double lastSpeedZ = 0;
double[,] accleHis = new double[3,10];
int[] accleHisIdx = new int[3]{0,0,0};
double[] twRate = new double[6]{0,0,0,0,0,0}; //rludbf
void SetBlocksValueFloat(List<IMyTerminalBlock> Blocks, string ValueName, float Value)
{
for(int i = 0; i < Blocks.Count; i ++)
{
Blocks[i].SetValueFloat(ValueName, Value);
}
}

string DimSpeed(double speed, List<IMyTerminalBlock> incThrusters, List<IMyTerminalBlock> decThrusters,
ref double dimPercent, double nowSpeed, double lastSpeed, bool isApprDir, double gravityDim, int dimIdx) {
string dr = "";

// limit t when take off, prevent burn floor
var tlimit = 1d;
if (flyByOn && approachIdx > 0 ) {
if (speed > tlimit) speed = tlimit;
if (speed < - tlimit) speed = -tlimit;
}

var nowA = (nowSpeed - lastSpeed) * 60;
var needA = (speed + nowSpeed - lastSpeed);

accleHis[dimIdx,accleHisIdx[dimIdx]] = needA;
accleHisIdx[dimIdx] = (accleHisIdx[dimIdx] + 1) % 10;

var needAGra = needA - gravityDim;
dr += "needA: " + needAGra;
var needF = needAGra * shipMass;

float percent;

float maxAllFi = 0;
float maxAllFd = 0;
for (int i = 0; i < incThrusters.Count; i++) {
maxAllFi += ((IMyThrust)incThrusters[i]).MaxEffectiveThrust;
}
for (int i = 0; i < decThrusters.Count; i++) {
maxAllFd += ((IMyThrust)decThrusters[i]).MaxEffectiveThrust;
}


twRate[dimIdx*2] = maxAllFi / shipMass;
twRate[dimIdx*2 + 1] = maxAllFd / shipMass;

float maxAllF=0;
if (needF > 0) {
maxAllF = maxAllFi;
}else {
maxAllF = maxAllFd;
}

if (maxAllF == 0)percent = 0;
else percent = (float) needF / maxAllF;
dr+="\nPer: " + percent+" "+(incThrusters.Count+decThrusters.Count);
float zF = 0F;
// zF = 0.000001F;

if (percent == 0) {
for (int i = 0; i < incThrusters.Count; i++) {
((IMyThrust)incThrusters[i]).ThrustOverridePercentage = zF;
}
for (int i = 0; i < decThrusters.Count; i++) {
((IMyThrust)decThrusters[i]).ThrustOverridePercentage = zF;
}
} else if (percent > 0) {
for (int i = 0; i < incThrusters.Count; i++) {
((IMyThrust)incThrusters[i]).ThrustOverridePercentage = percent;
}
for (int i = 0; i < decThrusters.Count; i++) {
((IMyThrust)decThrusters[i]).ThrustOverridePercentage = zF;
}
} else {
for (int i = 0; i < incThrusters.Count; i++) {
((IMyThrust)incThrusters[i]).ThrustOverridePercentage = zF;
}
for (int i = 0; i < decThrusters.Count; i++) {
((IMyThrust)decThrusters[i]).ThrustOverridePercentage =  -percent;
}
}

return dr;
}

double maxTo(double src, double max) {
if(Math.Abs(src) > max) {
if (src > 0) {
return max;
} else {
return -max;
}
}else {
return src;
}
}

Vector3D calcSpeedToMeAndAvoid(Vector3D maintainSpeed, bool force = false) {
Vector3D myPosition = MePosition;
Vector3D needSpeedToMe = Vector3D.TransformNormal(maintainSpeed - MeVelocity, refLookAtMatrix);
Vector3D tmp = needSpeedToMe;
Vector3D apDiff;
double rate = 0.1;

if (((flyByOn||dockingOn) && approachIdx == 0) || force) {
long nKey = 0;
double nDis = 0;
int count = 0;
foreach(var item in avoidMap.ToList()) {
if (nKey == 0 || (item.Value - myPosition).Length() < nDis) {
nKey = item.Key;
nDis = (item.Value - myPosition).Length();
}
count ++;
}
if (count > 0) {
Vector3D thatPos = avoidMap[nKey];
apDiff = thatPos - myPosition;
if (apDiff.Length() < 50) {
Vector3D avoidSpeedToMe = Vector3D.TransformNormal(apDiff * rate * ((50-apDiff.Length())/apDiff.Length()), refLookAtMatrix);
needSpeedToMe -= avoidSpeedToMe;
}
}

if (shipPosition!=Vector3D.Zero) {
Vector3D thatPos = estShipPosition();
apDiff = thatPos - myPosition;
if (apDiff.Length() < 190) {
Vector3D avoidSpeedToMe = Vector3D.TransformNormal(apDiff * rate * 10 * ((190-apDiff.Length())/apDiff.Length()), refLookAtMatrix);
needSpeedToMe -= avoidSpeedToMe;
if (needSpeedToMe.Length()<5) needSpeedToMe *= 5/ needSpeedToMe.Length();
}
}

if (!dockingOn) {
double elevation = 0;
bool getted = Cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation);
if (getted) {
var naturalGravityNormal = Vector3D.Normalize(naturalGravity);
apDiff = naturalGravityNormal * elevation;
if (apDiff.Length() < 5000 && 500 > elevation) {
Vector3D avoidSpeedToMe = Vector3D.TransformNormal(naturalGravityNormal * (500 - elevation) * rate, refLookAtMatrix);
avoidSpeedToMe = trimSpeed(avoidSpeedToMe);
needSpeedToMe -= avoidSpeedToMe;
}
}

if (asteroidPosition != Vector3D.Zero) {
apDiff = asteroidPosition - myPosition;
if (apDiff.Length() < 2000) {
Vector3D avoidSpeedToMe = Vector3D.TransformNormal(Vector3D.Normalize(apDiff) * 100, refLookAtMatrix);
avoidSpeedToMe = trimSpeed(avoidSpeedToMe);
needSpeedToMe -= avoidSpeedToMe;
}
}
}

}

return needSpeedToMe;
}

void MaintainSpeed() {
RefreshMaintainSpeed();
if (!flyByOn && !dockingOn) return;
if(shipMatrix == MatrixD.Zero) return;
maintainSpeedToMeAA = calcSpeedToMeAndAvoid(maintainSpeed);
inputVec_RT=maintainSpeedToMeAA*0.1;

MatrixD refWorldMatrix = Cockpit.WorldMatrix;
Vector3D shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Forward, refLookAtMatrix));
MatrixD refLookAtMatrixUp = MatrixD.CreateLookAt(new Vector3D(0,0,0), refWorldMatrix.Up, refWorldMatrix.Backward);
Vector3D shipRollToMe = new Vector3D(0,0,0);
if (upMode) {
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Up, refLookAtMatrixUp));
} else {
switch(flyByOffsetDirection) {
case "LEFT":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Left, refLookAtMatrixUp));
break;
case "RIGHT":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Right, refLookAtMatrixUp));
break;
case "UP":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Up, refLookAtMatrixUp));
break;
case "DOWN":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Down, refLookAtMatrixUp));
break;
case "FORWARD":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Up, refLookAtMatrixUp));
break;
case "BACKWARD":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Backward, refLookAtMatrix));
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Up, refLookAtMatrixUp));
break;
}
}

if (flyByOn) {
if (!LockTargetPosition.Equals(Vector3D.Zero)) {
// do nothing
} else if (homingTurret!=null && homingTurret.HasTarget){
Vector3D direction;
Vector3D.CreateFromAzimuthAndElevation(homingTurret.Azimuth, homingTurret.Elevation, out direction);
Vector3D worldDirection = Vector3D.TransformNormal(direction, homingTurret.WorldMatrix);
Vector3D myDirection = Vector3D.TransformNormal(worldDirection, refLookAtMatrix);
SetGyroYaw(AimRatio * myDirection.X);
SetGyroPitch(AimRatio * myDirection.Y);
SetGyroOverride(true);
} else
{
Vector3D t2m = targetPosition - MePosition;
if (approachIdx == 0) {
var sd = shipDirectionToMe;
if (needFlyByAim) {
Vector3D flyByAimPositionToMe = Vector3D.Normalize(Vector3D.TransformNormal(flyByAimPosition - MePosition, refLookAtMatrix));
sd = flyByAimPositionToMe;
} else {
// CODING start TODO
var t2mn = Vector3D.TransformNormal(Vector3D.Normalize(t2m), refLookAtMatrix);
Vector3D hsd;
double twr = twRate[5];
double t2ml = t2m.Length();
if (twr == 0 || t2m.Length() < 500) { 
hsd = sd;
} else { 
double stopTime=MeVelocity.Length() / twr;
double stopDis = twr * twr * stopTime * 0.5;
if (Vector3D.Dot(MeVelocity, t2m)<0 || t2ml > 1.7 * stopDis) { 
hsd = t2mn;
hsPosi=true;
} else if (t2ml > 1.2 * stopDis) {
if (hsPosi) hsd = t2mn;else hsd = -t2mn;
} else { 
hsd = -t2mn;hsPosi = false;
}
}
sd = hsd;
// CODING end
SetGyroOverride(true);
}

SetGyroYaw(AimRatio*sd.X);
if (isAeroDynamic && maintainSpeedToMeAA.Y > 50) {
var b = Math.Abs(maintainSpeedToMeAA.Z);
if (b < 10) b = 10;
var dy = (float)Math.Atan2(maintainSpeedToMeAA.Y, b);
//sd.Y += dy;
}
SetGyroPitch(AimRatio*sd.Y*0.2);
sd.Z=0;


}

}

var naturalGravityNormal = Vector3D.Normalize(naturalGravity);
double sr = 0;
if (naturalGravityLength > 0) {
// VTOL roll adjust
var diff = diffX;

// roll to side speed in gravity
var sideRoll = 0D;
var mstmaax = maintainSpeedToMeAA.X;
var sideRollDZ = 0.001;
if(Math.Abs(mstmaax) > sideRollDZ) { 
var needSide = 0D;
if(mstmaax > 0) needSide = mstmaax - sideRollDZ;
else needSide = mstmaax + sideRollDZ;
sideRoll = needSide * 0.001f; // radio
}
if (sideRoll > 0.05f) sideRoll = 0.05f;
if (sideRoll < -0.05f) sideRoll = -0.05f;
if(!isBig) diff += sideRoll;
sr = -1*diff;
} else {
sr = -AimRatio*0.5*shipRollToMe.X;
}
SetGyroRoll(sr);
SetGyroOverride(true);
}


if(((flyByOn && approachIdx > 0) || dockingOn) && notDocked()) {
shipDirectionToMe = new Vector3D(0,0,0);
switch(dockingForward) {
case "LEFT":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Left, refLookAtMatrix));
break;
case "RIGHT":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Right, refLookAtMatrix));
break;
case "UP":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Up, refLookAtMatrix));
break;
case "DOWN":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Down, refLookAtMatrix));
break;
case "FORWARD":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Forward, refLookAtMatrix));
break;
case "BACKWARD":
shipDirectionToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Backward, refLookAtMatrix));
break;
}

switch(dockingUp) {
case "LEFT":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Left, refLookAtMatrixUp));
break;
case "RIGHT":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Right, refLookAtMatrixUp));
break;
case "UP":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Up, refLookAtMatrixUp));
break;
case "DOWN":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Down, refLookAtMatrixUp));
break;
case "FORWARD":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Forward, refLookAtMatrixUp));
break;
case "BACKWARD":
shipRollToMe = Vector3D.Normalize(Vector3D.TransformNormal(shipMatrix.Backward, refLookAtMatrixUp));
break;
}

SetGyroYaw(AimRatio*shipDirectionToMe.X);
SetGyroPitch(AimRatio*shipDirectionToMe.Y);
SetGyroRoll(-AimRatio*shipRollToMe.X);
SetGyroOverride(true);

}

if (flyByOn || (dockingOn && notDocked())) {
var dr= DimSpeedAll(maintainSpeedToMeAA,refLookAtMatrix);
}

}

string DimSpeedAll(Vector3D maintainSpeedToMe, MatrixD refLookAtMatrix) {
string dr = "";
Vector3D nowSpeedToMe = Vector3D.TransformNormal(Cockpit.GetShipVelocities().LinearVelocity, refLookAtMatrix);
var nrl = msc.WorldMatrix.Right.Dot(naturalGravity);
var nud = msc.WorldMatrix.Up.Dot(naturalGravity);
if(isME) nud = 0; 
var nbf = msc.WorldMatrix.Backward.Dot(naturalGravity);
DimSpeed(maintainSpeedToMe.X, rightThrusters, leftThrusters, ref thrusterPercentX, nowSpeedToMe.X, lastSpeedX, "LEFT".Equals(dockingApproach)||"RIGHT".Equals(dockingApproach), nrl,0);
lastSpeedX = nowSpeedToMe.X;
dr = DimSpeed(maintainSpeedToMe.Y, upThrusters, downThrusters, ref thrusterPercentY, nowSpeedToMe.Y, lastSpeedY, "UP".Equals(dockingApproach)||"DOWN".Equals(dockingApproach), nud,1);
lastSpeedY = nowSpeedToMe.Y;
DimSpeed(maintainSpeedToMe.Z, backwardThrusters, forwardThrusters, ref thrusterPercentZ, nowSpeedToMe.Z, lastSpeedZ, "FORWARD".Equals(dockingApproach)||"BACKWARD".Equals(dockingApproach), nbf,2);
lastSpeedZ = nowSpeedToMe.Z;
return dr;
}

bool notDocked() {
bool isNotLocked = true;
isNotLocked = TestBoolValueForBlockList(landingGears, "IsLocked", false);
if (!isNotLocked) return false;
isNotLocked = TestBoolValueForBlockList(connectors, "IsConnected", false);
if (!isNotLocked) return false;
return true;
}

bool dockable() {
foreach ( IMyShipConnector obj in connectors) {
if (! obj.Status.ToString().Equals("Connectable")
&& !obj.Status.ToString().Equals("Connected"))
return false;
}
foreach ( IMyLandingGear obj in landingGears) {
var builder = new StringBuilder();
obj.GetActionWithName("SwitchLock").WriteValue(obj, builder);
if (!builder.ToString().Equals("Ready To Lock")
&& !builder.ToString().Equals("Locked"))
return false;
}
if (landingGears.Count == 0) return false;
return true;
}

void startFlyBy() {
isDkMv=false;
dockingOn=false;
isDown=false;
resetApproachIdx();
attackMode=false;
setDampenersOverride(Cockpit, false);
PlayActionList(thrusters, "OnOff_On");
offGridThrustLL.ForEach(l=>l.ForEach(t=>t.Enabled=true));
PlayActionList(landingGears, "Unlock");
PlayActionList(landingGears, "OnOff_Off");
PlayActionList(connectors, "Unlock");
PlayActionList(connectors, "OnOff_Off");
PlayActionList(spotlights, "OnOff_On");
flyByOn = true;
callComputer(fighterFcs, "ALLON");
aeroSpeedLevel = 0;
}

void startDocking(){
isDkMv=false;
attackMode=false;
isDown=false;
setDampenersOverride(Cockpit, false);
PlayActionList(thrusters, "OnOff_On");
offGridThrustLL.ForEach(l=>l.ForEach(t=>t.Enabled=true));
PlayActionList(connectors, "OnOff_On");
PlayActionList(landingGears, "OnOff_On");
flyByOn = false;
resetApproachIdx();
callComputer(fighterFcs, "ALLOFF");
dockingOn = true;
aeroSpeedLevel = 0;
}

Dictionary<long, Vector3D> avoidMap = new Dictionary<long, Vector3D>();
Dictionary<long, long> avoidLifeTimeMap = new Dictionary<long, long>();

void ParseSensor() {
MyDetectedEntityInfo mdei = new MyDetectedEntityInfo();
foreach(var sensor in sensors) {
var tmp = sensor.LastDetectedEntity;
if (tmp.EntityId!=0) mdei = tmp;
}
if (mdei.EntityId == 0) return;
MatrixD refWorldMatrix = mdei.Orientation;
Vector3D currentPos = mdei.Position;
Vector3D speed = mdei.Velocity;
Vector3D myPosition = MePosition;

string message = sonCode + ":" + refWorldMatrix.M11+","+refWorldMatrix.M12+","+refWorldMatrix.M13+","+refWorldMatrix.M14+","+
refWorldMatrix.M21+","+refWorldMatrix.M22+","+refWorldMatrix.M23+","+refWorldMatrix.M24+","+
refWorldMatrix.M31+","+refWorldMatrix.M32+","+refWorldMatrix.M33+","+refWorldMatrix.M34+","+
currentPos.X+","+currentPos.Y+","+currentPos.Z+","+refWorldMatrix.M44+","+
speed.X+","+speed.Y+","+speed.Z;

ParseMaintainSpeed(message);

}

int adm = 20;

void ParseMaintainSpeed(string arguments) {
String[] kv = arguments.Split(':');
String[] args;

if (kv[0].Equals(sonCode+"-AVOID")) {
args=kv[1].Split(',');
avoidMap[Convert.ToInt64(args[0])] = new Vector3D(Convert.ToDouble(args[1]), Convert.ToDouble(args[2]), Convert.ToDouble(args[3]));
avoidLifeTimeMap[Convert.ToInt64(args[0])] = t;
}

foreach(var item in avoidLifeTimeMap.ToList()) {
if (t > item.Value + 120) {
avoidMap.Remove(item.Key);
avoidLifeTimeMap.Remove(item.Key);
}
}

if (! kv[0].Equals(sonCode)) return;

args = kv[1].Split(',');
switch(args[0]) { 
case "STANDBYON":
isStandBy=true;
setAhhOn(false);
setDampenersOverride(msc, false);
List<IMyTerminalBlock> nsl = new List<IMyTerminalBlock>();
nsl.AddList(upThrusters);
nsl.AddList(leftThrusters);
nsl.AddList(rightThrusters);
PlayActionList(nsl, "OnOff_Off");
SetGyroOverride(false);
break;
case "STANDBYOFF":
isStandBy=false;
isDown = false;
resetThrusters();
break;
}
if(isStandBy) return;

switch(args[0]) {
case "FLYBYON":
if (shipPosition==Vector3D.Zero) break;
commandCache="FLYBYON";
commandStart=t;
break;
case "DOCKINGON":
if (shipPosition == Vector3D.Zero) break;
commandCache="DOCKINGON";
commandStart=t;
break;
case ("LOADMISSILEON"):
//TODO
break;
case "ATTACKON":
if (flyByOn) {
attackMode=true;
}
break;
case "ATTACKOFF":
if (flyByOn) {
attackMode=false;
}
break;
case "WEAPON1":
callComputer(fighterFcs,"WEAPON1");
break;
case "WEAPON2":
callComputer(fighterFcs,"WEAPON2");
break;
case "VFTUP":
VFTransformNew(true);
break;
case "VFTDOWN":
VFTransformNew(false);
break;
case "DKMVF7":
dockMove(18);
break;
case "DKMVF6":
dockMove(16.25);
break;
case "DKMVF5":
dockMove(12.5);
break;
case "DKMVB5":
dockMove(-12.5);
break;
default:
break;
}
if(args.Count() < 19) return;

lastMotherSignalTime = t;
shipMatrix = new MatrixD(Convert.ToDouble(args[0]),Convert.ToDouble(args[1]),Convert.ToDouble(args[2]),Convert.ToDouble(args[3]),
Convert.ToDouble(args[4]),Convert.ToDouble(args[5]),Convert.ToDouble(args[6]),Convert.ToDouble(args[7]),
Convert.ToDouble(args[8]),Convert.ToDouble(args[9]),Convert.ToDouble(args[10]),Convert.ToDouble(args[11]),
Convert.ToDouble(args[12]),Convert.ToDouble(args[13]),Convert.ToDouble(args[14]),Convert.ToDouble(args[15]));

shipPosition = new Vector3D(shipMatrix.M41, shipMatrix.M42, shipMatrix.M43);
shipPositionTime = t;

MatrixD shipLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(0,0,0), shipMatrix.Forward, shipMatrix.Up);

shipSpeed = new Vector3D(Convert.ToDouble(args[16]), Convert.ToDouble(args[17]), Convert.ToDouble(args[18]));
needFlyByAim = false;
if (susMode) {
flyByAimPosition = estShipPosition();
flyByAimSpeed = Vector3D.Zero;
needFlyByAim = true;
}

if (!flyByOn) return;

if (args.Count() >= 25) {
flyByAimPosition = new Vector3D(Convert.ToDouble(args[19]),Convert.ToDouble(args[20]),Convert.ToDouble(args[21]));
flyByAimSpeed = new Vector3D(Convert.ToDouble(args[22]),Convert.ToDouble(args[23]),Convert.ToDouble(args[24]));
needFlyByAim = true;
callComputer(fighterFcs, "FLYBYAIM:"+flyByAimPosition.X+","+flyByAimPosition.Y+","+flyByAimPosition.Z);

if (args.Count() >=26) {
Vector3D dir = flyByAimPosition - estShipPosition();
dir = Vector3D.Normalize(dir);
if (!isBig) {
double standardAttackAngle = Convert.ToDouble(args[25]);
MatrixD aimMatrix;
if (naturalGravityLength > 0.01f) {
  dir = naturalGravity;
  aimMatrix = MatrixD.CreateFromDir(Vector3D.Normalize(naturalGravity), shipMatrix.Forward);
} else {
  aimMatrix = MatrixD.CreateFromDir(dir, shipMatrix.Up);
}

var angle = standardAttackAngle + (commandWaitTic * 1d / commandAllTic) * MathHelper.TwoPi;
Vector3D upBaseAim = new Vector3D(Math.Cos(angle),Math.Sin(angle),0);
Vector3D up=Vector3D.TransformNormal(upBaseAim,aimMatrix);
var ad = t%600/600f*adm;
flyByAttackPosition = flyByAimPosition + 800*up - (droneAttackRange + ad)*dir;
var tp2m = flyByAimPosition - MePosition;
var tp2mn = Vector3D.Normalize(tp2m);
var fp2m = flyByAttackPosition - MePosition;
var fp2ml = fp2m.Dot(tp2mn);
if (fp2ml > tp2m.Length()) {
var nap2m = tp2m * (tp2m.Length()-800)/tp2m.Length();
flyByAttackPosition = MePosition + nap2m;
}
}else{
Vector3D tmp = Vector3D.Reject(dir, shipMatrix.Up);
if (tmp.Equals(Vector3D.Zero)) {
tmp = shipMatrix.Forward;
}else {
tmp = Vector3D.Normalize(tmp);
}
MatrixD rd = MatrixD.CreateFromDir(tmp, shipMatrix.Up);

Vector3D off;

switch(flyByOffsetDirection) {
case "LEFT":
off = rd.Left;
break;
case "RIGHT":
off = rd.Right;
break;
default:
off = rd.Up;
break;
}

flyByAttackPosition = flyByAimPosition + 1500*off - 100*dir;

}
}

}
if (needFlyByAim == false && radarHighThreatPosition!=Vector3D.Zero) {
flyByAimPosition = radarHighThreatPosition;
needFlyByAim = true;
}
}

void RefreshMaintainSpeed(){
// 计算速度
if(shipMatrix == MatrixD.Zero) return;
distanceInfo=displayVector3D(myToShipPosition);
targetPosition = calcApproach(myToShipPosition, estShipPosition(), ref approachIdx);

if (dockingOn && approachIdx == dockingList.Count - 1) {
// to maintain some speed when docking, modify the target position deeper.
double diffStop = 0.1d;
switch(dockingApproach) {
case "LEFT":
targetPosition -= shipMatrix.Left * diffStop;
break;
case "RIGHT":
targetPosition -= shipMatrix.Right * diffStop;
break;
case "UP":
targetPosition -= shipMatrix.Up * diffStop;
break;
case "DOWN":
targetPosition -= shipMatrix.Down * diffStop;
break;
case "FORWARD":
targetPosition -= shipMatrix.Forward * diffStop;
break;
case "BACKWARD":
targetPosition -= shipMatrix.Backward * diffStop;
break;
}
}

if (attackMode) {
targetPosition = flyByAttackPosition;
}

Vector3D diffPosition = targetPosition - MePosition;

if (attackMode) {
shipSpeed = flyByAimSpeed;
}

MatrixD shipLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(0,0,0), shipMatrix.Forward, shipMatrix.Up);

double nearRange = 500;

Vector3D shipSpeedBaseShip = Vector3D.TransformNormal(shipSpeed, shipLookAtMatrix);
Vector3D diffPositionBaseShip =  Vector3D.TransformNormal(diffPosition, shipLookAtMatrix);

// 
Vector3D absDiff = diffPosition;
Vector3D shipSpeedToMe = Vector3D.TransformNormal(shipSpeed, refLookAtMatrix);

// method 2
double aRate = 0.1;
if (diffPosition.Length() < nearRange) {
isNear = true;
setAhhOn(false);
maintainSpeed = diffPosition * aRate + shipSpeed;
} else {
isNear = false;
double a = twRate[5];//use front tw as only tw
double d = (targetPosition - MePosition).Length();
double t = (MeVelocity-shipSpeed).Length() / a;
double MAX_SPEED = 95;
double sd = 0.5 * a * t * t;
Vector3D needSpeed = (targetPosition - MePosition) * aRate + shipSpeed;
Vector3D nsn = Vector3D.Normalize(needSpeed);
Vector3D msn = Vector3D.Normalize(MeVelocity);
double dot = Vector3D.Dot(nsn, msn);
if ((d - nearRange) > sd) {
if ((d - nearRange) < sd * 3  && (MeVelocity.Length()>MAX_SPEED && dot > 0.95)) {
// keep speed
maintainSpeed = MeVelocity; 
} else {
maintainSpeed = (targetPosition - MePosition) * aRate + shipSpeed;
}
} else {
// slow down to ship speed
maintainSpeed = shipSpeed;
}
}

}

string displayVector3D(Vector3D tar) {
return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
}
string displayDouble(double tar) {
return ""+Math.Round(tar, 2);
}

double adjustSpeed(double distance) {
var speed = distance;
double abs = Math.Abs(speed);
double ret = 0;
double limit = 100;
if (attackMode) limit = 500;
if (abs > limit) ret = limit /5;
else ret = abs / 5;

if (speed > 0) return ret;
else return -ret;
}

Vector3D calcApproach(Vector3D myToShipPosition, Vector3D shipPosition, ref int approachIdx) {
MatrixD refWorldMatrix = Cockpit.WorldMatrix;
MatrixD shipLookAtMatrix = MatrixD.CreateLookAt(new Vector3D(0,0,0), shipMatrix.Forward, shipMatrix.Up);
Vector3D tp = new Vector3D(0,0,0);
float delta = 2f;


var useList = dockingList;
if (lndList.Count >= dockingList.Count && dockingOn) {
useList = lndList;
}
Vector3D testP = useList[approachIdx];

if (dockingOn && isDkMv) {
testP = DkMvTarget;
}

bool canProceed = false;
if ((myToShipPosition - testP).Length() < delta) canProceed = true;

if (canProceed == true && isDkMv == false) {

if (flyByOn && approachIdx != 0) {
approachIdx --;
}

if (dockingOn && approachIdx < dockingList.Count - 1) {
approachIdx ++;
}

testP = useList[approachIdx];

}

Vector3D approachPos = shipPosition + Vector3D.TransformNormal(testP, shipMatrix);

Vector3D nextPos = Vector3D.Zero;
if (flyByOn && approachIdx > 0) {
nextPos = useList[approachIdx - 1];
}
if (dockingOn && approachIdx < useList.Count - 1) {
nextPos = useList[approachIdx + 1];
}
if (nextPos != Vector3D.Zero) {
nextPos = shipPosition + Vector3D.TransformNormal(nextPos, shipMatrix);
// judge if tarpos dir = tarspeed dir, if so, extend pos    
var tarPosDir = Vector3D.Normalize(approachPos - MePosition);
var tarVecDir = Vector3D.Normalize(MeVelocity - shipSpeed);
var nextPosDir = Vector3D.Normalize(nextPos - approachPos);
bool canUse = Vector3D.Dot(tarPosDir, tarVecDir) > 0.99;
if (flyByOn) {
canUse = canUse & Vector3D.Dot(tarPosDir, nextPosDir) > 0.3;
}
if (dockingOn) {
canUse = canUse & Vector3D.Dot(tarPosDir, nextPosDir) > 0.99;
}
if (canUse) {
approachPos += tarPosDir * Vector3D.Dot(tarPosDir, nextPosDir) * (nextPos - approachPos).Length();
}
}

return approachPos;
}

void findByGroup(string groupName, List<IMyTerminalBlock> blocks) {
var group = GridTerminalSystem.GetBlockGroupWithName(groupName);
if (group == null) return;
group.GetBlocks(blocks);
}

/*
/// Whip's Rotor Thruster Manager v29 - 11/22/17 ///

Author's Notes

I hope y'all enjoy this code. I hope it makes VTOL and vector thrust craft more feasible :)

- Whiplash141
*/

//-----------------------------------------------
//         CONFIGURABLE VARIABLES
//-----------------------------------------------

const string controlSeatNameTag = "Reference";

const string ignoredThrustNameTag = "RCS";

bool ignoreThrustersOnConnectors = true;

bool turnOnRotorThrustersWhenDisabled = false;

bool useRotorThrustAsInertialDampeners = true;

const double dampenerScalingFactor = 50;

const double fullBurnToleranceAngle = 5;

const double minDampeningAngle = 75;

bool referenceIsOnSameGridAsProgram = true; //recommended setting: true

const double updatesPerSecond = 10;
bool isSetup = false;


List<IMyShipController> referenceList = new List<IMyShipController>();
List<List<IMyThrust>> offGridThrustLL = new List<List<IMyThrust>>();
bool useBst = true;

Vector3D lastSpeedVector = new Vector3D(0,0,0);

double minDampeningDotProduct ;
double fullBurnDotProduct ;

void Main_RT(string argument, UpdateType updateType)
{
minDampeningDotProduct = Math.Cos(minDampeningAngle * Math.PI / 180);
fullBurnDotProduct = Math.Cos(fullBurnToleranceAngle * Math.PI / 180);
if (!isSetup )
{
GrabBlocks();
isSetup = true;
}
if (!isSetup)
return;


try
{
if(inputVec_RT.Length() < 0.1 && aeroSpeedLevel>0){
if(mySpeedToMe.Z > (-10*aeroSpeedLevel)){
inputVec_RT.Z = MathHelper.Clamp( ft, -1, 1);
}
}
if(notDocked()) {
// TN
Vector3D needA = inputVec_RT * 10.0; // 1G
if ((flyByOn||dockingOn) && maintainSpeedToMeAA.Length()>0) {
var ms = maintainSpeedToMeAA;
if (ms.Length() > 10f) {
ms *= 10f/ms.Length();
}
needA += ms * 1.0;
}

Vector3D mstm = mySpeedToMe;
if (dampenersOn) {
Vector3D mstmn = -1 * mstm;

if(Math.Abs(inputVec.Length()) > 0.1) {
mstmn = Vector3D.Zero;
}

if (aeroSpeedLevel>0 ) {
mstmn.Z = 0;
}

needA += mstmn; 
}

var ngtome = Vector3D.TransformNormal(naturalGravity, refLookAtMatrix);
if ((flyByOn || dockingOn) || dampenersOn) needA -= ngtome;

var desiredDirectionVec = Vector3D.TransformNormal(needA, msc.WorldMatrix);

var mass = shipMass;
Vector3D thrustNeed = mass * desiredDirectionVec;
foreach (var block in thrusters)
{
var dir = -block.WorldMatrix.Forward;
var t = (IMyThrust)block;
thrustNeed -= dir * t.CurrentThrust;
}
var thrustNeedX = Vector3D.Dot(thrustNeed, msc.WorldMatrix.Right); // tnx
thrustNeed = Vector3D.Reject(thrustNeed, msc.WorldMatrix.Left); // tn y + z

needA_G = Vector3D.TransformNormal(thrustNeed,refLookAtMatrix) / mass; //
// dead zone
float dz = 5F;
if(flyByOn||dockingOn) dz=1F;
if(ngtome.Length() > 1.5) {
var ngtomefb = ngtome;
ngtomefb.X = 0;
var ngtmn = Vector3D.Normalize(ngtomefb);
var nafb = Vector3D.Reject(needA_G, ngtmn);
if(Math.Abs(nafb.Length())<dz) {
needA_G -= nafb;
} else {
needA_G -= dz * Vector3D.Normalize(nafb);
}
} else {
if (needA_G.Length() < dz) {
needA_G = Vector3D.Zero;
} else {
needA_G -= dz * Vector3D.Normalize(needA_G);
}
}
if (needA_G.Length() < 0.02) needA_G = Vector3D.Zero;

// FEATURE 0219 multilevel maxium thrust
Vector3D thrustNeedLeft = thrustNeed;
for(int i = 0; i < 3; i ++) {
bool iH = i >0;
if (iH && !useBst) break ;
int offIdx = 0;
if(iH) offIdx = 1;
var offGridThrust = offGridThrustLL[offIdx];
double thrustMax = 0;
var cTNL = thrustNeedLeft;
if(i == 1) {
cTNL = Vector3D.Reject(thrustNeedLeft, msc.WorldMatrix.Up);
} // i == 2 thrustneedY;
if(cTNL.Length() < 0.0001) continue;
var thrustNeedDir = Vector3D.Normalize(cTNL);

foreach (IMyThrust thisThrust in offGridThrust)
{
var thrustDirection = thisThrust.WorldMatrix.Forward;
float scale = -(float)thrustDirection.Dot(thrustNeedDir);
if (scale > 0)
{
thrustMax += thisThrust.MaxEffectiveThrust * scale;
}

}
float percent;
if (thrustMax < 1) percent = 0;
percent = (float)(cTNL.Length() / thrustMax);
if (percent > 1)percent = 1;
foreach (IMyThrust thisThrust in offGridThrust)
{
var thrustDirection = thisThrust.WorldMatrix.Forward;
float scale = -(float)thrustDirection.Dot(thrustNeedDir);
if (scale > 0.1)
{
SetOffThrusterOverride(thisThrust, percent ); 
thrustNeedLeft -= thisThrust.WorldMatrix.Backward * thisThrust.MaxEffectiveThrust * percent;
}else {
SetOffThrusterOverride(thisThrust, 0f);
}
}

}//FEATURE 0219

var tn2_x = thrustNeedX;
var tnl2tome = Vector3D.TransformNormal(thrustNeedLeft, refLookAtMatrix);
var tn2_y = tnl2tome.Y;
var tn2_z = tnl2tome.Z;
manageL2T(new Vector3D(tn2_x,tn2_y,tn2_z), mass);

// END 
} else {
offGridThrustLL.ForEach(l=>l.ForEach(t=>t.ThrustOverridePercentage=0));

manageL2T(Vector3D.Zero, msc.CalculateShipMass().PhysicalMass);
}

}
catch
{
}

}

void debug(string v)
{
if (!debugFix) debugInfo += "\n" + v;
}

bool GrabBlocks()
{
if (referenceIsOnSameGridAsProgram)
GridTerminalSystem.GetBlocksOfType(referenceList, block => block.CustomName.ToLower().Contains(controlSeatNameTag.ToLower()));
else
GridTerminalSystem.GetBlocksOfType(referenceList, block => block.CustomName.ToLower().Contains(controlSeatNameTag.ToLower()));

if (referenceList.Count == 0)
{
Echo($"[ERROR]: No remote or control seat with name tag '{controlSeatNameTag}' was found");
return false;
}
List<IMyThrust> tmpTs = new List<IMyThrust>();
if (!ignoreThrustersOnConnectors)
{
GridTerminalSystem.GetBlocksOfType(tmpTs, block => block.CubeGrid != referenceList[0].CubeGrid && !ignoreTag(block.CustomName));
}
else
{
var connectors = new List<IMyShipConnector>();
var connectorGrids = new List<IMyCubeGrid>();
GridTerminalSystem.GetBlocksOfType(connectors, block => block.CubeGrid != referenceList[0].CubeGrid);

foreach (IMyShipConnector thisConnector in connectors)
{
connectorGrids.Add(thisConnector.CubeGrid);
}

GridTerminalSystem.GetBlocksOfType(tmpTs, block => (block.CubeGrid != referenceList[0].CubeGrid && !connectorGrids.Contains(block.CubeGrid) &&  !ignoreTag(block.CustomName))
||
(block.CubeGrid == Me.CubeGrid && block.CustomName.Contains("Hydrogen")
)
);
}
var l1 = tmpTs.Where(t=>!isHy(t)).ToList();
var l2 = tmpTs.Where(t=>isHy(t)).ToList();
offGridThrustLL.Add(l1);
offGridThrustLL.Add(l2);
return true;
}

IMyShipController GetControlledShipController(List<IMyShipController> SCs)
{
foreach (IMyShipController thisController in SCs)
{
if (thisController.IsUnderControl && thisController.CanControlShip)
return thisController;
}

return SCs[0];
}

void SetOffThrusterOverride(IMyThrust thruster, float percent)
{
IMyThrust t = thruster;
if(percent < 0) percent = 0;
if(percent>0){
t.ThrustOverridePercentage = percent;
t.Enabled=true;
} else {
// CODING todo
t.Enabled=false;
}
}
void SetThrusterListOverride<T>(List<T> thrusterList, float percent) where T:class, IMyTerminalBlock
{
thrusterList.ForEach(delegate(T b){
IMyThrust t = (IMyThrust)b;
if (percent < 0) {
t.ThrustOverridePercentage = 0f;
t.Enabled = true;
}else if(percent>0){
t.ThrustOverridePercentage = percent;
t.Enabled = true;
} else {
if(dampenersOn) {
t.ThrustOverridePercentage = 0.0001f;
} else {
t.ThrustOverridePercentage = 0f;
}
t.Enabled = true;
}
});
}

float GetThrusterOverride(IMyThrust thruster)
{
return thruster.GetValue<float>("Override");
}

//Whip's Running Symbol Method v6
int whip_rsv = 0;
string RunningSymbol()
{
whip_rsv++;
string s = "";

if (whip_rsv == 0)
s = "|";
else if (whip_rsv == 1)
s = "/";
else if (whip_rsv == 2)
s = "--";
else if (whip_rsv == 3)
{
s = "\\";
whip_rsv = 0;
}

return s;
}

/*
/// CHANGE LOG ///
v29
...
*/

// vtol start
double[] vt_forwardHis = new double[10];
int vt_forwardHisIdx = 0;
float forwardMoveIndicator;
long lastVacTurn = 0;
float vacForwardAngle = 0;
const int pidc= 100;
PIDController[] vtolPIDList = new PIDController[pidc];
float tl = 0f;

bool VT_init = false;
void Main_VT()
{
IMyShipController remote = null;

List<List<IMyTerminalBlock>> rotorLeft = new List<List<IMyTerminalBlock>>(), rotorRight = new List<List<IMyTerminalBlock>>();

const string rotorLeftName = "Advanced Rotor Left";
const string rotorRightName = "Advanced Rotor Right";

float targetAngleLeft, targetAngleRight, targetAngleRear; // deprecated
float leftIndicator, rightIndicator;

string error;
StringBuilder sb = new StringBuilder();
MatrixD inverse;
Vector3D localLinearVelocity, localAngularVelocity;

const float maxAngularVelovityScale = 1;

error = "";

if(remote == null)
{
remote = getBlockByName(CockpitNameTag) as IMyShipController;
}
if(remote == null)
{
error += "no remote control on the same grid.\n";
}

if(rotorLeft.Count == 0)
{
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(blocks, x => x.CustomName.Contains(rotorLeftName));
if(blocks.Count > 0)
rotorLeft = sortByRelativePosition(blocks,"X",!limitInnerRotor);
// 
var tmp = getBlockByName("Rotor Leg L2");
if (tmp != null) rotorLeft.Add(new List<IMyTerminalBlock>(){tmp});
tmp = getBlockByName("Rotor Leg L1");
if (tmp != null) rotorLeft.Add(new List<IMyTerminalBlock>(){tmp});

GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(blocks, x => x.CustomName.Contains(rotorRightName));
if(blocks.Count > 0)
rotorRight = sortByRelativePosition(blocks,"X",limitInnerRotor);
tmp = getBlockByName("Rotor Leg R2");
if (tmp != null) rotorRight.Add(new List<IMyTerminalBlock>(){tmp});
tmp = getBlockByName("Rotor Leg R1");
if (tmp != null) rotorRight.Add(new List<IMyTerminalBlock>(){tmp});

}

if (t<10) {
foreach (var l in rotorLeft) {
PlayActionList(l, "OnOff_Off");
}
foreach (var l in rotorRight) {
PlayActionList(l, "OnOff_Off");
}
return;
}

if (!VT_init) {
foreach (var l in rotorLeft) {
PlayActionList(l, "OnOff_On");
}
foreach (var l in rotorRight) {
PlayActionList(l, "OnOff_On");
}
VT_init=true;
}


// a b k
if (vtolPIDList[0] == null) {
float pp=VTOL_PID_P,pi=0F,pd=0F, pim=1F;
for (int i = 0;i < pidc; i ++) {
vtolPIDList[i] = new PIDController(pp, pi, pd,pim,-pim,60/refreshInterval);
}
}

if(error != "")
{
return;
}
if(notDocked()){
Vector3D needA = needA_G;
 if (Math.Abs(needA.Y) < 0.1 && needA.Length()> 0.1) needA.Y = 0.1;
double dz = 2;
if(flyByOn||dockingOn)dz=1F;
if (needA.Y >= 0.1 ) // 
{
tl = (float)Math.Atan2(needA.Z, needA.Y);
} else if (needA.Z > dz) {
tl = 0.5f*(float)Math.PI;
} else if (needA.Z < -dz) {
tl = -0.5f*(float)Math.PI;
}
tl = MathHelper.Clamp(tl, -0.5f*(float)Math.PI, 0.5f*(float)Math.PI);

Vector3D mstm = mySpeedToMe;
if (naturalGravityLength < 0.01f && tl == 0 && needA.Y < 2 && legMode < 2) tl = -(float)Math.PI * 0.5f;
else if (naturalGravityLength > 0.01f && isAeroDynamic &&
(aeroSpeedLevel>0 || (flyByOn&& mstm.Z<-10))
) tl = -(float)Math.PI * 0.45f;

// FEATURE 0101 thruster back default
if (useCtn && needA.Length() < dz) {
if (ctnNA == 0) ctnNA = t;
else if (t - ctnNA > 120) tl = -0.5f*(float)Math.PI;
} else {
ctnNA = 0;
}

forwardMoveIndicator = tl; 
VTA = tl;
targetAngleLeft = tl;
targetAngleRight = -tl;
targetAngleRear = 0;
} else {
targetAngleRight = 0;
targetAngleLeft = 0;
targetAngleRear = 0;
}

if (legMode == 3) {
for (int i = 0; i < 3; i++) {
processWalkAngle(i);
}
}

for(int i = 0; i < rotorLeft.Count; i++){
List<IMyTerminalBlock> rl = rotorLeft[i];
List<IMyTerminalBlock> rr = new List<IMyTerminalBlock>();
if (i < rotorRight.Count) rr = rotorRight[i];

var ta = targetAngleLeft;

float tal, tar;
tal = tar = ta;

float unmanlimit=1f;
for (int j = 0; j < rl.Count; j++) {
var ca = ((IMyMotorStator)rl[j]).Angle;
float fa;
if (i == 2) {
fa =  (float)vtolPIDList[i*j*2].Filter(tal-((IMyMotorStator)rl[j]).Angle, 2)*unmanlimit;
} else {
fa = (float)vtolPIDList[i*j*2].Filter(tal-modangle(((IMyMotorStator)rl[j]).Angle), 2)*unmanlimit;
}
rl[j].SetValueFloat("Velocity", fa);
if (rr.Count > j)
rr[j].SetValueFloat("Velocity", (float)vtolPIDList[i*j*2+1].Filter(-tar-modangle(((IMyMotorStator)rr[j]).Angle), 2)*unmanlimit);
}
}
}

static float modangle(float a) {
if (a > Math.PI) return a - (float)(2*Math.PI);
return a;
}

int walkSeq = 0;
static float mpi = (float)Math.PI;
float wa = 0.0f * (float)Math.PI;
float wab = 0.03f * (float)Math.PI;
float wba = 0.1f * (float)Math.PI;
float wbab = 0.07f * (float)Math.PI;
float wua = 0.18f * (float)Math.PI;
float wmf = 0.0f * mpi;
float bal = 0f;
bool wf = false;
PIDController wbp2 = new PIDController(2F,0.1F,0F,1F,-1F,60/refreshInterval);
int walkLastT = 0;
static float slowRate = 1.5F;
static int walkInterval = (int) (10 * slowRate);

static float[] WTA = new float[6]; //l = 024 r = 135

void processWalkAngle(int i) {
if (inputVec.Z < -0.1) wf = true;
else if (inputVec.Z > 0.1) wf = false;

var ng = naturalGravity;
var forward = msc.WorldMatrix.Forward;
var left = msc.WorldMatrix.Left;
var diff = diffX;
if (walkSeq > 3) {
diff += 0.03;
} else {
diff -= 0.03;
}

if (walkSeq == 1 || walkSeq == 2 || walkSeq == 4|| walkSeq == 5) {
if(t > walkLastT + walkInterval) {
walkSeq ++;
walkLastT = t;
}
} else {
if (wf && t > walkLastT + walkInterval*2 && Math.Abs(diffZ)<0.05) {
if (walkSeq == 3) walkSeq = 4;
else if(walkSeq ==6) walkSeq = 1;
else walkSeq = 6;
walkLastT = t;
}
if (!wf && t>walkLastT + walkInterval*2) {
walkSeq = 0;
walkLastT = t;
}
}

if (i == 0) {
//tal = tar = 0f;
return;
}

if (t > walkLastT + walkInterval * 1) {
if (walkSeq == 3) bal = MathHelper.Clamp((float)wbp2.Filter(-diff, 2), -wbab, wbab);
}

float bau = bal;

float tal = 0f, tar = 0f;

if (i == 1) {
tal = tar = 0f;
} else {
tal = tar = 0f;
}

if (walkSeq == 0) {

} else if (walkSeq==1||walkSeq==4){
if (i==1) {
tar = -wba;
tal = 0;
}else{
tar = wba + wmf;
tal = -wab;
}
} else if (walkSeq == 2 || walkSeq==5) {
if (i==1){
tar = -0;
tal = -wua;
}else{
tar = 0 + wmf;
tal = wa + wua + wmf;
}
} else if (walkSeq == 3 || walkSeq == 6) {
if (i == 1) {
tal = - wba;
tar = - wbab + bau;
} else {
tal = wa + wba;
tar = -wab + wbab - bau;
}
}

if (walkSeq > 3 && i > 0) {
var tmp = tal;
tal = tar;
tar = tmp;
}

WTA[(i-1) * 2] = tal * slowRate;
WTA[(i-1) * 2 + 1] = tar * slowRate;

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

public double Filter(double input, int r, double cu, double step = 0.1)
{
  var w = Filter(input, r);
  return MathHelper.Clamp(w, cu - step, cu + step);
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

static float toRa(float i) {
return (i / 180F) * mpi;
}

List<IMyMotorStator> l2tRlY = new List<IMyMotorStator>();
List<IMyMotorStator> l2tRlX = new List<IMyMotorStator>();
List<bool> l2tSl = new List<bool>();
List<IMyThrust> l2tl = new List<IMyThrust>();
List<PIDController> l2tPlY = new List<PIDController>();
List<PIDController> l2tPlX = new List<PIDController>();
void initL2T() {
getBlockListByTN(ref l2tRlY, "L2T-Y");
float pp=10F,pi=0F,pd=0F, pim=1F;
foreach(var r in l2tRlY) {
l2tPlY.Add(new PIDController(pp, pi, pd,pim,-pim,60/refreshInterval));
l2tSl.Add(false);
}
getBlockListByTN(ref l2tRlX, "L2T-X");
foreach(var r in l2tRlX) {
l2tPlX.Add(new PIDController(pp, pi, pd,pim,-pim,60/refreshInterval));
}
getBlockListByTN(ref l2tl, "L2T");
}

void getBlockListByTN<T> (ref List<T> blocks, string name, bool sameGrid = false) where T:class, IMyTerminalBlock {
GridTerminalSystem.GetBlocksOfType<T>(blocks, x => x.CustomName.Contains(name) && (!sameGrid || x.CubeGrid == Me.CubeGrid));
}

void doRotorList(List<IMyMotorStator> rl, List<PIDController> pl, double ta, bool keep, List<bool> sl, bool f = false) {
for (int i = 0; i < rl.Count; i++) {
var r = rl[i];
var p = pl[i];
var ia = (float)p.Filter(ta - modangle(r.Angle),2, r.TargetVelocityRPM, 10);
if(!f && (keep || sl[i])) ia = 0;
r.SetValueFloat("Velocity", ia);
}
}

class MinQ {
Vector3D[] q;
int idx;
public MinQ(int l) {
q = new Vector3D[l];
for(int i = 0; i < l ; i++) {
q[i] = Vector3D.Zero;
}
}
public Vector3D f(Vector3D i) {
q[idx] = i;
var dm = Double.MaxValue;
Vector3D m = new Vector3D(dm, dm, dm);
foreach(var e in q) {
if (e.Length() < m.Length()) m = e;
}
idx = (idx +1) % q.Length;
return m;
}
}

MinQ l2tMin = new MinQ(10);

void manageL2T(Vector3D tn2mi, double mass) {
if (mass <= 0) return;
var tn2m = l2tMin.f(tn2mi);
if (isDown) {
doRotorList(l2tRlY, l2tPlY, -0.5, false, l2tSl,true);
doRotorList(l2tRlX, l2tPlX, 0, false, l2tSl,true);
SetThrusterListOverride(l2tl, 0);
return;
}

// dz
double dz = 2;
var nai = tn2mi / mass;
if (nai.Length() < 0.1 || nai.Z > -dz) {
doRotorList(l2tRlY, l2tPlY, 0, false, l2tSl, true);
doRotorList(l2tRlX, l2tPlX, 0, false, l2tSl, true);
SetThrusterListOverride(l2tl, 0);
return;
}


if (tn2m.Length() < dz * mass) {
tn2m = Vector3D.Zero;
} else {
tn2m -= dz * mass * Vector3D.Normalize(tn2m);
}

var na = tn2m / mass;

if(na == Vector3D.Zero) {
doRotorList(l2tRlY, l2tPlY, 0, true, l2tSl);
doRotorList(l2tRlX, l2tPlX, 0, true, l2tSl);
SetThrusterListOverride(l2tl, 0);
return;
}

double maxThrust = 0;
var naw = Vector3D.Normalize(Vector3D.TransformNormal(-na, msc.WorldMatrix));
double scaleMatch = 0.7;
double scaleMatchA = 0.98;
float[] ts = new float[l2tl.Count];
for(int i = 0; i < l2tl.Count; i++) {
var t = l2tl[i];
var d = t.WorldMatrix.Forward;
float scale = (float)d.Dot(naw);
ts[i] = scale;
if (l2tSl[i] && scale < scaleMatch) l2tSl[i] = false;
if (!l2tSl[i] && scale > scaleMatchA) l2tSl[i] = true;
if (l2tSl[i])
{
maxThrust += t.MaxEffectiveThrust * scale;
}
}
float percent = 0;
if(maxThrust > 0) {
percent = (float)(tn2m.Length() / maxThrust);
}

for(int i = 0; i < l2tl.Count; i++) {
var t = l2tl[i];
if (l2tSl[i])
{
t.ThrustOverridePercentage = percent * ts[i];
}else {
if(dampenersOn) {
t.ThrustOverridePercentage = 0.0001f;
} else {
t.ThrustOverridePercentage = 0f;
}
}
if (percent < 0.01) l2tSl[i] = false;
}

double dz2 = 2;
na.Y = dzd(na.Y, dz2);
na.X = dzd(na.X, dz2);
if (na.Z > -5) na.Z = -5;

var ay = Math.Atan2(-na.Y, -na.Z);
var ax = Math.Atan2(-na.X, -na.Z);

doRotorList(l2tRlY, l2tPlY, ay, false, l2tSl);
doRotorList(l2tRlX, l2tPlX, ax, false, l2tSl);

}

double dzd(double i, double d) {
if (Math.Abs(i) < d) {
return 0;
} else {
if (i > 0) return i - d;
else return i + d;
}
}

Vector3D DkMvTarget = Vector3D.Zero;
bool isDkMv = false;
void dockMove(double f) {
DkMvTarget = myToShipPosition + new Vector3D(0,0,-f);
flyByOn = false;
dockingOn = true;
isDkMv = true;
approachIdx = dockingList.Count - 1;
}

bool isHy(IMyTerminalBlock b) {
return b.CustomName.Contains("Hydrogen");
}


#endregion
    }
}


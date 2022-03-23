using Sandbox.ModAPI.Ingame;
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

namespace karm_p
{
    public class Program : MyGridProgram
    {
#region ingamescript
/// <summary>
/// start 
/// </summary>
long LOCK_WAIT_FRAME = 3;
double ARM_LENGTH = 5;

long t = 0;
bool inited=false;

static string debugInfo = "";
string initDebug = "";

class ArmGroup
{
public IMyMotorStator rotor1 = null, rotor2 = null, rotor3 = null;
public PIDController pid1 = new PIDController(20f, 0.1f, 0f, 1f, -1f, 60),
pid2 = new PIDController(20f, 0.1f, 0f, 1f, -1f, 60),
pid3 = new PIDController(20f, 0.1f, 0f, 1f, -1f, 60),
pidp = new PIDController(20f, 0.1f, 0f, 1f, -1f, 60);

public Vector2D endPos = new Vector2D(0, 1);
public Vector2D[] endPosCfg = new Vector2D[4];
public Vector2D curPos = Vector2D.Zero;
public Vector2D curSpeed = Vector2D.Zero;
public double cA1, cA2, cA3;
public double cS1, cS2, cS3;
public IMyLandingGear magnet;
public int magMode = 0;
public long tLockStart = 0;
public int lastMagMode = 0;
}
Dictionary<string, ArmGroup> armMap = new Dictionary<string, ArmGroup>();
IMyTerminalBlock radarComputer;

public Program() 
{ 
  Runtime.UpdateFrequency = UpdateFrequency.Update1; 
} 

public void Main(string argument, UpdateType updateSource) 
{ 
  if (!inited) { 
    init(); 
    return; 
  }
  debugInfo = "";
  parseArg(argument);

  update();

  op();

  show();

  t++;
}

private void parseArg(string argument)
{
    var kv = argument.Split(':');
    if(kv.Length<2) return;
    switch(kv[0])
    {
                case "SET":
                    {
                        var paras = kv[1].Split(',');
                        if (paras.Length >= 2 && armMap.ContainsKey(paras[0]))
                        {
                            ArmGroup arm;
                            armMap.TryGetValue(paras[0],out arm);
                            int pi = 0;
                            int.TryParse(paras[1], out pi);
                            if(pi < arm.endPosCfg.Length)
                            {
                                arm.endPos = arm.endPosCfg[pi];
                                arm.lastMagMode = arm.magMode;
                                if (pi == 0)
                                {
                                    arm.magMode = 2;
                                    arm.rotor1.SetValueBool("ShareInertiaTensor", false);
                                    arm.rotor2.SetValueBool("ShareInertiaTensor", false);
                                    arm.rotor3.SetValueBool("ShareInertiaTensor", false);
                                } else if(pi == 1)
                                {
                                    arm.magMode = 1;
                                    arm.rotor1.SetValueBool("ShareInertiaTensor", true);
                                    arm.rotor2.SetValueBool("ShareInertiaTensor", true);
                                    arm.rotor3.SetValueBool("ShareInertiaTensor", true);
                                } else
                                {
                                    arm.magMode = 0;
                                    arm.rotor1.SetValueBool("ShareInertiaTensor", true);
                                    arm.rotor2.SetValueBool("ShareInertiaTensor", true);
                                    arm.rotor3.SetValueBool("ShareInertiaTensor", true);
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
    }
    // debugInfo += $"\n {Math.Round(endPos.X,2)} {Math.Round(endPos.Y,2)}";
}

void update()
{
foreach (var arm in armMap.Values)
{
var nca1 = toDa(arm.rotor1.Angle);
var nca2 = toDa(arm.rotor2.Angle);
var nca3 = toDa(arm.rotor3.Angle);
if (arm.cA1 != 0) arm.cS1 = nca1 - arm.cA1;
if (arm.cA2 != 0) arm.cS2 = nca2 - arm.cA2;
if (arm.cA3 != 0) arm.cS3 = nca1 - arm.cA3;
arm.cA1 = nca1; arm.cA2 = nca2; arm.cA3 = nca3;

var ab = (90 + arm.cA2) * 0.5;
var armLength = 2 * Math.Sin(toRa(ab)) * ARM_LENGTH;
var aa = 90 - ab;
var ac = 90 + arm.cA1 - aa;
var ncurPos = new Vector2D(armLength * Math.Cos(toRa(ac)), armLength * Math.Sin(toRa(ac)));
if (arm.curPos != Vector2D.Zero) arm.curSpeed = ncurPos - arm.curPos;
arm.curPos = ncurPos;
}
}

private double toRa(double v)
{
return (v / 180) * Math.PI;
}

void show() {
Echo(initDebug);
Echo(debugInfo);
}

void init() {
initDebug = "init start";
List<IMyTerminalBlock> tmpL = new List<IMyTerminalBlock>();
List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
GridTerminalSystem.GetBlockGroups(groups, g=>g.Name.ToLower().Contains("[karm]"));
foreach(var g in groups)
{
g.GetBlocksOfType<IMyMotorStator> (tmpL, b => b.CustomName.Contains("Arm-L1"));
if (tmpL.Count < 1) continue;
ArmGroup arm = new ArmGroup();
arm.rotor1 = (IMyMotorStator)tmpL[0];
var cfg = new CustomConfiguration(arm.rotor1);
cfg.Load();
var posString = cfg.Get("POS");
if (posString == null || posString.Length == 0 || posString.Split(';').Length<2) { 
arm.endPosCfg[0] = arm.endPosCfg[1] = arm.endPosCfg[2] = arm.endPosCfg[3] = new Vector2D(0,1);
}else { 
var posSA = posString.Split(';');
for (int i = 0; i < posSA.Length; i++) { 
var posSS = posSA[i].Split(',');
if (posSS.Length < 2) { 
arm.endPosCfg[i] = new Vector2D(0,1);
continue;
}
double x, y;
bool s = true;
s&=double.TryParse(posSS[0], out x);
s&=double.TryParse(posSS[1], out y);
if(s) { 
arm.endPosCfg[i] = new Vector2D(x,y);
}
else
arm.endPosCfg[i] = new Vector2D(0,1);
}
}
g.GetBlocksOfType<IMyMotorStator> (tmpL, b => b.CustomName.Contains("Arm-L2"));
if (tmpL.Count < 1) continue;
arm.rotor2 = (IMyMotorStator)tmpL[0];
g.GetBlocksOfType<IMyMotorStator> (tmpL, b => b.CustomName.Contains("Arm-L3"));
if (tmpL.Count < 1) continue;
arm.rotor3 = (IMyMotorStator)tmpL[0];

g.GetBlocksOfType<IMyLandingGear> (tmpL, b => b.CustomName.Contains("Arm"));
if (tmpL.Count > 0) arm.magnet = (IMyLandingGear)tmpL[0];
var key = cfg.Get("ID");
if(key == null || key.Equals("")) continue;
armMap.Add(key, arm);
}
GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock> (tmpL, b => b.CustomName.Contains("radar"));
if (tmpL.Count > 0) radarComputer = tmpL[0];

inited=true;
initDebug = "init complete " + armMap.Count;
}

void op() {

getInput();
foreach(var arm in armMap.Values) { 
debugInfo += $"\n {Math.Round(arm.curPos.X,2)} {Math.Round(arm.curPos.Y,2)}";
var nr = arm.endPos - arm.curPos;
var nl = nr.Length();
var tarPos = arm.endPos;
if (nl > 0.1)
{
var cs = arm.curSpeed.Length();

var tdl = arm.pidp.Filter2(nl, 2, cs);
tarPos = Vector2D.Normalize(nr) * tdl + arm.curPos;
}

var tac = Math.Atan2(tarPos.Y, tarPos.X);
var taa = Math.Acos(tarPos.Length() * 0.5 / ARM_LENGTH);
var tab = Math.PI * 0.5 - taa;

tac = toDa(tac);
tab = toDa(tab);
taa = toDa(taa);

var ra1 = tac + taa - 90;
var ra2 = tab * 2 - 90;
var ra3 = 90 - (tac - taa);
debugInfo += $"\n {Math.Round(tac,2)} {Math.Round(tab)} {Math.Round(taa)} {Math.Round(ra1)} {Math.Round(ra2)} {Math.Round(ra3)}";

arm.rotor1.SetValueFloat("Velocity", (float)arm.pid1.Filter(toRa(ra1)- arm.rotor1.Angle,2));
arm.rotor2.SetValueFloat("Velocity", (float)arm.pid2.Filter(toRa(ra2) - arm.rotor2.Angle, 2));
arm.rotor3.SetValueFloat("Velocity", (float)arm.pid3.Filter(toRa(ra3)- arm.rotor3.Angle,2));

if(arm.magnet != null) { 
if(arm.magMode == 1 || arm.magMode == 0) { 
if(!arm.magnet.IsLocked && arm.magnet.DetailedInfo.Contains("Ready To Lock")&& arm.tLockStart ==0) {
arm.tLockStart = t;
var mp = arm.magnet.GetPosition();
callComputer(radarComputer, "ArmLock:"+mp.X +","+mp.Y+","+mp.Z);
}
if(arm.tLockStart !=0 && t > arm.tLockStart + LOCK_WAIT_FRAME) { 
arm.magnet.Lock();
arm.tLockStart = 0;
}
}else if (arm.magMode == 2) {
if(arm.magnet.IsLocked) {
arm.magnet.Unlock();
var mp = arm.magnet.GetPosition();
if(arm.lastMagMode == 0)
callComputer(radarComputer, "ArmUnlock:"+mp.X + "," + mp.Y + "," + mp.Z);
}
}
}
}
}

private double toDa(double tac)
{
    return (tac / Math.PI) * 180;
}

void getInput() {

}


// util
public class PIDController {
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

public double Filter2(double input, int round_d_digits, double cs)
{
var output = Filter(input, round_d_digits);
if (Math.Abs(output) < Math.Abs(cs)) return output;
var limit = Math.Abs(cs) * 0.1 + 0.5; // start Ratio TODO
return MathHelper.Clamp(output, -limit, limit);
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

static float modangle(float a) {
if (a > Math.PI) return a - (float)(2*Math.PI);
return a;
}

string displayVector3D(Vector3D tar) {
return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
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
void callComputer(IMyTerminalBlock computer, string cmd) {
if (computer == null) return;
    TerminalActionParameter tap = TerminalActionParameter.Deserialize(cmd, cmd.GetTypeCode());
    List<TerminalActionParameter> argumentList = new List<TerminalActionParameter>();
    argumentList.Add(tap);
    PlayAction(computer, "Run", argumentList);

}
void PlayAction(IMyTerminalBlock block, String action, List<TerminalActionParameter> args = null) {
if (block != null) {
block.GetActionWithName(action).Apply(block, args);
}
}

#endregion
    }
}

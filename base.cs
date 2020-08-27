long t = 0;
bool inited=false;

static string debugInfo = "";

IMyMotorStator rotor = null;
IMyShipController cockpit = null;

Vector3D inputVec = Vector3D.Zero;
int rLevel = 0;
long lastOp = 0;
PIDController pid = new PIDController(20f, 0.1f, 0f, 1f, -1f, 60);

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

  op();

  show();

  t++;
}

void show() {
Echo(debugInfo);
}

void init() {
List<IMyTerminalBlock> tmpL = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyShipController> (tmpL);
if (tmpL.Count < 1) return;
cockpit = (IMyShipController)tmpL[0];
GridTerminalSystem.GetBlocksOfType<IMyMotorStator> (tmpL, b => b.CustomName.Contains("Rudder"));
if (tmpL.Count < 1) return;
rotor = (IMyMotorStator)tmpL[0];
inited=true;

}

void op() {
debugInfo = "op";

getInput();
debugInfo += "\ngetInput";

float ta = rLevel * 5;
debugInfo += "\nta: " + ta;

rotor.SetValueFloat("Velocity", (float)pid.Filter((ta/180F)*Math.PI - modangle(rotor.Angle),2));


}

void getInput() {
if (t < lastOp + 30) return;
inputVec = cockpit.MoveIndicator;
debugInfo += displayVector3D(inputVec);

if (inputVec.X > 0.5) {
if (rLevel < 3) rLevel ++;
lastOp = t;
} else if (inputVec.X < -0.5) {
if (rLevel > -3) rLevel --;
lastOp = t;
}
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

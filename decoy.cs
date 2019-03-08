long t = 0;
bool inited = false;

List<IMyTerminalBlock> gatList = new List<IMyTerminalBlock>();
IMyTerminalBlock rotor = null;

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

  t++;
}

void init() {
  GridTerminalSystem.GetBlocksOfType<IMySmallGatlingGun> (gatList, b => b.CubeGrid == Me.CubeGrid);
  List<IMyTerminalBlock> tmpList = new List<IMyTerminalBlock>();
  GridTerminalSystem.GetBlocksOfType<IMyMotorStator> (tmpList, b => b.CubeGrid == Me.CubeGrid);
  if (tmpList.Count > 0) {
    rotor = tmpList[0];
  }
  inited = true;
}

void op() {
  bool fire = false;

  IMyMotorStator rotorTyped = (IMyMotorStator) rotor;

  float[] limit = new float[]{0F, 0.25F, 0.5F, 0.75F};
  float a = rotorTyped.Angle;
  for (int i = 0; i < 4; i++) {
    if (a > (limit[i] - 0.01) * MathHelper.TwoPi && a < (limit[i] + 0.01) * MathHelper.TwoPi) {
      fire = true;
    }
  }

  if (fire) {
    for (int i = 0; i < gatList.Count; i ++) {
      PlayAction(gatList[i], "ShootOnce");
    }
  }
}

void PlayAction(IMyTerminalBlock block, String action, List<TerminalActionParameter> args = null) { 
    if (block != null) { 
        block.GetActionWithName(action).Apply(block, args); 
    } 
}

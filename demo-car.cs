long t = 0;
bool inited = false;

IMyTerminalBlock rotorLeft = null;
IMyTerminalBlock rotorRight = null;

Vector3D inputVec;
IMyShipController Cockpit;


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

List<List<IMyTerminalBlock>> sortByPosition(List<IMyTerminalBlock> blocks, string dir, bool isAsc) { 
  IEnumerable<IGrouping<int, IMyTerminalBlock>> grouped; 
  switch (dir) { 
  case "X": 
    grouped = blocks.GroupBy(b=>(int)b.Position.X); //Position means relative pos to ship, GetPosition() mean absolute pos to world. 
  break; 
  case "Y": 
    grouped = blocks.GroupBy(b=>(int)b.Position.Y); 
  break; 
  case "Z": 
    grouped = blocks.GroupBy(b=>(int)b.Position.Z); 
  break; 
  default: 
  return null; 
  } 
 
  if(isAsc) 
    return grouped.OrderBy(g=>g.Key).Select(g=>g.ToList()).ToList(); 
  else 
    return grouped.OrderByDescending(g=>g.Key).Select(g=>g.ToList()).ToList(); 
}


void init() {
  List<IMyTerminalBlock> tmpList = new List<IMyTerminalBlock>();
  GridTerminalSystem.GetBlocksOfType<IMyMotorStator> (tmpList, b => b.CubeGrid == Me.CubeGrid);

  List<List<IMyTerminalBlock>> sortedRotors = sortByPosition(tmpList, "X", true);

  rotorLeft = sortedRotors[0][0];
  rotorRight = sortedRotors[1][0];

  GridTerminalSystem.GetBlocksOfType<IMyShipController> (tmpList, b => b.CubeGrid == Me.CubeGrid);
  Cockpit = (IMyShipController) tmpList[0];

  inited = true;
}

void op() {
  inputVec = Cockpit.MoveIndicator; 

  if (inputVec.X < -0.1) {
    rotorLeft.SetValue<float>("Velocity", -10);
    rotorRight.SetValue<float>("Velocity", 20);
  } else if (inputVec.X > 0.1) {
    rotorLeft.SetValue<float>("Velocity", -20);
    rotorRight.SetValue<float>("Velocity", 10);
  } else if (inputVec.Z < -0.1) {
    rotorLeft.SetValue<float>("Velocity", -20);
    rotorRight.SetValue<float>("Velocity", 20);
  } else if (inputVec.Z > 0.1) {
    rotorLeft.SetValue<float>("Velocity", 20);
    rotorRight.SetValue<float>("Velocity", -20);
  } else {
    rotorLeft.SetValue<float>("Velocity", 0);
    rotorRight.SetValue<float>("Velocity", 0);
  }

}

void PlayAction(IMyTerminalBlock block, String action, List<TerminalActionParameter> args = null) { 
    if (block != null) { 
        block.GetActionWithName(action).Apply(block, args); 
    } 
}

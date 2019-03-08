long t = 0;

List<IMyTerminalBlock> gatList = new List<>();
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

init() {
  
}

op() {
}

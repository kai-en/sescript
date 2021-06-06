static double STDBY_DISTANCE = 200;
static int f_no = 0;
double GUILD_RATE = 0.3;
double ATAN_BASE = 0.1;
double APID_P = 3;
double APID_D = 1.0;

double F_MASS = 1428.8;
double LaunchDist = 20;
int dropTime = 0;
bool isClearDown=false;
	    
string debugInfo = "";
string debugInfoF = "";
long lastDebugInfoF = 0;
//Vector3D testP = Vector3D.Zero;
//Vector3D testV = Vector3D.Zero;
//Vector3D[] testList = new Vector3D[100];
//int maxTestList = 100;
//int idxTestList = 0;
//long errorStart = 0;

string FTag = "#B#"+f_no;

string VERSION = "0.1";
const float mpi = (float)Math.PI;

static int commandMode = 0; // 0 standby, 1 attack, 2 supply
const int CM_ATK = 1;
const int CM_STD = 0;
const int CM_SUP = 2;

double xo=0, yo=-0.4, zo=0.5;
static int refreshRate = 60;

double Weapon_1_BulletInitialSpeed = 300; //武器1，子弹初速度，加特林机枪的默认子弹初速度是400   
double Weapon_1_BulletAcceleration = 0; //武器1，子弹加速度，加特林机枪默认子弹加速度是0   
double Weapon_1_BulletMaxSpeed = 300; //武器1，子弹最大速度，加特林机枪默认最大速度是400   

// global var
long timestamp = 0;
Vector3D ng;
Random rnd = new Random();

            //Constants And Classes
            #region Consts & Classes
            //Classes
            class FUNNEL
            {
                //Terminal Blocks On Each Funnel
                public IMyTerminalBlock GYRO;
                public IMyTerminalBlock TURRET;
                public IMyTerminalBlock MERGE;
                public List<IMyThrust> THRUSTERS = new List<IMyThrust>(); //Multiple
                public IMyTerminalBlock POWER;
                public List<IMyTerminalBlock> WARHEADS = new List<IMyTerminalBlock>(); //Multiple
	    public List<IMyTerminalBlock> SPOTLIST;
                public List<IMyTerminalBlock> GUNS = new List<IMyTerminalBlock>(); //Multiple

                //Permanent Funnel Details
                public double MissileAccel = 10;
                public double MissileThrust = 0;
                public double MissileMass = 0;
                public bool IsLargeGrid = false;
                public double FuseDistance = 2;
                public bool isRocket = false;
                public int runningMode = 0;
                public int runningSubMode = 0;
                public int hingeIdx = -1;
                public long lastSupStart = -7200;
                public long lastTimeStamp = 0;
                public long lastInRangeStart = 0;
                public bool allowAttack = true;
                // public const int maxHis = 200;
                // public double[] hisVelocity = new double[maxHis];
                // public int hisIdx = 0;

                //Runtime Assignables
                public bool HAS_DETACHED = false;
                public bool IS_CLEAR = false;
                public Vector3D TARGET_PREV_POS = new Vector3D();
                public Vector3D MIS_PREV_POS = new Vector3D();

                public double PREV_Yaw = 0;
                public double PREV_Pitch = 0;

	    // a b K
	    public int delayStartCount = 0;
	    public Vector3D TargetVelocity = Vector3D.Zero;
	    public Vector3D TargetVelocityPanel = Vector3D.Zero;

	    public double nearest = 1000000;
	    public Vector3D lastVelocity = Vector3D.Zero;
	    public Vector3D lastAVelocity = Vector3D.Zero;
	    public Vector3D lastForward = Vector3D.Zero;
	    public PIDController pidA = new PIDController(1F, 0F, 2F, 1F, 1F, refreshRate);
	    public PIDController pidE = new PIDController(1F, 0F, 2F, 1F, 1F, refreshRate);
	    public PIDController pidR = new PIDController(1F, 0F, 2F, 1F, 1F, refreshRate);
	    public PIDController pidT = new PIDController(1F, 0F, 0.5F, 1F, 1F, refreshRate);

                // funnel attack offset
                public Vector3D atkOffset = Vector3D.Zero;
                public long atkOffsetStart = 0;
	    }
            List<FUNNEL> FUNNELS = new List<FUNNEL>();

            //Consts
            double Global_Timestep = 1.0 / refreshRate;
            double PNGain = 3;
            double ThisShipSize = 10;
            string Lstrundata = "Please ensure you have read the \n setup and hints & tips, found within \n the custom data of this block\n";
            IEnumerator<bool> LaunchStateMachine;
	// a b K
	IMyTextPanel gcTargetPanel = null;
	String gcTargetPanelName="LCD Panel GC Target"+f_no;
	    IMyTerminalBlock fcsComputer = null;
	    String fcsComputerName="fcs";
List<Vector3D> LTPs = new List<Vector3D>();
List<Vector3D> LTVs = new List<Vector3D>();
	
            IMyShipController RC;
            List<IMyMotorStator> hingeList = new List<IMyMotorStator>();
            #endregion


            //Initialiser
            #region Setup Stages
            Program()
            {
                //Sets Runtime
                Runtime.UpdateFrequency = refreshRate == 6? UpdateFrequency.Update10 : UpdateFrequency.Update1;
                //Runtime.UpdateFrequency = UpdateFrequency.Update1;

                //Setup String
                string SetupString = "Kaien's Funnel System, Version " + VERSION; 
                Me.CustomData = SetupString;

                //Sets ShipSize
                ThisShipSize = (Me.CubeGrid.WorldVolume.Radius);
                ThisShipSize = LaunchDist == 0 ? ThisShipSize : LaunchDist;

                //Collects RC Unit 
                List<IMyTerminalBlock> TempCollection3 = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyShipController>(TempCollection3, b => b.CustomName.Contains("Reference"));
                if (TempCollection3.Count > 0)
                {RC = TempCollection3[0] as IMyShipController;}

	    // a b K
                List<IMyTerminalBlock> TempCollection4 = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TempCollection4, a => a.CustomName.Equals(gcTargetPanelName) );
	    if (TempCollection4.Count > 0)
                {gcTargetPanel = TempCollection4[0] as IMyTextPanel;}

		GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(TempCollection4, a => a.CustomName.Contains(fcsComputerName) );
	    if (TempCollection4.Count > 0)
                {fcsComputer = TempCollection4[0];}

                GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(hingeList, a => a.CustomName.Contains(FTag) );

            }
            #endregion

            //Main Method
            #region Main Method
            void Main(string argument)
            {
                timestamp ++;
                if (RC != null ) {
                ng = RC.GetNaturalGravity();
                }
                //General Layout Diagnostics
                OP_BAR();
                QuickEcho(FUNNELS.Count, "Active (Fired) Funnels:");
                QuickEcho(Runtime.LastRunTimeMs, "Runtime:");
                Echo("Command Mode: " + commandMode);
Echo(debugInfoF);
Echo(debugInfo);
                Echo("Version:  " + VERSION);
	    Echo("Agile: " + GUILD_RATE + " " + APID_P + " " + APID_D);
                Echo("\nInfo:\n---------------");
                Echo(Lstrundata);

                #region Block Error Readouts

                //Core Block Checks
                if (RC == null || RC.CubeGrid.GetCubeBlock(RC.Position) == null)
                { Echo(" ~ No Ship Control Found,\nInstall Forward Facing Cockpit/RC/Flightseat And Press Recompile"); RC = null; return; }
                if (gcTargetPanel == null && fcsComputer == null)
                { Echo(" ~ Searching For A new Seeker\n (Target Panel)\n Install Block And Press Recompile"); return; }

                #endregion

                //Begins Launch Scheduler
                //-----------------------------------------
                if (argument == "Fire" && LaunchStateMachine == null)
                {
                    LaunchStateMachine = FunnelLaunchHandler().GetEnumerator();
                } else if(argument == "SUP") {
                    commandMode = 2;
                } else if(argument == "ATK") {
                    commandMode = 1;
                } else if(argument == "STD") {
                    commandMode = 0;
                } else if(argument == "X+") {
                    xo+=0.1;
                } else if(argument == "Y+") {
                    yo+=0.1;
                } else if(argument == "Z+") {
                    zo+=0.1;
                } else if(argument == "X-") {
                    xo-=0.1;
                } else if(argument == "Y-") {
                    yo-=0.1;
                } else if(argument == "Z-") {
                    zo-=0.1;
                }
	    else if (argument != "Fire" && argument != "")
                {
                    Lstrundata = "Unknown/Incorrect launch argument,\ncheck spelling & caps,\nto launch argument should be just: Fire\n ";
                }

                //Runs Guidance Block (foreach missile)
                //---------------------------------------
                debugInfo = "";
	    Vector3D targetPanelPosition = Vector3D.Zero;
	    Vector3D targetPanelVelocity = Vector3D.Zero;
	    if (gcTargetPanel != null) {
                var panelInfo = gcTargetPanel.GetPublicTitle();
	    var tokens = panelInfo.Split(':');
	    if (panelInfo.StartsWith("[T:") && tokens.Length >= 7) {
	       targetPanelPosition = new Vector3D(Convert.ToDouble(tokens[1]),Convert.ToDouble(tokens[2]),Convert.ToDouble(tokens[3]));
	       targetPanelVelocity = new Vector3D(Convert.ToDouble(tokens[4]),Convert.ToDouble(tokens[5]),Convert.ToDouble(tokens[6]));
	    }
	    }

if (fcsComputer != null && targetPanelPosition == Vector3D.Zero) {
checkFcsTarget(out targetPanelPosition, out targetPanelVelocity);
}

                for (int i = 0; i < FUNNELS.Count; i++)
                {
                    var ThisFunnel = FUNNELS[i];

                    //Fires Straight (NO OVERRIDES)
                    if (ThisFunnel.IS_CLEAR == false)
                    {
                        if ((ThisFunnel.GYRO.GetPosition() - Me.GetPosition()).Length() > ThisShipSize)
                        { ThisFunnel.IS_CLEAR = true; }
                    }
                    STD_GUIDANCE(ThisFunnel, ThisFunnel.IS_CLEAR, i, targetPanelPosition, targetPanelVelocity); 

                    //Disposes If Out Of Range Or Destroyed 
                    bool Isgyroout = ThisFunnel.GYRO.CubeGrid.GetCubeBlock(ThisFunnel.GYRO.Position) == null;
                    bool Isthrusterout = ThisFunnel.THRUSTERS[0].CubeGrid.GetCubeBlock(ThisFunnel.THRUSTERS[0].Position) == null;
                    bool Isouttarange = (ThisFunnel.GYRO.GetPosition() - Me.GetPosition()).LengthSquared() > 9000 * 9000;
                    if (Isgyroout || Isthrusterout || Isouttarange)
                    { FUNNELS.Remove(ThisFunnel); }

                }

                //Launcher Statemachine Disposed If Complete
                //---------------------------------------------------
                if (LaunchStateMachine != null)
                {
                    if (!LaunchStateMachine.MoveNext() || !LaunchStateMachine.Current)
                    {
                        LaunchStateMachine.Dispose();
                        LaunchStateMachine = null;
                    }
                }

            }
            #endregion

            //SubMethod State Machine For Launching Funnels
            #region FunnelLaunchHandler
            public IEnumerable<bool> FunnelLaunchHandler()
            {
                yield return INIT_NEXT_FUNNEL();

	    // a b K
	    for (int i = 0; i < (f_no % 2) * 30; i++) {
	    	yield return true;
	    }

                //Disables Merge Block
                FUNNEL ThisFunnel = FUNNELS[FUNNELS.Count - 1];
                var MERGE_A = ThisFunnel.MERGE;
                (MERGE_A as IMyShipMergeBlock).Enabled = false;
                yield return true;
                yield return true;
                yield return true; //Safety Tick

	    for (int i = 0; i < dropTime; i++) {
	    	yield return true;
	    }

                //Launches Missile & Gathers Next Scanner
                PREP_FOR_LAUNCH(FUNNELS.Count - 1);

            }
            #endregion

            //Finds First Funnel Available
            #region Initialise Funnels Blocks 
            bool INIT_NEXT_FUNNEL()
            {

                //Finds Blocks (performs 1 gts)
                List<IMyTerminalBlock> GYROS_temp = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyGyro>(GYROS_temp, b => b.CustomName.Contains(FTag));
                List<IMyTerminalBlock> GYROS = GYROS_temp.Where(g => {
                  foreach(var f in FUNNELS) {
                    if (g.CubeGrid == f.GYRO.CubeGrid) return false;
                  }
                  return true;
                }).ToList();
                List<IMyTerminalBlock> TURRETS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(TURRETS, b => b.CustomName.Contains(FTag));
                List<IMyThrust> THRUSTERS = new List<IMyThrust>();
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(THRUSTERS, b => b.CustomName.Contains(FTag));
                List<IMyTerminalBlock> MERGES = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(MERGES, b => b.CustomName.Contains(FTag));
                List<IMyTerminalBlock> BATTERIES = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(BATTERIES, b => b.CustomName.Contains(FTag) && (b is IMyBatteryBlock || b is IMyReactor));
                List<IMyTerminalBlock> WARHEADS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyWarhead>(WARHEADS, b => b.CustomName.Contains(FTag));
                List<IMyTerminalBlock> SPOTS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyReflectorLight>(SPOTS, b => b.CustomName.Contains(FTag));
                List<IMyTerminalBlock> GUNS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMySmallGatlingGun>(GUNS, b => b.CustomName.Contains(FTag));

                //Diagnostics For No Gyro
                Lstrundata = "No More Funnel (Gyros) Detected";

                //Iterates Through List To Find Complete Funnel Based On Gyro
	    // a b K
	    GYROS = GYROS.OrderBy(g=>{
	    var rcmt = RC.WorldMatrix;
	    var tranmt = MatrixD.CreateLookAt(new Vector3D(), rcmt.Forward, rcmt.Up);
                var dis = Vector3D.TransformNormal(g.GetPosition()-RC.GetPosition(), tranmt);
                var x = -Math.Round(Math.Abs(dis.X),1);
                var z = -Math.Round(Math.Abs(dis.Z),1);
	    return (x * 1000 + z) * 100 + dis.X;
	    }).ToList();
                foreach (var Key_Gyro in GYROS)
                {
                    FUNNEL NEW_FUNNEL = new FUNNEL();
                    NEW_FUNNEL.GYRO = Key_Gyro;
                    NEW_FUNNEL.MIS_PREV_POS = Key_Gyro.GetPosition();// 
                    //NEW_FUNNEL.lastVelocity = RC.GetShipVelocities().LinearVelocity;

                    Vector3D GyroPos = Key_Gyro.GetPosition();
                    double Distance = 5;

                    //Sorts And Selects Turrets
                    List<IMyTerminalBlock> TempTurrets = TURRETS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < 100000);
                    TempTurrets.Sort((x, y) => (x.GetPosition() - Key_Gyro.GetPosition()).LengthSquared().CompareTo((y.GetPosition() - Key_Gyro.GetPosition()).LengthSquared()));

                    //Sorts And Selects Batteries
                    List<IMyTerminalBlock> TempPower = BATTERIES.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
                    TempPower.Sort((x, y) => (x.GetPosition() - Key_Gyro.GetPosition()).LengthSquared().CompareTo((y.GetPosition() - Key_Gyro.GetPosition()).LengthSquared()));

                    //Sorts And Selects Merges
                    List<IMyTerminalBlock> TempMerges = MERGES.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
                    TempMerges.Sort((x, y) => (compareXY(x, y, Key_Gyro)));

                    //Sorts And Selects Thrusters
                    NEW_FUNNEL.THRUSTERS = THRUSTERS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);

                    //Sorts And Selects Warheads
                    NEW_FUNNEL.WARHEADS = WARHEADS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);

                    //Sorts And Selects Warheads
                    NEW_FUNNEL.GUNS = GUNS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);

                    // a b K
                    List<IMyThrust> TempThrusters = THRUSTERS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
                    //TempThrusters.Sort((x, y) => (compareXY(x, y, Key_Gyro)));
	        NEW_FUNNEL.THRUSTERS = TempThrusters.Where(t => filterClose(t, Key_Gyro)).ToList();
	        // if(TempThrusters[0].Position.X != Key_Gyro.Position.X || TempThrusters[0].Position.Y != Key_Gyro.Position.Y) {
		// Lstrundata = "mismatch thruster " + TempThrusters[0].Position.X + " " + Key_Gyro.Position.X +" " +  TempThrusters[0].Position.Y + " " +  Key_Gyro.Position.Y;
		// return false;
	        // }

                    List<IMyTerminalBlock> TempWarheads = WARHEADS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
	        //TempWarheads.Sort((x, y) => (compareXY(x, y, Key_Gyro)));
	        NEW_FUNNEL.WARHEADS = TempWarheads.Where(t => filterClose(t, Key_Gyro)).ToList();

                    List<IMyTerminalBlock> TempSpots = SPOTS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
	        NEW_FUNNEL.SPOTLIST = TempSpots.Where(t => filterClose(t, Key_Gyro)).ToList();

                    //Checks All Key Blocks Are Present
                    bool HasTurret = TempTurrets.Count > 0;
	        // a b K
	        if (gcTargetPanel != null || fcsComputer != null) HasTurret = true;

                    bool HasPower = TempPower.Count > 0;
                    bool HasMerge = TempMerges.Count > 0;
                    bool HasThruster = NEW_FUNNEL.THRUSTERS.Count > 0;

                    //Echos Some Useful Diagnostics
                    Lstrundata = "Last Funnel Failed To Fire\nReason:" +
                        "\nHas Gyro: True" +
                        "\nHas Turret: " + HasTurret +
                        "\nHas Power: " + HasPower +
                        "\nHasMerge: " + HasMerge +
                        "\nHasThruster: " + HasThruster;

                    //Assigns and Exits Loop
                    if (HasTurret && HasPower && HasMerge && HasThruster)
                    {
		if(TempTurrets.Count>0){
		  // a b K
                          NEW_FUNNEL.TURRET = TempTurrets[0];
		}
                        NEW_FUNNEL.POWER = TempPower[0];
                        NEW_FUNNEL.MERGE = TempMerges[0];

                        // check subgrid
                        if (NEW_FUNNEL.GYRO.CubeGrid != Me.CubeGrid) {
                        NEW_FUNNEL.runningMode = CM_SUP;
                        NEW_FUNNEL.IS_CLEAR = true;
                        double nearest = 99999;
                        for (int i = 0; i < hingeList.Count; i++) {
                        double d = (hingeList[i].GetPosition() - NEW_FUNNEL.GYRO.GetPosition()).Length();
                        if (d < nearest) {
                        NEW_FUNNEL.hingeIdx = i;
                        nearest = d;
                        }
                        }
                        }

                        FUNNELS.Add(NEW_FUNNEL);
                        Lstrundata = "Launched Funnel:" + FUNNELS.Count;
                        return true;
                    }
                }
                return false;
            }
            #endregion

	// a b K
	int compareXY(IMyTerminalBlock a, IMyTerminalBlock b, IMyTerminalBlock s) {
	    if (a.CubeGrid != s.CubeGrid) {
	    return 100000000;
	    }
	    if (b.CubeGrid != s.CubeGrid) {
	    return -100000000;
	    }
	    var gyroLookAtMat = MatrixD.CreateLookAt(new Vector3D(), s.WorldMatrix.Forward, s.WorldMatrix.Up);
	    var da = Vector3D.TransformNormal(a.GetPosition() - s.GetPosition(), gyroLookAtMat);
	    var db = Vector3D.TransformNormal(b.GetPosition() - s.GetPosition(), gyroLookAtMat);
	    return (int)((Math.Pow(da.X,2) + Math.Pow(da.Z,2)) - (Math.Pow(db.X,2) + Math.Pow(db.Z,2)));
	}
	bool filterClose(IMyTerminalBlock t, IMyTerminalBlock s) {
	var gyroLookAtMat = MatrixD.CreateLookAt(new Vector3D(), s.WorldMatrix.Forward, s.WorldMatrix.Up);
	var dt = Vector3D.TransformNormal(t.GetPosition() - s.GetPosition(), gyroLookAtMat);
	var d = Math.Sqrt(Math.Pow(dt.X, 2) + Math.Pow(dt.Z, 2));
	return d < 2;
	}

            //Preps For Launch & Launches
            #region RFC Prep-For Launch Subroutine #RFC#
            /*=================================================                           
            Function: RFC Function bar #RFC#                  
            ---------------------------------------     */
            void PREP_FOR_LAUNCH(int INT)
            {
                Echo(INT + "");
                FUNNEL ThisFunnel = FUNNELS[INT];
                //Adds Additional Mass & Sets Accel (ovverrides If Possible)
                double number;
                if (double.TryParse(ThisFunnel.GYRO.CustomData, out number))
                { double.TryParse(ThisFunnel.GYRO.CustomData, out ThisFunnel.MissileMass); }
                else ThisFunnel.MissileMass = F_MASS;
                ThisFunnel.MissileAccel = ThisFunnel.MissileThrust / ThisFunnel.MissileMass;

                //Sets Grid Type
                ThisFunnel.IsLargeGrid = ThisFunnel.GYRO.CubeGrid.GridSizeEnum == MyCubeSize.Large;
                ThisFunnel.FuseDistance = ThisFunnel.IsLargeGrid ? 16 : 7;

	    PlayActionList(ThisFunnel.SPOTLIST, "OnOff_On");

                if(ThisFunnel.runningMode != CM_STD) return;
                ThisFunnel.MissileThrust = 0;
                
                //Preps Battery For Launch
                var POWER_A = ThisFunnel.POWER;
                if (ThisFunnel.POWER != null && ThisFunnel.POWER is IMyBatteryBlock)
                {
                    POWER_A.ApplyAction("OnOff_On");
                    //POWER_A.SetValue("Recharge", false);
                    //POWER_A.SetValue("Discharge", true);
                }

                //Removes Thrusters That Are Still on the same Grid As launcher
                List<IMyThrust> TemporaryThrust = new List<IMyThrust>();
                TemporaryThrust.AddRange(ThisFunnel.THRUSTERS);
                for (int i = 0; i < TemporaryThrust.Count; i++)
                {
                    var item = TemporaryThrust[i];
                    if (item.CubeGrid != ThisFunnel.GYRO.CubeGrid)
                    { ThisFunnel.THRUSTERS.Remove(item); continue; }
                }
                if (ThisFunnel.THRUSTERS.Count == 0)
                { 
                    Lstrundata = "Funnel Failed To Fire\nReason: No Detectable Thrusters On Funnel Or Funnel Still Attached To Launcher"; 
                    FUNNELS.Remove(ThisFunnel);
                    return;
                }
                
                //Retrieves Largest Thrust Direction
                Dictionary<Vector3D, double> ThrustDict = new Dictionary<Vector3D, double>();
                foreach (IMyThrust item in ThisFunnel.THRUSTERS)
                {
                    Vector3D Fwd = item.WorldMatrix.Forward;
                    double Thrval = item.MaxEffectiveThrust;

                    if (ThrustDict.ContainsKey(Fwd) == false)
                    { ThrustDict.Add(Fwd, Thrval); }
                    else
                    { ThrustDict[Fwd] = ThrustDict[Fwd] + Thrval; }
                }
                List<KeyValuePair<Vector3D, double>> ThrustList = ThrustDict.ToList();
                ThrustList.Sort((x, y) => y.Value.CompareTo(x.Value));
                Vector3D ThrForward = ThrustList[0].Key;

                //Preps Thrusters For Launch (removes any not on grid)
                TemporaryThrust = new List<IMyThrust>();
                TemporaryThrust.AddRange(ThisFunnel.THRUSTERS);
                for (int i = 0; i < TemporaryThrust.Count; i++)
                {
                    var item = TemporaryThrust[i];

                    //Retrieves Thrusters Only Going In The Forward
                    if (item.WorldMatrix.Forward != ThrForward)
                    { item.ApplyAction("OnOff_On"); ThisFunnel.THRUSTERS.Remove(item); continue; }

                    //Runs Std Operations
                    item.ApplyAction("OnOff_On");
                    double ThisThrusterThrust = (item as IMyThrust).MaxThrust;
                    (item as IMyThrust).ThrustOverride = (float)ThisThrusterThrust;
                    //RdavUtils.DiagTools.Diag_Plot(Me, ThisThrusterThrust);
                    ThisFunnel.MissileThrust += ThisThrusterThrust;
                }
                
                //Removes Any Warheads Not On The Grid
                List<IMyTerminalBlock> TemporaryWarheads = new List<IMyTerminalBlock>();
                TemporaryWarheads.AddRange(ThisFunnel.WARHEADS);
                for (int i = 0; i < ThisFunnel.WARHEADS.Count; i++)
                {
                    var item = TemporaryWarheads[i];

                    if (item.CubeGrid != ThisFunnel.GYRO.CubeGrid)
                    { ThisFunnel.WARHEADS.Remove(item); continue; }

                }

                //-----------------------------------------------------

            }
            #endregion


            //Standard Guidance Routine
            #region Guidance #RFC#
            void STD_GUIDANCE(FUNNEL ThisFunnel, bool isClear, int idx, Vector3D targetPanelPosition, Vector3D targetPanelVelocity)
            {
	    if (ThisFunnel.GYRO == null) return;
                var ENEMY_POS = new Vector3D();
	    // a b K
	    bool targetPanelHasTarget = targetPanelPosition != Vector3D.Zero;
	    // var panelInfo = gcTargetPanel.GetPublicTitle();
	    // var tokens = panelInfo.Split(':');
	    // Vector3D targetPanelPosition = Vector3D.Zero;
	    // Vector3D targetPanelVelocity = Vector3D.Zero;
	    // if (panelInfo.StartsWith("[T:") && tokens.Length >= 7) {
	    //    targetPanelPosition = new Vector3D(Convert.ToDouble(tokens[1]),Convert.ToDouble(tokens[2]),Convert.ToDouble(tokens[3]));
	    //    targetPanelVelocity = new Vector3D(Convert.ToDouble(tokens[4]),Convert.ToDouble(tokens[5]),Convert.ToDouble(tokens[6]));
	    //    targetPanelHasTarget = true;
	    // }

                // setup by mode
	    bool usePanel = false;
                if(targetPanelHasTarget && ThisFunnel.runningMode == CM_ATK){
	        ENEMY_POS = targetPanelPosition;
                    if (timestamp - ThisFunnel.atkOffsetStart > 1200) {
                    // reset atkOffset
	        Vector3D dir = ENEMY_POS - RC.GetPosition();
	        dir = Vector3D.Normalize(dir);
	        var rcmt = RC.WorldMatrix;
                    var up = rcmt.Up;
                    if (ng.Length() > 0.05) {
                    up = Vector3D.Normalize(-ng);
                    }
	        Vector3D tmp = Vector3D.Reject(dir, up);
	        if (tmp.Equals(Vector3D.Zero)) {
		tmp = rcmt.Forward;
	        }else {
		tmp = Vector3D.Normalize(tmp);
	        }
	        MatrixD offsetMD = MatrixD.CreateFromDir(tmp, rcmt.Up);
                    double right = rnd.Next(1, 800) - 400;
                    if (right < 0) right -= 200;
                    else right += 200;
                    double back = rnd.Next(1, 800) - 400;
                    if (back < 0) back -= 200;
                    else back += 200;
                    double upL = rnd.Next(1, 800) - 400;
                    Vector3D offsetV = Vector3D.Zero;
                    if (ng.Length() > 0.05 || upL > 0) {
                      upL = Math.Abs(upL) + 200;
                      offsetV = Vector3D.Normalize(offsetMD.Up * upL *4 + offsetMD.Right*right + offsetMD.Backward*back) * 400;
                    } else {
                      upL -= 200;
                      offsetV = Vector3D.Normalize(offsetMD.Up * upL + offsetMD.Right*right + offsetMD.Backward*back) * 400;
                    }
                    ThisFunnel.atkOffset = offsetV;
                    ThisFunnel.atkOffsetStart = timestamp;
                    }
	        usePanel = true;
	        ENEMY_POS = ENEMY_POS + ThisFunnel.atkOffset;
                    if(commandMode == CM_STD) ThisFunnel.runningMode = CM_STD;
	    }	    
                else if(ThisFunnel.runningMode == CM_STD || (ThisFunnel.runningMode == CM_ATK && !targetPanelHasTarget))
                {
                    // 散开
                    Vector3D dir = RC.WorldMatrix.Left;
                    Vector3D dx = RC.WorldMatrix.Left;
                    Vector3D dy = RC.WorldMatrix.Up;
                    int angleIndi = (idx+2) / 4;
                    int posIndi = (idx+2) % 4;
                    if (posIndi == 0) {
                       dir = dx * Math.Cos(angleIndi * mpi * 20F/180F) + dy * Math.Sin(angleIndi * mpi * 20F/180F);
                    } else if (posIndi == 1) {
                       dir = -dx * Math.Cos(angleIndi * mpi * 20F/180F) + dy * Math.Sin(angleIndi * mpi * 20F/180F);
                    } else if (posIndi == 2) {
                       dir = dx * Math.Cos(angleIndi * mpi * 20F/180F) - dy * Math.Sin(angleIndi * mpi * 20F/180F);
                    } else if (posIndi == 3) {
                       dir = -dx * Math.Cos(angleIndi * mpi * 20F/180F) - dy * Math.Sin(angleIndi * mpi * 20F/180F);
                    }
                    
                    if (f_no != 0) dir = RC.WorldMatrix.Right;
                    ENEMY_POS = RC.GetPosition() + dir* STDBY_DISTANCE;
                    ThisFunnel.TargetVelocity = RC.GetShipVelocities().LinearVelocity;
                    bool canRunNext = (ENEMY_POS - ThisFunnel.MIS_PREV_POS).Length() < 2 && (ThisFunnel.TargetVelocity - ThisFunnel.lastVelocity).Length()<1;
//                    debugInfo += "\nidx can runnext: " + idx + " " + canRunNext + "\n"; 
//                    debugInfo += "\nidx now hinge: " + ThisFunnel.hingeIdx + "\n"; 
                    if (ThisFunnel.runningMode == CM_STD) {
                         if (commandMode == CM_ATK) ThisFunnel.runningMode = CM_ATK;
                         else if (commandMode == CM_SUP && canRunNext) {
                           List<IMyMotorStator> hingeCanUse = new List<IMyMotorStator>();
                           int usedIdx = -1;
                           double length = 99999;
//                           debugInfo += "\ntryget: " + idx + "\n";
                           for(int i = 0; i < hingeList.Count; i++) {
                               bool used = false;
                               foreach(var f in FUNNELS) {
                                   if (f.hingeIdx == i) used = true;
                               }
                               if (used) continue;
//                               debugInfo += "\nidx can use: " + idx + " " + i + "\n"; 
                               var l = (hingeList[i].GetPosition() - ThisFunnel.GYRO.GetPosition()).Length();
                               if (l < length) {
                                   length = l;
                                   usedIdx = i; 
                               }
                           }
                           // change mode
                           // check ammo TODO use lastSupStart instead
                           if (timestamp < ThisFunnel.lastSupStart + 7200) usedIdx = -1;
                           if (usedIdx != -1) {
                              ThisFunnel.runningMode = CM_SUP;
                              ThisFunnel.hingeIdx = usedIdx;
                              ThisFunnel.lastSupStart = timestamp;
                           }
                         }
                    }
                } else if (ThisFunnel.runningMode == CM_SUP) {
//                  debugInfo += "\nsuping: " + idx + " " + ThisFunnel.hingeIdx;
                  // 对应hinge是否已连接, 可连接, 未连接
                  var hinge = hingeList[ThisFunnel.hingeIdx];
                  var sb = hinge.IsAttached;
                  if (!sb && commandMode == CM_SUP) {
                  hinge.ApplyAction("Attach");
                  }
                  if (sb && commandMode == CM_STD) {
                  hinge.ApplyAction("Detach");
                  //debugInfoF += "\ntc: " + idx + " " + ThisFunnel.THRUSTERS.Count + "\n";
                    foreach (var t in ThisFunnel.THRUSTERS) {
                    t.ApplyAction("OnOff_On");
                    }
                    ThisFunnel.hingeIdx = -1;
                  }

                  int hingeStatus = 0;
                  if (sb) hingeStatus = 2;
                  
                  if (hingeStatus == 0) {
                  ENEMY_POS = RC.GetPosition();
                  Vector3D offO = hingeList[ThisFunnel.hingeIdx].GetPosition() - RC.GetPosition();
                  Vector3D off = offO + Vector3D.TransformNormal(new Vector3D(xo, yo - 1, zo), RC.WorldMatrix);
                  Vector3D pre = RC.GetPosition() + off;
                  Vector3D diffxz = Vector3D.Reject(pre - ThisFunnel.MIS_PREV_POS, RC.WorldMatrix.Up);
//                  debugInfo += "diffxz: " + diffxz.Length() + "\n";
                  if (diffxz.Length() < 1) ThisFunnel.runningSubMode = 1;
                  else ThisFunnel.runningSubMode = 0;
                  if (diffxz.Length() < 0.2) {
                  off = offO + Vector3D.TransformNormal(new Vector3D(xo, yo, zo), RC.WorldMatrix);
                  }
                  
//                  debugInfo += "XYZ: " + displayVector3D(off) + "\n";
                  ENEMY_POS += off;
                  ThisFunnel.TargetVelocity = RC.GetShipVelocities().LinearVelocity;
                  if (commandMode == CM_STD) {
                  ThisFunnel.runningMode = CM_STD;
                  ThisFunnel.hingeIdx = -1;
                  }
                  } else if (hingeStatus == 2) {
                  // 已
                    if (commandMode == CM_STD) {
                      bool sa = hinge.IsAttached;
                      if (!sa) {
                      ThisFunnel.runningMode = CM_STD;
                      ThisFunnel.hingeIdx = -1;
                      }
                    } else if (commandMode == CM_SUP){
                    }
                    ((IMyGyro)ThisFunnel.GYRO).GyroOverride = false;
                    foreach (var t in ThisFunnel.THRUSTERS) {
                    t.ApplyAction("OnOff_Off");
                    }
                    return;
                  }
                }

                Vector3D TargetPosition = ENEMY_POS;
                double td = (TargetPosition - ThisFunnel.TARGET_PREV_POS).Length();
//                debugInfo += "\ntd: " + td + "\n";
                // if (timestamp > lastDebugInfoF + 60
                // && td > (15D/60)) {
                // debugInfoF = "tderror";
                // lastDebugInfoF=timestamp;
                // }

                //Sorts CurrentVelocities
                Vector3D MissilePosition = ThisFunnel.GYRO.CubeGrid.WorldVolume.Center;
                // testList[idxTestList] = MissilePosition;
                // idxTestList = (idxTestList + 1)%maxTestList;

                //Vector3D MissilePosition = ThisFunnel.GYRO.GetPosition(); // testing mv stable
                Vector3D MissilePositionPrev = ThisFunnel.MIS_PREV_POS;
                Vector3D MissileVelocity = (MissilePosition - MissilePositionPrev) / (Global_Timestep * (timestamp - ThisFunnel.lastTimeStamp));
                // double maxVelocity = 0, minVelocity = 999, avgVelocity = 0;;
                // foreach(double v in ThisFunnel.hisVelocity) {
                // if (v > maxVelocity) maxVelocity = v;
                // if (v< minVelocity) minVelocity = v;
                // avgVelocity += v;
                // }
                // avgVelocity /= FUNNEL.maxHis;
                // debugInfo += "\navgVelocityV: " + avgVelocity + "\n";
                // debugInfo += "\nmv: " + MissileVelocity.Length() + "\n";
                double md = (MissileVelocity-ThisFunnel.lastVelocity).Length();
//                debugInfo += "\nmd: " +md + "\n";
                // if (timestamp > lastDebugInfoF + 60
                // && md > 10) {
                // debugInfoF = "mderror"; // PD没报错的前提下md先报错?
                // lastDebugInfoF = timestamp;
                // }
//                debugInfo += "\ntimeinterval: " + (timestamp - ThisFunnel.lastTimeStamp) + "\n";
                if (ThisFunnel.lastVelocity.Length() > 5 && (MissileVelocity-ThisFunnel.lastVelocity).Length() >10) {
                //debugInfoF += "return";
                //return;
                }

//                debugInfo += "\nmv2: " + MissileVelocity.Length() + "\n";
                ThisFunnel.lastTimeStamp = timestamp;
                ThisFunnel.lastVelocity = MissileVelocity;
                ThisFunnel.MIS_PREV_POS = MissilePosition;
                // ThisFunnel.hisVelocity[ThisFunnel.hisIdx] = MissileVelocity.Length();
                // ThisFunnel.hisIdx = (ThisFunnel.hisIdx + 1) % FUNNEL.maxHis;


                Vector3D TargetPositionPrev = ThisFunnel.TARGET_PREV_POS;
                Vector3D TargetVelocityNew = (TargetPosition - ThisFunnel.TARGET_PREV_POS) / Global_Timestep;
                Vector3D TargetAcc = (TargetVelocityNew - ThisFunnel.TargetVelocity) / Global_Timestep;
                //ThisFunnel.TargetVelocity = TargetVelocityNew;
                ThisFunnel.TARGET_PREV_POS = TargetPosition;

	    // a b K
	    if (usePanel) {
                    TargetAcc = (targetPanelVelocity - ThisFunnel.TargetVelocityPanel) / Global_Timestep;
	        ThisFunnel.TargetVelocity = targetPanelVelocity;
                    ThisFunnel.TargetVelocityPanel = targetPanelVelocity;
                    TargetVelocityNew = targetPanelVelocity;
	    }
                if(isClear == false) {
                    TargetPosition = RC.GetPosition() + RC.WorldMatrix.Up* 100000D;
                    if (isClearDown)
                    TargetPosition = RC.GetPosition() + RC.WorldMatrix.Down* 100000D;
                    TargetAcc = Vector3D.Zero;
                    ThisFunnel.TargetVelocity = RC.GetShipVelocities().LinearVelocity;
                    ThisFunnel.TargetVelocityPanel = ThisFunnel.TargetVelocity;
                }

// 新算法

                //Setup LOS rates and PN system
                Vector3D LOS_Old = Vector3D.Normalize(TargetPositionPrev - MissilePositionPrev);
                Vector3D LOS_New = Vector3D.Normalize(TargetPosition - MissilePosition);
                Vector3D Rel_Vel = Vector3D.Normalize(ThisFunnel.TargetVelocity - MissileVelocity); 
                Vector3D targetRange = TargetPosition - MissilePosition;
	    Vector3D targetV = ThisFunnel.TargetVelocity - MissileVelocity;
//                debugInfo += "\nmv: " + MissileVelocity.Length() + "\n";

                //And Assigners
                Vector3D am = new Vector3D(1, 0, 0); double LOS_Rate; Vector3D LOS_Delta;
                Vector3D MissileForwards = ThisFunnel.THRUSTERS[0].WorldMatrix.Backward;
		ThisFunnel.lastAVelocity = MissileForwards - ThisFunnel.lastForward;
		if (ThisFunnel.lastForward == Vector3D.Zero) ThisFunnel.lastAVelocity = Vector3D.Zero;
		ThisFunnel.lastForward = MissileForwards;

                //Vector/Rotation Rates
                if (LOS_Old.Length() == 0)
                { LOS_Delta = new Vector3D(0, 0, 0); LOS_Rate = 0.0; }
                else
                { LOS_Delta = LOS_New - LOS_Old; LOS_Rate = LOS_Delta.Length() / Global_Timestep; }

// 4 推力 / 质量 = 可以提供的加速度的长度 sdl
double sdl = ThisFunnel.THRUSTERS[0].MaxEffectiveThrust * ThisFunnel.THRUSTERS.Count / ThisFunnel.MissileMass;
//debugInfo += "\nsdl: " + sdl + "\n";
//debugInfo += "\nat: " + ThisFunnel.THRUSTERS[0].MaxEffectiveThrust * ThisFunnel.THRUSTERS.Count + "\n";
//debugInfo += "\nmass: " + ThisFunnel.MissileMass + "\n";

// 1 求不需要的速度
debugInfo += "\ntr: " + targetRange.Length() + "\n";
Vector3D tarN = Vector3D.Normalize(targetRange);
Vector3D graN = Vector3D.Normalize(ng);
double sdlg = sdl;
// 重力下直接认为可用加速度需要减去重力(不考虑方向, 即以完全逆向为前提, 仅考虑最小可用加速度)
if (ng.Length() > 0.05) sdlg = sdl - ng.Length();
double dt = targetV.Length() / sdlg;
double dr = targetV.Length() * dt * 0.5;
Vector3D rv = targetV;
double MAX_SPEED = 100;
int mode = 0;
if (Vector3D.Dot(Vector3D.Normalize(targetV), tarN) > 0) {
// 目标远离, 全力加速
rv = tarN * MAX_SPEED + targetV;
} else {
// 减速时间
double drb = dr * 2; // 缓冲
double dra = dr * 10; // 加速区间
if ( targetRange.Length() > dra || ThisFunnel.runningMode == CM_ATK) {
mode = 1;
// 全力加速
if (ThisFunnel.runningMode == CM_ATK
&& targetRange.Length() < 1600) {
rv = tarN * MAX_SPEED;
} else {
rv = tarN * MAX_SPEED + targetV;
}
} else if (targetRange.Length() > drb ) {
mode = 2;
rv = Vector3D.Reject(targetV, tarN); // 仅处理侧向速度
} else {
// 开始减速
mode = 3;
rv = targetV;
}
}


if (targetRange.Length() < Math.Max(100, dr* 2) || ThisFunnel.runningMode != CM_ATK) {
double APPROACH_RATE = 0.1;
if (ThisFunnel.runningMode == CM_SUP && ThisFunnel.runningSubMode ==1) {
APPROACH_RATE = 1;
}
Vector3D step1 = targetRange * APPROACH_RATE + targetV; //
//debugInfo += "\ntv: " + targetV.Length() + "\n";
//debugInfo += "\nstep1: " + step1.Length() + "\n";
if (step1.Length() == 0) {
rv = Vector3D.Zero;
} else {
Vector3D side = Vector3D.Reject(step1, tarN);
Vector3D dire = step1 - side;
//debugInfo += "\nside: " + side.Length()  + "\n";
rv = step1;
if (Vector3D.Dot(Vector3D.Normalize(step1), tarN) > 0 && rv.Length() < 0.1) rv = Vector3D.Zero;
}
mode = 4;
}

// pid modify
Vector3D rdo = rv;
// if (rdo.Length()>0 && rdo.Length()<1) {
// Vector3D rdon = Vector3D.Normalize(rdo);
// double newLen = Math.Atan2(rdo.Length(), ATAN_BASE);
// rdo = rdon * newLen;
// }// 负效果

// if (rdo.Length() > sdlg) {
// var tempd = (rdo.Length() - sdlg) / rdo.Length();
// rdo *= tempd;
// } // testing 减少稳定性

// if (rdo.Length() > 0) {
// double newLen = ThisFunnel.pidT.Filter(rdo.Length(), 5);
// double newD = newLen / rdo.Length();
// rdo = rdo * newD;
// } // 负作用

// 3 加上抵抗重力所需的加速度 = 需要抵消的加速度 rd
Vector3D rd = rdo - ng;
double rdl = rd.Length();

// 8 总加速度
Vector3D sd = rd;
//debugInfo += "\nrd: " + rd.Length() + "\n";
double percent = rd.Length() / sdl;
if (percent > 1) percent = 1;
//debugInfo += "\np1: " + percent + "\n";

// 9 总加速度方向
Vector3D nam = ThisFunnel.GYRO.WorldMatrix.Up;// 要求gyro 头前
if (sd.Length() > 0) nam = Vector3D.Normalize(sd);

if (targetRange.Length() < ThisFunnel.nearest)
ThisFunnel.nearest = targetRange.Length();

am = nam;
// dead zone
// if (targetRange.Length() < 0.01 && targetV.Length() < 0.01) {
// am = ThisFunnel.GYRO.WorldMatrix.Up;
// percent = 0;
// mode = 5;
// }
//debugInfo += "\nmode: " + mode + "\n";
//debugInfo += "\nrunmode: " + ThisFunnel.runningMode + "\n";

double tad = Vector3D.Dot(am, ThisFunnel.GYRO.WorldMatrix.Up);
double TA_L = 0.7; // 喷射偏向上限
if (tad < TA_L) percent = 0; // 转向中, 不能喷
else percent *= tad*tad; // 有影响

//debugInfo += "\nperc: " + percent + "\n";
//debugInfo += "\ntad: " + tad + "\n";

// 攻击模式
var trr = targetPanelPosition - MissilePosition;
if (ThisFunnel.runningMode == CM_ATK && trr.Length() < 800 && ThisFunnel.lastInRangeStart == 0) {
ThisFunnel.lastInRangeStart = timestamp;
}
if (ThisFunnel.lastInRangeStart != 0 && timestamp > ThisFunnel.lastInRangeStart + 300) {
ThisFunnel.allowAttack = true;
}

if (ThisFunnel.runningMode == CM_ATK && trr.Length() > 800) {
ThisFunnel.allowAttack = false;
ThisFunnel.lastInRangeStart = 0;
}

if (ThisFunnel.runningMode == CM_ATK && trr.Length() < 800 && ThisFunnel.allowAttack) {
// 预瞄
Vector3D HitPosition = HitPointCaculate(MissilePosition, MissileVelocity, Vector3D.Zero, targetPanelPosition, ThisFunnel.TargetVelocity, Vector3D.Zero, Weapon_1_BulletInitialSpeed,Weapon_1_BulletAcceleration,Weapon_1_BulletMaxSpeed);
Vector3D hr = HitPosition - MissilePosition;
Vector3D hrn = ThisFunnel.GYRO.WorldMatrix.Up;
if (hr.Length() > 0) {
hrn = Vector3D.Normalize(hr);
}
am = hrn;
percent = 0;
if (Vector3D.Dot(am, ThisFunnel.GYRO.WorldMatrix.Up) > 0.99) {
foreach(var gun in ThisFunnel.GUNS) {
if (gun == null) continue;
try{
gun.ApplyAction("ShootOnce");
}catch(Exception e){}
}
}
} // 攻击模式end


// 设置推力
foreach(var t in ThisFunnel.THRUSTERS) {
if (t == null) continue;
try{
if (percent == 0) {
t.Enabled = false;
}else {
t.Enabled = true;
}
t.ThrustOverridePercentage = (float)percent;
}catch(Exception e){}
}

var missileLookAt = MatrixD.CreateLookAt(new Vector3D(), ThisFunnel.GYRO.WorldMatrix.Up, ThisFunnel.GYRO.WorldMatrix.Backward);
var amToMe = Vector3D.TransformNormal(am, missileLookAt);
var rr = Vector3D.Normalize(Vector3D.Reject(ThisFunnel.GYRO.WorldMatrix.Up, Vector3D.Normalize(RC.GetNaturalGravity())));
var rangle = 1 - Vector3D.Dot(rr, tarN);



// 新算法 end

                double Yaw; double Pitch;
                double gain = APID_P;
                // if (targetRange.Length() < 0.3 && targetV.Length() < 0.1) gain *= 0.2; // 负作用
                GyroTurn6(am, gain, APID_D, ThisFunnel.GYRO as IMyGyro, ThisFunnel.PREV_Yaw, ThisFunnel.PREV_Pitch, out Pitch, out Yaw, ref ThisFunnel, isClear, idx);

                //Updates For Next Tick Round
                ThisFunnel.PREV_Yaw = Yaw;
                ThisFunnel.PREV_Pitch = Pitch;

                //Detonates warheads in close proximity
                if ((TargetPosition - MissilePosition).LengthSquared() < 20 * 20 && ThisFunnel.WARHEADS.Count > 0) //Arms
                { foreach (var item in ThisFunnel.WARHEADS) { (item as IMyWarhead).IsArmed = true; } }
	    bool targetNeer = (TargetPosition - MissilePosition).LengthSquared() < ThisFunnel.FuseDistance * ThisFunnel.FuseDistance;
	    bool targetGetFar = (TargetPosition - MissilePosition).LengthSquared() > (TargetPositionPrev - MissilePositionPrev).LengthSquared() && (TargetPosition - MissilePosition).LengthSquared() < 4*4 ;
                if ((targetGetFar)&& ThisFunnel.WARHEADS.Count > 0) //A mighty earth shattering kaboom
                {
		foreach (var item in ThisFunnel.WARHEADS){ (item as IMyWarhead).Detonate();}
                }


            }
            #endregion

            //Utils
            #region RFC Function bar #RFC#
            string LeftPad = "   ";
            string Scriptname = "Kaien's Funnel System";
            /*=================================================                           
          Function: RFC Function bar #RFC#                  
          ---------------------------------------     */
            string[] FUNCTION_BAR = new string[] { "", " ===||===", " ==|==|==", " =|====|=", " |======|", "  ======" };
            int FUNCTION_TIMER = 0;                                     //For Runtime Indicator
            void OP_BAR()
            {
                FUNCTION_TIMER++;
                Echo(LeftPad + "~ " + Scriptname + " ~  \n               " + FUNCTION_BAR[FUNCTION_TIMER] + "");
                if (FUNCTION_TIMER == 5) { FUNCTION_TIMER = 0; }
            }
            #endregion

            //Quick Echoing & Rounding For Values
            void QuickEcho(object This, string Title = "")
            {
                if (This is Vector3D)
                { Echo(Title + " " + Vector3D.Round(((Vector3D)This), 3)); }
                else if (This is double)
                { Echo(Title + " " + Math.Round(((double)This), 3)); }
                else
                { Echo(Title + " " + This); }
            }

            //Use For Turning 
            #region GyroTurnMis #RFC#
            /*=======================================================================================                             
            Function: GyroTurn6                    
            ---------------------------------------                            
            function will: A Variant of PD damped gyroturn used for missiles
            //----------==--------=------------=-----------=---------------=------------=-----=-----*/
            void GyroTurn6(Vector3D TARGETVECTOR, double GAIN, double DAMPINGGAIN, IMyGyro GYRO, double YawPrev, double PitchPrev, out double NewPitch, out double NewYaw, ref FUNNEL missile, bool isClear, int idx)
            {
                //Pre Setting Factors
                NewYaw = 0;
                NewPitch = 0;

                Vector3D ShipUp = GYRO.WorldMatrix.Backward;
                Vector3D ShipForward = GYRO.WorldMatrix.Up; 

                //Create And Use Inverse Quatinion                   
                Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
                var InvQuat = Quaternion.Inverse(Quat_Two);

                Vector3D DirectionVector = TARGETVECTOR; //RealWorld Target Vector
		
                Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //Target Vector In Terms Of RC Block

                //Convert To Local Azimuth And Elevation
                double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
                Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out  ShipForwardAzimuth, out ShipForwardElevation);

                //Post Setting Factors
                NewYaw = ShipForwardAzimuth;
                NewPitch = ShipForwardElevation;
                //debugInfo += "\nspitch: " + NewPitch + "\n";
                var rclookmatrix = MatrixD.CreateLookAt(new Vector3D(), RC.WorldMatrix.Forward, RC.WorldMatrix.Up);
                var amToMe = Vector3D.TransformNormal(TARGETVECTOR, rclookmatrix);
                //debugInfo += "\n" + displayVector3D(amToMe) + "\n";
                
                //Applies Some PID Damping
                //ShipForwardAzimuth = ShipForwardAzimuth + DAMPINGGAIN * ((ShipForwardAzimuth - YawPrev) / Global_Timestep);
                //ShipForwardElevation = ShipForwardElevation + DAMPINGGAIN * ((ShipForwardElevation - PitchPrev) / Global_Timestep);

                //Does Some Rotations To Provide For any Gyro-Orientation
                var REF_Matrix = MatrixD.CreateWorld(GYRO.GetPosition(), (Vector3)ShipForward, (Vector3)ShipUp).GetOrientation();
                var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, 0)), REF_Matrix); //Converts To World
                var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(GYRO.WorldMatrix.GetOrientation()));  //Converts To Gyro Local
		

                //Logic Checks for NaN's
                if (double.IsNaN(TRANS_VECT.X) || double.IsNaN(TRANS_VECT.Y) || double.IsNaN(TRANS_VECT.Z))
                { return; }

                //Applies To Scenario
                GYRO.Pitch = (float)MathHelper.Clamp((modAngle(-TRANS_VECT.X)) * GAIN , -30, 30);
		//GYRO.Pitch = (float)MathHelper.Clamp( missile.pidE.Filter(-TRANS_VECT.X, 2) , -30, 30);
                //GYRO.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y)) * GAIN, -30, 30);
                GYRO.Roll = (float)MathHelper.Clamp((modAngle(-TRANS_VECT.Z)) * GAIN , -30, 30);
		//GYRO.Roll = (float)MathHelper.Clamp( missile.pidA.Filter(-TRANS_VECT.Z, 2) , -1000, 1000);

		// CODING
	    // a b K
	    // assume the gyro is in front of the missile, use Yaw to make the missile fit gravity
	    var ng = RC.GetNaturalGravity();
	    if (ng.Length() > 0.01 && isClear) {
	       MatrixD gyroMat = GYRO.WorldMatrix;
                   var dir = RC.WorldMatrix.Forward;
	       var diff = diffGravity(gyroMat.Left, dir, gyroMat.Forward);
	       // GYRO.Yaw =(float) missile.pidR.Filter(-diff,2); // 负作用
	       GYRO.Yaw = (float)-diff * (float)GAIN * 0.05F; // testing 有影响 0.05 = 0.37
	    } else {
                   MatrixD gyroMat = GYRO.WorldMatrix;
	       var diff = diffGravity(gyroMat.Left, RC.WorldMatrix.Down, gyroMat.Up);
                   if (Math.Abs(Vector3D.Dot(gyroMat.Up, RC.WorldMatrix.Down)) < 0.99) {
	       GYRO.Yaw = (float)-diff * (float)GAIN * 0.05F; // testing 有影响 0.05 = 0.37
                   } else {
                    GYRO.Yaw =0;
                   }
                }
                GYRO.GyroOverride = true;
            }
            #endregion

// a b K
double diffGravity(Vector3D dir, Vector3D ng, Vector3D axis) {
if (ng.Length() == 0) return 0;
var naturalGravityLength = ng.Length();
if (naturalGravityLength == 0) return 0;
var ngDir = Vector3D.Normalize(ng);
var tempP = new Vector3D(ngDir.Y * axis.Z - ngDir.Z * axis.Y,
ngDir.Z * axis.X - ngDir.X * axis.Z,
ngDir.X * axis.Y - ngDir.Y * axis.X);
if (tempP.Length() == 0) return 0;
var vertialPlaneLaw = Vector3D.Normalize(tempP);
if (dir.Length() == 0) return 0;
var angle = Math.Asin(dir.Dot(vertialPlaneLaw));
var diff = Math.PI / 2 + angle;
var leftOrRight = Math.Acos(dir.Dot(ngDir));
if (leftOrRight > Math.PI / 2) {
diff = -diff;
}

if (Math.Abs(diff) > 0.0001f) { // 这里坚决不能省略, 否则会出引擎问题
return modAngle(diff);
}else {
return 0;
}

}

void PlayActionList(List<IMyTerminalBlock> blocks, String action) {
    if(blocks == null) return;
	for(int i = 0; i < blocks.Count; i ++)
	{
		var a = blocks[i].GetActionWithName(action);
		if (a!=null) a.Apply(blocks[i]);
	}
}

string displayVector3D(Vector3D tar) {
return Math.Round(tar.X, 2) + ", " + Math.Round(tar.Y, 2) + ", " + Math.Round(tar.Z, 2);
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

double modAngle (double a) {
var r = a;
if ( r > Math.PI) {
r = r - MathHelper.TwoPi;
}
if ( r < -Math.PI) {
r = r + MathHelper.TwoPi;
}
return r;
}

// void debugF(string msg) {
//     if (timestamp > lastDebugInfoF + 180){
//         debugInfoF += "\n" + timestamp + " " + msg;
//         lastDebugInfoF = timestamp;
//     }
// }

public static Vector3D HitPointCaculate(Vector3D Me_Position, Vector3D Me_Velocity, Vector3D Me_Acceleration, Vector3D Target_Position, Vector3D Target_Velocity, Vector3D Target_Acceleration,    
							double Bullet_InitialSpeed, double Bullet_Acceleration, double Bullet_MaxSpeed)   
{   
	//迭代算法   
	Vector3D HitPoint = new Vector3D();   
	Vector3D Smt = Target_Position - Me_Position;//发射点指向目标的矢量   
	Vector3D Velocity = Target_Velocity - Me_Velocity; //目标飞船和自己飞船总速度

           // 仅针对无加速炮弹的精确算法
           if (Bullet_Acceleration < 0.01) {
           if (Smt.Length() == 0) return Vector3D.Zero;
           Vector3D tarN = Vector3D.Normalize(Smt);
           Vector3D rv = Vector3D.Reject(Velocity, tarN);
           // 轴向(剩余)速度长度
           double av = Math.Sqrt(Bullet_MaxSpeed*Bullet_MaxSpeed - rv.Length()*rv.Length());
           // 敌方轴向速度
           Vector3D tvAx = Velocity - rv;
           double needTime = av / (tvAx.Length());
           // 总速度
           Vector3D bv = rv + tarN*av;
           return Me_Position + bv * needTime;
           }

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

void checkFcsTarget(out Vector3D targetPosition, out Vector3D targetVelocity) {
CustomConfiguration cfgTarget = new CustomConfiguration(fcsComputer);
cfgTarget.Load();

string tmpS = "";
cfgTarget.Get("Position", ref tmpS);
Vector3D.TryParse(tmpS, out targetPosition);
cfgTarget.Get("Aiming", ref tmpS);
//LockTargetAiming = tmpS == "True";
// if fcs have target launch missile immediatly
// if(targetPanelPosition!=Vector3D.Zero && autoFireMissile ) fireMissile();

cfgTarget.Get("Velocity", ref tmpS);
Vector3D.TryParse(tmpS, out targetVelocity);

//cfgTarget.Get("Asteroid", ref tmpS);
//Vector3D.TryParse(tmpS, out asteroidPosition);

//cfgTarget.Get("radarHighThreatPosition", ref tmpS);
//Vector3D.TryParse(tmpS, out radarHighThreatPosition);

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
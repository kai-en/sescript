static int SMOKE_DENSITY = 4; // larger means less smoke
static int rgms_no = 0;
// static float missileGravityRate = 5F;
double GUILD_RATE = 0.3;
double ATAN_BASE = 0.5;
double APID_P = 10;
double APID_D = 1.0;

//double MISSILE_MASS = 3398.8;
//double MISSILE_MASS = 5316.4;
//double MISSILE_MASS = 1989;
//double MISSILE_MASS = 1407.4;
double LaunchDist = 150;
//int dropTime = 0;
bool isClearDown=false;
	    
static string debugInfo = "";
static long timestamp = 0;
const int MISSILE_BUILD_TIME = 20;
bool isAeroDynamic = false;
double aero_liftrate = 0.1;
bool isPn = false;

static bool autoFireMissile = false;
static bool isTrackVelocity = true;
static long lastAutoFire = 1000;
static long AUTO_FIRE_INTERVAL = 600;
static long AUTO_FIRE_MAX = 4;

IMyTerminalBlock pgtimer = null;

//Introduction
            #region Introduction
            /*Introduction
            ----------------------
            Hello and thank you for downloading Rdavs Easy Guided Missile Script
            This Script adds an advanced but leightweight tracking algorithm for
            player made missiles that excels in dogfights and ship to ship warfare.
             
            Simply paste or install this code into the programmable block and refer
            to the custom-data of this block for easily accessible instructions
            and hints and tips.
             
            Additional information can be found on the workshop page for the script
            Any questions or queries don't hesitate to contact me!

            Rdav 16/07/2018

            ChangeLog:
             * Release Version
             * changed to 'contains #A#' on init
             * changed to any turrets
             * //Update 1.01
             * Fixed damaged block/ no thrusters on missile rare exception 
             * added launch contingency error checking
             * // Update 1.02
             * Refactored missile tag now is changable
             * Updated sound block tag
             * Updated so can use turrets on missiles (rename turrets on missiles custom info to "NoUse")
             * Updated so missiles will accelerate out even if their launch distance is incorrectly set
             * //Update 1.03
             */

            //--------------------- Player Editable Values -----------------------------

            //CHANGE LAUNCH DISTANCE HERE: 
            //(only uses this value of it is not 0)
            //Launch distance of the missile (in m) before guidance

            //CHANGE MISSILE TAG HERE:
            //Changes The prefix tag that the missile uses
            //Note that the instructions will not update to this tag 
            static string MissileTag = "#A#"+rgms_no;

            //-------------- Don't Touch Anything Beneath This Line --------------------

            string VERSION = "1.03";

            #endregion

            //Constants And Classes
            #region Consts & Classes
            //Classes
            class MISSILE
            {
                //Terminal Blocks On Each Missile
                public IMyTerminalBlock GYRO;
                public IMyTerminalBlock TURRET;
                public IMyTerminalBlock MERGE;
                public List<IMyThrust> THRUSTERS = new List<IMyThrust>(); //Multiple
                public IMyTerminalBlock POWER;
                public List<IMyTerminalBlock> WARHEADS = new List<IMyTerminalBlock>(); //Multiple
	    public List<IMyTerminalBlock> SPOTLIST= new List<IMyTerminalBlock>();
	    public List<IMyTerminalBlock> LANDINGLIST= new List<IMyTerminalBlock>();
                public IMyTerminalBlock SMOKE;

                //Permanent Missile Details
                public double MissileAccel = 10;
                public double MissileMass = 0;
                public double MISSILE_MASS = 5136.4;
                public int THRUSTER_COUNT = 4;
                public bool IS_SIDE = false;
                public double THRUST_PERCENT = 1;
                public int MISSILE_TTL=6000;
                public double MissileThrust = 0;
                public bool IsLargeGrid = false;
                public double FuseDistance = 2;

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
	    public PIDController pidA = new PIDController(1F, 0F, 2F, 1F, 1F, 60);
	    public PIDController pidE = new PIDController(1F, 0F, 2F, 1F, 1F, 60);
	    public PIDController pidR = new PIDController(1F, 0F, 2F, 1F, 1F, 60);
	    }
            List<MISSILE> MISSILES = new List<MISSILE>();

            //Consts
            double Global_Timestep = 0.01667; 
            double PNGain = 3;
	double Offset = 0;
            double ThisShipSize = 10;
            string Lstrundata = "Please ensure you have read the \n setup and hints & tips, found within \n the custom data of this block\n";
            IEnumerator<bool> LaunchStateMachine;
            IMyLargeTurretBase Turret;
	// a b K
	IMyTextPanel gcTargetPanel = null;
	String gcTargetPanelName="LCD Panel GC Target"+rgms_no;
	IMyTerminalBlock fcsComputer = null;
	String fcsComputerName="fcs";
List<Vector3D> LTPs = new List<Vector3D>();
List<Vector3D> LTVs = new List<Vector3D>();

            List<IMyTextSurface> displaySurfaces = new List<IMyTextSurface>(); 
	
            IMySoundBlock Alarm;
            IMyShipController RC;
            class Refueler {
              public IMyMotorStator h1;
              public IMyMotorStator h2;
              public IMyShipMergeBlock m;
              public IMyGridTerminalSystem gts;
              public int status; // 0 = standby 1 processing 2 finished
              public long pStart = 0;
              public Refueler(IMyMotorStator h, IMyGridTerminalSystem gts) {
                this.h1 = h;
                this.gts = gts;
                List<IMyMotorStator> h2clist = new List<IMyMotorStator>();
                gts.GetBlocksOfType<IMyMotorStator>(h2clist, h2 => h2.CustomName.Contains("[MS 2]") && (h2.GetPosition() - h.GetPosition()).Length() < 1.1);
                if (h2clist.Count == 0) throw new Exception("Refueler init error");
                this.h2 = h2clist[0];
              }
              
              private void checkStart() {
                List<IMyShipMergeBlock> mclist = new List<IMyShipMergeBlock>();
                gts.GetBlocksOfType<IMyShipMergeBlock>(mclist, m => m.CustomName.Contains(MissileTag) && (m.GetPosition() - h1.GetPosition()).Length() < 1.6);
                if (mclist.Count == 0) return;
                this.m = mclist[0];
                debugInfo += "\n" + Math.Round((this.m.GetPosition() - h1.GetPosition()).Length(), 2);
                status = 1;
                pStart = timestamp;
              }
              
              private void doingRefuel() {
                if (timestamp < pStart + (MISSILE_BUILD_TIME + 3) * 60) {
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 8) * 60) {
                  this.h1.SetValueFloat("Velocity", (float)5);
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 9) * 60) {
                  if (!this.h2.IsAttached) {
                    this.h2.ApplyAction("Attach");
                  }
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 19) * 60) {
                  if(this.m.Enabled) {
                    this.m.Enabled = false;
                  }
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 20) * 60) {
                  if(!this.m.Enabled) {
                    this.m.Enabled = true;
                  }
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 21) * 60) {
                  if (this.h2.IsAttached) {
                    this.h2.ApplyAction("Detach");
                  }
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 26) * 60) {
                  this.h1.SetValueFloat("Velocity", (float)-5);
                } else if (timestamp < pStart + (MISSILE_BUILD_TIME + 27) * 60) {
                  status = 2;
                }
              }
              
              private void checkFired() {
                if ((m.GetPosition() - h1.GetPosition()).Length() > 10) {
                  status = 0;
                  this.m = null;
                }
              }
              
              public void process() {
                switch(status) {
                case(0):
                checkStart();
                break;
                case(1):
                doingRefuel();
                break;
                case(2):
                checkFired();
                break;
                default:
                break;
                }
              }
            }
            List<Refueler> refuelerList = new List<Refueler>();
            #endregion

            //Initialiser
            #region Setup Stages
            Program()
            {
                //Sets Runtime
                Runtime.UpdateFrequency = Global_Timestep == 0.1667? UpdateFrequency.Update10 : UpdateFrequency.Update1;

                //Setup String
                string SetupString = "Rdavs Guided Missile Script Hints Tips & Setup \n================================================== \n \n \nSystem Setup \n=================== \n \nTo Set Up The Ship: \n------------------------ \n- Put this Code Into a P-block \n- Install a sound block (optional) \n- Install a turret called #A# (seeker turret) \n- Recompile if prompted to 'reset' \n- To fire from the toolbar 'run' this Pb with \n  the argument 'Fire' \n \n\nTo Set Up A Missile: \n--------------------------- \n- Every Missile Requires: \n    ~ 1 gyro\n    ~ Forward thrusters \n    ~ 1 Merge Block \n    ~ 1 battery/power source \n    ~ Warheads (optional) \n    ~ Side thrusters (if in gravity)\n \n- Call everything on missile #A# \n- Weld/paste missile onto launching ship \n- Same missile design can be pasted multiple times \n- Missile(s) are now ready to fire!\n\n \n\nSystem Usage \n=================== \n \nMove to engagement distance (800m), a distinctive  \ntarget lock bleeping will sound from the sound block. \n(if sound block installed) \n \n'Run' The programmable block with the argument  \n'Fire' this will launch the next available missile.  \nthis action can be bound to the toolbar of any cockpit. \n \nA ship can have up to 100 missiles active at any one  \npoint, missile setup for every missile is identical  \nand thus missiles can be printed, copy pasted, etc. \n\n- NOTE:\n- For ALL missiles the weight of the missile\n  (can be found in the 'info' tab in kg)\n  should be written as a number into the \n  custom data of the missiles gyro.\n  This is not compulsory but is necessary for\n  accurate guidance.\n\n\nTroubleshooting: \n============================\n\nif you find that your missiles are:\n\n - Sinking and hit the ground in gravity\n - Miss the target / have sub par guidance\n - Not tracking an enemy\n - Not firing at all\n\nHere are some of the most common faults:\n\n- Check the terminal of the PB, this might show\n  some useful error readouts\n\n- Boost the acceleration of the missile! \n  (especially if unable to hit targets)\n\n- Ensure the turret (called #A#) is turned on \n  and set to attack enemy targets, what this turret\n  targets is what the script will track\n\n- The weight of the missile should be input\n  into the gyros custom data,\n\n- Lateral (side) thrusters are not compulsory but\n  are very useful at helping the guidance\n  especially in natural gravity\n\n \n \nHints & Tips \n=================== \n \n  \nPerformance Tips:  \n------------------------ \n \n- Use light and fast missiles for best small ship  \n  tracking capability, the key to a good hit rate is \n  good missile design!\n \n- For 'best' target tracking ensure your missile has  \n  at least 3x the acceleration of the ship you are  \n  intending to take out. \n \n- Lateral (sideways) thrusters can be used for better  \n  gravity correction and handling. \n \n \nUsage Tips: \n---------------------- \n \n- ID'ing the target can be done with common sense, (ie looking \n  at what the seeker turret is currently looking at) \n \n- you can fire a missile without lock to 'laser guide it'  \n  (ie it follows where you are pointing your ship) \n  towards an enemy, once close tracking will engage \n \n- The missile will (by default) not engage guidance until further  \n  than the ships radius away from where it launched. This should  \n  make it practical for launch tubes/ not damaging the launching ship.  \n\n- If a customn launch distance is required this can be changed by changing\n  the 'launch distance' value in the code shortly after the introduction\n\n- An extra seeker turret can be put on the missile itself (must be called #A#)\n  this missile will then use that turret to guide itself.\n\n- The missile tracking is good but as with anything depends on the pilots\n  ability to use them well, read up on real-world missile usage to help \n  boost your hit rate.\n\n\n\n  \n"; 
                Me.CustomData = SetupString;

                //Sets ShipSize
                ThisShipSize = (Me.CubeGrid.WorldVolume.Radius);
                ThisShipSize = LaunchDist == 0 ? ThisShipSize : LaunchDist;

                //Collects First Turret
                List<IMyTerminalBlock> TempCollection = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(TempCollection, a => a.CustomName.Contains(MissileTag) && a.DetailedInfo != "NoUse");
                if (TempCollection.Count > 0)
                {Turret = TempCollection[0] as IMyLargeTurretBase; }

                //Collects Sound/Alarm
                List<IMyTerminalBlock> TempCollection2 = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMySoundBlock>(TempCollection2, a => a.DetailedInfo != "NoUse");
                if (TempCollection2.Count > 0)
                { 
                    Alarm = TempCollection2[0] as IMySoundBlock;
                    Alarm.SelectedSound = "SoundBlockAlert2";
                    Alarm.LoopPeriod = 99999;
                    Alarm.Play();
                    Alarm.Enabled = false;
                }

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

                List<IMyMotorStator> TempCollection5 = new List<IMyMotorStator>();
                GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(TempCollection5, a => a.CustomName.Contains("[MS 1]") );
                TempCollection5 = TempCollection5.OrderBy(g=>{
	    var rcmt = RC.WorldMatrix;
	    var tranmt = MatrixD.CreateLookAt(new Vector3D(), rcmt.Forward, rcmt.Up);
                var dis = Vector3D.TransformNormal(g.GetPosition()-RC.GetPosition(), tranmt);
                var x = -Math.Round((Math.Abs(dis.X)-1.25) * 0.2, 0);
                var z = -Math.Round(Math.Abs(dis.Z), 1);
	    return (x * 1000 + z) * 100 + dis.X;
	    }).ToList();

                foreach(var h in TempCollection5) {
                  Refueler r;
                  try {
                  r = new Refueler(h, GridTerminalSystem);
                  } catch {
                  continue;
                  }
                  refuelerList.Add(r);
                }

                List<IMyTextSurface> tmpList =  new List<IMyTextSurface>();
                GridTerminalSystem.GetBlocksOfType<IMyTextSurface> (tmpList, b => ((IMyTerminalBlock)b).CustomName.Contains("M_LCD"));
                displaySurfaces.AddRange(tmpList);
                if (RC is IMyTextSurfaceProvider) {
                    var tmp =((IMyTextSurfaceProvider)RC).GetSurface(4);
                    if (tmp != null) displaySurfaces.Add(tmp);
                }

                List<IMyTerminalBlock> tList = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTimerBlock> (tList, b => ((IMyTerminalBlock)b).CustomName.Contains("PG-"));
                if (tList.Count > 0) pgtimer = tList[0];
            }
            #endregion

            //Main Method
            #region Main Method
            void Main(string argument)
            {
                timestamp ++;
                //General Layout Diagnostics
                OP_BAR();
                QuickEcho(MISSILES.Count, "Active (Fired) Missiles:");
                QuickEcho(refuelerList.Count, "Refuelers:");
                QuickEcho(autoFireMissile, "Auto fire:");
                QuickEcho(isTrackVelocity, "Track V:");
                QuickEcho(isPn?"PN":"Parallel", "Guild rate:");
                QuickEcho(Runtime.LastRunTimeMs, "Runtime:");
Echo(debugInfo);
                Echo("Version:  " + VERSION);
	    //Echo("PNGain: " + PNGain);
	    Echo("Offset: " + Offset);
	    Echo("Agile: " + GUILD_RATE + " " + ATAN_BASE + " " + APID_P + " " + APID_D);
                Echo("\nInfo:\n---------------");
                Echo(Lstrundata);

                #region Block Error Readouts

                //Core Block Checks
                if (RC == null || RC.CubeGrid.GetCubeBlock(RC.Position) == null)
                { Echo(" ~ No Ship Control Found,\nInstall Forward Facing Cockpit/RC/Flightseat And Press Recompile"); RC = null; return; }
                if ((Turret == null || Turret.CubeGrid.GetCubeBlock(Turret.Position) == null)
		&& gcTargetPanel == null && fcsComputer == null
		)
                { Echo(" ~ Searching For A new Seeker\n (Guidance Turret)\n Name Of Turret Needs to be: '#A#'\nInstall Block And Press Recompile"); Turret = null; return; }
                if (Alarm == null || Alarm.CubeGrid.GetCubeBlock(Alarm.Position) == null)
                { Echo(" ~ No Sound Block For Lock Tone Found,\nInstall Block And Press Recompile\n (script will work fine without) "); Alarm = null; }

                #endregion

                //Sounds 60 hz growl lock Alarm
                if (Turret != null && Alarm != null)
                {
                    if (!Turret.GetTargetedEntity().IsEmpty() && !Alarm.Enabled)
                    { Alarm.Enabled = true; }
                    else if (Turret.GetTargetedEntity().IsEmpty() && Alarm.Enabled)
                    { Alarm.Enabled = false; }
                }

                //Begins Launch Scheduler
                //-----------------------------------------
                if (argument == "Fire" && LaunchStateMachine == null)
                {
                    LaunchStateMachine = MissileLaunchHandler().GetEnumerator();
                }
	    else if ( argument.Contains(":")) {
	        string[] args = argument.Split(':');
	        if (args[0] == "SetPN") {
	            double value = 0;
	            if (double.TryParse(args[1], out value)) {
		    PNGain = value;
		}
	       }else if (args[0] == "SetOffset") {
	            double value = 0;
	            if (double.TryParse(args[1], out value)) {
		    Offset = value;
		}	           
	       }else if (args[0] == "SetAuto") {
                       if (args[1] == "True") autoFireMissile = true;
                       else autoFireMissile = false;
	       }else if (args[0] == "SetTrack") {
                       if (args[1] == "True") isTrackVelocity = true;
                       else isTrackVelocity = false;
                   }else if (args[0] == "SetUsePN") {
                       if (args[1] == "True") isPn = true;
                       else isPn = false;
                   }
	    }
	    else if (argument != "Fire" && argument != "")
                {
                    Lstrundata = "Unknown/Incorrect launch argument,\ncheck spelling & caps,\nto launch argument should be just: Fire\n ";
                }

	    // autoFire
                autoFireProcess();

	    bool targetPanelHasTarget = false;
	    Vector3D targetPanelPosition = Vector3D.Zero;
	    Vector3D targetPanelVelocity = Vector3D.Zero;

            
if (fcsComputer != null && targetPanelPosition == Vector3D.Zero) {
checkFcsTarget(out targetPanelPosition, out targetPanelVelocity);
targetPanelHasTarget = targetPanelPosition!=Vector3D.Zero;

}

                //Runs Guidance Block (foreach missile)
                //---------------------------------------
                for (int i = 0; i < MISSILES.Count; i++)
                {
                    var ThisMissile = MISSILES[i];

                    //Runs Standard System Guidance
                    if (ThisMissile.IS_CLEAR == true)
                    { }

                    //Fires Straight (NO OVERRIDES)
                    else if (ThisMissile.IS_CLEAR == false)
                    {
                        if ((ThisMissile.GYRO.GetPosition() - Me.GetPosition()).Length() > ThisShipSize)
                        { ThisMissile.IS_CLEAR = true; }
                    }
                    STD_GUIDANCE(ThisMissile, ThisMissile.IS_CLEAR, targetPanelPosition, targetPanelVelocity, targetPanelHasTarget); 

                    //Disposes If Out Of Range Or Destroyed (misses a beat on one missile)
                    bool Isgyroout = ThisMissile.GYRO.CubeGrid.GetCubeBlock(ThisMissile.GYRO.Position) == null;
                    bool Isthrusterout = ThisMissile.THRUSTERS[0].CubeGrid.GetCubeBlock(ThisMissile.THRUSTERS[0].Position) == null;
                    bool Isouttarange = (ThisMissile.GYRO.GetPosition() - Me.GetPosition()).LengthSquared() > 9000 * 9000;
                    if (Isgyroout || Isthrusterout || Isouttarange)
                    { MISSILES.Remove(ThisMissile); }

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

                debugInfo = "refueler";
                foreach(var r in refuelerList) {
                  r.process();
                }
                drawMissile();
            }
            #endregion

Color backColor = new Color(0, 0, 0, 255); 
Color borderColor = new Color(178, 255, 255, 255);
Color fullColor = new Color(127, 255, 183, 255);
Color unfullColor = new Color(255, 255, 183, 255); 
            void drawMissile() {
                 foreach(var surface in displaySurfaces) {
                         surface.ContentType = ContentType.SCRIPT; 
        surface.Script = ""; 
 
        Vector2 surfaceSize = surface.TextureSize;
        Vector2 screenCenter = surfaceSize * 0.5f; 
        Vector2 viewportSize = surface.SurfaceSize; 

        using (var frame = surface.DrawFrame()) 
        {
	List<String> sts = new List<String>();
	surface.GetSprites(sts);
            MySprite sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", color: backColor); 
            sprite.Position = screenCenter; 
            frame.Add(sprite); 

            var count = refuelerList.Count;
            if (count == 0) return;
            float persize = 0;
            float persizeH = surfaceSize.Y * 0.7f;
            if (count < 4) {
              persize = 0.25f * surfaceSize.X;
            } else if (count < 16) {
              persize = (1f / count) * surfaceSize.X;
            } else {
              persize = 0.0625f * surfaceSize.X;
            }

            for (int i = 0; i < count; i++) {
	Vector2 borderSize = new Vector2 (persize, persizeH);

	sprite = new MySprite(SpriteType.TEXTURE, "SquareHollow", size: borderSize, color: borderColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, surfaceSize.Y*0.5f);
	frame.Add(sprite); 
            }

            for (int i = 0; i < count; i++) {
              var refueler = refuelerList[i];
              var status = refueler.status;
              if (status == 0) continue;
              
	Vector2 borderSize = new Vector2 (persize * 0.33f, persizeH * 0.7f);

	sprite = new MySprite(SpriteType.TEXTURE, "SquareHollow", size: borderSize, color: borderColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, persizeH*0.65f + (surfaceSize.Y-persizeH) * 0.5f);
	frame.Add(sprite);
            borderSize = new Vector2 (persize * 0.33f, persizeH * 0.3f);
	sprite = new MySprite(SpriteType.TEXTURE, "Triangle", size: borderSize, color: borderColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, persizeH*0.15f + (surfaceSize.Y-persizeH) * 0.5f);
	frame.Add(sprite);
            borderSize = new Vector2 (persize , persizeH * 0.35f);
	sprite = new MySprite(SpriteType.TEXTURE, "Triangle", size: borderSize, color: borderColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, persizeH*0.475f + (surfaceSize.Y-persizeH) * 0.5f);
	frame.Add(sprite);
            borderSize = new Vector2 (persize , persizeH * 0.35f);
	sprite = new MySprite(SpriteType.TEXTURE, "Triangle", size: borderSize, color: borderColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, persizeH*0.825f + (surfaceSize.Y-persizeH) * 0.5f);
	frame.Add(sprite);
            var fuelSize = new Vector2 (persize * 0.33f - 2, persizeH * 0.7f - 2);
	sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: fuelSize, color: backColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, persizeH*0.65f + (surfaceSize.Y-persizeH) * 0.5f);
	frame.Add(sprite);

            float persent = 0;
            Color fColor = unfullColor;
            if (status == 2) {
            persent = 1f;
            fColor = fullColor;
            } else if (status == 1) {
              float t = (float) (timestamp - refueler.pStart);
              persent = t/((MISSILE_BUILD_TIME + 27) * 60);
            }
            var fSize = new Vector2 (persize * 0.33f - 2, (persizeH * 0.7f - 2) * persent);
	sprite = new MySprite(SpriteType.TEXTURE, "SquareSimple", size: fSize, color: fColor);
	sprite.Position = new Vector2(i*persize + persize*0.5f, persizeH*0.65f + (surfaceSize.Y-persizeH) * 0.5f
               + fuelSize.Y * (1 - persent) * 0.5f
            );
	frame.Add(sprite);            
            }

        }
                 }
            }

            //SubMethod State Machine For Launching Missiles
            #region MissileLaunchHandler
            public IEnumerable<bool> MissileLaunchHandler()
            {

                //Gathers Missile Information
                yield return INIT_NEXT_MISSILE();

	    // a b K
	    for (int i = 0; i < (rgms_no % 2) * 30; i++) {
	    	yield return true;
	    }

                //Disables Merge Block
                MISSILE ThisMissile = MISSILES[MISSILES.Count - 1];

                var MERGE_A = ThisMissile.MERGE;
                (MERGE_A as IMyShipMergeBlock).Enabled = false;

                foreach (IMyThrust thruster in ThisMissile.THRUSTERS) {
                    ((IMyTerminalBlock)thruster).ApplyAction("OnOff_On");
                    double ThisThrusterThrust = thruster.MaxThrust;
	        ThisThrusterThrust *= ThisMissile.THRUST_PERCENT;
                    thruster.ThrustOverride = (float)ThisThrusterThrust;
                }

		var POWER_A = ThisMissile.POWER;

		yield return true;

		if (ThisMissile.POWER != null && ThisMissile.POWER is IMyBatteryBlock)
                {
                    POWER_A.ApplyAction("OnOff_Off");
                    //POWER_A.SetValue("Recharge", false);
                    //POWER_A.SetValue("Discharge", true);
                    ThisMissile.MissileMass += POWER_A.Mass;
                }
		yield return true;

                if (ThisMissile.POWER != null && ThisMissile.POWER is IMyBatteryBlock)
                {
                    POWER_A.ApplyAction("OnOff_On");
                    //POWER_A.SetValue("Recharge", false);
                    //POWER_A.SetValue("Discharge", true);
                    ThisMissile.MissileMass += POWER_A.Mass;
                }

                // 
                //for (int i = 0; i < 5; i++) yield return true;


                pgtimer?.ApplyAction("TriggerNow");
                if (ThisMissile.LANDINGLIST.Count > 0) {
                for (int i = 0; i < 60; i++) yield return true;

                foreach (IMyTerminalBlock land in ThisMissile.LANDINGLIST) {
                    land.ApplyAction("Unlock");
                }
                }

	    //for (int i = 0; i < dropTime; i++) {
	    //	yield return true;
	    //}

                //Launches Missile & Gathers Next Scanner
                PREP_FOR_LAUNCH(MISSILES.Count - 1);

            }
            #endregion

            //Standard Guidance Routine
            #region RdavNav Missile Guidance #RFC#
            /*=================================================                           
             RdavNav             
             ---------------------------------------     */
            void STD_GUIDANCE(MISSILE This_Missile, bool isClear, Vector3D targetPanelPosition, Vector3D targetPanelVelocity, bool targetPanelHasTarget)
            {
                This_Missile.MISSILE_TTL --;
                if (This_Missile.MISSILE_TTL <=0) {
                foreach (var item in This_Missile.WARHEADS) { (item as IMyWarhead).IsArmed = true; }
                foreach (var item in This_Missile.WARHEADS){ (item as IMyWarhead).Detonate();}
                }

                //Targeting Module
                //-----------------------------------------------

                //Retrieves Target Position
                var This_Missile_Director = This_Missile.TURRET as IMyLargeTurretBase;
                var ENEMY_POS = new Vector3D();

                //Logical Determination Of Enemy Position
	    // a b K


	    bool usePanel = false;
	    
                if (This_Missile_Director != null &&
		This_Missile_Director.GetTargetedEntity().IsEmpty() == false)
                {
                    ENEMY_POS = This_Missile_Director.GetTargetedEntity().Position; //Also based on position for critical hits
                }
	    // a b K
	    else if(targetPanelHasTarget){
	        ENEMY_POS = targetPanelPosition;
	        usePanel = true;
	        Vector3D dir = ENEMY_POS - RC.GetPosition();
	        dir = Vector3D.Normalize(dir);
	        var rcmt = RC.WorldMatrix;
	        Vector3D tmp = Vector3D.Reject(dir, rcmt.Up);
	        if (tmp.Equals(Vector3D.Zero)) {
		tmp = rcmt.Forward;
	        }else {
		tmp = Vector3D.Normalize(tmp);
	        }
	        MatrixD rd = MatrixD.CreateFromDir(tmp, rcmt.Up);
	        ENEMY_POS = ENEMY_POS + Offset * rd.Right;

	    }
	    
                //else if (!(This_Missile.TARGET_PREV_POS == null)) //new Vector3D()
                //{ENEMY_POS = This_Missile.TARGET_PREV_POS;}
                else
                {
                    ENEMY_POS = RC.GetPosition() + RC.WorldMatrix.Forward * ((This_Missile.GYRO.GetPosition() - Me.GetPosition()).Length() + 300);
                }

                //Sorts CurrentVelocities
                Vector3D MissilePosition = This_Missile.GYRO.CubeGrid.WorldVolume.Center;
                Vector3D MissilePositionPrev = This_Missile.MIS_PREV_POS;
		Vector3D lastVelocity = This_Missile.lastVelocity;
                Vector3D MissileVelocity = (MissilePosition - MissilePositionPrev) / Global_Timestep;
		This_Missile.lastVelocity = MissileVelocity;

                Vector3D TargetPosition = ENEMY_POS;
                Vector3D TargetPositionPrev = This_Missile.TARGET_PREV_POS;
                Vector3D TargetVelocityNew = (TargetPosition - This_Missile.TARGET_PREV_POS) / Global_Timestep;
                Vector3D TargetAcc = (TargetVelocityNew - This_Missile.TargetVelocity) * 60;
                This_Missile.TargetVelocity = TargetVelocityNew;

	    // a b K
	    if (usePanel) {
                    TargetAcc = (targetPanelVelocity - This_Missile.TargetVelocityPanel) * 60;
	        This_Missile.TargetVelocity = targetPanelVelocity;
                    This_Missile.TargetVelocityPanel = targetPanelVelocity;
	    }
                if (This_Missile.MISSILE_TTL % SMOKE_DENSITY == 0)
                PlayAction(This_Missile.SMOKE, "ShootOnce");
                if(isClear == false) {
                    TargetPosition = RC.GetPosition() + RC.WorldMatrix.Up* 100000D;
                    if (isClearDown)
                    TargetPosition = RC.GetPosition() + RC.WorldMatrix.Down* 100000D;
                    TargetAcc = Vector3D.Zero;
                    This_Missile.TargetVelocity = RC.GetShipVelocities().LinearVelocity;
                    This_Missile.TargetVelocityPanel = This_Missile.TargetVelocity;
                }

                //Uses RdavNav Navigation APN Guidance System
                //-----------------------------------------------

                //Setup LOS rates and PN system
                Vector3D LOS_Old = Vector3D.Normalize(TargetPositionPrev - MissilePositionPrev);
                Vector3D LOS_New = Vector3D.Normalize(TargetPosition - MissilePosition);
                Vector3D Rel_Vel = Vector3D.Normalize(This_Missile.TargetVelocity - MissileVelocity); 
                Vector3D targetRange = TargetPosition - MissilePosition;
	    Vector3D targetV = This_Missile.TargetVelocity - MissileVelocity;

                //And Assigners
                Vector3D am = new Vector3D(1, 0, 0); double LOS_Rate; Vector3D LOS_Delta;
                Vector3D MissileForwards = This_Missile.THRUSTERS[0].WorldMatrix.Backward;
		This_Missile.lastAVelocity = MissileForwards - This_Missile.lastForward;
		if (This_Missile.lastForward == Vector3D.Zero) This_Missile.lastAVelocity = Vector3D.Zero;
		This_Missile.lastForward = MissileForwards;

                //Vector/Rotation Rates
                if (LOS_Old.Length() == 0)
                { LOS_Delta = new Vector3D(0, 0, 0); LOS_Rate = 0.0; }
                else
                { LOS_Delta = LOS_New - LOS_Old; LOS_Rate = LOS_Delta.Length() / Global_Timestep; }

                //-----------------------------------------------

                //Closing Velocity
                double Vclosing = (This_Missile.TargetVelocity - MissileVelocity).Length();

                //If Under Gravity Use Gravitational Accel
                //Vector3D GravityComp = -RC.GetNaturalGravity() * missileGravityRate;

                //Calculate the final lateral acceleration
                Vector3D LateralDirection = Vector3D.Normalize(Vector3D.Cross(Vector3D.Cross(Rel_Vel, LOS_New), Rel_Vel));
                Vector3D LateralAccelerationComponent = LateralDirection * PNGain * LOS_Rate * Vclosing + LOS_Delta * 9.8 * (0.5 * PNGain); //Eases Onto Target Collision LOS_Delta * 9.8 * (0.5 * Gain)

                //If Impossible Solution (ie maxes turn rate) Use Drift Cancelling For Minimum T
                double OversteerReqt = (LateralAccelerationComponent).Length() / This_Missile.MissileAccel;
                if (OversteerReqt > 0.98)
                {
                    LateralAccelerationComponent = This_Missile.MissileAccel * Vector3D.Normalize(LateralAccelerationComponent + (OversteerReqt * Vector3D.Normalize(-MissileVelocity)) * 40);
                }

                //Calculates And Applies Thrust In Correct Direction (Performs own inequality check)
                double ThrustPower = RdavUtils.Vector_Projection_Scalar(MissileForwards, Vector3D.Normalize(LateralAccelerationComponent)); //TESTTESTTEST
                ThrustPower = This_Missile.IsLargeGrid ? MathHelper.Clamp(ThrustPower, 0.9, 1) : ThrustPower;

                ThrustPower = This_Missile.THRUST_PERCENT;
                foreach (IMyThrust thruster in This_Missile.THRUSTERS)
                {
                    if (thruster.ThrustOverridePercentage !=  ThrustPower) //12 increment inequality to help conserve on performance
                    {
                        thruster.ThrustOverridePercentage = (float)ThrustPower;
                    }
                }

                //Calculates Remaining Force Component And Adds Along LOS
                double RejectedAccel = Math.Sqrt(This_Missile.MissileAccel * This_Missile.MissileAccel - LateralAccelerationComponent.LengthSquared()); //Accel has to be determined whichever way you slice it
                if (double.IsNaN(RejectedAccel)) { RejectedAccel = 0; }
                LateralAccelerationComponent = LateralAccelerationComponent + LOS_New * RejectedAccel;

                //-----------------------------------------------


                //Guides To Target Using Gyros
                // am = Vector3D.Normalize(LateralAccelerationComponent + GravityComp);
		
var FDIR = This_Missile.GYRO.WorldMatrix.Up;
if (This_Missile.IS_SIDE) FDIR = This_Missile.GYRO.WorldMatrix.Forward;
var UDIR = This_Missile.GYRO.WorldMatrix.Backward;
if (This_Missile.IS_SIDE) UDIR = This_Missile.GYRO.WorldMatrix.Up;

if(true) {
// 新算法
// 4 推力 / 质量 = 可以提供的加速度的长度 sdl

double thrust = This_Missile.THRUSTERS[0].MaxEffectiveThrust * This_Missile.THRUSTERS.Count;
thrust *= This_Missile.THRUST_PERCENT;

double sdl = thrust / This_Missile.MISSILE_MASS;

// 1 求不需要的速度
//debugInfo = "TR: " + targetRange.Length();
//debugInfo += "\nIS_SIDE: " + This_Missile.IS_SIDE;
Vector3D tarN = Vector3D.Normalize(targetRange);
//debugInfo += "\nTV: " + targetV.Length();
Vector3D rv = Vector3D.Reject(targetV, tarN);
//debugInfo += "\nRV: " + rv.Length();
//Vector3D ra = Vector3D.Reject(TargetAcc, tarN);

// 1.1 拦截方式
var otv = This_Missile.TargetVelocity;
double trackMinDis = 200;
if (otv.Length() > 10 && isTrackVelocity && targetRange.Length() > trackMinDis) {
var rrd = Vector3D.Reject(targetRange, Vector3D.Normalize(otv));
if (rrd.Length() < 100) rrd = Vector3D.Zero;
else rrd -= Vector3D.Normalize(rrd) * 100;
var rrv = rrd * 0.3; // rr rate
if (rrv.Length() > 90) { //
rrv = Vector3D.Normalize(rrv) * 90;
}
rrv = Vector3D.Reject(rrv, tarN);
rv += rrv;
}

// 2 换算不需要的加速度 平行制导率
Vector3D rvN = Vector3D.Normalize(rv);
double newLen = Math.Atan2(rv.Length(), ATAN_BASE);
Vector3D newRv = rvN * newLen;
Vector3D rdo = newRv * GUILD_RATE * 60
//+ ra * 0.5
;

if(isPn) { // use PN
double PN_RATE = 3000;
Vector3D losD = (LOS_New - LOS_Old) * 60
//+ ra * 0.5
;
double losDl = losD.Length();
Vector3D sideN = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(MissileVelocity)));
Vector3D graN = Vector3D.Normalize(RC.GetNaturalGravity());
double rdol_pn = Math.Atan2(losDl, ATAN_BASE) * PN_RATE;
if (rdol_pn > sdl * 0.5) rdol_pn = sdl * 0.5;
Vector3D rdo_pn = sideN * rdol_pn;


// 3 加上抵抗重力所需的加速度 = 需要抵消的加速度 rd
Vector3D rd = rdo - RC.GetNaturalGravity();
// 3.1 aerodynamic
if (isAeroDynamic) {
var vN = Vector3D.Normalize(MissileVelocity);
var aero_a = Vector3D.Reject (FDIR, vN);
aero_a = Vector3D.Reject(aero_a, This_Missile.GYRO.WorldMatrix.Left);
var aero_dot = Vector3D.Dot(FDIR, vN) ;
if ( aero_dot > 0.7 || aero_dot < 0) aero_a = Vector3D.Zero;
var lift = aero_a * MissileVelocity.Length() * aero_liftrate;
rd -= lift;
}

double rdl = rd.Length();
Vector3D rd_pn = rdo_pn - RC.GetNaturalGravity();
double rdl_pn = rd_pn.Length();


// 5 剩余加速度长度
// 5.1 rd相对于LOS 需要的侧向加速度
Vector3D rd2_pn = Vector3D.Reject(rd_pn, tarN);
double rd2l_pn = rd2_pn.Length();
if (sdl < rd2l_pn) rd2l_pn = sdl;
double pdl_pn = Math.Sqrt(sdl*sdl - rd2l_pn * rd2l_pn);

// 6 剩余加速度方向  = los
//Vector3D pdN = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(rd)));
//if (pdN.Length() == 0) pdN = LOS_New;
//Vector3D pdN = LOS_New;

//Vector3D pdN_pn = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(rd_pn)));
Vector3D pdN_pn = LOS_New;

// 7 剩余加速度
//Vector3D pd = pdN * pdl;
Vector3D pd_pn = pdN_pn * pdl_pn;

// 8 总加速度
//Vector3D sd = rd2 + pd;
Vector3D sd_pn = rd2_pn + pd_pn;

// 9 总加速度方向
Vector3D nam = Vector3D.Normalize(sd_pn);

if (targetRange.Length() < This_Missile.nearest)
This_Missile.nearest = targetRange.Length();
//double pn_test = (Vector3D.Normalize(MissileVelocity) - Vector3D.Normalize(lastVelocity)).Length() / ((LOS_New - LOS_Old).Length()*60);

am = nam;

} else { // use parallel

// 1.1 比例导引法 PN

Vector3D losD = (LOS_New - LOS_Old) * 60
//+ ra * 0.5
;
double losDl = losD.Length();
Vector3D sideN = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(MissileVelocity)));
Vector3D graN = Vector3D.Normalize(RC.GetNaturalGravity());

// 3 加上抵抗重力所需的加速度 = 需要抵消的加速度 rd
Vector3D rd = rdo - RC.GetNaturalGravity();
// 3.1 aerodynamic
if (isAeroDynamic) {
var vN = Vector3D.Normalize(MissileVelocity);
var aero_a = Vector3D.Reject (FDIR, vN);
aero_a = Vector3D.Reject(aero_a, This_Missile.GYRO.WorldMatrix.Left);
var aero_dot = Vector3D.Dot(FDIR, vN) ;
if ( aero_dot > 0.7 || aero_dot < 0) aero_a = Vector3D.Zero;
var lift = aero_a * MissileVelocity.Length() * aero_liftrate;
rd -= lift;
}

double rdl = rd.Length();

// 5 剩余加速度长度
// 5.1 rd相对于LOS 需要的侧向加速度
Vector3D rd2 = Vector3D.Reject(rd, tarN);
double rd2l = rd2.Length();
if (sdl < rd2l) sdl = rd2l;
// 5.2 剩余加速度长度 
double pdl = Math.Sqrt(sdl*sdl - rd2l * rd2l);

// 6 剩余加速度方向  = los
//Vector3D pdN = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(rd)));
//if (pdN.Length() == 0) pdN = LOS_New;
Vector3D pdN = LOS_New;

// 7 剩余加速度
Vector3D pd = pdN * pdl;

// 8 总加速度
Vector3D sd = rd2 + pd;

// 9 总加速度方向
Vector3D nam = Vector3D.Normalize(sd);

if (targetRange.Length() < This_Missile.nearest)
This_Missile.nearest = targetRange.Length();
//double pn_test = (Vector3D.Normalize(MissileVelocity) - Vector3D.Normalize(lastVelocity)).Length() / ((LOS_New - LOS_Old).Length()*60);

am = nam;
}

var missileLookAt = MatrixD.CreateLookAt(new Vector3D(), FDIR, UDIR);
var amToMe = Vector3D.TransformNormal(am, missileLookAt);
var rr = Vector3D.Normalize(Vector3D.Reject(FDIR, Vector3D.Normalize(RC.GetNaturalGravity())));
var rangle = 1 - Vector3D.Dot(rr, tarN);
//debugInfo += "\nRA: " + rangle;

//debugInfo += "\namToMe: " + displayVector3D(amToMe);
//debugInfo += "\nnearest" + This_Missile.nearest;


}

                double Yaw; double Pitch;
                GyroTurn6(am, APID_P, APID_D, This_Missile.THRUSTERS[0], This_Missile.GYRO as IMyGyro, This_Missile.PREV_Yaw, This_Missile.PREV_Pitch, out Pitch, out Yaw, ref This_Missile, isClear);

                //Updates For Next Tick Round
                This_Missile.TARGET_PREV_POS = TargetPosition;
                This_Missile.MIS_PREV_POS = MissilePosition;
                This_Missile.PREV_Yaw = Yaw;
                This_Missile.PREV_Pitch = Pitch;

                //Detonates warheads in close proximity
                bool targetNeer = false;
                if ((TargetPosition - MissilePosition).Length() < 40 && This_Missile.WARHEADS.Count > 0) //Arms
                { foreach (var item in This_Missile.WARHEADS) { (item as IMyWarhead).IsArmed = true; } targetNeer = true; }
	    bool targetGetFar = (TargetPosition - MissilePosition).LengthSquared() > (TargetPositionPrev - MissilePositionPrev).LengthSquared() && (TargetPosition - MissilePosition).LengthSquared() < 4*4 ;
                if (targetGetFar){
                foreach (var item in This_Missile.WARHEADS){ (item as IMyWarhead).StartCountdown();}
                }
                bool missileStop = Vector3D.Dot(Vector3D.Normalize(MissileVelocity), Vector3D.Normalize(lastVelocity))<0.8;
                missileStop = missileStop && targetNeer;
                if ((missileStop)&& This_Missile.WARHEADS.Count > 0) //A mighty earth shattering kaboom
                {
		foreach (var item in This_Missile.WARHEADS){ (item as IMyWarhead).Detonate();}
                }

            }
            #endregion

            //Finds First Missile Available
            #region RFC Initialise Missile Blocks #RFC#
            /*=================================================                           
            Function: RFC Function bar #RFC#                  
            ---------------------------------------     */
            bool INIT_NEXT_MISSILE()
            {

                //Finds Missile Blocks (performs 1 gts)
                List<IMyTerminalBlock> GYROS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyGyro>(GYROS, b => b.CustomName.Contains(MissileTag));
                List<IMyTerminalBlock> TURRETS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(TURRETS, b => b.CustomName.Contains(MissileTag));
                List<IMyThrust> THRUSTERS = new List<IMyThrust>();
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(THRUSTERS, b => b.CustomName.Contains(MissileTag));
                List<IMyTerminalBlock> MERGES = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(MERGES, b => {
                if (!b.CustomName.Contains(MissileTag)) return false;
                bool found = false;
                foreach(var r in refuelerList) {
                  if (r.m != b) continue;
                  found = true;
                  if(r.status != 2) break;
                  return true;
                }
                if (found) return false;
                else return true;
                });
                List<IMyTerminalBlock> BATTERIES = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(BATTERIES, b => b.CustomName.Contains(MissileTag) && (b is IMyBatteryBlock || b is IMyReactor));
                List<IMyTerminalBlock> WARHEADS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyWarhead>(WARHEADS, b => b.CustomName.Contains(MissileTag));
                List<IMyTerminalBlock> SPOTS = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyReflectorLight>(SPOTS, b => b.CustomName.Contains(MissileTag));

                //Diagnostics For No Gyro
                Lstrundata = "No More Missile (Gyros) Detected";

                //Iterates Through List To Find Complete Missile Based On Gyro
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
                    MISSILE NEW_MISSILE = new MISSILE();
                    NEW_MISSILE.GYRO = Key_Gyro;
			CustomConfiguration cfg = new CustomConfiguration(NEW_MISSILE.GYRO);
			cfg.Load();
			cfg.Get("MISSILE_MASS", ref NEW_MISSILE.MISSILE_MASS);
			cfg.Get("THRUSTER_COUNT", ref NEW_MISSILE.THRUSTER_COUNT);
			cfg.Get("IS_SIDE", ref NEW_MISSILE.IS_SIDE);
			cfg.Get("THRUST_PERCENT", ref NEW_MISSILE.THRUST_PERCENT);

                    Vector3D GyroPos = Key_Gyro.GetPosition();
                    double Distance = 3;

                    //Sorts And Selects Turrets
                    List<IMyTerminalBlock> TempTurrets = TURRETS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < 100000);
                    TempTurrets.Sort((x, y) => (x.GetPosition() - Key_Gyro.GetPosition()).LengthSquared().CompareTo((y.GetPosition() - Key_Gyro.GetPosition()).LengthSquared()));

                    //Sorts And Selects Batteries
                    List<IMyTerminalBlock> TempPower = BATTERIES.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
                    TempPower.Sort((x, y) => (x.GetPosition() - Key_Gyro.GetPosition()).LengthSquared().CompareTo((y.GetPosition() - Key_Gyro.GetPosition()).LengthSquared()));

                    //Sorts And Selects Merges
                    List<IMyTerminalBlock> TempMerges = MERGES.FindAll(b => (b.GetPosition() - GyroPos).Length() < 2);
                    TempMerges.Sort((x, y) => (compareP(x, y, Key_Gyro)));

                    //Sorts And Selects Thrusters
                    //NEW_MISSILE.THRUSTERS = THRUSTERS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);

                    //Sorts And Selects Warheads
	        List<IMyTerminalBlock> TempWarhead = WARHEADS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
	        TempWarhead.Sort((x, y) => (compareP(x, y, Key_Gyro)));
	        if (TempWarhead.Count > 0)
                    NEW_MISSILE.WARHEADS.Add(TempWarhead[0]);

                    // a b K
                    List<IMyThrust> TempThrusters = THRUSTERS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
                    TempThrusters.Sort((x, y) => (compareP(x, y, Key_Gyro)));
	        List<IMyThrust> T2Thrusters = new List<IMyThrust>();
	        int TCount = NEW_MISSILE.THRUSTER_COUNT < TempThrusters.Count ? NEW_MISSILE.THRUSTER_COUNT :  TempThrusters.Count ;
	        for(int i = 0; i < TCount; i++) {
		T2Thrusters.Add(TempThrusters[i]);
	        }
	        NEW_MISSILE.THRUSTERS = T2Thrusters;


                    List<IMyTerminalBlock> TempSpots = SPOTS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
	        TempSpots.Sort((x, y) => (compareP(x, y, Key_Gyro)));
	        if(TempSpots.Count > 0)
	        NEW_MISSILE.SPOTLIST.Add(TempSpots[0]);

                    if (TempMerges.Count > 0) {
                    List<IMyTerminalBlock> LANDS = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(LANDS, b => b.CustomName.Contains(MissileTag));
                    List<IMyTerminalBlock> TempLands = LANDS.FindAll(b => (b.GetPosition() - TempMerges[0].GetPosition()).Length() < 1.26);
	        NEW_MISSILE.LANDINGLIST = TempLands;
                    }

                    List<IMyTerminalBlock> TempSmokes = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyUserControllableGun>(TempSmokes, b => b.CustomName.Contains(MissileTag));
	        TempSmokes.Sort((x, y) => (compareP(x, y, Key_Gyro)));
	        if(TempSmokes.Count > 0)
	        NEW_MISSILE.SMOKE = TempSmokes[0];

                    //Checks All Key Blocks Are Present
                    bool HasTurret = TempTurrets.Count > 0;
	        // a b K
	        if (gcTargetPanel != null || fcsComputer != null) HasTurret = true;

                    bool HasPower = TempPower.Count > 0;
                    bool HasMerge = TempMerges.Count > 0;
                    bool HasThruster = NEW_MISSILE.THRUSTERS.Count > 0;

                    //Echos Some Useful Diagnostics
                    Lstrundata = "Last Missile Failed To Fire\nReason:" +
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
                          NEW_MISSILE.TURRET = TempTurrets[0];
		}
                        NEW_MISSILE.POWER = TempPower[0];
                        NEW_MISSILE.MERGE = TempMerges[0];
                        MISSILES.Add(NEW_MISSILE);
                        Lstrundata = "Launched Missile:" + MISSILES.Count;
                        return true;
                    }
                }
                return false;
            }
            #endregion

	// a b K
	int compareP(IMyTerminalBlock a, IMyTerminalBlock b, IMyTerminalBlock s) {
	    return (a.GetPosition() - s.GetPosition()).Length().CompareTo((b.GetPosition() - s.GetPosition()).Length());
	}
	
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
                MISSILE ThisMissile = MISSILES[INT];
                ThisMissile.MissileMass = 0;
                ThisMissile.MissileThrust = 0;
                
                //Preps Battery For Launch
                var POWER_A = ThisMissile.POWER;
                if (ThisMissile.POWER != null && ThisMissile.POWER is IMyBatteryBlock)
                {
                    //POWER_A.ApplyAction("OnOff_On");
                    //POWER_A.SetValue("Recharge", false);
                    //POWER_A.SetValue("Discharge", true);
                    ThisMissile.MissileMass += POWER_A.Mass;
                }

                //Removes Thrusters That Are Still on the same Grid As launcher
                List<IMyThrust> TemporaryThrust = new List<IMyThrust>();
                TemporaryThrust.AddRange(ThisMissile.THRUSTERS);
                for (int i = 0; i < TemporaryThrust.Count; i++)
                {
                    var item = TemporaryThrust[i];
                    if (item.CubeGrid != ThisMissile.GYRO.CubeGrid)
                    { ThisMissile.THRUSTERS.Remove(item); continue; }
                }
                if (ThisMissile.THRUSTERS.Count == 0)
                { 
                    Lstrundata = "Missile Failed To Fire\nReason: No Detectable Thrusters On Missile Or Missile Still Attached To Launcher"; 
                    MISSILES.Remove(ThisMissile);
                    return;
                }
                
                //Retrieves Largest Thrust Direction
                Dictionary<Vector3D, double> ThrustDict = new Dictionary<Vector3D, double>();
                foreach (IMyThrust item in ThisMissile.THRUSTERS)
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
                TemporaryThrust.AddRange(ThisMissile.THRUSTERS);
                for (int i = 0; i < TemporaryThrust.Count; i++)
                {
                    var item = TemporaryThrust[i];

                    //Retrieves Thrusters Only Going In The Forward
                    if (item.WorldMatrix.Forward != ThrForward)
                    { item.ApplyAction("OnOff_On"); ThisMissile.THRUSTERS.Remove(item); continue; }

                    //Runs Std Operations
                    //item.ApplyAction("OnOff_On");
                    double ThisThrusterThrust = (item as IMyThrust).MaxThrust;
                    //(item as IMyThrust).ThrustOverride = (float)ThisThrusterThrust;
                    RdavUtils.DiagTools.Diag_Plot(Me, ThisThrusterThrust);
                    ThisMissile.MissileThrust += ThisThrusterThrust;
                    ThisMissile.MissileMass += item.Mass;
                }
                
                //Removes Any Warheads Not On The Grid
                List<IMyTerminalBlock> TemporaryWarheads = new List<IMyTerminalBlock>();
                TemporaryWarheads.AddRange(ThisMissile.WARHEADS);
                for (int i = 0; i < ThisMissile.WARHEADS.Count; i++)
                {
                    var item = TemporaryWarheads[i];

                    if (item.CubeGrid != ThisMissile.GYRO.CubeGrid)
                    { ThisMissile.WARHEADS.Remove(item); continue; }

                    ThisMissile.MissileMass += item.Mass;
                }

                //-----------------------------------------------------

                //Adds Additional Mass & Sets Accel (ovverrides If Possible)
                ThisMissile.MissileMass += ThisMissile.GYRO.Mass;
                ThisMissile.MissileMass += ThisMissile.MERGE.Mass;
                double number;
                if (double.TryParse(ThisMissile.GYRO.CustomData, out number))
                { double.TryParse(ThisMissile.GYRO.CustomData, out ThisMissile.MissileMass); }
                ThisMissile.MissileAccel = ThisMissile.MissileThrust / ThisMissile.MissileMass;

                //Sets Grid Type
                ThisMissile.IsLargeGrid = ThisMissile.GYRO.CubeGrid.GridSizeEnum == MyCubeSize.Large;
                ThisMissile.FuseDistance = ThisMissile.IsLargeGrid ? 16 : 7;

	    PlayActionList(ThisMissile.SPOTLIST, "OnOff_On");
            }
            #endregion

            //Utils
            #region RFC Function bar #RFC#
            string LeftPad = "   ";
            string Scriptname = "Rdav Missile Guidance";
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

            //Utils For Maths Functions
            #region RdavUtils #RFC#
            /*=================================================                           
            Generic Rdav Utils Class For Math & Functionality          
            --------------------------------------------     */
            static class RdavUtils
            {

                //Use For Solutions To Quadratic Equation
                public static bool Quadractic_Solv(double a, double b, double c, out double X1, out double X2)
                {
                    //Default Values
                    X1 = 0;
                    X2 = 0;

                    //Discrim Check
                    Double Discr = b * b - 4 * c;
                    if (Discr < 0)
                    { return false; }

                    //Calcs Values
                    else
                    {
                        X1 = (-b + Math.Sqrt(Discr)) / (2 * a);
                        X2 = (-b - Math.Sqrt(Discr)) / (2 * a);
                    }
                    return true;
                }

                //Handles Calculation Of Area Of Diameter
                public static double CalculateArea(double OuterDiam, double InnerDiam)
                {
                    //Handles Calculation Of Area Of Diameter
                    //=========================================
                    double PI = 3.14159;
                    double Output = ((OuterDiam * OuterDiam * PI) / 4) - ((InnerDiam * InnerDiam * PI) / 4);
                    return Output;
                }

                //Use For Magnitudes Of Vectors In Directions (0-IN.length)
                public static double Vector_Projection_Scalar(Vector3D IN, Vector3D Axis_norm)
                {
                    double OUT = 0;
                    OUT = Vector3D.Dot(IN, Axis_norm);
                    if (OUT + "" == "NaN")
                    { OUT = 0; }
                    return OUT;
                }

                //Use For Vector Components, Axis Normalized, In not (vector 0 - in.length)
                public static Vector3D Vector_Projection(Vector3D IN, Vector3D Axis_norm)
                {
                    Vector3D OUT = new Vector3D();
                    OUT = Vector3D.Dot(IN, Axis_norm) * Axis_norm;
                    if (OUT + "" == "NaN")
                    { OUT = new Vector3D(); ; }
                    return OUT;
                }

                //Use For Intersections Of A Sphere And Ray
                public static bool SphereIntersect_Solv(BoundingSphereD Sphere, Vector3D LineStart, Vector3D LineDirection, out Vector3D Point1, out Vector3D Point2)
                {
                    //starting Values
                    Point1 = new Vector3D();
                    Point2 = new Vector3D();

                    //Spherical intersection
                    Vector3D O = LineStart;
                    Vector3D D = LineDirection;
                    Double R = Sphere.Radius;
                    Vector3D C = Sphere.Center;

                    //Calculates Parameters
                    Double b = 2 * (Vector3D.Dot(O - C, D));
                    Double c = Vector3D.Dot((O - C), (O - C)) - R * R;

                    //Calculates Values
                    Double t1, t2;
                    if (!Quadractic_Solv(1, b, c, out t1, out t2))
                    { return false; } //does not intersect
                    else
                    {
                        Point1 = LineStart + LineDirection * t1;
                        Point2 = LineStart + LineDirection * t2;
                        return true;
                    }
                }

                //Basic Gets Predicted Position Of Enemy (Derived From Keen Code)
                public static Vector3D GetPredictedTargetPosition2(IMyTerminalBlock shooter, Vector3 ShipVel, MyDetectedEntityInfo target, float shotSpeed)
                {
                    Vector3D predictedPosition = target.Position;
                    Vector3D dirToTarget = Vector3D.Normalize(predictedPosition - shooter.GetPosition());

                    //Run Setup Calculations
                    Vector3 targetVelocity = target.Velocity;
                    targetVelocity -= ShipVel;
                    Vector3 targetVelOrth = Vector3.Dot(targetVelocity, dirToTarget) * dirToTarget;
                    Vector3 targetVelTang = targetVelocity - targetVelOrth;
                    Vector3 shotVelTang = targetVelTang;
                    float shotVelSpeed = shotVelTang.Length();

                    if (shotVelSpeed > shotSpeed)
                    {
                        // Shot is too slow 
                        return Vector3.Normalize(target.Velocity) * shotSpeed;
                    }
                    else
                    {
                        // Run Calculations
                        float shotSpeedOrth = (float)Math.Sqrt(shotSpeed * shotSpeed - shotVelSpeed * shotVelSpeed);
                        Vector3 shotVelOrth = dirToTarget * shotSpeedOrth;
                        float timeDiff = shotVelOrth.Length() - targetVelOrth.Length();
                        var timeToCollision = timeDiff != 0 ? ((shooter.GetPosition() - target.Position).Length()) / timeDiff : 0;
                        Vector3 shotVel = shotVelOrth + shotVelTang;
                        predictedPosition = timeToCollision > 0.01f ? shooter.GetPosition() + (Vector3D)shotVel * timeToCollision : predictedPosition;
                        return predictedPosition;
                    }
                }

                //Generic Diagnostics Tools 
                public static class DiagTools
                {
                    //Used For Customdata Plotting
                    public static void Diag_Plot(IMyTerminalBlock Block, object Data1)
                    {
                        Block.CustomData = Block.CustomData + Data1 + "\n";
                    }

                    //Used For Fast Finding/Dynamically Renaming A Block Based On Type
                    public static void Renam_Block_Typ(IMyGridTerminalSystem GTS, string RenameTo)
                    {
                        List<IMyTerminalBlock> TempCollection = new List<IMyTerminalBlock>();
                        GTS.GetBlocksOfType<IMyRadioAntenna>(TempCollection);
                        if (TempCollection.Count < 1)
                        { return; }
                        TempCollection[0].CustomName = RenameTo;
                    }

                    //Used For Fast Finding/Dynamically Renaming A Block Based On CustomData
                    public static void Renam_Block_Cust(IMyGridTerminalSystem GTS, string customnam, string RenameTo)
                    {
                        List<IMyTerminalBlock> TempCollection = new List<IMyTerminalBlock>();
                        GTS.GetBlocksOfType<IMyTerminalBlock>(TempCollection, a => a.CustomData == customnam);
                        if (TempCollection.Count < 1)
                        { return; }
                        else
                        { TempCollection[0].CustomName = RenameTo; }
                    }

                }
            }

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

            //Quick GotHere Diagnostics
            void GotHere(int number)
            {
                Echo("GotHere " + number);
            }

            #endregion

            //Use For Turning 
            #region GyroTurnMis #RFC#
            /*=======================================================================================                             
            Function: GyroTurn6                    
            ---------------------------------------                            
            function will: A Variant of PD damped gyroturn used for missiles
            //----------==--------=------------=-----------=---------------=------------=-----=-----*/
            void GyroTurn6(Vector3D TARGETVECTOR, double GAIN, double DAMPINGGAIN,IMyTerminalBlock REF, IMyGyro GYRO, double YawPrev, double PitchPrev, out double NewPitch, out double NewYaw, ref MISSILE missile, bool isClear)
            {
                //Pre Setting Factors
                NewYaw = 0;
                NewPitch = 0;

                //Retrieving Forwards And Up
                Vector3D ShipUp = REF.WorldMatrix.Up;
                Vector3D ShipForward = REF.WorldMatrix.Backward; //Backward for thrusters

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

                //Applies Some PID Damping
                ShipForwardAzimuth = ShipForwardAzimuth + DAMPINGGAIN * ((ShipForwardAzimuth - YawPrev) / Global_Timestep);
                ShipForwardElevation = ShipForwardElevation + DAMPINGGAIN * ((ShipForwardElevation - PitchPrev) / Global_Timestep);

                //Does Some Rotations To Provide For any Gyro-Orientation
                var REF_Matrix = MatrixD.CreateWorld(REF.GetPosition(), (Vector3)ShipForward, (Vector3)ShipUp).GetOrientation();
                var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, 0)), REF_Matrix); //Converts To World
                var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(GYRO.WorldMatrix.GetOrientation()));  //Converts To Gyro Local
		

                //Logic Checks for NaN's
                if (double.IsNaN(TRANS_VECT.X) || double.IsNaN(TRANS_VECT.Y) || double.IsNaN(TRANS_VECT.Z))
                { return; }

                //Applies To Scenario
                GYRO.Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X) * GAIN , -30, 30);
		//GYRO.Pitch = (float)MathHelper.Clamp( missile.pidE.Filter(-TRANS_VECT.X, 2) , -30, 30);

		if (missile.IS_SIDE)
                GYRO.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y)) * GAIN, -30, 30);
		else	
                GYRO.Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z)) * GAIN , -30, 30);
		//GYRO.Roll = (float)MathHelper.Clamp( missile.pidA.Filter(-TRANS_VECT.Z, 2) , -1000, 1000);

	    // a b K
	    // assume the gyro is in front of the missile, use Yaw to make the missile fit gravity
	    var ng = RC.GetNaturalGravity();
	    if (ng.Length() > 0.01 && isClear) {
	       MatrixD gyroMat = GYRO.WorldMatrix;
	       double diff;
	       if (missile.IS_SIDE) {
	       diff = diffGravity(gyroMat.Left, ng, gyroMat.Forward);
	       GYRO.Roll = (float) missile.pidR.Filter(diff,2);
	       } else {
	       diff = diffGravity(gyroMat.Left, ng, gyroMat.Up);
	       GYRO.Yaw =(float) missile.pidR.Filter(-diff,2);
	       }
	    } 
                GYRO.GyroOverride = true;
            }
            #endregion

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

if (Math.Abs(diff) > 0.0001f) {
return diff;
}else {
return 0;
}

}

// a b K
void PlayActionList(List<IMyTerminalBlock> blocks, String action) {
    if(blocks == null) return;
	for(int i = 0; i < blocks.Count; i ++)
	{
		var a = blocks[i].GetActionWithName(action);
		if (a!=null) a.Apply(blocks[i]);
	}
}
void PlayAction(IMyTerminalBlock b, String action) {
    if(b == null) return;

		var a = b.GetActionWithName(action);
		if (a!=null) a.Apply(b);
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

// (sqrt (+ 0.25 1 1 ))
// (sqrt 0.5)

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

void autoFireProcess() {
if (!autoFireMissile) return;
if (timestamp < lastAutoFire + AUTO_FIRE_INTERVAL) return;
lastAutoFire = timestamp;
if (MISSILES.Count > AUTO_FIRE_MAX) return;
// check target from fcs only
Vector3D targetPosition=Vector3D.Zero, targetVelocity=Vector3D.Zero;
checkFcsTarget(out targetPosition, out targetVelocity);
if(targetPosition == Vector3D.Zero) return;
// fire
                if (LaunchStateMachine == null)
                {
                    LaunchStateMachine = MissileLaunchHandler().GetEnumerator();
                }
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

// (/ (sqrt 5) 2)

// (sqrt (+ (* 1.5 1.5) (* 0.5 0.5)))
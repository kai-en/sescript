static int rgms_no = 0;
// static float missileGravityRate = 5F;
double GUILD_RATE = 0.3;
double ATAN_BASE = 0.5;
double APID_P = 10;
double APID_D = 1.0;

//double MISSILE_MASS = 3398.8;
double MISSILE_MASS = 5276.4;
//double MISSILE_MASS = 1661.4;
//double MISSILE_MASS = 1407.4;
double LaunchDist = 150;
int dropTime = 0;
	    
string debugInfo = "";

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
            string MissileTag = "#A#"+rgms_no;

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
	    public List<IMyTerminalBlock> SPOTLIST;

                //Permanent Missile Details
                public double MissileAccel = 10;
                public double MissileMass = 0;
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
	
            IMySoundBlock Alarm;
            IMyShipController RC;
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
                GridTerminalSystem.GetBlocksOfType<IMyShipController>(TempCollection3);
                if (TempCollection3.Count > 0)
                {RC = TempCollection3[0] as IMyShipController;}

	    // a b K
                List<IMyTerminalBlock> TempCollection4 = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TempCollection4, a => a.CustomName.Equals(gcTargetPanelName) );
	    if (TempCollection4.Count > 0)
                {gcTargetPanel = TempCollection4[0] as IMyTextPanel;}

            }
            #endregion

            //Main Method
            #region Main Method
            void Main(string argument)
            {

                //General Layout Diagnostics
                OP_BAR();
                QuickEcho(MISSILES.Count, "Active (Fired) Missiles:");
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
		&& gcTargetPanel == null
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
	       }
	    }
	    else if (argument != "Fire" && argument != "")
                {
                    Lstrundata = "Unknown/Incorrect launch argument,\ncheck spelling & caps,\nto launch argument should be just: Fire\n ";
                }

                //Runs Guidance Block (foreach missile)
                //---------------------------------------
                for (int i = 0; i < MISSILES.Count; i++)
                {
                    var ThisMissile = MISSILES[i];

                    //Runs Standard System Guidance
                    if (ThisMissile.IS_CLEAR == true)
                    { STD_GUIDANCE(ThisMissile); }

                    //Fires Straight (NO OVERRIDES)
                    else if (ThisMissile.IS_CLEAR == false)
                    {
                        if ((ThisMissile.GYRO.GetPosition() - Me.GetPosition()).Length() > ThisShipSize)
                        { ThisMissile.IS_CLEAR = true; }
                    }

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
            }


            #endregion

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
                yield return true;
                yield return true;
                yield return true;
                yield return true;
                yield return true; //Safety Tick

	    if(Me.CubeGrid.GridSizeEnum == MyCubeSize.Large) dropTime = 0;
	    for (int i = 0; i < dropTime; i++) {
	    	yield return true;
	    }

                //Launches Missile & Gathers Next Scanner
                PREP_FOR_LAUNCH(MISSILES.Count - 1);

            }
            #endregion

            //Standard Guidance Routine
            #region RdavNav Missile Guidance #RFC#
            /*=================================================                           
             RdavNav             
             ---------------------------------------     */
            void STD_GUIDANCE(MISSILE This_Missile)
            {

                //Targeting Module
                //-----------------------------------------------

                //Retrieves Target Position
                var This_Missile_Director = This_Missile.TURRET as IMyLargeTurretBase;
                var ENEMY_POS = new Vector3D();

                //Logical Determination Of Enemy Position
	    // a b K
	    bool targetPanelHasTarget = false;
	    var panelInfo = gcTargetPanel.GetPublicTitle();
	    // [T:-3400.63210157891:-14844.4586962264:-13932.3639607262:0.002283879:0.002356248:0.001983097
	    var tokens = panelInfo.Split(':');
	    Vector3D targetPanelPosition = Vector3D.Zero;
	    Vector3D targetPanelVelocity = Vector3D.Zero;
	    if (panelInfo.StartsWith("[T:") && tokens.Length >= 7) {
	       targetPanelPosition = new Vector3D(Convert.ToDouble(tokens[1]),Convert.ToDouble(tokens[2]),Convert.ToDouble(tokens[3]));
	       targetPanelVelocity = new Vector3D(Convert.ToDouble(tokens[4]),Convert.ToDouble(tokens[5]),Convert.ToDouble(tokens[6]));
	       targetPanelHasTarget = true;
	    }

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

	    var thrusterLowerLimit = 0.4f;
	    //float thrusterLowerLimit = 0.9F;
	    if ((TargetPosition - MissilePosition).Length() < 1000) thrusterLowerLimit = 0.4F; // a b k 
                ThrustPower = MathHelper.Clamp(ThrustPower, thrusterLowerLimit, 1); //for improved thrust performance on the get-go
                ThrustPower = 1;
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
		

if(true) {
// 新算法
// 4 推力 / 质量 = 可以提供的加速度的长度 sdl

double sdl = This_Missile.THRUSTERS[0].MaxEffectiveThrust * This_Missile.THRUSTERS.Count / MISSILE_MASS;

// 1 求不需要的速度
debugInfo = "TR: " + targetRange.Length() + "\n";
Vector3D tarN = Vector3D.Normalize(targetRange);
Vector3D rv = Vector3D.Reject(targetV, tarN);
debugInfo += "RV: " + rv.Length() + "\n";
//Vector3D ra = Vector3D.Reject(TargetAcc, tarN);

// 2 换算不需要的加速度 平行制导率
Vector3D rvN = Vector3D.Normalize(rv);
double newLen = Math.Atan2(rv.Length(), ATAN_BASE);
Vector3D newRv = rvN * newLen;
Vector3D rdo = newRv * GUILD_RATE * 60
//+ ra * 0.5
;

// 1.1 比例导引法 PN
double PN_RATE = 3000;
Vector3D losD = (LOS_New - LOS_Old) * 60
//+ ra * 0.5
;
double losDl = losD.Length();
Vector3D sideN = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(MissileVelocity)));
Vector3D graN = Vector3D.Normalize(RC.GetNaturalGravity());
double rdol_pn = Math.Atan2(losDl, ATAN_BASE) * PN_RATE;
//if (rdol_pn > sdl * 0.5) rdol_pn = sdl * 0.5;
Vector3D rdo_pn = sideN * rdol_pn;


// 3 加上抵抗重力所需的加速度 = 需要抵消的加速度 rd
Vector3D rd = rdo - RC.GetNaturalGravity();
double rdl = rd.Length();
Vector3D rd_pn = rdo_pn - RC.GetNaturalGravity();
double rdl_pn = rd_pn.Length();


// 5 剩余加速度长度
// 5.1 rd相对于LOS 需要的侧向加速度
Vector3D rd2 = Vector3D.Reject(rd, tarN);
double rd2l = rd2.Length();
if (sdl < rd2l) sdl = rd2l;
// 5.2 剩余加速度长度 
double pdl = Math.Sqrt(sdl*sdl - rd2l * rd2l);

Vector3D rd2_pn = Vector3D.Reject(rd_pn, tarN);
double rd2l_pn = rd2_pn.Length();
if (sdl < rd2l_pn) rd2l_pn = sdl;
double pdl_pn = Math.Sqrt(sdl*sdl - rd2l_pn * rd2l_pn);

// 6 剩余加速度方向  = los
//Vector3D pdN = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(rd)));
//if (pdN.Length() == 0) pdN = LOS_New;
Vector3D pdN = LOS_New;

//Vector3D pdN_pn = Vector3D.Normalize(Vector3D.Reject(LOS_New, Vector3D.Normalize(rd_pn)));
Vector3D pdN_pn = LOS_New;

// 7 剩余加速度
Vector3D pd = pdN * pdl;
Vector3D pd_pn = pdN_pn * pdl_pn;

// 8 总加速度
Vector3D sd = rd2 + pd;
Vector3D sd_pn = rd2_pn + pd_pn;

// 9 总加速度方向
Vector3D nam = Vector3D.Normalize(sd);
//if(targetRange.Length()>500)
//nam = Vector3D.Normalize(sd_pn);

if (targetRange.Length() < This_Missile.nearest)
This_Missile.nearest = targetRange.Length();
double pn_test = (Vector3D.Normalize(MissileVelocity) - Vector3D.Normalize(lastVelocity)).Length() / ((LOS_New - LOS_Old).Length()*60);

am = nam;

var missileLookAt = MatrixD.CreateLookAt(new Vector3D(), This_Missile.GYRO.WorldMatrix.Up, This_Missile.GYRO.WorldMatrix.Backward);
var amToMe = Vector3D.TransformNormal(am, missileLookAt);
var rr = Vector3D.Normalize(Vector3D.Reject(This_Missile.GYRO.WorldMatrix.Up, Vector3D.Normalize(RC.GetNaturalGravity())));
var rangle = 1 - Vector3D.Dot(rr, tarN);
debugInfo += "RA: " + rangle + "\n";

debugInfo += "amToMe: " + displayVector3D(amToMe) + "\n";
debugInfo += This_Missile.nearest + "\n";


// CODING
}

                double Yaw; double Pitch;
                GyroTurn6(am, APID_P, APID_D, This_Missile.THRUSTERS[0], This_Missile.GYRO as IMyGyro, This_Missile.PREV_Yaw, This_Missile.PREV_Pitch, out Pitch, out Yaw, ref This_Missile);

                //Updates For Next Tick Round
                This_Missile.TARGET_PREV_POS = TargetPosition;
                This_Missile.MIS_PREV_POS = MissilePosition;
                This_Missile.PREV_Yaw = Yaw;
                This_Missile.PREV_Pitch = Pitch;

                //Detonates warheads in close proximity
                if ((TargetPosition - MissilePosition).LengthSquared() < 20 * 20 && This_Missile.WARHEADS.Count > 0) //Arms
                { foreach (var item in This_Missile.WARHEADS) { (item as IMyWarhead).IsArmed = true; } }
	    bool targetNeer = (TargetPosition - MissilePosition).LengthSquared() < This_Missile.FuseDistance * This_Missile.FuseDistance;
	    bool targetGetFar = (TargetPosition - MissilePosition).LengthSquared() > (TargetPositionPrev - MissilePositionPrev).LengthSquared() && (TargetPosition - MissilePosition).LengthSquared() < 4*4 ;
                if ((targetGetFar)&& This_Missile.WARHEADS.Count > 0) //A mighty earth shattering kaboom
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
                GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(MERGES, b => b.CustomName.Contains(MissileTag));
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
	    return Vector3D.TransformNormal(g.GetPosition()-RC.GetPosition(), tranmt).Z;
	    }).ToList();
                foreach (var Key_Gyro in GYROS)
                {
                    MISSILE NEW_MISSILE = new MISSILE();
                    NEW_MISSILE.GYRO = Key_Gyro;

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
                    NEW_MISSILE.THRUSTERS = THRUSTERS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);

                    //Sorts And Selects Warheads
                    NEW_MISSILE.WARHEADS = WARHEADS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);

                    // a b K
                    List<IMyThrust> TempThrusters = THRUSTERS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
                    //TempThrusters.Sort((x, y) => (compareXY(x, y, Key_Gyro)));
	        //NEW_MISSILE.THRUSTERS = new List<IMyThrust>(){TempThrusters[0]};
	        NEW_MISSILE.THRUSTERS = TempThrusters.Where(t => filterClose(t, Key_Gyro)).ToList();
	        // if(TempThrusters[0].Position.X != Key_Gyro.Position.X || TempThrusters[0].Position.Y != Key_Gyro.Position.Y) {
		// Lstrundata = "mismatch thruster " + TempThrusters[0].Position.X + " " + Key_Gyro.Position.X +" " +  TempThrusters[0].Position.Y + " " +  Key_Gyro.Position.Y;
		// return false;
	        // }

                    List<IMyTerminalBlock> TempWarheads = WARHEADS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
	        //TempWarheads.Sort((x, y) => (compareXY(x, y, Key_Gyro)));
	        //NEW_MISSILE.WARHEADS = new List<IMyTerminalBlock>(){TempWarheads[0]};
	        NEW_MISSILE.WARHEADS = TempWarheads.Where(t => filterClose(t, Key_Gyro)).ToList();

                    List<IMyTerminalBlock> TempSpots = SPOTS.FindAll(b => (b.GetPosition() - GyroPos).LengthSquared() < Distance * Distance);
	        NEW_MISSILE.SPOTLIST = TempSpots.Where(t => filterClose(t, Key_Gyro)).ToList();

                    //Checks All Key Blocks Are Present
                    bool HasTurret = TempTurrets.Count > 0;
	        // a b K
	        if (gcTargetPanel != null) HasTurret = true;

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
                    POWER_A.ApplyAction("OnOff_On");
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
                    item.ApplyAction("OnOff_On");
                    double ThisThrusterThrust = (item as IMyThrust).MaxThrust;
                    (item as IMyThrust).ThrustOverride = (float)ThisThrusterThrust;
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
            void GyroTurn6(Vector3D TARGETVECTOR, double GAIN, double DAMPINGGAIN,IMyTerminalBlock REF, IMyGyro GYRO, double YawPrev, double PitchPrev, out double NewPitch, out double NewYaw, ref MISSILE missile)
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
                //GYRO.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y)) * GAIN, -30, 30);
                GYRO.Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z)) * GAIN , -30, 30);
		//GYRO.Roll = (float)MathHelper.Clamp( missile.pidA.Filter(-TRANS_VECT.Z, 2) , -1000, 1000);

		// CODING
	    // a b K
	    // assume the gyro is in front of the missile, use Yaw to make the missile fit gravity
	    var ng = RC.GetNaturalGravity();
	    if (ng.Length() > 0.01) {
	       MatrixD gyroMat = GYRO.WorldMatrix;
	       var diff = diffGravity(gyroMat.Left, ng, gyroMat.Up);
	       GYRO.Yaw =(float) missile.pidR.Filter(-diff,2);
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

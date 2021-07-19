using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Sandbox.Game.AI.Pathfinding.Obsolete;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.Utils;
using VRageMath;


// Code is based on Gauge's Balanced Deformation code, but heavily modified for more control. 
namespace MikeDude.ArmorBalance
{
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class ArmorBalance : MySessionComponentBase
    {
        public const float lightArmorLargeDamageMod = 1f; //1.0 Vanilla
        public const float lightArmorLargeDeformationMod = 0.5f; //varies for every block
        public const float lightArmorSmallDamageMod = 1f; //1.0 Vanilla
        public const float lightArmorSmallDeformationMod = 0.5f; //varies for every block

        public const float heavyArmorLargeDamageMod = 1f; //0.5 Vanilla ONLY for full cube, 1.0 all else
        public const float heavyArmorLargeDeformationMod = 0.3f; //varies for every block
        public const float heavyArmorSmallDamageMod = 1f; //0.5 Vanilla ONLY for full cube, 1.0 all else
        public const float heavyArmorSmallDeformationMod = 0.3f; //varies for every block

        public const float turretLargeDamageMod = 1f; //1.0 Vanilla
        public const float weaponLargeDamageMod = 1f; //1.0 Vanilla
        public const float turretSmallDamageMod = 1f; //1.0 Vanilla
        public const float weaponSmallDamageMod = 1f; //1.0 Vanilla

        public const float powerProducerPCUMult = 3f; //1f will make a large reactor 300 PCU for reference
        public const float refineryPCUMult = 300f;
        public const float assemblerPCUMult = 1200f;
        public const float gasGeneratorPCUMult = 1.6f;
		public const float jumpDrivePCUMult = 0.00075f;
		public const float cargoPCUMult = 1.1f;
		public const float gasTankPCUMult = 0.00003f;
		public const float thrusterPCUMult = 0.00001f;
		public const float suspensionPCUMult = 0.0005f;
		public const float toolPCUMult = 250f;

        public const int gravityGeneratorPCU = 60;
        public const int virtualMassPCU = 40;
        public const int upgradeModulePCU = 100;
		public const int conveyorPCU = 1;
		public const int lightArmorPCU = 1;
		public const int heavyArmorPCU = 1;
		public const int blastDoorPCU = 1;
		public const int gyroPCU = 1;
		public const int projectorPCU = 500;
		
		
        private bool isInit = false;

        private void DoWork()
        {

			//Tuples not working in SE C# v6
			/*var PCUModTable = new (string Type, int PCU)[]
            {
                ("MyObjectBuilder_LargeGatlingTurret/null", 200),
                ("MyObjectBuilder_LargeGatlingTurret/SmallGatlingTurret", 200),
                ("MyObjectBuilder_SmallGatlingGun/null", 200),
                ("MyObjectBuilder_LargeTurretBaseDefinition/MXA_CoilgunL", 200),
                ("MyObjectBuilder_LargeTurretBaseDefinition/OKI122mmVT", 200),
                ("MyObjectBuilder_WeaponBlockDefinition/OKI230mmBCfixed", 200),
                ("MyObjectBuilder_WeaponBlockDefinition/OKI122mmSGfixed", 200),
                ("MyObjectBuilder_LargeMissileTurret/null", 250),
                    // ... new lines here
            };

            var PCUModLookupByType = new Dictionary<MyDefinitionId, int>();
            foreach (var entry in PCUModTable)
            {
                MyDefinitionId typeId;
                if ( MyDefinitionId.TryParse(entry.Type, out typeId) )
                {
                    PCUModLookupByType.Add(typeId, entry.PCU);
                }
                else
                {
                    throw new Exception("Bad Entry in PCU Mod Table. Entry=" + entry.Type);
                }
            }*/



            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {


                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;
				/*int PCUMod = 0;
				if (blockDef != null && PCUModLookupByType.TryGetValue( def.Id, out PCUMod) )
				{
					blockDef.PCU = PCUMod;
				}*/

				MyLargeTurretBaseDefinition turretDef = def as MyLargeTurretBaseDefinition;
				MyWeaponBlockDefinition weaponDef = def as MyWeaponBlockDefinition;

				MyPowerProducerDefinition powerProducerDef = def as MyPowerProducerDefinition;
				MyGravityGeneratorDefinition gravityDef = def as MyGravityGeneratorDefinition;
				MyGravityGeneratorSphereDefinition gravitySphereDef = def as MyGravityGeneratorSphereDefinition;
				MyVirtualMassDefinition virtualMassDef = def as MyVirtualMassDefinition;
				MySpaceBallDefinition spaceBallDef = def as MySpaceBallDefinition;
				MyRefineryDefinition refineryDef = def as MyRefineryDefinition;
				MyAssemblerDefinition assemblerDef = def as MyAssemblerDefinition;
				MyUpgradeModuleDefinition moduleDef = def as MyUpgradeModuleDefinition;
				MySurvivalKitDefinition survivalKitDef = def as MySurvivalKitDefinition;
				MyOxygenGeneratorDefinition gasGeneratorDef = def as MyOxygenGeneratorDefinition;
				MyJumpDriveDefinition jumpDriveDef = def as MyJumpDriveDefinition;//jumpDrive
				MyCargoContainerDefinition cargoDef = def as MyCargoContainerDefinition;//cargoContainer
				MyGasTankDefinition gasTankDef = def as MyGasTankDefinition;//h2Tank, it SHOULD include O2 tanks too
				MyThrustDefinition thrustDef = def as MyThrustDefinition;//thruster
				MyGyroDefinition gyroDef = def as MyGyroDefinition;
				MyConveyorSorterDefinition sorterDef = def as MyConveyorSorterDefinition;
				MyProjectorDefinition projectorDef = def as MyProjectorDefinition;
				MyMotorSuspensionDefinition suspensionDef = def as MyMotorSuspensionDefinition;
				MyShipToolDefinition toolDef = def as MyShipToolDefinition;

				//everything else


                if (blockDef != null && turretDef == null && weaponDef == null && sorterDef == null)
                {
					blockDef.PCU = (int) 1;	
				}					

				//light armor
                if (blockDef != null && (blockDef.EdgeType == "Light" && (blockDef.BlockTopology != MyBlockTopology.TriangleMesh)))
                {
                    if (blockDef.CubeSize == MyCubeSize.Large)
                    {
                        blockDef.GeneralDamageMultiplier = lightArmorLargeDamageMod;
                        blockDef.DeformationRatio = lightArmorLargeDeformationMod;
                    }

                    if (blockDef.CubeSize == MyCubeSize.Small)
                    {
                        blockDef.GeneralDamageMultiplier = lightArmorSmallDamageMod;
                        blockDef.DeformationRatio = lightArmorSmallDeformationMod;
                    }
					blockDef.PCU = lightArmorPCU;
                }
				//heavy armor
                if (blockDef != null && (blockDef.EdgeType == "Heavy" && (blockDef.BlockTopology != MyBlockTopology.TriangleMesh)))
                {
                    if (blockDef.CubeSize == MyCubeSize.Large)
                    {
                        blockDef.GeneralDamageMultiplier = heavyArmorLargeDamageMod;
                        blockDef.DeformationRatio = heavyArmorLargeDeformationMod;
                    }

                    if (blockDef.CubeSize == MyCubeSize.Small)
                    {
                        blockDef.GeneralDamageMultiplier = heavyArmorSmallDamageMod;
                        blockDef.DeformationRatio = heavyArmorSmallDeformationMod;
                    }
					blockDef.PCU = blastDoorPCU;
                }
				//blast doors
                if (blockDef != null && (blockDef.BlockPairName.Contains("Blast"))) 
				{
					blockDef.PCU = blastDoorPCU;
                }
				//Conveyors
                if (blockDef != null && (blockDef.BlockPairName.Contains("Conveyor"))) 
				{
					blockDef.PCU = conveyorPCU;
                }
				//Power production blocks
                if (powerProducerDef != null)
                {
					float newPCU = powerProducerDef.MaxPowerOutput * powerProducerPCUMult;
					powerProducerDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//gravity generators
                if (gravityDef != null)
                {
					gravityDef.PCU = gravityGeneratorPCU;				
                }
                if (gravitySphereDef != null)
                {
					gravitySphereDef.PCU = gravityGeneratorPCU;				
                }
				//mass blocks
                if (virtualMassDef != null)
                {					
                    if (virtualMassDef.CubeSize == MyCubeSize.Large)
                    {
                        virtualMassDef.PCU = gravityGeneratorPCU;
                    }

                    if (virtualMassDef.CubeSize == MyCubeSize.Small)
                    {
                        virtualMassDef.PCU = (int) gravityGeneratorPCU/5;
                    }
                }
				//space balls (mass blocks)
                if (spaceBallDef != null)
                {
					
                    if (spaceBallDef.CubeSize == MyCubeSize.Large)
                    {
                        spaceBallDef.PCU = gravityGeneratorPCU/2;
                    }

                    if (spaceBallDef.CubeSize == MyCubeSize.Small)
                    {
                        spaceBallDef.PCU = (int) gravityGeneratorPCU/10;
                    }
                }
				//refineries
                if (refineryDef != null)
                {
					float newPCU = refineryDef.RefineSpeed * refineryDef.MaterialEfficiency * refineryPCUMult;
					refineryDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//survival kits (assemblers)
                if (survivalKitDef != null)
                {
					float newPCU = survivalKitDef.AssemblySpeed * assemblerPCUMult;
					survivalKitDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//assemblers
                if (assemblerDef != null)
                {
					float newPCU = assemblerDef.AssemblySpeed * assemblerPCUMult;
					assemblerDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//production upgrade modules
                if (moduleDef != null)
                {
					moduleDef.PCU = upgradeModulePCU;				
                }
				//H2/O2 generators
                if (gasGeneratorDef != null)
                {
					float newPCU = gasGeneratorDef.IceConsumptionPerSecond * gasGeneratorPCUMult;
					gasGeneratorDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//Gas tanks
                if (gasTankDef != null)
                {
					float newPCU = gasTankDef.Capacity * gasTankPCUMult;
					gasTankDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//Cargo containers
                if (cargoDef != null)
                {
					float cargoEfficiencyMult = 1f;
					
					if (cargoDef.Id.SubtypeName.Contains("LargeContainer"))
					{
						cargoEfficiencyMult = 0.8f;
					}
					if (cargoDef.Id.SubtypeName.Contains("Medium"))
					{
						cargoEfficiencyMult = 0.9f;
					}
					if (cargoDef.Id.SubtypeName.Contains("SmallContainer"))
					{
						cargoEfficiencyMult = 1f;
					}

					float newPCU = cargoDef.InventorySize.Volume * cargoPCUMult * cargoEfficiencyMult;
					cargoDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//Thrusters
                if (thrustDef != null)
                {
					float newPCU = thrustDef.ForceMagnitude * thrusterPCUMult;
					thrustDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//Suspension
                if (suspensionDef != null)
                {
					float newPCU = suspensionDef.PropulsionForce * suspensionPCUMult;
					suspensionDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }
				//JumpDrives
                if (jumpDriveDef != null)
                {
					float newPCU = (float) jumpDriveDef.MaxJumpDistance * jumpDrivePCUMult;
					jumpDriveDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);	
				}					
				//Gyros
                if (gyroDef != null)
                {
					gyroDef.PCU = (int) gyroPCU;	
				}					
				//Projectors
                if (projectorDef != null)
                {
					projectorDef.PCU = (int) projectorPCU;	
				}					
				//Tools
                if (toolDef != null)
                {
					float newPCU = toolDef.SensorRadius * toolPCUMult;
					toolDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }

                /*if (turretDef != null)
                {
                    if (turretDef.CubeSize == MyCubeSize.Large)
                    {
                        turretDef.GeneralDamageMultiplier = turretLargeDamageMod;
                    }

                    if (turretDef.CubeSize == MyCubeSize.Small)
                    {
                        turretDef.GeneralDamageMultiplier = turretSmallDamageMod;
                    }
                }
                if (weaponDef != null)
                {
                    if (weaponDef.CubeSize == MyCubeSize.Large)
                    {
                        weaponDef.GeneralDamageMultiplier = weaponLargeDamageMod;
                    }

                    if (weaponDef.CubeSize == MyCubeSize.Small)
                    {
                        weaponDef.GeneralDamageMultiplier = weaponSmallDamageMod;
                    }
                }*/
            }
        }
        
        public override bool UpdatedBeforeInit()
        {
            DoWork();
            return true;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            DoWork();
        }

        public override void UpdateBeforeSimulation()
        {
            if (!isInit && MyAPIGateway.Session == null)
            {
                DoWork();
                isInit = true;
            }
        }
    }
}

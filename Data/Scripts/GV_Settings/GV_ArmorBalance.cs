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

        public const float powerProducerPCUMult = 30f; //1f will make a large reactor 300 PCU for reference
        public const int gravityGeneratorPCU = 400;
        public const int virtualMassPCU = 400;
        public const float refineryPCUMult = 800f;
        public const float assemblerPCUMult = 3000f;
        public const int upgradeModulePCU = 400;
        public const float gasGeneratorPCUMult = 16f;
		
        private bool isInit = false;

        private void DoWork()
        {
            foreach (MyDefinitionBase def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyCubeBlockDefinition blockDef = def as MyCubeBlockDefinition;
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
				//MyLargeTurretBaseDefinition turretDef = def as MyLargeTurretBaseDefinition;
				//MyWeaponBlockDefinition weaponDef = def as MyWeaponBlockDefinition;

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
                }

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
                }
				
                if (powerProducerDef != null)
                {
					float newPCU = powerProducerDef.MaxPowerOutput * powerProducerPCUMult;
					powerProducerDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }

                if (gravityDef != null)
                {
					gravityDef.PCU = gravityGeneratorPCU;				
                }
                if (gravitySphereDef != null)
                {
					gravitySphereDef.PCU = gravityGeneratorPCU;				
                }
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

                if (refineryDef != null)
                {
					float newPCU = refineryDef.RefineSpeed * refineryDef.MaterialEfficiency * refineryPCUMult;
					refineryDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }

                if (survivalKitDef != null)
                {
					float newPCU = survivalKitDef.AssemblySpeed * assemblerPCUMult;
					survivalKitDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }

                if (assemblerDef != null)
                {
					float newPCU = assemblerDef.AssemblySpeed * assemblerPCUMult;
					assemblerDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
                }

                if (moduleDef != null)
                {
					moduleDef.PCU = upgradeModulePCU;				
                }
				
                if (gasGeneratorDef != null)
                {
					float newPCU = gasGeneratorDef.IceConsumptionPerSecond * gasGeneratorPCUMult;
					gasGeneratorDef.PCU = (int) MathHelper.Clamp(newPCU,1f,1000000f);			
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

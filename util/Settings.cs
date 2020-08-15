using System.Collections.Generic;
using SharedModConfig;

namespace CustomGameStats
{
    public class Settings
    {
        //mod naming
        public static string modName = "CustomGameStats";
        public static string playerStatsTitle = " - Player Stats";
        public static string aiStatsTitle = " - AI Stats";

        //rules names
        public static string toggleSwitch = "ToggleSwitch";
        public static string gameBehaviour = "GameBehaviour";
        public static string strictMinimum = "StrictMinimum";

        //rules section
        public static string toggleSection = "Toggle Custom Stats";
        public static string behaviourSection = "Enforce Game Behaviour";
        public static string strictSection = "Enforce Strict Minimums";

        //descriptors
        public static string multDesc = "Is a percent modifier?";
        public static string modDesc = "Modifier value: ";
        public static string toggleDesc = "Enable/disable";
        public static string behaviourDesc = "Prevents unexpected behaviour to occur. Use with caution!";
        public static string strictDesc = "Prevents values from being reduced below zero. Do not touch unless you know what you are doing! (Ignored if Enforce Game Behaviour enabled)";

        //stat stacks
        public static string playerStats = "PlayerStatStack";
        public static string aiStats = "AiStatStack";

        //stat minimums
        public static float minimum = 0f;
        public static float minimumMod = 0.01f;

        //stat names
        public static string modMult = "Mult";
        public static string foodDepleteMod = "FoodDepleteRate";
        public static string drinkDepleteMod = "DrinkDepleteRate";
        public static string sleepDepleteMod = "SleepDepleteRate";
        public static string detectabilityMod = "Detectability";
        public static string visualDetectabilityMod = "VisualDetectability";
        public static string pouchCapacityMod = "PouchCapacity";
        public static string foodEffectEfficiencyMod = "FoodEffectEfficiency";
        public static string buyMod = "BuyModifier";
        public static string sellMod = "SellModifier";
        public static string healthMod = "MaxHealth";
        public static string healthRegenMod = "HealthRegen";
        public static string healthBurnMod = "HealthBurn";
        public static string staminaMod = "MaxStamina";
        public static string staminaRegenMod = "StaminaRegen";
        public static string staminaUseMod = "StaminaUse";
        public static string staminaCostReducMod = "StaminaCostReduction";
        public static string staminaBurnMod = "StaminaBurn";
        public static string manaMod = "MaxMana";
        public static string manaRegenMod = "ManaRegen";
        public static string manaUseMod = "ManaUse";
        public static string manaBurnMod = "ManaBurn";
        public static string impactMod = "Impact";
        public static string allDamagesMod= "AllDamages";
        public static string physicalDamageMod = "PhysicalDamage";
        public static string etherealDamageMod = "EtherealDamage";
        public static string decayDamageMod = "DecayDamage";
        public static string electricDamageMod = "ElectricDamage";
        public static string frostDamageMod = "FrostDamage";
        public static string fireDamageMod = "FireDamage";
        public static string darkDamageMod = "DarkDamage";
        public static string lightDamageMod = "LightDamage";
        public static string damageProtectionMod = "DamageProtection";
        public static string physicalProtectionMod = "PhysicalProtection";
        public static string etherealProtectionMod = "EtherealProtection";
        public static string decayProtectionMod = "DecayProtection";
        public static string electricProtectionMod = "ElectricProtection";
        public static string frostProtectionMod = "FrostProtection";
        public static string fireProtectionMod = "FireProtection";
        public static string darkProtectionMod = "DarkProtection";
        public static string lightProtectionMod = "LightProtection";
        public static string allResistancesMod = "AllResistances";
        public static string damageResistanceMod = "DamageResistance";
        public static string physicalResistanceMod = "PhysicalResistance";
        public static string etherealResistanceMod = "EtherealResistance";
        public static string decayResistanceMod = "DecayResistance";
        public static string electricResistanceMod = "ElectricResistance";
        public static string frostResistanceMod = "FrostResistance";
        public static string fireResistanceMod = "FireResistance";
        public static string darkResistanceMod = "DarkResistance";
        public static string lightResistanceMod = "LightResistance";
        public static string impactResistanceMod = "ImpactResistance";
        public static string stabilityRegenMod = "StabilityRegen";
        public static string envColdProtectionMod = "EnvColdProtection";
        public static string envHeatProtectionMod = "EnvHeatProtection";
        public static string coldRegenMod = "ColdRegen";
        public static string heatRegenMod = "HeatRegen";
        public static string waterproofMod = "Waterproof";
        public static string corruptionResistanceMod = "CorruptionResistance";
        public static string temperatureMod = "TemperatureModifier";
        public static string moveSpeedMod = "MovementSpeed";
        public static string speedMod = "Speed";
        public static string attackSpeedMod = "AttackSpeed";
        public static string dodgeInvulnerabilityMod = "DodgeInvulnerabilityModifier";
        public static string skillCooldownMod = "SkillCooldownModifier";

        //stat sections
        public static string foodDepleteSection = "Food Depletion Rate";
        public static string drinkDepleteSection = "Drink Depletion Rate";
        public static string sleepDepleteSection = "Sleep Depletion Rate";
        public static string detectabilitySection = "Detectability";
        public static string visualDetectabilitySection = "Visual Detectability";
        public static string pouchCapacitySection = "Pouch Capacity";
        public static string foodEffectEfficiencySection = "Food Efficiency";
        public static string buySection = "Buying Rate";
        public static string sellSection = "Selling Rate";
        public static string healthSection = "Maximum Health";
        public static string healthRegenSection = "Health Regeneration";
        public static string healthBurnSection = "Health Burn Rate";
        public static string staminaSection = "Maximum Stamina";
        public static string staminaRegenSection = "Stamina Regeneration";
        public static string staminaUseSection = "Stamina Usage Rate";
        public static string staminaCostSection = "Stamina Cost Reduction";
        public static string staminaBurnSection = "Stamina Burn Rate";
        public static string manaSection = "Maximum Mana";
        public static string manaRegenSection = "Mana Regeneration";
        public static string manaUseSection = "Mana Usage Rate";
        public static string manaBurnSection = "Mana Burn Rate";
        public static string impactSection = "Impact Damage";
        public static string allDamagesSection = "All Damages";
        public static string physicalDamageSection = "Physical Damage";
        public static string etherealDamageSection = "Ethereal Damage";
        public static string decayDamageSection = "Decay Damage";
        public static string electricDamageSection = "Electric Damage";
        public static string frostDamageSection = "Frost Damage";
        public static string fireDamageSection = "Fire Damage";
        public static string darkDamageSection = "Dark Damage";
        public static string lightDamageSection = "Light Damage";
        public static string damageProtectionSection = "Damage Protection";
        public static string physicalProtectionSection = "Physical Protection";
        public static string etherealProtectionSection = "Ethereal Protection";
        public static string decayProtectionSection = "Decay Protection";
        public static string electricProtectionSection = "Electric Protection";
        public static string frostProtectionSection = "Frost Protection";
        public static string fireProtectionSection = "Fire Protection";
        public static string darkProtectionSection = "Dark Protection";
        public static string lightProtectionSection = "Light Protection";
        public static string allResistancesSection = "All Resistances";
        public static string damageResistanceSection = "Damage Resistance";
        public static string physicalResistanceSection = "Physical Resistance";
        public static string etherealResistanceSection = "Ethereal Resistance";
        public static string decayResistanceSection = "Decay Resistance";
        public static string electricResistanceSection = "Electric Resistance";
        public static string frostResistanceSection = "Frost Resistance";
        public static string fireResistanceSection = "Fire Resistance";
        public static string darkResistanceSection = "Dark Resistance";
        public static string lightResistanceSection = "Light Resistance";
        public static string impactResistanceSection = "Impact Resistance";
        public static string stabilityRegenSection = "Stability Regeneration";
        public static string envColdProtectionSection = "Cold Weather Protection";
        public static string envHeatProtectionSection = "Hot Weather Protection";
        public static string coldRegenSection = "Cold Temp. Regeneration";
        public static string heatRegenSection = "Hot Temp. Regeneration";
        public static string waterproofSection = "Wet Weather Protection";
        public static string corruptionResistanceSection = "Corruption Resistance";
        public static string temperatureSection = "Temperature Rate";
        public static string moveSpeedSection = "Movement Speed";
        public static string speedSection = "Speed";
        public static string attackSpeedSection = "Attack Speed";
        public static string dodgeInvulnerabilitySection = "Dodge Rate";
        public static string skillCooldownSection = "Skill Cooldown Rate";

        //rules settings
        public List<BBSetting> rulesSettings = new List<BBSetting>
        {
            new BoolSetting
            {
                Name = toggleSwitch,
                SectionTitle = toggleSection,
                Description = toggleDesc,
                DefaultValue = true
            },
            new BoolSetting
            {
                Name = gameBehaviour,
                SectionTitle = behaviourSection,
                Description = behaviourDesc,
                DefaultValue = true
            },
            new BoolSetting
            {
                Name = strictMinimum,
                SectionTitle = strictSection,
                Description = strictDesc,
                DefaultValue = true
            }
        };

        //stat settings
        public List<BBSetting> playerSettings = new List<BBSetting>
        {
            new BoolSetting
            {
                Name = foodDepleteMod + modMult,
                SectionTitle = foodDepleteSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = foodDepleteMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = drinkDepleteMod + modMult,
                SectionTitle = drinkDepleteSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = drinkDepleteMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = sleepDepleteMod + modMult,
                SectionTitle = sleepDepleteSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = sleepDepleteMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = foodEffectEfficiencyMod + modMult,
                SectionTitle = foodEffectEfficiencySection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = foodEffectEfficiencyMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = pouchCapacityMod + modMult,
                SectionTitle = pouchCapacitySection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = pouchCapacityMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = buyMod + modMult,
                SectionTitle = buySection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = buyMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = sellMod + modMult,
                SectionTitle = sellSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = sellMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = detectabilityMod + modMult,
                SectionTitle = detectabilitySection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = detectabilityMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = visualDetectabilityMod + modMult,
                SectionTitle = visualDetectabilitySection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = visualDetectabilityMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            }
        };

        public List<BBSetting> characterSettings = new List<BBSetting>
        {
            new BoolSetting
            {
                Name = temperatureMod + modMult,
                SectionTitle = temperatureSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = temperatureMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = waterproofMod + modMult,
                SectionTitle = waterproofSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = waterproofMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = envColdProtectionMod + modMult,
                SectionTitle = envColdProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = envColdProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = coldRegenMod + modMult,
                SectionTitle = coldRegenSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = coldRegenMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = envHeatProtectionMod + modMult,
                SectionTitle = envHeatProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = envHeatProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = heatRegenMod + modMult,
                SectionTitle = heatRegenSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = heatRegenMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = speedMod + modMult,
                SectionTitle = speedSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = speedMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = moveSpeedMod + modMult,
                SectionTitle = moveSpeedSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = moveSpeedMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = attackSpeedMod + modMult,
                SectionTitle = attackSpeedSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = attackSpeedMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = stabilityRegenMod + modMult,
                SectionTitle = stabilityRegenSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = stabilityRegenMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = dodgeInvulnerabilityMod + modMult,
                SectionTitle = dodgeInvulnerabilitySection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = dodgeInvulnerabilityMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = skillCooldownMod + modMult,
                SectionTitle = skillCooldownSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = skillCooldownMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = healthMod + modMult,
                SectionTitle = healthSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = healthMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = healthRegenMod + modMult,
                SectionTitle = healthRegenSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = healthRegenMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = healthBurnMod + modMult,
                SectionTitle = healthBurnSection,
                Description =  multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = healthBurnMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = staminaMod + modMult,
                SectionTitle = staminaSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = staminaMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = staminaRegenMod + modMult,
                SectionTitle = staminaRegenSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = staminaRegenMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = staminaUseMod + modMult,
                SectionTitle = staminaUseSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = staminaUseMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = staminaCostReducMod + modMult,
                SectionTitle = staminaCostSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = staminaCostReducMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = staminaBurnMod + modMult,
                SectionTitle = staminaBurnSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = staminaBurnMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = manaMod + modMult,
                Description = multDesc,
                SectionTitle = manaSection,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = manaMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = manaRegenMod + modMult,
                SectionTitle = manaRegenSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = manaRegenMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = manaUseMod + modMult,
                SectionTitle = manaUseSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = manaUseMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = manaBurnMod + modMult,
                SectionTitle = manaBurnSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = manaBurnMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = allDamagesMod + modMult,
                SectionTitle = allDamagesSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = allDamagesMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = physicalDamageMod + modMult,
                SectionTitle = physicalDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = physicalDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = impactMod + modMult,
                SectionTitle = impactSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = impactMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = etherealDamageMod + modMult,
                SectionTitle = etherealDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = etherealDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = decayDamageMod + modMult,
                SectionTitle = decayDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = decayDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = electricDamageMod + modMult,
                SectionTitle = electricDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = electricDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = frostDamageMod + modMult,
                SectionTitle = frostDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = frostDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = fireDamageMod + modMult,
                SectionTitle = fireDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = fireDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = darkDamageMod + modMult,
                SectionTitle = darkDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = darkDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = lightDamageMod + modMult,
                SectionTitle = lightDamageSection,
                Description = multDesc,
                DefaultValue = true
            },
            new FloatSetting
            {
                Name = lightDamageMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = damageProtectionMod + modMult,
                SectionTitle = damageProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = damageProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = physicalProtectionMod + modMult,
                SectionTitle = physicalProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = physicalProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = etherealProtectionMod + modMult,
                SectionTitle = etherealProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = etherealProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = decayProtectionMod + modMult,
                SectionTitle = decayProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = decayProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = electricProtectionMod + modMult,
                SectionTitle = electricProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = electricProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = frostProtectionMod + modMult,
                SectionTitle = frostProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = frostProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = fireProtectionMod + modMult,
                SectionTitle = fireProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = fireProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = darkProtectionMod + modMult,
                SectionTitle = darkProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = darkProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = lightProtectionMod + modMult,
                SectionTitle = lightProtectionSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = lightProtectionMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = allResistancesMod + modMult,
                SectionTitle = allResistancesSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = allResistancesMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = damageResistanceMod + modMult,
                SectionTitle = damageResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = damageResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = corruptionResistanceMod + modMult,
                SectionTitle = corruptionResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = corruptionResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = physicalResistanceMod + modMult,
                SectionTitle = physicalResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = physicalResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = impactResistanceMod + modMult,
                SectionTitle = impactResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = impactResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = etherealResistanceMod + modMult,
                SectionTitle = etherealResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = etherealResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = decayResistanceMod + modMult,
                SectionTitle = decayResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = decayResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = electricResistanceMod + modMult,
                SectionTitle = electricResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = electricResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = frostResistanceMod + modMult,
                SectionTitle = frostResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = frostResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = fireResistanceMod + modMult,
                SectionTitle = fireResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = fireResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = darkResistanceMod + modMult,
                SectionTitle = darkResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = darkResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            },
            new BoolSetting
            {
                Name = lightResistanceMod + modMult,
                SectionTitle = lightResistanceSection,
                Description = multDesc,
                DefaultValue = false
            },
            new FloatSetting
            {
                Name = lightResistanceMod,
                Description = modDesc,
                DefaultValue = 0f,
                MinValue = -500f,
                MaxValue = 500f,
                RoundTo = 0
            }
        };

        //util
        public static void SoftRegister(ModConfig _config)
        {
            Dictionary<string, BBSetting> _dict = new Dictionary<string, BBSetting>();

            foreach (BBSetting _bbs in _config.Settings)
            {
                _dict.Add(_bbs.Name, _bbs);
            }

            AT.SetValue(_dict, typeof(ModConfig), _config, "m_Settings");
        }
    }
}

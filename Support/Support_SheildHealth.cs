//+=========================================================================================================+\\
//|			Made by..																						|\\
//|		   ____   ____  _		__	  _		   																|\\
//|		  |_  _| |_  _|(_)	      [  |	/ |_		 														|\\
//| 		\ \   / /  __   .--.   .--.  | |  ,--. `| |-' .--.   _ .--.  									|\\
//| 		 \ \ / /  [  | ( (`\]/ .'`\ \| | `'_\ : | | / .'`\ \[ `/'`\] 									|\\
//| 		  \ ' /    | |  `'.'.| \__. || | // | |,| |,| \__. | | |     									|\\
//|    		   \_/    [___][\__) )'.__.'[___]\'-;__/\__/ '.__.' [___]    									|\\
//|								BL_ID: 20490 | BL_ID: 48980													|\\
//|				Forum Profile(48980): http://forum.blockland.us/index.php?action=profile;u=144888;			|\\
//|																											|\\
//+=========================================================================================================+\\

datablock PlayerData(PlayerJetHealth : PlayerStandardArmor)
{
	minJetEnergy = 0;
	jetEnergyDrain = 0;
	canJet = 0;
	repairRate = 0;
	
	//Custom variables for Support_JetEnergyDamage
	useJetEnergy = 1; //Use jet energy as a shield
	useHealthAfterJetEnergy = 0; //Use real health after energy is gone

	maxEnergy = 100;
	maxDamage = 100;

	uiName = "Jet Health Player";
	showEnergyBar = true;
};

if(isPackage("Support_JetEnergyDamage"))
	deactivatePackage("Support_JetEnergyDamage");

package Support_JetEnergyDamage
{
	function Armor::Damage(%armor, %object, %source, %position, %damage, %damageType)
	{
		if(%object.getState() !$= "dead") //Are they dead?
		{
			//Let's check the datablock whether or not we have the "useJetEnergy" variable
			if(%armor.useJetEnergy || %object.useJetEnergy)
			{
				//getEnergyLevel() is the player's current energy, do we have any?
				if((%energy = %object.getEnergyLevel()) > 0)
				{
					//If the damage is more than the current energy, whatever is left over we will apply to their actual health.
					if(%damage > %energy)
					{
						%damage -= %energy;
						%energy = 0;
					}
					else //Otherwise just subtract their energy by the damage they received
					{
						%energy -= %damage;
						%damage = 0;
					}
					
					//This is their energy, set it to a level.
					%object.setEnergyLevel(%energy);
					
					//Check the datablock whether or not we have the "useHealthAfterEnergy", we use the datablock's health if this is true, otherwise we just kill them.
					if(!%armor.useHealthAfterEnergy && !%object.useHealthAfterEnergy && %damage > 0)
						%damage = %armor.getMaxHealth() * getWord(%object.getScale(), 2);
				}
				else if(!%armor.useHealthAfterEnergy && !%object.useHealthAfterEnergy)
					%damage = %armor.getMaxHealth() * getWord(%object.getScale(), 2);
			}
		}
		//Always parent default functions, some might even require a return
		Parent::Damage(%armor, %object, %source, %position, %damage, %damageType);
	}
};
activatePackage("Support_JetEnergyDamage");

//Support_HealthDetection.cs
//For mods that use anything such as: %this.getDatablock().maxDamage, %this.getDamageLevel() - This is what they need to be replaced with in case they use this mod.

function ShapeBase::getHealth(%this)
{
	if(!isObject(%this))
		return -1;
	if(%this.maxHealth > 0)
		return %this.health;
	return %this.getDatablock().maxDamage - %this.getDamageLevel();
}

function ShapeBase::getMaxHealth(%this)
{
	if(!isObject(%this))
		return -1;
	if(%this.maxHealth > 0)
		return %this.maxHealth;
	return %this.getDatablock().maxDamage;
}

function ShapeBase::getHealthLevel(%this) //This is used to set their damage level, which is opposite of their health
{
	%level = %this.getDatablock().maxDamage - (%this.getDatablock().maxDamage / %this.getMaxHealth() * %this.getHealth());
	%level = mClampF(%level, 0, %this.getDatablock().maxDamage);
	return %level;
}
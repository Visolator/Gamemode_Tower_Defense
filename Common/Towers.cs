$TD::FilePath_Towers = "Add-Ons/Gamemode_Tower_Defense/Towers/";

if(!isObject(TowerGroup))
{
	new ScriptGroup(TowerGroup)
	{
		class = TowerSO;
		filePath = "config/server/Towers/";
	};
	TowerGroup.schedule(1000, Load);
}
else
	TowerGroup.schedule(1000, Load);

function TowerSO::findScript(%this, %TowerName)
{
	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(%TowerName == %obj)
			return %obj;
		else if(strPos(%obj.uiName, %TowerName) >= 0)
			return %obj;
	}
	return -1;
}

/// <summary>
/// Loads the Tower program.
/// </summary>
function TowerSO::Load(%this)
{
	//--PARAMETERS - READ CAREFULLY--\\
	
	//Description: This will display the current Tower fields it can support.

	//--KEY--\\
	//	//_!	<- Required field, otherwise it will be confused
	//	//->	<- Normal
	//	//-?	<- Can depend on things
	//	-/- 	<- Comment, do not copy that part if you plan to

	//~MAIN~\\
	//_!	tree	string	- Where does this go to?
	//_!	description	string	- When people see the Tower available, this is the description shown.
	//_!	cost	int	- How much does it cost?
	//->	adminLevel	int	- Admin level required to aquire
	//_!	height	int	- Tower height
	//_!	canBuild	bool	- Can they build?

	//--YOU MAY USE FUNCTIONS, HERE IS HOW IT GOES--\\
	//	CMD function(args...);

	%this.deleteAll();
	$TowerTrees = "";
	%path = $TD::FilePath_Towers @ "*";

	registerTower("None");

	registerTower("Default", "Description A class that can plant towers. Has to select a different class though." TAB 
		"height -1" TAB
		"canBuild 1");

	registerTower("Bowman", "Description Medium range, slow reload." TAB 
		"canBuild 1" TAB 
		"cost 310" TAB 
		"height 1" TAB 
		"health 400" TAB 
		"weaponName Bow" TAB 
		"canUpgrade 1" TAB
		"upgradePaths Damage, Health, Range, Range" TAB
		"holdTime 0.8" TAB 
		"releaseTime 0.2" TAB
		"range 35" TAB
		"damageMultiplier 1.5" TAB
		"customAvatar 1" TAB
		"Accent 1" TAB
		"AccentColor 0.900 0.000 0.000 1.000" TAB
		"Authentic 0" TAB
		"Chest 0" TAB
		"ChestColor 0.000 0.500 0.250 1.000" TAB
		"DecalName LinkTunic" TAB
		"FaceName smiley" TAB
		"Hat 4" TAB
		"HatColor 0.392157 0.196078 0 1" TAB
		"HeadColor 1 0.878431 0.611765 1" TAB
		"Hip 0" TAB
		"HipColor 0.392157 0.196078 0 1" TAB
		"LArm 0" TAB
		"LArmColor 0.000 0.500 0.250 1.000" TAB
		"LHand 0" TAB
		"LHandColor 1 0.878431 0.611765 1" TAB
		"LLeg 0" TAB
		"LLegColor 0.392157 0.196078 0 1" TAB
		"Pack 5" TAB
		"PackColor 0.392157 0.196078 0 1" TAB
		"RArm 0" TAB
		"RArmColor 0.000 0.500 0.250 1.000" TAB
		"RHand 0" TAB
		"RHandColor 1 0.878431 0.611765 1" TAB
		"RLeg 0" TAB
		"RLegColor 0.392157 0.196078 0 1" TAB
		"SecondPack 0" TAB
		"SecondPackColor 0.200 0.200 0.200 1.000" TAB
		"TorsoColor 0.000 0.500 0.250 1.000");

	//This works fine but it has too much knockback.
	//registerTower("Rocketman", "Description Medium range, blast mobs around!" TAB 
	//	"canBuild 1" TAB 
	//	"cost 950" TAB 
	//	"height 2" TAB 
	//	"health 300" TAB 
	//	"weaponName Rocket L." TAB 
	//	"holdTime 2" TAB 
	//	"releaseTime 0.2" TAB
	//	"range 40" TAB
	//	"customAvatar 1" TAB 
	//	"Accent 0" TAB 
	//	"AccentColor 0.560748 0.0981308 0.0747664 1" TAB 
	//	"Chest 0" TAB 
	//	"ChestColor 0 0.5 0.25 1" TAB 
	//	"DecalName Archer" TAB 
	//	"FaceName ChefSmiley" TAB 
	//	"Hat 5" TAB 
	//	"HatColor 0.2 0.2 0.2 1" TAB 
	//	"HeadColor 1 0.878 0.611 1" TAB 
	//	"Hip 0" TAB 
	//	"HipColor 0.2 0.2 0.2 1" TAB 
	//	"LArm 0" TAB 
	//	"LArmColor 0.9 0.9 0.9 1" TAB 
	//	"LHand 0" TAB 
	//	"LHandColor 1 0.878 0.611 1" TAB 
	//	"LLeg 0" TAB 
	//	"LLegColor 0.078 0.078 0.078 1" TAB 
	//	"Pack 3" TAB 
	//	"PackColor 0.9 0.9 0 1" TAB 
	//	"RArm 0" TAB 
	//	"RArmColor 0.9 0.9 0.9 1" TAB 
	//	"RHand 0" TAB 
	//	"RHandColor 1 0.878 0.611 1" TAB 
	//	"RLeg 0" TAB 
	//	"RLegColor 0.078 0.078 0.078 1" TAB 
	//	"SecondPack 1" TAB 
	//	"SecondPackColor 0.9 0.9 0 1");

	//registerTower("Spearman", "Description Long range, kill large mobs!" TAB 
	//	"canBuild 1" TAB 
	//	"cost 800" TAB 
	//	"height 2" TAB 
	//	"health 250" TAB 
	//	"weaponName Spear" TAB 
	//	"holdTime 2.8" TAB 
	//	"releaseTime 1" TAB
	//	"range 45" TAB
	//	"customAvatar 1" TAB
	//	"Accent 0" TAB
	//	"AccentColor 0.500 0.500 0.500 1.000" TAB
	//	"Authentic 0" TAB
	//	"Chest 0" TAB
	//	"ChestColor 0.392157 0.196078 0 1" TAB
	//	"DecalName Medieval-Tunic" TAB
	//	"FaceName smiley" TAB
	//	"Hat 0" TAB
	//	"HatColor 0.392157 0.196078 0 1" TAB
	//	"HeadColor 1 0.878431 0.611765 1" TAB
	//	"Hip 0" TAB
	//	"HipColor 0.392157 0.196078 0 1" TAB
	//	"LArm 0" TAB
	//	"LArmColor 0.200 0.200 0.200 1.000" TAB
	//	"LHand 0" TAB
	//	"LHandColor 1 0.878431 0.611765 1" TAB
	//	"LLeg 0" TAB
	//	"LLegColor 1 0.878431 0.611765 1" TAB
	//	"Pack 0" TAB
	//	"PackColor 0.200 0.200 0.200 1.000" TAB
	//	"RArm 0" TAB
	//	"RArmColor 0.200 0.200 0.200 1.000" TAB
	//	"RHand 0" TAB
	//	"RHandColor 1 0.878431 0.611765 1" TAB
	//	"RLeg 0" TAB
	//	"RLegColor 1 0.878431 0.611765 1" TAB
	//	"SecondPack 0" TAB
	//	"SecondPackColor 0.200 0.200 0.200 1.000" TAB
	//	"TorsoColor 0.392157 0.196078 0 1");

	//announce("Loading saved files for mob classes. -> Path: " @ %path);
	%file = findFirstFile(%path);
	if(isFile(%file))
	{
		%fileExt = fileExt(%file);
		%name = fileBase(%file);
		if(%fileExt $= ".cs") //Just making sure
		{
			if(isObject(%obj = isRegisteredTower(fileBase(%path))))
				%obj.delete();

			exec(%file);
		}
	}
	else
		return;

	while(%file !$= "")
	{
		%file = findNextFile(%path);
		%fileExt = fileExt(%file);
		%name = fileBase(%file);
		if(%fileExt $= ".cs") //Just making sure
		{
			if(isObject(%obj = isRegisteredTower(fileBase(%path))))
				%obj.delete();

			exec(%file);
		}
	}
}

/// <summary>
/// When the Tower is created, we need to format the variables, so we put it into a command, see -> registerTower
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function Tower::onAdd(%this)
{
	TowerGroup.add(%this);
	%this.parseCommand(%this.command);
}

function Tower::getTreeParsed(%this)
{
	return strReplace(%this.tree, " ", "_");
}

/// <summary>
/// Parses Tower objects' commands, see -> Tower::onAdd
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function Tower::parseCommand(%this, %com)
{
	//echo("       -> Parasing Tower command line: " @ %this.uiName);
	//echo("       -> CommandLine: " @ %com);
	for(%i=0;%i<getFieldCount(%com);%i++)
	{
		%field = getField(%com, %i);
		%name = getWord(%field, 0);
		%value = collapseEscape(getWords(%field, 1, getWordCount(%field)-1));

		if(%name $= "tree")
		{
			for(%a=0;%a<getFieldCount($TowerTrees);%a++)
				if(getField($TowerTrees, %a) $= %value)
					%remove = 1;

			if(!%remove)
				$TowerTrees = %value TAB $TowerTrees;
		}

		//echo("         PARAMETER FOUND: " @ %cmd);
		//echo("             VALUE: " @ %value);
		%cmd = %this @ "." @ %name @ " = \"" @ %value @ "\";";
		//echo("             PARSED: " @ %cmd);
		eval(%cmd);
	}

	%this.command = "";

	if(trim(%this.type) $= "")
		%this.type = "Default";

	if(isObject(%image = findItemByName(%this.weaponName).image))
	{
		if(isObject(%proj = %item.image.projectile))
			%intDamage = %proj.directDamage SPC %proj.explosion.radiusDamage;

		%damage = mFloor(getWord(%intDamage, 0));
		//%radiusDamage = mFloor(getWord(%intDamage, 1));
		%this.TD_Damage = %damage;
	}
}

/// <summary>
/// Registers a Tower into the TowerSO program. Easy to see
/// </summary>
/// <param name="name">Name of the created mob.</param>
/// <param name="parm">Parameters, each variable must be in a different field.</param>
function registerTower(%name, %parm)
{
	%strName = stripChars(%name, $TD::Chars);
	%strName = strReplace(%strName, " ", "_");
	%objName = "Tower_" @ %strName;
	echo("Registering a Tower.. - " @ %name @ " (" @ %objName @ ")");
	for(%i=0;%i<getFieldCount(%parm);%i++)
	{
		%field = getField(%parm,%i);
		%var = getWord(%field,0);
		%value = getWords(%field, 1, getWordCount(%field)-1);
		//echo("   PARAMETER FOUND: " @ %var);
		//echo("     VALUE: " @ %value);

		//for(%a=0;%a<getWordCount($City::TowerSO_RequiredFields);%a++)
		//{
		//	%requirement = getWord($City::TowerSO_RequiredFields,%a);
		//	if(%var $= %requirement && !%met_[%requirement])
		//	{
		//		%met_[%requirement] = 1;
		//		%metCount++;
		//	}
		//}
	}

	//if(%metCount < getWordCount($City::TowerSO_RequiredFields))
	//{
	//	warn(" - Unable to add the Tower. Make sure you have made the parameters correctly.");
	//	warn(" - Requirement amount: " @ mFloor(%metCount) @ "/" @ getWordCount($City::TowerSO_RequiredFields));
	//	return;
	//}

	if(isObject(%objName))
	{
		warn("Warning: Tower data \"" @ %objName @ "\" already exists. Overwriting.");
		%objName.delete();
	}

	%obj = new ScriptObject(%objName)
	{
		class = Tower;
		uiName = %name;
		command = "chatColor <color:ffff00>" TAB collapseEscape(%parm);
		description = %description;
	};

	//if(isObject(%obj = TowerGroup.findScript(%name)))
	//	%obj.save($City::FilePath_Towers @ %obj.uiName @ ".cs");
}

/// <summary>
/// Returns whether the Tower exists or not.
/// </summary>
/// <param name="Tower">Tower object or name.</param>
function isRegisteredTower(%Tower)
{
	return TowerGroup.findScript(%Tower);
}

function GameConnection::TD_SetAvatar(%this, %towerName)
{
	if(!isObject(%Tower = TowerGroup.findScript(%towerName)))
	{
		%this.chatMessage("\c5Invalid tower to force. (\c3" @ %towerName @ "\c5)");
		return;
	}

	if(!isObject(%player = %this.player))
	{
		%this.chatMessage("\c5You need to spawn first.");
		return;
	}

	if(%tower.customAvatar)
	{
		%this.llegColor = %tower.llegColor;
		%this.secondPackColor = %tower.secondPackColor;
		%this.lhand = %tower.lhand;
		%this.hip = %tower.hip;
		%this.faceName = %tower.faceName;
		%this.rarmColor = %tower.rarmColor;
		%this.hatColor = %tower.hatColor;
		%this.hipColor = %tower.hipColor;
		%this.chest = %tower.chest;
		%this.rarm = %tower.rarm;
		%this.packColor = %tower.packColor;
		%this.pack = %tower.pack;
		%this.decalName = %tower.decalName;
		%this.larmColor = %tower.larmColor;
		%this.secondPack = %tower.secondPack;
		%this.larm = %tower.larm;
		%this.chestColor = %tower.chestColor;
		%this.accentColor = %tower.accentColor;
		%this.rhandColor = %tower.rhandColor;
		%this.rleg = %tower.rleg;
		%this.rlegColor = %tower.rlegColor;
		%this.accent = %tower.accent;
		%this.headColor = %tower.headColor;
		%this.rhand = %tower.rhand;
		%this.lleg = %tower.lleg;
		%this.lhandColor = %tower.lhandColor;
		%this.hat = %tower.hat;
	}

	%this.applyBodyParts();
	%this.applyBodyColors();
	if(isObject(%image = findItemByName(%tower.weaponName).image))
	{
		%player.mountImage(%image, 0);
		if(%image.armReady)
			%player.playThread(1, "armReadyRight");

		if(isObject(%image2 = %player.getMountedImage(1)))
			if(%image.armReady)
				%player.playThread(1, "armReadyBoth");
	}
}

datablock fxDTSBrickData(TD_8xCoverCube)
{	
	category = "";
	subCategory = "";
		
	uiName = "";
		
	isTowerBrick = true;

	brickFile = "./Bricks/8x Cube.blb";
	iconName = "";

	topArea = 64;
	bottomArea = 64;
	northArea = 160;
	eastArea = 160;
	southArea = 160;
	westArea = 160;
	canCoverTop = 1;
	canCoverBottom = 1;
	canCoverNorth = 1;
	canCoverEast = 1;
	canCoverSouth = 1;
	canCoverWest = 1;
	hasPrint = 0;
	indestructable = 0;
	alwaysShowWireFrame = 0;
	isWaterBrick = 1;
};

datablock fxDTSBrickData(TD_8xCube)
{	
	category = "";
	subCategory = "";
		
	uiName = "";
		
	isTowerBrick = true;

	brickFile = "./Bricks/8x Cube.blb";
	iconName = "";

	topArea = 64;
	bottomArea = 64;
	northArea = 160;
	eastArea = 160;
	southArea = 160;
	westArea = 160;
	canCoverTop = 1;
	canCoverBottom = 1;
	canCoverNorth = 1;
	canCoverEast = 1;
	canCoverSouth = 1;
	canCoverWest = 1;
	hasPrint = 0;
	brickSizeX = 8;
	brickSizeY = 8;
	brickSizeZ = 20;
	indestructable = 0;
	alwaysShowWireFrame = 0;
	isWaterBrick = 0;
};

datablock fxDTSBrickData(TD_4xCoverCube)
{	
	category = "";
	subCategory = "";
		
	uiName = "";
		
	isTowerBrick = true;

	brickFile = "./Bricks/4x Cube.blb";
	iconName = "";

	topArea = 64;
	bottomArea = 64;
	northArea = 160;
	eastArea = 160;
	southArea = 160;
	westArea = 160;
	canCoverTop = 1;
	canCoverBottom = 1;
	canCoverNorth = 1;
	canCoverEast = 1;
	canCoverSouth = 1;
	canCoverWest = 1;
	hasPrint = 0;
	indestructable = 0;
	alwaysShowWireFrame = 0;
	isWaterBrick = 1;
};

datablock fxDTSBrickData(TD_4xCube)
{	
	category = "";
	subCategory = "";
		
	uiName = "";
		
	isTowerBrick = true;

	brickFile = "./Bricks/4x Cube.blb";
	iconName = "";

	topArea = 64;
	bottomArea = 64;
	northArea = 160;
	eastArea = 160;
	southArea = 160;
	westArea = 160;
	canCoverTop = 1;
	canCoverBottom = 1;
	canCoverNorth = 1;
	canCoverEast = 1;
	canCoverSouth = 1;
	canCoverWest = 1;
	hasPrint = 0;
	indestructable = 0;
	alwaysShowWireFrame = 0;
	isWaterBrick = 0;
};

datablock fxDTSBrickData(TD_4xTallCoverCube)
{	
	category = "";
	subCategory = "";
		
	uiName = "";
		
	isTowerBrick = true;

	brickFile = "./Bricks/4x Tall Cube.blb";
	iconName = "";

	topArea = 64;
	bottomArea = 64;
	northArea = 160;
	eastArea = 160;
	southArea = 160;
	westArea = 160;
	canCoverTop = 1;
	canCoverBottom = 1;
	canCoverNorth = 1;
	canCoverEast = 1;
	canCoverSouth = 1;
	canCoverWest = 1;
	hasPrint = 0;
	indestructable = 0;
	alwaysShowWireFrame = 0;
	isWaterBrick = 1;
};

datablock fxDTSBrickData(TD_4xTallCube)
{	
	category = "";
	subCategory = "";
		
	uiName = "";
		
	isTowerBrick = true;

	brickFile = "./Bricks/4x Tall Cube.blb";
	iconName = "";

	topArea = 64;
	bottomArea = 64;
	northArea = 160;
	eastArea = 160;
	southArea = 160;
	westArea = 160;
	canCoverTop = 1;
	canCoverBottom = 1;
	canCoverNorth = 1;
	canCoverEast = 1;
	canCoverSouth = 1;
	canCoverWest = 1;
	hasPrint = 0;
	indestructable = 0;
	alwaysShowWireFrame = 0;
	isWaterBrick = 0;
};

function GameConnection::getHiddenTowerDB(%this)
{
	%tower = %this.getTower();
	switch(%tower.towerHiddenSize)
	{
		case "8x":
			return nameToID("TD_8xCoverCube");

		case "4x":
			return nameToID("TD_4xCoverCube");

		default:
			return nameToID("TD_4xTallCoverCube");
	}
}

function GameConnection::getTowerDB(%this)
{
	%tower = %this.getTower();
	switch(%tower.towerSize)
	{
		case "8x":
			return nameToID("TD_8xCube");

		case "4x Tall":
			return nameToID("TD_4xTallCube");

		default:
			return nameToID("TD_4xCube");
	}
}

function GameConnection::getTower(%this)
{
	return nameToID(%this.towerScript);
}

function GameConnection::setTower(%this, %Tower, %val)
{
	if(!isObject(%Tower = TowerGroup.findScript(%Tower)))
	{
		%this.chatMessage("Invalid Tower to select. Selecting default class");
		%this.setTower("None");
		return;
	}

	%this.towerScript = %tower.getName();
	%this.towerName = %Tower.uiName;
	%this.isBuilder = mFloor(%Tower.canBuild);
	if(%this.towerName !$= "None")
	{
		//%this.chatMessage("Tower set to \c3" @ %this.towerName);
		//TD_Debug(%this.getPlayerName() @ " - " @ %this.towerName);
	}
}

function GameConnection::TD_Tower(%this)
{
	return TowerGroup.findScript(%this.towerName);
}

//Start - Temp commands

function serverCmdListTowers(%this)
{
	//%this.chatMessage("Sorry this command doesn't exist, but it says hello to you.");
	%this.chatMessage(TowerGroup.getCount() @ " towers detected.");
	for(%i = 0; %i < TowerGroup.getCount(); %i++)
	{
		%tower = TowerGroup.getObject(%i);
		%this.chatMessage(%tower.uiName @ " - " @ %tower.description);
	}
	%this.chatMessage("/setTower \"tower name\" \"person (or yourself)\"");
}

function serverCmdSetTower(%this, %Tower, %name)
{
	if(!%this.isAdmin)
		return;

	if(trim(%name) $= "" || !isObject(%name = findClientByName(%name)) || !%this.isAdmin)
		%name = %this;
	else
		%name.chatMessage(%this.getPlayerName() @ " has attempted to forcefully set your Tower.");

	if(!isObject(%TowerObj = TowerGroup.findScript(%Tower)))
	{
		%this.chatMessage("Invalid Tower to force.");
		return;
	}
	%this.chatMessage("Tower forced to " @ %TowerObj.uiName @ " on " @ %name.getPlayerName());
	%name.setTower(%TowerObj);
	%name.chatMessage("Tower forced to " @ %TowerObj.uiName);
}

function serverCmdTD_AcceptUpgrade(%this)
{
	if(!isObject(%this))
		return;

	if(!isObject(%minigame = %this.minigame))
		return;

	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "Dead")
		return;

	if(!isObject(%bot = %this.TempTDData["UpgradeBot"]))
	{
		%this.chatMessage("ERROR: No tower is selected to be upgraded!");
		return;
	}

	if(!isObject(%script = %bot.script))
	{
		%this.chatMessage("ERROR: No script is selected to be upgraded!");
		%this.TempTDData["UpgradeBot"] = 0;
		return;
	}

	if($Sim::Time - %this.lastTDUpgradePrompt > 5)
	{
		%this.chatMessage("ERROR: Tower selection request is too old!");
		%this.TempTDData["UpgradeBot"] = 0;
		return;
	}

	if($Sim::Time - %bot.lastUpgradeTime < 3)
	{
		%this.chatMessage("ERROR: Request time too fast!");
		%this.TempTDData["UpgradeBot"] = 0;
		return;
	}

	%bot.damageMultiplier = (%script.damageMultiplier !$= "" ? %script.damageMultiplier : 1) * %damage;
	%bot.setMaxHealth(%script.health * %health);
	%bot.range = %script.range * %range;
	%bot.shootWaitTime = %script.shootWaitTime;
		
	%minigame.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c3" @ %this.getPlayerName() @ "\c6 has reset a tower's path: \c3" @ %bot.uiName);
	%bot.TD_setShapeName();
	%bot.lastUpgradeTime = $Sim::Time;
	%bot.TDData["Path"] = "";
	%bot.TDData["UpgradeLevel"] = 0;

	%this.TempTDData["UpgradeBot"] = 0;
}

function serverCmdTD_AcceptUpgrade(%this)
{
	if(!isObject(%this))
		return;

	if(!isObject(%minigame = %this.minigame))
		return;

	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "Dead")
		return;

	if(!isObject(%bot = %this.TempTDData["UpgradeBot"]))
	{
		%this.chatMessage("ERROR: No tower is selected to be upgraded!");
		%this.TempTDData["UpgradeBot"] = 0;
		return;
	}

	if(!isObject(%script = %bot.script))
	{
		%this.chatMessage("ERROR: No script is selected to be upgraded!");
		%this.TempTDData["UpgradeBot"] = 0;
		return;
	}

	if($Sim::Time - %this.lastTDUpgradePrompt > 5)
	{
		%this.chatMessage("ERROR: Tower selection request is too old!");
		%this.TempTDData["UpgradeBot"] = 0;
		return;
	}

	%power = %bot.TDData["Path"];
	%level = %bot.TDData["UpgradeLevel"];

	%upgradeCost = mAbs(%bot.TD_getUpgrade(%power, "Cost", %level + 1));
	if(%minigame.TD_Resources >= %upgradeCost)
	{
		if(%level + 1 > %bot.TD_getMaxLevel(%power))
		{
			%this.chatMessage("Max level reached!");
			return;
		}

		%level = (%bot.TDData["UpgradeLevel"] += 1);

		%damage = (100 + %bot.TD_getUpgrade(%power, "Damage", %level)) / 100;
		%health = (100 + %bot.TD_getUpgrade(%power, "Health", %level)) / 100;
		%range = (100 + %bot.TD_getUpgrade(%power, "Range", %level)) / 100;
		%speed = (100 + %bot.TD_getUpgrade(%power, "Speed", %level)) / 100;

		%value = (100 + %bot.TD_getUpgrade(%power, %power, %level)) / 100;
		switch$(%power)
		{
			case "Damage":
				%valueStr = ((%script.damageMultiplier !$= "" ? %script.damageMultiplier : 1) * %value) @ "+";

			case "Health":
				%valueStr = (%script.health * %value) @ "HP";

			case "Range":
				%valueStr = "~" @ (%script.range * %value);

			case "Speed":
				%valueStr = (%script.shootWaitTime * %value) @ "s";
		}

		if(%damage != 0)
			%bot.damageMultiplier = (%script.damageMultiplier !$= "" ? %script.damageMultiplier : 1) * %damage;

		if(%health != 0)
		{
			%bot.setMaxHealth(%script.health * %health);
			%bot.setInvulnerbilityTime(1);
		}

		if(%range != 0)
			%bot.range = %script.range * %range;

		if(%speed != 0)
			%bot.shootWaitTime = %script.shootWaitTime * %speed;
		
		%minigame.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c3" @ %this.getPlayerName() @ "\c6 has upgraded a tower: \c3" @ %bot.uiName @ " \c6for \c3$" @ %upgradeCost @ " \c6resources \c6(\c3" @ %power @ " \c6(\c3" @ %valueStr @ "\c6)" @ " \c6| Level: \c3" @ %level @ "\c6)");
		%minigame.TD_AddResources(-%upgradeCost);
		%bot.TD_setShapeName();
		%bot.lastUpgradeTime = $Sim::Time;
	}
	else
		%this.chatMessage("Not enough resources to upgrade this tower!");

	%this.TempTDData["UpgradeBot"] = 0;
}

//Percent only from the tower's max health from the beginning
//"Max" is not a percent!
//--Why does this exist? If the tower is allowed to have upgrades but it fails to have any upgrade data, these are the default ones.--\\

//Modes:
// 0 - Use only the percent
// 1 - Use the percent times the upgrade count
$Server::TD_DefaultUpgrade["CostMode"] = 1;

$Server::TD_DefaultUpgrade["Health", "Health"] = 22;
$Server::TD_DefaultUpgrade["Health", "Damage"] = -2;
$Server::TD_DefaultUpgrade["Health", "Speed"] = 2;
$Server::TD_DefaultUpgrade["Health", "Range"] = -1;
$Server::TD_DefaultUpgrade["Health", "Cost"] = 5;
$Server::TD_DefaultUpgrade["Health", "Max"] = 5;

$Server::TD_DefaultUpgrade["Damage", "Health"] = -1;
$Server::TD_DefaultUpgrade["Damage", "Damage"] = 60;
$Server::TD_DefaultUpgrade["Damage", "Speed"] = 4;
$Server::TD_DefaultUpgrade["Damage", "Range"] = -3;
$Server::TD_DefaultUpgrade["Damage", "Cost"] = 7;
$Server::TD_DefaultUpgrade["Damage", "Max"] = 3;

$Server::TD_DefaultUpgrade["Speed", "Health"] = -2;
$Server::TD_DefaultUpgrade["Speed", "Damage"] = -2;
$Server::TD_DefaultUpgrade["Speed", "Speed"] = -18;
$Server::TD_DefaultUpgrade["Speed", "Range"] = -2;
$Server::TD_DefaultUpgrade["Speed", "Cost"] = 3;
$Server::TD_DefaultUpgrade["Speed", "Max"] = 3;

$Server::TD_DefaultUpgrade["Range", "Health"] = -1;
$Server::TD_DefaultUpgrade["Range", "Damage"] = -4;
$Server::TD_DefaultUpgrade["Range", "Speed"] = 4;
$Server::TD_DefaultUpgrade["Range", "Range"] = 22;
$Server::TD_DefaultUpgrade["Range", "Cost"] = 7;
$Server::TD_DefaultUpgrade["Range", "Max"] = 3;
//--

//Sent in fields
function AIPlayer::TD_getUpgradePathData(%this, %path)
{
	if(!isObject(%script = %this.script))
		return "Error: Invalid script";

	%paths = %this.TD_UpgradePaths();
	if(getFieldCount(%paths) == 0)
		return "Error: None";

	for(%i = 0; %i < getFieldCount(%paths); %i++)
	{
		%path = getField(%paths, %i);
		//Finish path code
	}

	return %data;
}

function AIPlayer::TD_getUpgrade(%this, %path, %thing, %level, %debug)
{
	%level = mClampF(mFloor(%level), 0, $Server::TD_DefaultUpgrade[%path, "Max"]);
	if(%debug)
		talk("Default level set to " @ %level);

	%mode = $Server::TD_DefaultUpgrade["CostMode"];
	if(%debug)
		talk("Default CostMode set to " @ %mode);

	%value = $Server::TD_DefaultUpgrade[%path, %thing];
	if(%debug)
		talk("Value set to " @ %value);

	if(isObject(%script = %this.script))
	{
		if(%script.costMode !$= "")
		{
			%mode = mClampF(%mode, 0, 2);
			if(%debug)
				talk("CostMode set to " @ %mode);
		}

		if(%script.upgrade_[%path, %thing] !$= "")
		{
			%value = %script.upgrade_[%path, %thing];
			if(%debug)
				talk("Value set to " @ %value);
		}

		if(%thing $= "cost")
		{
			%value = %script.cost * ($Server::TD_DefaultUpgrade[%path, "Cost"] / 100);
			if(%debug)
				talk("Default NewCost set to " @ %value);

			if(%script.upgrade_[%path, %thing] !$= "")
			{
				%value = %script.cost * (%script.upgrade_[%path, %thing] / 100);
				if(%debug)
					talk("NewCost set to " @ %value);
			}
		}
	}

	if(%mode == 1 && %thing !$= "Cost")
	{
		%value *= %level;
		if(%debug)
			talk("NewCostValue set to " @ %value);
	}
	else if(%mode == 2)
	{
		%value *= %level;
		if(%debug)
			talk("NewValue set to " @ %value);
	}

	return mFloatLength(%value, 1);
}

function AIPlayer::TD_resetUpgrades(%this, %client)
{
	if(!isObject(%client))
		return;

	if(!isObject(%minigame = %client.minigame))
	{
		%client.chatMessage("You are not in a minigame!");
		return;
	}

	if(!isObject(%player = %client.player))
	{
		%client.chatMessage("You are dead!");
		return;
	}

	if(%player.getState() $= "dead")
	{
		%client.chatMessage("You are dead!");
		return;
	}

	if(!%this.isTowerBot)
	{
		%client.chatMessage("This is not a tower bot!");
		return;
	}

	if(!isObject(%script = %this.script))
	{
		%client.chatMessage("This tower isn't ran by a script!");
		%this.script = TowerGroup.findScript(%this.uiName);
		return;
	}

	if(%script.doNotUpgrade)
	{
		%client.chatMessage("This tower cannot be reset!");
		return;
	}

	if(%cost > 0)
	{
		if(%cost > %minigame.TD_Resources)
		{
			commandToClient(%client, 'MessageBoxOK', "TDUpgrade - Not enough", "You need more resources!");
			return;
		}

		%client.lastTDUpgradePrompt = $Sim::Time;
		%client.TempTDData["UpgradeBot"] = %this;
		commandToClient(%client, 'MessageBoxYesNo', "TDUpgrade - Confirm", "Are you sure you want to reset this tower?", 'TD_AcceptResetUpgrade');
	}
	else
		%client.chatMessage("Uh oh! The cost is very odd, please tell an administrator: " @ %cost);
}

function AIPlayer::TD_Upgrade(%this, %power, %client)
{
	if(!isObject(%client))
		return;

	if(!isObject(%minigame = %client.minigame))
	{
		%client.chatMessage("You are not in a minigame!");
		return;
	}

	if(!isObject(%player = %client.player))
	{
		%client.chatMessage("You are dead!");
		return;
	}

	if(%player.getState() $= "dead")
	{
		%client.chatMessage("You are dead!");
		return;
	}

	if(!%this.isTowerBot)
	{
		%client.chatMessage("This is not a tower bot!");
		return;
	}

	if(!isObject(%script = %this.script))
	{
		%client.chatMessage("This tower isn't ran by a script!");
		%this.script = TowerGroup.findScript(%this.uiName);
		return;
	}

	if(%script.doNotUpgrade)
	{
		%client.chatMessage("This tower cannot be upgraded!");
		return;
	}

	if(%this.TDData["Path"] $= "")
	{
		%list = %this.TD_UpgradePaths();
		for(%i = 0; %i < getFieldCount(%list); %i++)
		{
			%field = getField(%list, %i);

			if(%power $= %field)
			{
				%path = %field;
				%hasPath = true;
				break;
			}
		}

		if(!%hasPath)
			%client.chatMessage("Invalid path \"" @ %power @ "\"! Paths: " @ %this.TD_UpgradePathsString());
		else
		{
			%this.TDData["Path"] = %field;
			%this.TDData["UpgradeLevel"] = 0;

			%data = %this.TD_getUpgradePathData(%field);
			%client.chatMessage("\c6Path set: \c4" @ %field);
			%client.TD_Call("Send", "UpgradeGUI", "setField", 
				%field, 
				%this.maxUpgradePath[%field], 
				%this.TDData["UpgradeLevel"], //The client will do all the math
				%data); //See: AIPlayer::TD_getUpgradePathData()
		}
	}
	else
	{
		%power = %this.TDData["Path"];
		//If it exists we will be like yeah let's add
		if(!%script.doNotUpgrade)
		{
			%cost = %this.TD_getUpgrade(%power, "Cost");
			if(%cost > 0)
			{
				if(%cost > %minigame.TD_Resources)
				{
					commandToClient(%client, 'MessageBoxOK', "TDUpgrade - Not enough", "You need more resources!");
					return;
				}

				%client.lastTDUpgradePrompt = $Sim::Time;
				%client.TempTDData["UpgradeCost"] = %cost;
				%client.TempTDData["UpgradeBot"] = %this;
				commandToClient(%client, 'MessageBoxYesNo', "TDUpgrade - Confirm", "Are you sure you want to upgrade this tower?<br>Auto-Path: " @ strUpr(%power) @ "<br>Cost: $" @ %cost, 'TD_AcceptUpgrade');
			}
			else
				%client.chatMessage("Uh oh! The cost is very odd, please tell an administrator: " @ %cost);
		}
		else
		{
			//This is where the upgrades go if they go the standard way
			//%client.lastTDUpgradePrompt = $Sim::Time;
			%client.chatMessage("Sorry, upgrading on this tower is prohibited.");
		}
	}
}

function AIPlayer::TD_UpgradePaths(%this)
{
	if(isObject(%script = %this.script))
		%paths = %script.upgradePaths;
	else
		%paths = %this.upgradePaths;

	if(%paths $= "")
		%paths = "Damage, Health, Range, Speed";

	return strReplace(%paths, ", ", "" TAB "");
}

function AIPlayer::TD_UpgradePathsString(%this)
{
	if(isObject(%script = %this.script))
		%paths = %script.upgradePaths;
	else
		%paths = %this.upgradePaths;

	if(%paths $= "")
		%paths = "Damage, Health, Range, Speed";

	return %paths;
}

function AIPlayer::TD_getMaxLevel(%this, %path)
{
	%level = $Server::TD_DefaultUpgrade[%path, "Max"];
	if(isObject(%script = %this.script))
	{
		if(%script.upgrade_[%path, "Max"] !$= "")
			%level = %script.upgrade_[%path, "Max"];
	}

	return %level;
}
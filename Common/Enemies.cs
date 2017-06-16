$TD::FilePath_Enemys = "Add-Ons/Gamemode_Tower_Defense/Enemies/";

if(!isObject(EnemyGroup))
{
	new ScriptGroup(EnemyGroup)
	{
		class = EnemySO;
		filePath = "config/server/Enemys/";
	};
	EnemyGroup.schedule(1000, Load);
}
else
	EnemyGroup.schedule(1000, Load);

function EnemySO::findScript(%this, %EnemyName)
{
	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(%EnemyName == %obj)
			return %obj;
		else if(strPos(%obj.uiName, %EnemyName) >= 0)
			return %obj;
	}
	return -1;
}

/// <summary>
/// Loads the Enemy program.
/// </summary>
function EnemySO::Load(%this)
{
	//--PARAMETERS - READ CAREFULLY--\\
	
	//Description: This will display the current Enemy fields it can support.

	//--KEY--\\
	//	//_!	<- Required field, otherwise it will be confused
	//	//->	<- Normal
	//	//-?	<- Can depend on things
	//	-/- 	<- Comment, do not copy that part if you plan to

	//~MAIN~\\
	//_!	tree	string	- Where does this go to?
	//_!	description	string	- When people see the Enemy available, this is the description shown.
	//_!	cost	int	- How much does it cost?
	//->	adminLevel	int	- Admin level required to aquire
	//_!	height	int	- Enemy height
	//_!	canBuild	bool	- Can they build?

	//--YOU MAY USE FUNCTIONS, HERE IS HOW IT GOES--\\
	//	CMD function(args...);

	%this.deleteAll();
	$EnemyTrees = "";
	%path = $TD::FilePath_Enemys @ "*";

	registerEnemy("Blockhead", "Description Default class, has a gun." TAB 
		"weaponName Gun" TAB 
		"range 25" TAB 
		"tCloseHoldTime 0.2" TAB 
		"tCloseShootWaitTime 0.2" TAB 
		"holdTime 0.25" TAB 
		"shootWaitTime 2" TAB 
		"healthPerLevel 15" TAB
		"damageMultiplier 0.5" TAB 
		"defaultRound 1" TAB
		"defaultGame Easy" TAB
		"type Ballistic" TAB
		"health 100" TAB
		"customAvatar 1" TAB
		"Accent 0" TAB
		"AccentColor 0.500 0.500 0.500 1.000" TAB
		"Authentic 0" TAB
		"Chest 0" TAB
		"ChestColor 0.200 0.200 0.200 1.000" TAB
		"DecalColor 0" TAB
		"DecalName AAA-None" TAB
		"FaceName smiley" TAB
		"Hat 4" TAB
		"HatColor 0.200 0.200 0.200 1.000" TAB
		"HeadColor 1 0.878431 0.611765 1" TAB
		"Hip 0" TAB
		"HipColor 0.200 0.000 0.800 1.000" TAB
		"LArm 0" TAB
		"LArmColor 0.900 0.000 0.000 1.000" TAB
		"LHand 0" TAB
		"LHandColor 1 0.878431 0.611765 1" TAB
		"LLeg 0" TAB
		"LLegColor 0.200 0.000 0.800 1.000" TAB
		"Pack 0" TAB
		"PackColor 0.500 0.500 0.500 1.000" TAB
		"RArm 0" TAB
		"RArmColor 0.900 0.000 0.000 1.000" TAB
		"RHand 0" TAB
		"RHandColor 1 0.878431 0.611765 1" TAB
		"RLeg 0" TAB
		"RLegColor 0.200 0.000 0.800 1.000" TAB
		"SecondPack 0" TAB
		"SecondPackColor 0 1 0 1" TAB
		"TorsoColor 1 1 1 1");

	//announce("Loading saved files for mob classes. -> Path: " @ %path);
	%file = findFirstFile(%path);
	if(isFile(%file))
	{
		%fileExt = fileExt(%file);
		%name = fileBase(%file);
		if(%fileExt $= ".cs") //Just making sure
		{
			if(isObject(%obj = isRegisteredEnemy(fileBase(%path))))
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
			if(isObject(%obj = isRegisteredEnemy(fileBase(%path))))
				%obj.delete();

			exec(%file);
		}
	}
}

/// <summary>
/// When the Enemy is created, we need to format the variables, so we put it into a command, see -> registerEnemy
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function Enemy::onAdd(%this)
{
	EnemyGroup.add(%this);
	%this.parseCommand(%this.command);
}

function Enemy::getTreeParsed(%this)
{
	return strReplace(%this.tree, " ", "_");
}

/// <summary>
/// Parses Enemy objects' commands, see -> Enemy::onAdd
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function Enemy::parseCommand(%this, %com)
{
	//echo("       -> Parasing Enemy command line: " @ %this.uiName);
	//echo("       -> CommandLine: " @ %com);
	for(%i=0;%i<getFieldCount(%com);%i++)
	{
		%field = getField(%com, %i);
		%name = getWord(%field, 0);
		%value = collapseEscape(getWords(%field, 1, getWordCount(%field)-1));

		if(%name $= "tree")
		{
			for(%a=0;%a<getFieldCount($EnemyTrees);%a++)
				if(getField($EnemyTrees, %a) $= %value)
					%remove = 1;

			if(!%remove)
				$EnemyTrees = %value TAB $EnemyTrees;
		}

		//echo("         PARAMETER FOUND: " @ %cmd);
		//echo("             VALUE: " @ %value);
		%cmd = %this @ "." @ %name @ " = \"" @ %value @ "\";";
		//echo("             PARSED: " @ %cmd);
		eval(%cmd);
	}

	%this.command = "";
}

/// <summary>
/// Registers a Enemy into the EnemySO program. Easy to see
/// </summary>
/// <param name="name">Name of the created mob.</param>
/// <param name="parm">Parameters, each variable must be in a different field.</param>
function registerEnemy(%name, %parm)
{
	%strName = stripChars(%name, $TD::Chars);
	%strName = strReplace(%strName, " ", "_");
	%objName = "Enemy_" @ %strName;
	echo("Registering an Enemy.. - " @ %name @ " (" @ %objName @ ")");
	for(%i=0;%i<getFieldCount(%parm);%i++)
	{
		%field = getField(%parm,%i);
		%var = getWord(%field,0);
		%value = getWords(%field, 1, getWordCount(%field)-1);
		//echo("   PARAMETER FOUND: " @ %var);
		//echo("     VALUE: " @ %value);

		//for(%a=0;%a<getWordCount($City::EnemySO_RequiredFields);%a++)
		//{
		//	%requirement = getWord($City::EnemySO_RequiredFields,%a);
		//	if(%var $= %requirement && !%met_[%requirement])
		//	{
		//		%met_[%requirement] = 1;
		//		%metCount++;
		//	}
		//}
	}

	//if(%metCount < getWordCount($City::EnemySO_RequiredFields))
	//{
	//	warn(" - Unable to add the Enemy. Make sure you have made the parameters correctly.");
	//	warn(" - Requirement amount: " @ mFloor(%metCount) @ "/" @ getWordCount($City::EnemySO_RequiredFields));
	//	return;
	//}

	if(isObject(%objName))
	{
		warn("Warning: Enemy data \"" @ %objName @ "\" already exists. Overwriting.");
		%objName.delete();
	}

	%obj = new ScriptObject(%objName)
	{
		class = Enemy;
		uiName = %name;
		command = "chatColor <color:ffff00>" TAB collapseEscape(%parm);
		description = %description;
	};

	//if(isObject(%obj = EnemyGroup.findScript(%name)))
	//	%obj.save($City::FilePath_Enemys @ %obj.uiName @ ".cs");
}

/// <summary>
/// Returns whether the Enemy exists or not.
/// </summary>
/// <param name="Enemy">Enemy object or name.</param>
function isRegisteredEnemy(%Enemy)
{
	return EnemyGroup.findScript(%Enemy);
}

function serverCmdListEnemys(%this)
{
	//%this.chatMessage("Sorry this command doesn't exist, but it says hello to you.");
	%this.chatMessage(EnemyGroup.getCount() @ " Enemys detected.");
	for(%i = 0; %i < EnemyGroup.getCount(); %i++)
	{
		%Enemy = EnemyGroup.getObject(%i);
		%this.chatMessage(%Enemy.uiName @ " - " @ %Enemy.description);
	}
}
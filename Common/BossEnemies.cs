$TD::FilePath_BossEnemys = "Add-Ons/Gamemode_Tower_Defense/Bosses/";

if(!isObject(BossEnemyGroup))
{
	new ScriptGroup(BossEnemyGroup)
	{
		class = BossEnemySO;
		filePath = "config/server/BossEnemys/";
	};
	BossEnemyGroup.schedule(1000, Load);
}
else
	BossEnemyGroup.schedule(1000, Load);

function BossEnemySO::findScript(%this, %BossEnemyName)
{
	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(%BossEnemyName == %obj)
			return %obj;
		else if(strPos(%obj.uiName, %BossEnemyName) >= 0)
			return %obj;
	}
	return -1;
}

/// <summary>
/// Loads the BossEnemy program.
/// </summary>
function BossEnemySO::Load(%this)
{
	//--PARAMETERS - READ CAREFULLY--\\
	
	//Description: This will display the current BossEnemy fields it can support.

	//--KEY--\\
	//	//_!	<- Required field, otherwise it will be confused
	//	//->	<- Normal
	//	//-?	<- Can depend on things
	//	-/- 	<- Comment, do not copy that part if you plan to

	//~MAIN~\\
	//_!	tree	string	- Where does this go to?
	//_!	description	string	- When people see the BossEnemy available, this is the description shown.
	//_!	cost	int	- How much does it cost?
	//->	adminLevel	int	- Admin level required to aquire
	//_!	height	int	- BossEnemy height
	//_!	canBuild	bool	- Can they build?

	//--YOU MAY USE FUNCTIONS, HERE IS HOW IT GOES--\\
	//	CMD function(args...);

	%this.deleteAll();
	$BossEnemyTrees = "";
	%path = $TD::FilePath_BossEnemys @ "*";

	//Sorry, you have to make your own bosses.

	//announce("Loading saved files for mob classes. -> Path: " @ %path);
	%file = findFirstFile(%path);
	if(isFile(%file))
	{
		%fileExt = fileExt(%file);
		%name = fileBase(%file);
		if(%fileExt $= ".cs") //Just making sure
		{
			if(isObject(%obj = isRegisteredBossEnemy(fileBase(%path))))
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
			if(isObject(%obj = isRegisteredBossEnemy(fileBase(%path))))
				%obj.delete();

			exec(%file);
		}
	}
}

/// <summary>
/// When the BossEnemy is created, we need to format the variables, so we put it into a command, see -> registerBossEnemy
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function BossEnemy::onAdd(%this)
{
	BossEnemyGroup.add(%this);
	%this.parseCommand(%this.command);
}

function BossEnemy::getTreeParsed(%this)
{
	return strReplace(%this.tree, " ", "_");
}

/// <summary>
/// Parses BossEnemy objects' commands, see -> BossEnemy::onAdd
/// Do not use this function.
/// </summary>
/// <param name="this">Name of the created mob.</param>
/// <param name="com">Parameters, each variable must be in a different field.</param>
function BossEnemy::parseCommand(%this, %com)
{
	//echo("       -> Parasing BossEnemy command line: " @ %this.uiName);
	//echo("       -> CommandLine: " @ %com);
	for(%i=0;%i<getFieldCount(%com);%i++)
	{
		%field = getField(%com, %i);
		%name = getWord(%field, 0);
		%value = collapseEscape(getWords(%field, 1, getWordCount(%field)-1));

		if(%name $= "tree")
		{
			for(%a=0;%a<getFieldCount($BossEnemyTrees);%a++)
				if(getField($BossEnemyTrees, %a) $= %value)
					%remove = 1;

			if(!%remove)
				$BossEnemyTrees = %value TAB $BossEnemyTrees;
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
/// Registers a BossEnemy into the BossEnemySO program. Easy to see
/// </summary>
/// <param name="name">Name of the created mob.</param>
/// <param name="parm">Parameters, each variable must be in a different field.</param>
function registerBossEnemy(%name, %parm)
{
	%strName = stripChars(%name, $TD::Chars);
	%strName = strReplace(%strName, " ", "_");
	%objName = "BossEnemy_" @ %strName;
	echo("Registering an BossEnemy.. - " @ %name @ " (" @ %objName @ ")");
	for(%i=0;%i<getFieldCount(%parm);%i++)
	{
		%field = getField(%parm,%i);
		%var = getWord(%field,0);
		%value = getWords(%field, 1, getWordCount(%field)-1);
		//echo("   PARAMETER FOUND: " @ %var);
		//echo("     VALUE: " @ %value);

		//for(%a=0;%a<getWordCount($City::BossEnemySO_RequiredFields);%a++)
		//{
		//	%requirement = getWord($City::BossEnemySO_RequiredFields,%a);
		//	if(%var $= %requirement && !%met_[%requirement])
		//	{
		//		%met_[%requirement] = 1;
		//		%metCount++;
		//	}
		//}
	}

	//if(%metCount < getWordCount($City::BossEnemySO_RequiredFields))
	//{
	//	warn(" - Unable to add the BossEnemy. Make sure you have made the parameters correctly.");
	//	warn(" - Requirement amount: " @ mFloor(%metCount) @ "/" @ getWordCount($City::BossEnemySO_RequiredFields));
	//	return;
	//}

	if(isObject(%objName))
	{
		warn("Warning: BossEnemy data \"" @ %objName @ "\" already exists. Overwriting.");
		%objName.delete();
	}

	%obj = new ScriptObject(%objName)
	{
		class = BossEnemy;
		uiName = %name;
		command = "chatColor <color:ffff00>" TAB collapseEscape(%parm);
		description = %description;
	};

	//if(isObject(%obj = BossEnemyGroup.findScript(%name)))
	//	%obj.save($City::FilePath_BossEnemys @ %obj.uiName @ ".cs");
}

/// <summary>
/// Returns whether the BossEnemy exists or not.
/// </summary>
/// <param name="BossEnemy">BossEnemy object or name.</param>
function isRegisteredBossEnemy(%BossEnemy)
{
	return BossEnemyGroup.findScript(%BossEnemy);
}

function serverCmdListBossEnemys(%this)
{
	//%this.chatMessage("Sorry this command doesn't exist, but it says hello to you.");
	%this.chatMessage(BossEnemyGroup.getCount() @ " BossEnemys detected.");
	for(%i = 0; %i < BossEnemyGroup.getCount(); %i++)
	{
		%BossEnemy = BossEnemyGroup.getObject(%i);
		%this.chatMessage(%BossEnemy.uiName @ " - " @ %BossEnemy.description);
	}
}
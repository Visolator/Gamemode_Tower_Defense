if($Pref::Server::TD_FontSize <= 0)
	$Pref::Server::TD_FontSize = 22;

if($Pref::Server::TD_MaxQueue <= 0)
	$Pref::Server::TD_MaxQueue = 4;

$Server::TD_Weapon["Default"] = "bowItem";

$Server::TD_Weapon["Easy", 0] = "ShoopLazerItem";
$Server::TD_Weapon["Easy", 1] = "RainbowizerItem";
$Server::TD_Weapon["Easy", 2] = "HotShotLasgunItem";
$Server::TD_Weapon["Easy", 3] = "BioRifleItem";

$Server::TD_Weapon["Medium", 0] = "AutoPistolItem";
$Server::TD_Weapon["Medium", 1] = "PlasmagunItem";
$Server::TD_Weapon["Medium", 2] = "ChainswordItem";
$Server::TD_Weapon["Medium", 3] = "SwordItem";

$Server::TD_Weapon["Hard", 0] = "LaspistolItem";
$Server::TD_Weapon["Hard", 1] = "LaspistolItem";
$Server::TD_Weapon["Hard", 2] = "StubgunItem";
$Server::TD_Weapon["Hard", 3] = "StubgunItem";

$Server::TD_Weapon["Expert", 0] = "chainsawItem";
$Server::TD_Weapon["Expert", 1] = "chainsawItem";
$Server::TD_Weapon["Expert", 2] = "StubgunItem";
$Server::TD_Weapon["Expert", 3] = "StubgunItem";

$Server::TD_Weapon["Nightmare", 0] = "BoltpistolItem";
$Server::TD_Weapon["Nightmare", 1] = "StubgunItem";
$Server::TD_Weapon["Nightmare", 2] = "StubgunItem";
$Server::TD_Weapon["Nightmare", 3] = "StubgunItem";

function GameConnection::TD_AddItems(%this)
{
	if(!isObject(%minigame = %this.minigame))
		return;

	if(!isObject(%player = %this.player))
		return;

	if(%this.isBuilder)
	{
		%player.clearTools();
		//%player.addNewItem("Hammer");
		//%player.addNewItem("TD Hammer");
		%player.addNewItem("Sword");
		%player.addNewItem("Laspistol");
		%player.addNewItem("Tower Healer");
	}
	else
	{
		%player.clearTools();
		%num = 0;

		if(%minigame.numMembers >= 3)
			%num = 1;
		if(%minigame.numMembers >= 8)
			%num = 2;
		if(%minigame.numMembers >= 14)
			%num = 3;

		%tool = $Server::TD_Weapon[%minigame.TD_Difficult, %num];
		if(!isObject(%tool))
			%tool = $Server::TD_Weapon["Default"];

		%player.addNewItem(%tool);
		%player.addNewItem("Tower Healer");
	}
}

function MinigameSO::TD_Loop(%this)
{
	cancel(%this.TD_Loop);
	if(%this.isChangingRound)
		return;

	if($Sim::Time - %this.lastReset > 10 && !%this.giveWeapons)
	{
		%this.giveWeapons = 1;
		for(%i = 0; %i < %this.numMembers; %i++)
		{
			%member = %this.member[%i];
			if(isObject(%member) &&  %member.getClassName() $= "GameConnection")
				%member.TD_AddItems();
		}
	}

	if($Sim::Time - %this.lastReset > 30 && !isEventPending(%this.spawnRound))
	{
		if(%this.builderCount <= 0)
		{
			if(!%this.failed)
			{
				%this.failed = 1;
				%this.scheduleReset();
				TD_CheckRecord(%this.TD_Difficult, %this.TD_Round);
			}
		}
		else
		{
			%builders = 0;
			if(%this.numMembers > 0)
			{
				for(%i = 0; %i < %this.numMembers; %i++)
				{
					%member = %this.member[%i];
					if(isObject(%member) &&  %member.getClassName() $= "GameConnection")
						if(%member.isBuilder)
							%builders++;
				}
			}

			if(%this.lastBuilders > 0 && %builders != %this.lastBuilders)
				%this.messageAll('MsgAdminForce', "\c3" @ %builders @ " \c5builder" @ (%builders == 1 ? "" : "s") @ " left!");

			%this.builderCount = %builders;
			%this.lastBuilders = %builders;
		}

		%spawn = FindBrick_Object("Start");
		if(isObject(%spawn))
		{
			%location = %spawn.getTopPosition();
			%this.TD_SpawnBot(%location, %spawn);
		}
	}

	%this.TD_Loop = %this.schedule(100, TD_Loop);
}

if(!strLen($Pref::Server::TD_MaxRounds))
	$Pref::Server::TD_MaxRounds = 1001;

function TD_CheckRecord(%difficult, %round)
{
	%round = mFloor(%round);
	if(%round > $Pref::Server::TD_Record["Round"])
	{
		$Pref::Server::TD_Record["Round"] = %round;
		$Pref::Server::TD_Record["Difficult"] = strUpr(%difficult);
		messageAll('MsgAdminForce', "\c6NEW TOWER DEFENSE RECORD! Round " @ %round @ " survived on " @ strUpr(%difficult));
		schedule(500, 0, messageAll, 'MsgAdminForce');
		schedule(1000, 0, messageAll, 'MsgAdminForce');
	}
}

function MinigameSO::TD_ScheduleSpawnRound(%this, %time, %round)
{
	cancel(%this.spawnRound);
	if(TowerDefenseBotGroup.getLiving() + %this.botQueue > 0)
		return;

	if(%round >= $Pref::Server::TD_MaxRounds)
	{
		if(!%this.message)
		{
			%this.message = 1;
			%this.messageAll('MsgClearBricks', "<font:arial:24>\c6Round \c4" @ %round-1 @ " \c6ended! Max round has been achieved, resetting.");
			TD_CheckRecord(%this.TD_Difficult, %this.TD_Round);
		}
		%this.reset();
		return;
	}

	%this.isChangingRound = 1;
	%amt = mFloatLength(mClampF(80 * ((%this.TD_Difficult_Give / 2) + ((%round-1) * %this.TD_Difficult_Give)), 0, %this.TD_Difficult_Give * 600), 2);
	if(%amt > 0)
	{
		%winMsg = " (+\c4$" @ %amt @ "\c6)";
		%this.TD_AddResources(%amt);
	}
	%this.messageAll('MsgUploadStart', "<font:arial:24>\c6Round \c4" @ %round-1 @ " \c6ended" @ %winMsg @ "! Next round starts in 10 seconds.");
	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%member = %this.member[%i];
		if(isObject(%member) && %member.getClassName() $= "GameConnection")
		{
			if(isObject(%pl = %member.player))
			{
				%pl.addHealth(%pl.getMaxHealth());
			}
			else
			{
				if(%this.isSlayerMinigame && %this.lives > 0)
				{
					%member.setDead(0);
					%member.setLives(1);
				}

				%member.spawnPlayer();
				%member.setControlObject(%member.player);
				%member.chatMessage("<font:impact:20>\c5Sorry, looks like you cannot build towers this round. Instead, help fight for points to get player perks! (Coming soon)");
				%member.centerPrint("<font:impact:20>\c5Sorry, looks like you cannot build towers this round.<br>Help fight with your weapon!", 5);
			}
		}
	}
	
	for(%i=0;%i<TowerDefenseGroup.getCount();%i++)
		TowerDefenseGroup.getObject(%i).setHealth(TowerDefenseGroup.getObject(%i).getMaxHealth());
	%this.spawnRound = %this.schedule(%time, TD_SpawnRound, %round);
}

function TD_TowerCount()
{
	%count = 0;
	for(%i=0;%i<getWordCount($TD::Towers);%i++)
		%count += isObject(getWord($TD::Towers, %i));
	return %count;
}

//Tower defense stuff

function MinigameSO::TD_SpawnRound(%this, %round)
{
	cancel(%this.spawnRound);
	if(TowerDefenseBotGroup.getCount() + %this.botQueue > 0)
		return;

	%TD_DiffID = %this.TD_DifficultID;

	%overwriteTD = -1;
	for(%i = 0; %i < EnemyGroup.getCount(); %i++)
	{
		%enemy = EnemyGroup.getObject(%i);
		%TD_eDiffID = %enemy.defaultGame;

		switch$(%TD_eDiffID)
		{
			case "Easy":
				%TD_eDiffID = 0;

			case "Medium":
				%TD_eDiffID = 1;

			case "Hard":
				%TD_eDiffID = 2;

			case "Expert":
				%TD_eDiffID = 3;

			case "Nightmare":
				%TD_eDiffID = 4;

			default:
				%TD_eDiffID = -1;				
		}

		if(%this.TD_Round >= %enemy.defaultRound && %enemy.defaultRound >= 1 && %enemy.defaultRound > %overwriteRound)
		{
			%overwriteRound = %enemy.defaultRound;
			%defaultBot = %enemy;
		}

		if(%TD_eDiffID >= 0)
			if(%TD_eDiffID > %overwriteTD && %TD_eDiffID <= %TD_DiffID)
			{
				%overwriteTD = %TD_eDiffID;
				%defaultGameBot = %enemy;
			}
	}

	if(%overwriteTD > %overwriteRound)
		%defaultBot = 0;

	if(isObject(%defaultGameBot) && !isObject(%defaultBot))
		%defaultBot = %defaultGameBot;

	if(isObject(%defaultBot) && nameToID(%defaultBot) != nameToID(%this.TD_DefaultBot))
	{
		%this.TD_DefaultBot = %defaultBot;
		if(%this.TD_Round > 1)
		{
			if(isObject(%defaultGameBot))
				%this.messageAll('', "<font:arial:24>\c5Warning\c6: (GameBot) New default bot has been created. Rounds \c5" @ %this.TD_Round @ "+ \c6now use this enemy: \c3" @ %defaultBot.uiName);
			else
				%this.messageAll('', "<font:arial:24>\c5Warning\c6: New default bot has been created. Rounds \c5" @ %this.TD_Round @ "+ \c6now use this enemy: \c3" @ %defaultBot.uiName);
		}
	}

	%this.TD_Round = mClampF(%round, 0, 10000);
	%this.oldQueue += getRandom(1, 3);
	%this.botQueue = %this.oldQueue;
	%this.isBossRound = 0;
	%this.bossQueue = 0;

	//Before we start a boss round let's make sure one exists.
	for(%i = 0; %i < BossEnemyGroup.getCount(); %i++)
	{
		%bossObj = BossEnemyGroup.getObject(%i);
		if(%this.TD_Round >= %bossObj.startRound)
		{
			%foundABoss = 1;
			break; //We don't need to keep looping to find it, we just need to know.
		}
	}

	if((%this.botQueue / 5 == mFloor(%this.botQueue / 5) || %this.TD_Round - %this.lastBossRound > getRandom(6, 8) || %this.TD_ScheduleBossRound) && %foundABoss)
	{
		%this.TD_ScheduleBossRound = 0;
		%this.isBossRound = 1;
		%this.lastBossRound = %round;
		%this.bossQueue = mFloor(%this.botQueue / 5);
		%this.botQueue = mFloor(%this.botQueue / 4);
		%this.messageAll('MsgUploadEnd', "<font:arial:24>\c6Boss round \c0" @ %round @ " \c6has begun!");
		%this.TD_DisplayHint();
	}
	else
		%this.messageAll('MsgUploadEnd', "<font:arial:24>\c6Round \c4" @ %round @ " \c6has begun!");

	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%member = %this.member[%i];
		if(isObject(%member) &&  %member.getClassName() $= "GameConnection")
			%member.TD_AddItems();
	}

	%this.isChangingRound = 0;
	%this.TD_Loop();
}

function MinigameSO::TD_DisplayHint(%this)
{
	%font = "<font:arial:" @ $Pref::Server::TD_FontSize @ ">";
	%tip = "\c6[\c7TIP\c6]: ";
	%hint = "\c6[\c2HINT\c6]: ";

	%r = getRandom(0, 4);
	switch(%r)
	{
		case 0:
			%this.messageAll('', %font @ %hint @ "Towers have weaknesses, and so do the enemies. Don't underestimate/overestimate your enemy.");

		case 1:
			%this.messageAll('', %font @ %tip @ "Don't plant the same towers, chances will make your team lose faster.");

		case 2:
			%this.messageAll('', %font @ %hint @ "Towers can create accidents if you get too close to them.");

		default:
			%this.messageAll('', %font @ %tip @ "Are you confused on how Tower Defense works? Type in your chat \c4/TD");
	}
}

function MinigameSO::TD_Chance(%this, %enemy)
{
	if(!isObject(%this))
		return;

	if(!isObject(%script = EnemyGroup.findScript(%enemy)))
		return -1;

	if(%this.TD_Round < %script.startRound)
		return 0;

	return 100 / mClampF(mFloor(%script.rarity - ((%this.TD_Round - %script.startRound) / 2)), 2, 100);
}

$Server::TD_TooClose = 10;

function MiniGameSO::TD_SpawnBot(%this, %pos, %spawnBrick, %forcedTower)
{
	if(!isObject(%aa = EnemyGroup.findScript(%forcedTower)))
	{
		if(TowerDefenseBotGroup.getLiving() > 20)
			return -1;

		if(%this.botQueue <= 0 && !isObject(%aa))
		{
			if(TowerDefenseBotGroup.getLiving() <= 0)
				%this.TD_ScheduleSpawnRound(10 * 1000, %this.TD_Round + 1);

			return 0;
		}

		if($Sim::Time - %this.lastBotSpawn < 0.5)
			return;
	}

	%this.lastBotSpawn = $Sim::Time;

	%enemyScript = EnemyGroup.findScript(%this.TD_DefaultBot);
	if(%this.isBossRound && %this.bossQueue > 0 && !isObject(%aa))
	{
		if(isObject(%newScript = BossEnemyGroup.getObject(getRandom(0, BossEnemyGroup.getCount()-1))))
		{
			if(%this.TD_Round >= %newScript.startRound)
			{
				%enemyScript = %newScript;
				%foundBoss = 1;
			}
		}
	}

	if(!%foundBoss && !isObject(%aa))
	{
		%rarityPerRound = mClampF(%newScript.rarityPerRound * (%this.TD_Round - %newScript.startRound), 1, 100);
		%maxRarity = (%newScript.maxRarity > 0 ? mClampF(%newScript.maxRarity, 1, 100) : 100);
		%rarity = mClampF(100 - %newScript.rarityPer * %rarityPerRound, 1, %maxRarity);
		%isRarity = (getRandom(1, %rarity) == 1);

		if(%newScript.rarityPer <= 0)
			%isRarity = (getRandom(1, mClampF(mClampF(mFloor(%newScript.rarity - ((%this.TD_Round - %newScript.startRound) / 2)), 2, 100), mFloor(%newScript.minRarity), 100)) == 1);

		if(isObject(%newScript = EnemyGroup.getObject(getRandom(0, EnemyGroup.getCount()-1))))
		{
			if(%this.TD_Round >= %newScript.startRound && %isRarity)
			{
				if(%enemyScript != %newScript)
					%enemyScript = %newScript;
			}
		}
	}

	if(isObject(%aa))
		%enemyScript = %aa;

	%this.botQueue--;
	if(%this.bossQueue > 0 && %foundBoss)
		%this.bossQueue--;

	%round = mFloor(%this.TD_Round);
	%health = mClampF(%enemyScript.health * %this.TD_EnemyHealthMultiplier + (%enemyScript.healthPerLevel * (%round-1)) * %this.TD_EnemyHealthMultiplier, 0.1, 500000);
	
	%bot = new AIPlayer()
	{
		position = %pos;
		//spawnBrick = %spawnBrick;
		dataBlock = PlayerNoJet;
		isEnemyBot = 1;
		friendlyfire = 1;
		minigame = %this;
		range = mClampF(%enemyScript.range, 10, 1000);
		tooClose = mClampF($Server::TD_TooClose, 0, 1000);
		holdTime = mClampF(%enemyScript.holdTime, 0.1, 9999);
		shootWaitTime = mClampF(%enemyScript.shootWaitTime, 0.1, 1000);
		tCloseShootWaitTime = mClampF(%enemyScript.tCloseShootWaitTime, 0.1, 9999);
		tCloseHoldTime = mClampF(%enemyScript.tCloseHoldTime, 0.1, 1000);
		isTerrorist = mClampF(%enemyScript.isTerrorist, 0, 1);
		playersOnly = mClampF(%enemyScript.playersOnly, 0, 1);
		uiName = %enemyScript.uiName;
		name = %enemyScript.uiName;
		hName = %enemyScript.uiName;
		damageMultiplier = %enemyScript.damageMultiplier;
		spawnTime = $Sim::Time;
		isStuck = 0;
		isBot = 1; //Enable minigame damage
		immunities = %enemyScript.immunities;
		impulseImmune = %enemyScript.impulseImmune;
		script = nameToID(%enemyScript);
	};
	%bot.player = %bot;

	if(%enemyScript.customAvatar)
	{
		%bot.llegColor = %enemyScript.llegColor;
		%bot.secondPackColor = %enemyScript.secondPackColor;
		%bot.lhand = %enemyScript.lhand;
		%bot.hip = %enemyScript.hip;
		%bot.faceName = %enemyScript.faceName;
		%bot.rarmColor = %enemyScript.rarmColor;
		%bot.hatColor = %enemyScript.hatColor;
		%bot.hipColor = %enemyScript.hipColor;
		%bot.chest = %enemyScript.chest;
		%bot.rarm = %enemyScript.rarm;
		%bot.packColor = %enemyScript.packColor;
		%bot.pack = %enemyScript.pack;
		%bot.decalName = %enemyScript.decalName;
		%bot.larmColor = %enemyScript.larmColor;
		%bot.secondPack = %enemyScript.secondPack;
		%bot.larm = %enemyScript.larm;
		%bot.chestColor = %enemyScript.chestColor;
		%bot.accentColor = %enemyScript.accentColor;
		%bot.rhandColor = %enemyScript.rhandColor;
		%bot.rleg = %enemyScript.rleg;
		%bot.rlegColor = %enemyScript.rlegColor;
		%bot.accent = %enemyScript.accent;
		%bot.headColor = %enemyScript.headColor;
		%bot.rhand = %enemyScript.rhand;
		%bot.lleg = %enemyScript.lleg;
		%bot.lhandColor = %enemyScript.lhandColor;
		%bot.hat = %enemyScript.hat;

		GameConnection::ApplyBodyParts(%bot);
		GameConnection::ApplyBodyColors(%bot);
	}
	else
		applyDefaultCharacterPrefs(%bot);

	%bot.pathNodes = %this.pathNodes;
	TowerDefenseBotGroup.add(%bot);
	if(%bot.isTerrorist)
	{
		%bot.setPlayerShapeName("SUICIDE BOMBER");
		%bot.setShapeNameDistance(50);
		%bot.setShapeNameColor("1 0 0 1");
	}
	%bot.addvelocity(vectorAdd(vectorScale(%bot.getForwardVector(), 5), "0 0 10"));

	if(isObject(%datablock = findPlayertypeByName(%enemyScript.datablockName)))
		%bot.setDatablock(%datablock);

	if(isObject(%wep = findItemByName(%enemyScript.weaponName)))
		%bot.setWeapon(%wep);

	%bot.setMaxHealth(%health);
	%bot.setMoveTolerance(0);
	%bot.setMoveSlowdown(0);
	%bot.lastStuck = $Sim::Time - 1;
	%bot.setInvulnerbilityTime(5);
	%bot.FindPath();
	%bot.TD_EnemyLoop();
	if((%speed = %enemyScript.speedFactor) > 0)
		%bot.setSpeedFactor(%speed);

	if((%scale = %enemyScript.scale) > 0)
		%bot.schedule(100, setScale, %scale SPC %scale SPC %scale);

	return 1;
}

function serverCmdGiveTerrorist(%this, %name)
{
	if(!isObject(%name = findClientByName(%name)))
		return;
	if(!isObject(%player = %name.player))
		return;
	if(!isObject(%mini = %this.minigame))
		return;
	%this.chatMessage("You gave someone a terrorist.");
	%name.chatMessage("You have gotten a surprise!");
	%mini.schedule(3000, TD_SpawnBot, %player.getPosition(), FindBrick_Object("Start"), "Terrorist");
}

function MinigameSO::TD_SetRound(%this, %round)
{
	cancel(%this.resetSch);
	cancel(%this.spawnRound);
	TowerDefenseBotGroup.deleteAll();
	%this.TD_ScheduleSpawnRound(10, %round);
}

function MinigameSO::TD_Pick(%this)
{
	cancel(%this.resetSch);
	cancel(%this.spawnRound);
	if($Server::MapChanger::Changing)
		return;

	%count = mClampF(mFloor(1 + %this.numMembers / 3), 1, %this.numMembers);
	%this.noHealers = 0;
	%this.TD_DifficultColor = "\c6";
	%this.TD_Difficult = $Pref::Server::TD_Difficult;
	switch$(%this.TD_Difficult)
	{
		case "Easy":
			%this.TD_DifficultID = 0;
			%this.TD_Difficult = "Easy";
			%this.TD_DifficultColor = "\c2";
			%this.TD_Difficult_Give = 1;
			%this.TD_Difficult_Earn = 1;
			%this.TD_EnemyHealthMultiplier = 1;
			%this.TD_CostMultiplier = 0.8;

		case "Medium":
			%this.TD_DifficultID = 1;
			%this.TD_Difficult = "Medium";
			%this.TD_DifficultColor = "\c3";
			%this.TD_Difficult_Give = 1;
			%this.TD_Difficult_Earn = 0.9;
			%this.TD_EnemyHealthMultiplier = 1;
			%this.TD_CostMultiplier = 1;

		case "Hard":
			%this.TD_DifficultID = 2;
			%this.TD_Difficult = "Hard";
			%this.TD_DifficultColor = "<color:FFA500>";
			%this.TD_Difficult_Give = 0.6;
			%this.TD_Difficult_Earn = 0.75;
			%this.TD_EnemyHealthMultiplier = 1.1;
			%this.TD_CostMultiplier = 1.2;

		case "Expert":
			%this.TD_DifficultID = 3;
			%this.TD_Difficult = "Expert";
			%this.TD_DifficultColor = "\c0";
			%this.TD_Difficult_Give = 0.5;
			%this.TD_Difficult_Earn = 0.65;
			%this.TD_EnemyHealthMultiplier = 1.25;
			%this.TD_CostMultiplier = 1.3;
			%this.noHealers = 1;

		case "Nightmare":
			%this.TD_DifficultID = 4;
			%this.TD_Difficult = "Nightmare";
			%this.TD_DifficultColor = "\c5";
			%this.TD_Difficult_Give = 0.4;
			%this.TD_Difficult_Earn = 0.5;
			%this.TD_EnemyHealthMultiplier = 1.4;
			%this.TD_CostMultiplier = 1.5;
			%this.noHealers = 1;

		default:
			%this.TD_DifficultID = 0;
			%this.TD_Difficult = "Easy";
			%this.TD_DifficultColor = "\c2";
			%this.TD_Difficult_Give = 1;
			%this.TD_Difficult_Earn = 1;
			%this.TD_EnemyHealthMultiplier = 1;
			%this.TD_CostMultiplier = 0.8;
	}

	if(isObject(%this.TD_Path))
		%this.TD_Path.delete();

	%this.TD_Path = -1;
	%this.pathNodes = -1;
	%this.builderCount = %count;
	%this.TD_Resources = 1000 * %this.TD_Difficult_Earn;
	%this.TD_Round = 1;
	TowerDefenseBotGroup.deleteAll();

	%this.builderCount = 0;
	if(%this.numMembers > 0)
	{
		for(%i = 0; %i < %this.numMembers; %i++)
		{
			%member = %this.member[%i];
			if(isObject(%member) && %member.getClassName() $= "GameConnection")
			{
				%member.isBuilder = 0;
				%member.canBuild = 0;
				%member.setTower("None");
			}
		}
	}
	//%this.messageAll('MsgUploadStart', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c6Creating a bot path...");
	%this.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c6Building list has been created!" @ ($Pref::Server::TD_PickMode == 1 ? " Want to be in the queue? \c6Type \c2/td queue" : ""));
	switch($Pref::Server::TD_PickMode)
	{
		case 0:
			%count = mClampF(%this.numMembers/4, 1, 4);

			for(%i = 0; %i < %count; %i++)
				%this.TD_PickRandom(1);

		case 1:
			%queueObj = TD_getQueue();
			%setQueue = mClampF(%this.numMembers/5, 1, $Pref::Server::TD_MaxQueue);
			%queueLeft = %setQueue;

			if(TD_getQueue().getCount() > 0)
			{
				while(%queueObj.getCount() > 0 && %queueLeft > 0)
				{
					%queueClient = TD_QueueSO_pickNext(%queueObj);

					%this.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">  - \c4" @ %queueClient.getPlayerName() @ " \c6has been choosen \c6(\c4QUEUE\c6)");
					if(!isObject(%queuePlayer = %queueClient.player))
						if(!isObject(%queuePlayer = %queueClient.createPlayer(%queueClient.getSpawnPoint())))
							continue;

					%queueClient.chatMessage("<font:arial:24>\c6You have been choosen to be a builder! Look around and use your plant key.");
					%queueClient.canUpgradeTowers = 0;
					%queueClient.setTower("Default");
					%queueClient.isBuilder = 1;
					%queueClient.isTowerBot = 1;
					%queueClient.canBuild = 1;
					%queueClient.TD_AddItems();
					%queuePlayer.setShapeNameColor("0 0.5 1 1");
					%this.builderCount++;
					%queueLeft--;
				}
			}
			else
				%this.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c4TD_QueueSO \c6- Nobody is in the queue. Randomly selecting a player. To be in the queue, please do \c2/td queue");

			if(%setQueue - %queueLeft <= 0)
				%this.TD_PickRandom(1);

		default:
			if(%this.numMembers > 0)
			{
				for(%i = 0; %i < %this.numMembers; %i++)
				{
					%member = %this.member[%i];
					if(isObject(%member) && %member.getClassName() $= "GameConnection")
					{
						%member.chatMessage("\c6You can build towers! Use your building keys to switch towers.");
						%member.canUpgradeTowers = 0;
						%member.isBuilder = 1;
						%member.canBuild = 1;
						%member.isTowerBot = 1;
						%this.builderCount++;
					}
				}
			}
	}

	if(isObject(getNTBrick("StartNode", 0)) && isObject(getNTBrick("EndNode", 0)))
	{
		$tempMinigame = %this;
		cancel(TD_PathHandler.genPathSch);
		%this.TD_Path = TD_PathHandler.generatePath(getNTBrick("StartNode", 0), getNTBrick("EndNode", 0), "TD_OnPathComplete");
	}
}

function TD_OnPathComplete(%path, %result)
{
	if(!isObject($tempMinigame))
		return;

	cancel($tempMinigame.spawnRound);
	cancel(TD_PathHandler.genPathSch);

	if(%result $= "")
	{
		%result = %path;
		%path = -1;
	}

	$tempMinigame.TD_Path = %path;
	$tempMinigame.pathNodes = %result;
	//talk("Path: " @ %result);

	if(isObject(TowerDefenseBotGroup))
		TowerDefenseBotGroup.deleteAll();

	//$tempMinigame.messageAll('MsgUploadEnd', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c6Game is now set! Starting in 30 seconds. Difficult set to " @ strUpr($tempMinigame.TD_Difficult));

	if($tempMinigame.numMembers > 0)
	{
		for(%i = 0; %i < $tempMinigame.numMembers; %i++)
		{
			%member = $tempMinigame.member[%i];
			if(isObject(%member) && %member.getClassName() $= "GameConnection" && !%member.isBuilder)
			{
				%member.chatMessage("<font:impact:20>\c6Sorry, looks like you cannot build towers this round.");
				%member.centerPrint("<font:impact:20>\c6Sorry, looks like you cannot build towers this round.", 5);
				%queueObj = TD_getQueue();
				if(%queueObj.isMember(%member))
				{
					messageClient(%member, 'MsgUploadEnd', "\c7[\c4Tower Defense\c7] \c6You are now in \c3#" @ TD_QueueSO_getQueueNum(%queueObj, %this) @ "\c6 of the queue. \c3You are allowed to upgrade any towers.");
					%member.canUpgradeTowers = 1;
				}
				else
				{
					messageClient(%member, 'MsgUploadEnd', "\c7[\c4Tower Defense\c7] \c3Want to be in the queue? \c3Type \c2/td queue");
					%member.canUpgradeTowers = 0;
				}
			}
		}
	}
	$tempMinigame.spawnRound = $tempMinigame.schedule(30 * 1000, TD_SpawnRound, 1);
	$tempMinigame.TD_DefaultBot = "";
	$tempMinigame.TD_Loop();
}

function MinigameSO::TD_PickRandom(%this, %reset)
{
	cancel(%this.randomSch);

	%memberCount = %this.numMembers;
	if(%memberCount <= 0)
	{
		%this.randomSch = %this.schedule(3000, TD_PickRandom);
		return;
	}

	if(%reset)
		%this.tryCount = 0;
	else
		%this.tryCount++;

	%member = %this.member[getRandom(0, %memberCount-1)];
	%this.lastPickedMember = %member;

	if(%this.tryCount > 40)
		return -1;

	if(!isObject(%member))
		return %this.TD_PickRandom();

	if(%member.getClassName() !$= "GameConnection")
		return %this.TD_PickRandom();

	if(%member.isBuilder)
	{
		//talk("TD_PickRandom() - Existing builder.");
		return %this.TD_PickRandom();
	}

	if(!isObject(%player = %member.player))
	{
		//talk("TD_PickRandom() - No player.");
		return %this.TD_PickRandom();
	}

	if($Server::TD_Revoke[%member.getBLID()] && %memberCount > 1)
	{
		//talk("TD_PickRandom() - Revoked.");
		return %this.TD_PickRandom();
	}

	if($Sim::Time - %player.AntiMoveTime > 5 && %player.AntiMoveTime > 0 && %memberCount > 1)
	{
		//talk("TD_PickRandom() - They haven't moved.");
		return %this.TD_PickRandom();
	}

	if(!isObject(%member.minigame))
	{
		//talk("TD_PickRandom() - No minigame.");
		return %this.TD_PickRandom();
	}

	%this.lastPerson = %member;
	%this.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">  - \c4" @ %member.getPlayerName() @ " \c6has been choosen!");
	//%member.chatMessage("You have been choosen to be a builder! Look around and use your plant key.");
	%member.play2D(errorSound);
	%member.schedule(250, play2D, errorSound);
	%member.schedule(500, play2D, errorSound);
	%member.setTower("Default");
	%member.isBuilder = 1;
	%member.isTowerBot = 1;
	%member.canBuild = 1;
	%member.lastBuildGame = %this.resetCount;
	%member.TD_AddItems();
	%player.setShapeNameColor("0 0.5 1 1");
	%this.builderCount++;

	return %member;
}

function GameConnection::TD_UnRevokeBuilder(%this)
{
	$Server::TD_Revoke[%this.getBLID()] = 1;
	%this.chatMessage("You can now be choosen to build again.");
	if(isObject(%minigame = %this.minigame) && %minigame.TD_Round > 0)
		%minigame.messageAll('', "\c4" @ strReplace(%this.getPlayerName() @ "'s", "s's", "s'") @ " \c6building capabilities is no longer revoked until server reset or assistance.");
}

function GameConnection::TD_RevokeBuilder(%this)
{
	$Server::TD_Revoke[%this.getBLID()] = 1;
	%this.chatMessage("Sorry, you have been revoked to do any tower building.");
	if(isObject(%minigame = %this.minigame) && %minigame.TD_Round > 0)
	{
		%minigame.messageAll('', "\c4" @ strReplace(%this.getPlayerName() @ "'s", "s's", "s'") @ " \c6building capabilities is now revoked until server reset or assistance.");
		if(%this.isBuilder)
		{
			%minigame.builderCount--;
			if($Pref::Server::TD_RandomPick)
				%minigame.schedule(500, TD_PickRandom, 1);
		}
	}
	%this.isBuilder = 0;
	%this.canBuild = 0;
}

//Server Voting

if($Pref::Server::TD::TimeoutSeconds <= 0)
	$Pref::Server::TD::TimeoutSeconds = 90;

function servercmdTDRevoke(%client, %var1, %var2, %var3, %var4, %var5, %var6, %var7, %var8, %var9, %var10, %var11, %var12, %var13, %var14, %var15, %var16, %var17, %var18, %var19, %var20)
{
	if(ClientGroup.getCount() < 6)
	{
		%client.chatMessage("\c5Sorry, there must be at least 6 people to start a building revoke poll.");
		return;
	}

	if($Pref::Server::TD::Admin)
		if(!%client.isAdmin)
			return;

	if(!%client.isAdmin)
		if($Sim::Time - %client.lastTDVote < $Pref::Server::TD::TimeoutSeconds)
			return;

	for(%a = 1; %a < 21; %a++)
		%vote = %vote SPC %var[%a];

	%vote = trim(stripMLControlChars(%vote));
	%vote = findClientByName(%vote);
	if(!isObject(%vote))
	{
		%client.chatMessage("Invalid player to request");
		return;
	}

	if(!$TDVote::isVote)
	{
		messageall('', "\c6(\c5" @ %client.getPlayerName() @ "\c6) \c5Created a poll to revoke the player's building capabilities: \c3" @ %vote.getPlayerName() @ " \c5- Type \c3/TDVote Yes \c3\c5or\c3 /TDVote No\c5. Vote ends in 30 seconds.");
		$TDVote::Sch = schedule(30000, 0, "TDVote_End");
		$TDVote::isVote = 1;
		%client.lastTDVote = $Sim::Time;
		$TDVote::BL_ID = %vote.getBLID();

		%client.hasTDvoted = 1;
		$TDVote::Average++;
	}
}

function servercmdTDVote(%client, %a)
{
	if(!$TDVote::isVote)
		return;

	if(%client.hasTDvoted)
		return;

	switch$(%a)
	{
		case "no":
			echo("[Voting] " @ %client.getPlayerName() @ " voted no.");
			%client.hasTDvoted = 1;
			$TDVote::Average--;
			messageAll('', "\c3" @ %client.getPlayerName() @ "\c5 voted no!");

		case "yes":
			echo("[Voting] " @ %client.getPlayerName() @ " voted yes.");
			%client.hasTDvoted = 1;
			$TDVote::Average++;
			messageAll('', "\c3" @ %client.getPlayerName() @ "\c5 voted yes!");

		default:
			%client.chatMessage("\c5Don't be silly! \c6/TDVote \c3yes \c6or \c3no\c6!");
	}
}

function TDVote_Cancel()
{
	cancel($TDVote::Sch);
	if($TDVote::isVote)
	{
		for(%a = 0; %a < ClientGroup.getcount(); %a++)
			ClientGroup.getObject(%a).hasTDvoted = 0;
	}
	$TDVote::Average = 0;
	$TDVote::isVote = 0;
	$TDVote::BL_ID = -1;
}

function TDVote_End()
{
	cancel($TDVote::Sch);
	if($TDVote::isVote)
	{
		if($TDVote::Average == 0)
			Messageall('', "\c5The vote was a \c4tie\c5!");
		else if($TDVote::Average < 0)
			Messageall('', "\c5The greater vote was \c0no\c5, by \c4" @ mAbs($TDVote::Average) @ " \c5person(s)! They are not going to be revoked from building capabilities. (BL_ID: " @ $TDVote::BL_ID @ ")");
		else if($TDVote::Average > 0)
			Messageall('', "\c5The greater vote was \c2yes\c5, by \c4" @ mAbs($TDVote::Average) @ " \c5person(s)! They are now revoked. (BL_ID: " @ $TDVote::BL_ID @ ")");
		
		for(%a = 0; %a < ClientGroup.getcount(); %a++)
			ClientGroup.getObject(%a).hasTDvoted = 0;
	}

	$Server::TD_Revoke[$TDVote::BL_ID] = 1;
	if(isObject(%client = findClientByBL_ID($TDVote::BL_ID)))
	{
		%client.chatMessage("Sorry, you have been revoked to do any tower building.");
		if(isObject(%minigame = %client.minigame) && %minigame.TD_Round > 0)
		{
			%minigame.messageAll('', "\c4" @ strReplace(%client.getPlayerName() @ "'s", "s's", "s'") @ " \c6building capabilities is now revoked until server reset or assistance.");
			if(%client.isBuilder)
			{
				%minigame.builderCount--;
				if($Pref::Server::TD_RandomPick)
					%minigame.schedule(500, TD_PickRandom, 1);
			}
		}
		%client.isBuilder = 0;
		%client.canBuild = 0;
	}

	$TDVote::Average = 0;
	$TDVote::isVote = 0;
	$TDVote::BL_ID = -1;
}

function serverCmdTDVoteCancel(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isEventPending($TDVote::Sch))
		return;

	messageAll('', "\c5" @ %this.getPlayerName() @ " \c6has canceled the vote.");
	TDVote_Cancel();
}

function serverCmdTDVoteEnd(%this)
{
	if(!%this.isAdmin)
		return;

	messageAll('', "\c5" @ %this.getPlayerName() @ " \c6has ended the vote.");
	TDVote_End();
}
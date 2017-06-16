//+=========================================================================================================+\\
//|			Made by..																						|\\
//|		   ____   ____  _				 __	  		  _		   												|\\
//|		  |_  _| |_  _|(_)	      		[  |		/ |_		 											|\\
//| 		\ \   / /  __   .--.   .--.  | |  ,--. `| |-' .--.   _ .--.  									|\\
//| 		 \ \ / /  [  | ( (`\]/ .'`\ \| | `'_\ : | | / .'`\ \[ `/'`\] 									|\\
//| 		  \ ' /    | |  `'.'.| \__. || | // | |,| |,| \__. | | |     									|\\
//|    		   \_/    [___][\__) )'.__.'[___]\'-;__/\__/ '.__.' [___]    									|\\
//|								BL_ID: 20490 | BL_ID: 48980													|\\
//|				Forum Profile(48980): http://forum.blockland.us/index.php?action=profile;u=144888;			|\\
//|																											|\\
//+=========================================================================================================+\\


//NOTES: Add/Update
// + Self tower: 250 resources per minute times the weapon cost (Using the wrench GUI)
// + Tower upgrades

datablock PlayerData(PlayerTDArmor : PlayerStandardArmor)
{
	runForce = 9000;
	runEnergyDrain = 0;
	minRunEnergy = 0;
	maxForwardSpeed = 16;
	maxBackwardSpeed = 16;
	maxSideSpeed = 16;

	maxForwardCrouchSpeed = 8;
	maxBackwardCrouchSpeed = 8;
	maxSideCrouchSpeed = 8;

	jumpForce = 1250;
	jumpEnergyDrain = 0;
	minJumpEnergy = 0;
	jumpDelay = 0;

	minJetEnergy = 0;
	jetEnergyDrain = 0;
	canJet = 0;

	uiName = "Tower Defense Player";
	showEnergyBar = false;

	runSurfaceAngle  = 55;
	jumpSurfaceAngle = 55;
	airControl = 0.6;
};

$TD::Chars = "`~!@#^&*-=+{}\\|;:\'\",<>/?[].";

function TD_loadFilePath(%path)
{
	if(strPos(%path,"*") <= 0)
		%path = %path @ "*";

	if(getFileCount(%path) <= 0)
		return -1;

	for(%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path))
	{
		%fileExt = fileExt(%file);
		if(%fileExt !$= ".cs")
			continue;

		exec(%file);
	}

	return 1;
}

if(!$TD::LoadedSupport)
{
	$TD::LoadedSupport = 1;
	TD_loadFilePath("add-ons/Gamemode_Tower_Defense/Support/*");
}

function TD_CreateBrickgroup()
{
	if(!isObject(BrickGroup_666))
	{
		%group = new SimGroup(BrickGroup_666)
		{
			client = -1;
			bl_id = 666;
			name = "Tower Defense";
			DoNotDelete = 1;
			checkVal = 22;
		};
		MainBrickgroup.add(%group);
	}

	if(isObject(Slayer))
	{
		if(!$TD::LoadedSlayer)
		{
			$TD::LoadedSlayer = 1;
			Slayer.Gamemodes.addMode("Tower Defense", "TD", 0, 1);
		}
	}
}
schedule(0, 0, TD_CreateBrickgroup);

if(!isObject(LaserLoopSound))
	datablock AudioProfile(LaserLoopSound)
	{
		filename = "./LaserLoop.wav";
		description = AudioClosest3DLonger;
		preload = false;
	};

if(!isObject(TowerDefenseGroup))
	new SimGroup(TowerDefenseGroup);

if(!isObject(TowerDefenseBotGroup))
{
	new SimGroup(TowerDefenseBotGroup)
	{
		class = "TD_BotGroup";
	};
}

function SimGroup::getTowerCount(%this, %searchName)
{
	if(%this.getName() !$= "TowerDefenseGroup")
		return -1;

	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(%obj.getClassName() $= "AIPlayer")
			if(%obj.getState() !$= "Dead")
				if((%name = %obj.uiName) !$= "")
					if(%name $= %searchName)
						%count++;
	}

	return mFloor(%count);
}

function SimGroup::getLiving(%this)
{
	if(%this.getName() !$= "TowerDefenseBotGroup")
		return -1;

	for(%i=0;%i<%this.getCount();%i++)
	{
		%obj = %this.getObject(%i);
		if(%obj.getClassName() $= "AIPlayer")
			if(%obj.getState() !$= "Dead")
				%count++;
	}
	return mFloor(%count);
}

if(isPackage("Server_TowerDefense"))
	deactivatePackage("Server_TowerDefense");

schedule(0, 0, deactivatePackage, SnowBallPackage);

function Player::TD_ifResource(%this, %value)
{

}
registerOutputEvent("Player", "TD_ifResource", "int 1 999999 1");

function reapEm()
{
	%temps = 0; %groups = MainBrickgroup.getCount();
	for(%i=0;%i<%groups;%i++)
	{
		%group = MainBrickgroup.getObject(%i);
		%bricks = %group.getCount();
		for(%j=0;%j<%bricks;%j++)
		{
			%brick = %group.getObject(%j);
			if(!%brick.isPlanted) %temp[-1+%temps++] = %brick;
		}
	}
	%count = ClientGroup.getCount();
	for(%i=0;%i<%count;%i++)
	{
		%this = ClientGroup.getObject(%i);
		if(!isObject(%pl = %this.player)) continue;
		if(!isObject(%temp = %pl.tempbrick)) continue;
		for(%j=0;%j<%temps;%j++)
			if(%temp[%j] == %temp) %shift++;
			else %temp[%j - %shift] = %temp[%j];
		%temps -= %shift;
	}
	for(%i=0;%i<%temps;%i++) %temp[%i].delete();
}

$Pref::Server::TD_MaxTowers = 30;

function doNothing(){}

function GameConnection::TD_ParseMessage(%this, %message)
{
	%message = trim(stripMLControlChars(%message));
	if($Pref::Server::TD_BlockMessage_Build)
	{
		if(striPos(%message, "how to build") >= 0 ||
			striPos(%message, "how can build") >= 0 ||
			striPos(%message, "how can i build") >= 0 ||
			striPos(%message, "how do we build") >= 0 ||
			striPos(%message, "how do i build") >= 0 ||
			striPos(%message, "build towers") >= 0 ||
			striPos(%message, "do i place a tower") >= 0 ||
			striPos(%message, "become a builder") >= 0 ||
			striPos(%message, "be a builder") >= 0 ||
			striPos(%message, "how can we build") >= 0)
		{
			%this.chatMessage("\c5Sorry, you were restricted from saying this message: \c3\"" @ %message @ "\"");
			%this.chatMessage("   \c5Please do \c3/td queue\c5, this will make you be able to build towers eventually.");
			return false;
		}
	}

	return true;
}

package Server_TowerDefense
{
	function serverCmdMessageSent(%this, %message)
	{
		if(!%this.TD_ParseMessage(%message))
			return;

		if(%this.isBuilder && %this.nameColor $= "")
		{
			%recolor = 1;
			%this.nameColor = "007FFF";
		}

		Parent::serverCmdMessageSent(%this, %message);
		if(%recolor)
			%this.nameColor = "";
	}

	function ShapeBase::Damage(%this, %attacker, %position, %damage, %damageType)
	{
		if(!isObject(%this))
			return;

		if(isObject(%attacker))
		{
			switch$(%targetClass = %attacker.getClassName())
			{
				case "Player" or "AIPlayer":
					%targetObj = %attacker;
					%targetClient = %attacker.client;

				case "Projectile":
					%targetObj = %attacker.sourceObject;
					%targetClient = %attacker.client;

				case "GameConnection":
					%targetObj = %attacker.player;
					%targetClient = %attacker;
			}
		}

		if(isObject(%targetObj))
			if(%targetObj.getClassName() $= "Player" && %this.isEnemyBot && %targetObj != %this)
			{
				//if(vectorDist(%this.getPosition(), %targetObj.getPosition()) <= %this.range && !isObject(%this.killTarg))
				//	%this.killTarg = %targetObj;

				%damage *= 0.5;
				if(%this.spawnTime > 0)
					if($Sim::Time - %this.spawnTime < 10)
						%damage = 0;
			}

		if(%targetObj.damageMultiplier > 0)
			%damage *= mFloatLength(mClampF(%targetObj.damageMultiplier, 0.01, 100), 4);

		if(%this.isEnemyBot && %targetObj.isTowerBot && isObject(%minigame = %targetObj.minigame))
		{
			if(%damage > %this.getMaxHealth() * 1.5)
				%avoidMassiveDamage = 1;

			if(getWordCount(%this.immunities) > 0)
			{
				for(%i = 0; %i < getWordCount(%this.immunities); %i++)
				{
					%immune = getWord(%this.immunities, %i);
					if(%immune $= %targetObj.TD_Type)
						%isImmune = 1;
				}

				if(%isImmune)
				{
					%pts = mClampF(%damage / 4 / getRandom(3, 5), 0, (%avoidMassiveDamage ? %this.getHealth() / 4 : %this.getHealth() / 2));
					//talk("IMMUNE: " @ %pts @ " -> " @ %damage);
					%minigame.TD_AddResources(%pts);
					if(isObject(%owner = %targetObj.ownerClient))
					{
						if(%owner.lastRound != %minigame.TD_Round)
						{
							%owner.lastRound = %minigame.TD_Round;
							%owner.roundScore = 0;
						}

						if(isObject(%controlClient = %targetObj.getControllingClient()) && %controlClient.roundScore < mFloor(10 * (1 + %minigame.TD_Round * 0.6)))
						{
							%damage *= 0.8 * mClampF(mFloatLength((%targetObj.range + 10) / vectorDist(%targetObj.getPosition(), %this.getPosition()), 2) - 1, 0, 1);
							%newPts = mClampF(%pts * 0.025, 0.01, 1);
							%controlClient.roundScore += %newPts;
							if(%controlClient.roundScore < mFloor(5 * (1 + %minigame.TD_Round * 0.2)))
								%controlClient.incScore(%newPts);
						}

						if(%controlClient != %owner && %owner.roundScore < mFloor(5 * (1 + %minigame.TD_Round * 0.2)))
						{
							%newPts = mClampF(%pts * 0.0125, 0.01, 1);
							%owner.roundScore += %newPts;
							if(%owner.roundScore < mFloor(10 * (1 + %minigame.TD_Round * 0.6)))
								%owner.incScore(%newPts);
						}
					}
				}
			}
			else
			{
				%pts = mClampF(%damage / 3 / getRandom(3, 4), 0, (%avoidMassiveDamage ? %this.getMaxHealth() / 4 : %this.getMaxHealth() / 2));
				//talk("NOT IMMUNE: " @ %pts @ " -> " @ %damage);
				%minigame.TD_AddResources(%pts);
				if(isObject(%owner = %targetObj.ownerClient))
				{
					if(%owner.lastRound != %minigame.TD_Round)
					{
						%owner.lastRound = %minigame.TD_Round;
						%owner.roundScore = 0;
					}

					if(isObject(%controlClient = %targetObj.getControllingClient()) && %controlClient.roundScore < mFloor(10 * (1 + %minigame.TD_Round * 0.6)))
					{
						%damage *= 0.8 * mClampF(mFloatLength((%targetObj.range + 10) / vectorDist(%targetObj.getPosition(), %this.getPosition()), 2) - 1, 0, 1);
						%newPts = mClampF(%pts * 0.025, 0.01, 1);
						%controlClient.roundScore += %newPts;
						if(%controlClient.roundScore < mFloor(10 * (1 + %minigame.TD_Round * 0.6)))
							%controlClient.incScore(%newPts);
					}

					if(%controlClient != %owner && %owner.roundScore < mFloor(5 * (1 + %minigame.TD_Round * 0.2)))
					{
						%newPts = mClampF(%pts * 0.0125, 0.01, 1);
						%owner.roundScore += %newPts;
						if(%owner.roundScore < mFloor(10 * (1 + %minigame.TD_Round * 0.6)))
							%owner.incScore(%newPts);
					}
				}
			}
		}
		else if(%this.isEnemyBot && isObject(%targetObj) && %targetObj.getClassName() $= "Player" && isObject(%minigame = %targetClient.minigame))
		{
			%pts = mClampF(%damage / 2 / getRandom(2, 6), 0.05, %this.getMaxHealth() / 2);
			%minigame.TD_AddResources(%pts);
			%pts = mClampF(%pts * 0.1, 0.01, 5);
			if(isObject(%targetClient) && !%targetClient.isBuilder)
			{
				if(%targetClient.lastRound != %minigame.TD_Round)
				{
					%targetClient.lastRound = %minigame.TD_Round;
					%targetClient.roundScore = 0;
				}
				%targetClient.roundScore += %pts;
				if(%targetClient.roundScore < mFloor(10 * (1 + %minigame.TD_Round * 0.6)))
					%targetClient.incScore(%pts);
			}

			if(vectorDist(%this, %targetObj) <= %this.tooClose && $Pref::Server::TD_AutoTargClose)
				%this.killTarg = %targetObj;
		}

		if(isObject(%client = %this.client) && %client.isBuilder)
			if(%targetObj.isTowerBot)
				return;

		if(%this.spawnTime > 0 && %this.getClassName() $= "AIPlayer")
			if($Sim::Time - %this.spawnTime < 5 && %this != %targetObj)
			{
				%this.TD_SetShapeName();
				return;
			}

		if(%damage == 0)
		{
			if(%this.getClassName() $= "AIPlayer")
				%this.TD_SetShapeName();
			
			return;
		}

		Parent::Damage(%this, %attacker, %position, %damage, %damageType);
		if(isObject(%this) && %this.isTowerBot && %this.getClassName() $= "AIPlayer")
		{
			if(%this.getHealth() <= 0)
			{
				if(!%this.deadTower)
				{
					%this.deadTower = 1;
					%this.setShapeNameDistance(0);
					if(isObject(%tower = %this.baseTower))
					{
						if(isObject(%minigame = %tower.minigame))
							%minigame.messageAll('', "<bitmap:base/client/ui/ci/skull> \c3" @ strReplace(%tower.tower_ownerName @ "\c6's " @ %tower.tower_name, "s's", "'s"));

						$TD::Towers = removeItemFromList($TD::Towers, %tower.getID());
						%tower.schedule(0, killBrick);
						if(isObject(%client = %this.getControllingClient()))
						{
							if(isObject(%player = %client.player))
								%client.setControlObject(%player);
						}
					}
				}

				if(%this.isHealer)
					%minigame.healers--;
			}
			else
				%this.TD_SetShapeName();
		}

		if(isObject(%this) && %this.getState() $= "dead")
			%this.setPlayerShapeName("");
	}

	function GameConnection::onDeath(%this,%sourceObject,%sourceClient,%damageType,%damageArea)
	{
		Parent::onDeath(%this,%sourceObject,%sourceClient,%damageType,%damageArea);
		if(isObject(%minigame = %this.minigame))
		{
			%this.isBuilder = 0;
			%this.canBuild = 0;
		}
	}

	function fxDTSBrick::setName(%this, %name)
	{
		Parent::setName(%this, %name);
		if(%name $= "_Node" || %name $= "_EndNode" || %name $= "_StartNode")
			TD_PathHandler.addNode(%this);
		else
			TD_PathHandler.removeNode(%this);
	}

	function AIPlayer::applyImpulse(%this, %pos, %vel)
	{
		if(%this.isTowerBot)
			return;

		if(%this.impulseImmune > 0)
			%vel = vectorScale(%vel, %this.impulseImmune);

		return Parent::applyImpulse(%this, %pos, %vel);
	}

	function ProjectileData::radiusImpulse(%this, %obj, %col, %factor, %pos, %force)
	{
		if(isObject(%source = %obj.sourceObject) && isObject(%col) && %source.getClassName() !$= "fxDTSBrick")
		{
			%velScale = mClampF(mFloatLength((%source.range + 10) / vectorDist(%source.getPosition(), %col.getPosition()), 2) - 1, 0, 1);
			if(isObject(%client = %source.getControllingClient()) && %source.getClassName() $= "AIPlayer" && %source.isTowerBot)
				%force *= mFloor(%velScale);

			if(%force == 0)
				return;
		}

		return Parent::radiusImpulse(%this, %obj, %col, %factor, %pos, %force);
	}

	function AIPlayer::setVelocity(%this, %a)
	{
		if(%this.isTowerBot)
			return;
		return Parent::setVelocity(%this, %a);
	}

	function AIPlayer::addVelocity(%this, %a)
	{
		if(%this.isTowerBot)
			return;
		return Parent::addVelocity(%this, %a);
	}

	function Slayer_MinigameSO::Reset(%this, %client)
	{
		Parent::Reset(%this, %client);
	}

	function MinigameSO::Reset(%this, %client)
	{
		Parent::Reset(%this, %client);
	}

	function HammerImage::onHitObject(%this, %obj, %slot, %col, %pos, %normal)
	{
		if(%col.getClassName() $= "fxDTSBrick" && isObject(%client = %obj.client) && %client.getClassName() $= "GameConnection" && %col.isBaseTowerBrick && 
			(getBrickgroupFromObject(%col).bl_id == %client.getBLID() || %client.isSuperAdmin || %client.isBuilderHost))
		{
			%client.killTower = %col;
			commandToClient(%client, 'MessageBoxYesNo', "Tower Defense - Destroy?", "Are you sure you want to destroy this tower?", 'ConfirmDestroy');
			return;
		}

		if(%col.getClassName() $= "AIPlayer" && %col.isHealer && isObject(%client = %obj.client) && %client.getClassName() $= "GameConnection" && isObject(%client.minigame))
		{
			%client.minigame.TD_AddResources(400);
			%col.schedule(0, delete);
			return;
		}

		if(%col.getClassName() $= "fxDTSBrick" && %col.isBaseTowerBrick && %obj.isHealer)
		{
			if(isObject(%bot = %col.bot))
			{
				if(%bot.getHealth() < %bot.getMaxHealth())
				{
					%bot.addHealth(getRandom(2, 15));
					if(isObject(%brick = %bot.brickShield))
					{
						cancel(%brick.changeSch);
						%brick.setEmitter(nameToID(brickDeployExplosionEmitter));
						%brick.changeSch = %brick.schedule(100, setEmitter, -1);
					}
				}
				%bot.TD_SetShapeName();
			}
			return;
		}

		if(%col.isTowerBrick)
			return;

		Parent::onHitObject(%this, %obj, %slot, %col, %pos, %normal);
	}

	function TD_HammerImage::onHitObject(%this, %obj, %slot, %col, %pos, %normal)
	{
		if(%col.getClassName() $= "fxDTSBrick" && isObject(%client = %obj.client) && %client.getClassName() $= "GameConnection" && %col.isBaseTowerBrick && 
			(getBrickgroupFromObject(%col).bl_id == %client.getBLID() || %client.isSuperAdmin || %client.isBuilderHost))
		{
			%client.killTower = %col;
			commandToClient(%client, 'MessageBoxYesNo', "Tower Defense - Destroy?", "Are you sure you want to destroy this tower?", 'ConfirmDestroy');
			return;
		}

		if(%col.getClassName() $= "AIPlayer" && %col.isHealer && isObject(%client = %obj.client) && %client.getClassName() $= "GameConnection" && isObject(%client.minigame))
		{
			%client.minigame.TD_AddResources(400);
			%col.schedule(0, delete);
			return;
		}

		if(%col.getClassName() $= "fxDTSBrick" && %col.isBaseTowerBrick && %obj.isHealer)
		{
			if(isObject(%bot = %col.bot))
			{
				if(%bot.getHealth() < %bot.getMaxHealth())
				{
					%bot.addHealth(getRandom(2, 15));
					if(isObject(%brick = %bot.brickShield))
					{
						cancel(%brick.changeSch);
						%brick.setEmitter(nameToID(brickDeployExplosionEmitter));
						%brick.changeSch = %brick.schedule(100, setEmitter, -1);
					}
				}
				%bot.TD_SetShapeName();
			}
			return;
		}

		if(%col.isTowerBrick)
			return;

		Parent::onHitObject(%this, %obj, %slot, %col, %pos, %normal);
	}

	function fxDTSBrick::onDeath(%this)
	{
		if(%this.isBotBrick)
		{
			if(isObject(%bot = %this.bot))
			{
				if(isObject(%client = %bot.getControllingClient()))
				{
					if(isObject(%player = %client.player))
						%client.setControlObject(%player);
				}
				%bot.delete();
			}
		}
		if(isObject(%shape = %this.shape_name))
			%shape.delete();

		TD_PathHandler.removeNode(%this);
		Parent::onDeath(%this);
	}

	function fxDTSBrick::onRemove(%this)
	{
		if(%this.isBotBrick)
		{
			if(isObject(%bot = %this.bot))
			{
				if(isObject(%client = %bot.getControllingClient()))
				{
					if(isObject(%player = %client.player))
						%client.setControlObject(%player);
				}
				%bot.delete();
			}
		}
		if(isObject(%shape = %this.shape_name))
			%shape.delete();

		TD_PathHandler.removeNode(%this);
		Parent::onRemove(%this);
	}

	function serverCmdSuperShiftBrick(%this, %a, %b, %c)
	{
		if(!isObject(%player = %this.player))
			return;

		if(!isObject(%this.minigame))
			return Parent::serverCmdSuperShiftBrick(%this, %a, %b, %c);

		if(%this.isBuilder)
			return;

		return Parent::serverCmdSuperShiftBrick(%this, %a, %b, %c);
	}

	function serverCmdClearBricks(%this, %a, %b)
	{
		%this.chatMessage("You can't do this, you'll break the towers.");
			return;

		Parent::serverCmdClearBricks(%this, %a, %b);
	}

	function serverCmdShiftBrick(%this, %a, %b, %c)
	{
		if(!isObject(%player = %this.player))
			return;

		if(!isObject(%this.minigame))
			if(!%this.canBuild)
				return Parent::serverCmdShiftBrick(%this, %a, %b, %c);

		if(%this.isBuilder)
		{
			%vec = %a SPC %b SPC %c;
			switch$(%vec)
			{
				case "0 1 0":
					%this.setBuilderOption("Left");

				case "0 -1 0":
					%this.setBuilderOption("Right");

				default:
					doNothing();
			}
		}
	}

	function Armor::onDisabled(%this, %obj)
	{
		if(%obj.isTowerBot)
		{
			%tower = %obj.baseTower;
			if(isObject(%tower))
			{
				if(isObject(%shield = %tower.brickShield))
				{
					if(isObject(%shape = %shield.shape_name))
						%shape.delete();
				}
			}
		}
		Parent::onDisabled(%this, %obj);
	}

	function serverCmdPlantBrick(%this)
	{
		if(!isObject(%player = %this.player))
			return;

		if(!isObject(%minigame = %this.minigame))
		{
			if(%this.isForcedToBuild && %this.canBuild)
				return;

			return Parent::serverCmdPlantBrick(%this);
		}

		if(%player.isInConfigMode)
		{
			%this.enterConfig();
			return;
		}

		if(!%this.isBuilder)
		{
			if(!%this.canPlant) //The fuck is this?
			{
				%this.centerPrint("You can't build!", 2);
				return;
			}
		}

		%adminLevel = %this.isAdmin + %this.isSuperAdmin + (%this.getBLID() == getNumKeyID());

		if(isObject(%temp = %player.tempBrick))
		{
			if(!%temp.isTowerBrick)
			{
				%this.centerPrint("That's not a tower!", 2);
				return;
			}

			if(!%temp.canPlantTower(%this, 1))
				return;

			%data = %this.getTowerDB();
			%hideData = %this.getHiddenTowerDB();
			
			if(%temp.isTowerBrick)
			{
				%brickgroup = getBrickgroupFromObject(%this);
				%data = %temp.getDatablock();
				%pos = %temp.getPosition();
				if(%data.isTowerBrick)
				{
					%tower = %this.getTower();
					if(!isObject(%tower))
					{
						%this.centerPrint("Invalid tower!", 2);
						return;
					}

					if(%tower.height < 0)
					{
						%this.centerPrint("\c6Use your \c4left\c6/\c4right \c6brick shift keys to select different towers!", 4);
						return;
					}

					if(!%tower.canPlant && !%tower.canBuild)
					{
						%this.centerPrint("You cannot plant towers!", 2);
						return;
					}

					if(!%temp.isOnGrid())
					{
						%this.centerPrint("That isn't on the grid!", 2);
						return;
					}

					if(TD_TowerCount() >= $Pref::Server::TD_MaxTowers && !%this.canBuildInfiniteTowers)
					{
						%this.centerPrint("Sorry, max towers reached.", 2);
						return;
					}

					%minLevel = %tower.minLevel;
					if(%tower.minLevel[%minigame.TD_Difficult] > 0)
						%minLevel = %tower.minLevel[%minigame.TD_Difficult];

					if(%minigame.TD_Round < %minLevel && !%this.allTowersUnlocked)
						return;
					else if(TowerDefenseGroup.getTowerCount(%tower.uiName) > %tower.maxTowers && %tower.maxTowers > 0)
						return;

					%cost = %tower.cost * %minigame.TD_CostMultiplier;

					if(%cost > %minigame.TD_Resources && !%this.canBuildInfiniteTowers && !%this.canBuildAnyTower)
					{
						%this.centerPrint("You don't have enough for that!", 2);
						return;
					}

					if(%tower.adminLevel > %adminLevel)
					{
						%this.centerPrint("You aren't corrupted enough to use this!", 2);
						return;
					}

					%curBrick = new fxDtsBrick()
					{
						client = %brickgroup.client;
						position = %pos;
						colorID = findclosestcolor("150 150 150 255");
						dataBlock = %data;
						rotation = %temp.rotation;
						angleID = %temp.angleID;
						isPlanted = 1;
						scale = %temp.getScale();
						stackBL_ID = %brickGroup.bl_id;
						printID = %temp.printID;
						isPlanted = 1;
						isTowerBrick = 1;
						isBaseTowerBrick = 1;
						tower_weapon = %tower.weaponName;
						tower_name = %tower.uiName;
						tower_cost = %cost;
						tower_maxhealth = %tower.health;
						tower_ownerName = %this.getPlayerName();
						tower_ownerBL_ID = %this.getBLID();
						minigame = %minigame;
					};
					%curBrick.setTrusted(1);
					%err = %curBrick.plant();

					if(!isObject(%brickgroup))
					{
						%curBrick.schedule(0, delete);
						%errs++;
						return 0;
					}
					%brickgroup.add(%curBrick);
					if(%err)
					{
						%curBrick.schedule(0, delete);
						return 0;
					}
					if(!isObject(%curBrick.getDownBrick(0)))
					{
						%curBrick.schedule(0, delete);
						return 0;
					}

					//Find planted brick's client, position and world box size
					%BrPosXYZ = %curBrick.getPosition();
					%BrWrldBox = %curBrick.getWorldBox();
					
					//Determine brick size to use for container search below
					%BrWBx = getWord(%BrWrldBox,3) - getWord(%BrWrldBox,0);
					%BrWBy = getWord(%BrWrldBox,4) - getWord(%BrWrldBox,1);
					%BrWBz = getWord(%BrWrldBox,5) - getWord(%BrWrldBox,2);
					
					//Run a container search to see if brick is within a ModTer brick
					%BrSizeXYZ = %BrWBx - 0.1 SPC %BrWBy - 0.1 SPC %BrWBz - 0.1;
					initContainerBoxSearch(%BrPosXYZ,%BrSizeXYZ,$TypeMasks::FxBrickAlwaysObjectType);
					
					//Check all bricks within container search area (failsafe is just a precaution, but not necessary)
					while(isObject(%TmpBrObj = containerSearchNext()) && %failSafe++ <= 1000)
					{
						//If brick has the ModTer print aspect ratio, it's probably a ModTer brick ("%TmpBrObj != %brick" prevents issues with planting ModTer bricks)
						if(%TmpBrObj != %curBrick)
						{
							%curBrick.schedule(0, delete);
							return;
						}
					}

					%player.gotHint = 1;
					%minigame.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c3" @ %this.getPlayerName() @ "\c6 has bought a tower: \c3" @ %tower.uiName @ " \c6(\c3$" @ %cost @ "\c6)");
					if($Pref::Server::TD_ScorePlant)
						if(%tower.scorePlant > 0)
							%this.incScore(%tower.scorePlant);

					if(%tower.height >= 2)
					{
						//%this.undoStack.push(%curBrick TAB "PLANT");
						%height = %tower.height - 1;
						for(%i = %height; %i > 0; %i --)
						{
							%newPos = mFloatLength(getWord(%pos, 0), 1) SPC 
								mFloatLength(getWord(%pos, 1), 1) SPC 
								getWord(%pos, 2) + ((1 + %height - %i) * (%data.brickSizeY / 2));

							//talk("Debug: ");
							//talk("  -> Y Math (Currently using): " @ getWord(%pos, 2) + ((1 + %height - %i) * (%data.brickSizeY / 2)));
							//talk("  -> Z Math: " @ getWord(%pos, 2) + ((1 + %height - %i) * (%data.brickSizeZ / 2)));

							%newBrick = new fxDtsBrick()
							{
								client = %brickgroup.client;
								position = %newPos;
								colorID = findclosestcolor("150 150 150 255");
								dataBlock = %data;
								rotation = %temp.rotation;
								angleID = %temp.angleID;
								isPlanted = 1;
								scale = %temp.getScale();
								stackBL_ID = %brickGroup.bl_id;
								printID = %temp.printID;
								isPlanted = 1;
								isTowerBrick = 1;
								tower_weapon = %tower.weaponName;
								tower_name = %tower.uiName;
								tower_cost = %cost;
								tower_ownerName = %this.getPlayerName();
								tower_ownerBL_ID = %this.getBLID();
							};
							%newBrick.setTrusted(1);
							%newBrick.plant();
							BrickGroup_666.add(%newBrick);
						}
					}

					%data = %curBrick.getDatablock();
					%pos = %curBrick.getTopPosition();
					if(isObject(%newBrick))
					{
						%data = %newBrick.getDatablock();
						%pos = %newBrick.getTopPosition();
					}

					%newHidePos = mFloatLength(getWord(%pos, 0), 1) SPC 
						mFloatLength(getWord(%pos, 1), 1) SPC 
						mFloatLength(getWord(%pos, 2) + (%hideData.brickSizeY / 2), 1);

					%hideBrick = new fxDtsBrick()
					{
						client = %brickgroup.client;
						position = %newHidePos;
						colorID = findclosestcolor("150 150 150 255");
						dataBlock = %hideData;
						rotation = %temp.rotation;
						angleID = %temp.angleID;
						isPlanted = 1;
						scale = %temp.getScale();
						stackBL_ID = %brickGroup.bl_id;
						printID = %temp.printID;
						isPlanted = 1;
						isTowerBrick = 1;
						isBotHitBox = 1; //If he gets hit here it should do some work
						tower_weapon = %tower.weaponName;
						tower_name = %tower.uiName;
						tower_cost = %cost;
						tower_maxhealth = %tower.health;
						tower_ownerName = %this.getPlayerName();
						tower_ownerBL_ID = %this.getBLID();
					};
					%hideBrick.setTrusted(1);
					%hideBrick.plant();
					BrickGroup_666.add(%hideBrick);
					%hideBrick.setRendering(0);
					%hideBrick.setRaycasting(0);

					%aPos = %hideBrick.getPosition();
					%lscale = %hideBrick.getDatablock().brickSizeY / 2;
					%botBrick_Pos = vectorAdd(%aPos, "0 0 -" @ %lscale);

					%botBrick = new fxDtsBrick()
					{
						client = %brickgroup.client;
						position = %botBrick_Pos;
						colorID = findclosestcolor("0 255 0 255");
						dataBlock = brick4x4fData;
						rotation = %temp.rotation;
						angleID = %temp.angleID;
						isPlanted = 1;
						scale = %temp.getScale();
						stackBL_ID = %brickGroup.bl_id;
						printID = %temp.printID;
						isPlanted = 1;
						isTowerBrick = 1;
						isBotBrick = 1;
						tower_weapon = %tower.weaponName;
						tower_name = %tower.uiName;
						tower_cost = %cost;
						tower_maxhealth = %tower.health;
						minigame = %minigame;
						brickShield = %hideBrick;
						baseTower = %curBrick;
					};
					%botBrick.setTrusted(1);
					%botBrick.plant();
					%botBrick.disappear(-1);
					%curBrick.brickShield = %hideBrick;
					%botBrick.TD_SpawnBot(%this);
					BrickGroup_666.add(%botBrick);

					if(!%this.canBuildInfiniteTowers)
						%minigame.TD_Resources -= %cost;
					$TD::Towers = $TD::Towers SPC %curBrick.getID();
					ServerPlay3D(brickPlantSound, %hideBrick.getTransform());
				}
			}
		}
	}

	function serverCmdSit(%this)
	{
		if(!isObject(%this.minigame))
			Parent::serverCmdSit(%this);
	}

	function GameConnection::CreatePlayer(%this, %transform)
	{
		cancel(%this.applySch);
		%this.applySch = %this.schedule(500, "TD_Apply");
		if(isObject(%player = %this.player))
			if(isObject(%temp = %player.tempBrick))
				%temp.delete();

		if(!%this.TD_ClientData["DisableHUD"])
			commandToClient(%this, 'TDCMD', "Close", "HUD");

		return Parent::CreatePlayer(%this, %transform);
	}

	function GameConnection::onClientLeaveGame(%this)
	{
		if(%this.isBuilder && isObject(%minigame = %this.minigame) && %minigame.TD_Round > 0)
		{
			if($Pref::Server::TD_RandomPick)
				%minigame.schedule(500, TD_PickInit, 1);
		}

		if(TD_getQueue().isMember(%this))
		{
			TD_getQueue().bringToFront(%this);
			TD_getQueue().remove(%this);
		}

		return Parent::onClientLeaveGame(%this);
	}

	function Armor::onTrigger(%data, %obj, %slot, %toggle)
	{
		Parent::onTrigger(%data, %obj, %slot, %toggle);
		if(%obj.getClassName() $= "AIPlayer" && %obj.isTowerBot && isObject(%client = %obj.getControllingClient()))
			if(%slot == 2 && %toggle)
			{
				if(isObject(%player = %client.player))
				{
					%client.setControlObject(%player);
					%obj.setCrouching(0);
					%obj.lastControlExitTime = $Sim::Time;
				}
			}
	}

	function Armor::onCollision(%this, %obj, %col, %vec, %force)
	{
		Parent::onCollision(%this, %obj, %col, %vec, %force);
		if(!isObject(%col))
			return;

		if(%col.getClassName() $= "Player" && %obj.getClassName() $= "AIPlayer")
		{
			if(%obj.getState() !$= "dead" && %col.getState() !$= "dead")
			{
				//%col.spawnExplosion(SpawnProjectile,getWord(%col.getScale(),2));
				%p1 = %obj.getPosition();
				%p2 = %col.getPosition();
				%p_1 = getWord(%p1,0)/5 SPC getWord(%p1,1)/5 SPC getWord(%p1,2)/15;
				%p_2 = getWord(%p2,0)/5 SPC getWord(%p2,1)/5 SPC getWord(%p2,2)/15;
				%col.setVelocity("0 0 1");
				%col.setVelocity(vectorAdd(vectorScale(vectorSub(%p_1, %p_2), -25), "0 0 4"));
				if(!%obj.isHealer)
				{
					if(isEventPending(%col.resetSpeedFactorData))
					{
						cancel(%col.resetSpeedFactorData);
						%col.setSpeedFactor(0.75);
						%col.resetSpeedFactorData = %col.schedule(3000, setDatablock, %col.oldDatablock);
					}
					else
					{
						%col.oldDatablock = %col.getDatablock();
						%col.setDatablock(PlayerNoJet);
						%col.setSpeedFactor(0.75);
						%col.resetSpeedFactorData = %col.schedule(3000, setDatablock, %col.oldDatablock);
					}
				}
				else if(vectorDist(%obj.getPosition(), %col.getPosition()) <= %obj.range && !isObject(%obj.killTarg))
					%obj.killTarg = %col;
			}
		}
	}

	function serverCmdUseInventory(%this, %slot)
	{
		if(%this.getClassName() $= "GameConnection")
		{
			if(%this.isBuilder)
			{
				if(%slot < 4)
					%this.setBuilderOption("Left");
				else
					%this.setBuilderOption("Right");

				return;
			}
		}

		return Parent::serverCmdUseInventory(%this, %slot);
	}

	function Player::activateStuff(%this)
	{
		Parent::activateStuff(%this);
		if(isObject(%client = %this.client))
		{
			if(%client.isBuilder)
				serverCmdPlantBrick(%client);
		}
	}

	function serverCmdCancelBrick(%this)
	{
		if(!isObject(%minigame = %this.minigame))
			return Parent::serverCmdCancelBrick(%this);

		if(!isObject(%player = %this.player))
			return Parent::serverCmdCancelBrick(%this);

		if(%player.getState() $= "dead")
			return Parent::serverCmdCancelBrick(%this);

		if(%this.isBuilder)
		{
			cancel(%this.toggleLightSch);
			%this.buildClick = 0;

			if(%player.isInConfigMode)
			{
				%this.centerPrint("You're in an options mode!", 2);
				return;
			}

			%this.canBuild = !%this.canBuild;
			if(!%this.canBuild)
			{
				if(isObject(%temp = %player.tempbrick))
					%temp.delete();
			}
			if(isObject(%temp = %player.tempbrick) && !%temp.isTowerBrick)
				%temp.delete();
			%buildMsg = (%this.canBuild ? "You can now build!" : "You can no longer build!");
			%this.centerPrint(%buildMsg, 2);
		}
		else
			return Parent::serverCmdCancelBrick(%this);
	}
};
activatePackage("Server_TowerDefense");

function AIPlayer::TD_SetShapeName(%this)
{
	if(!%this.isTowerBot && !%this.isHealer)
		return;

	if(isObject(%client = %this.getControllingClient()))
		%name = "(" @ %client.getPlayerName() @") ";

	%this.setPlayerShapeName(%name @ %this.uiName @ ", " @ mFloor(mClampF(%this.getHealth(), 0, %this.getMaxHealth())) @ "/" @ %this.getMaxHealth() @ "HP");
}

function MinigameSO::TD_AddResources(%this, %amt)
{
	%amt = mFloatLength(%amt, 2);
	%this.TD_Resources = mClampF(%this.TD_Resources + %amt, 0, 999999);
}

function Slayer_TD_canDamage(%mini, %objA, %objB)
{
	return %objA.TD_CanDamage(%objB);
}

function TD::minigameCanDamage(%mode, %objA, %objB)
{
	return %objA.TD_CanDamage(%objB);
}

function Slayer_TD_preReset(%mini, %client)
{
	if(isObject(%mini))
	{
		cancel(%mini.TD_Loop);
		cancel(%mini.spawnRound);
		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%member = %mini.member[%i];
			if(isObject(%member) && %member.getClassName() $= "GameConnection")
			{
				//%member.isTowerBot = 1; //Prevent being killed for now
				if(isObject(%player = %member.player))
					if(isObject(%temp = %player.tempBrick))
						%temp.delete();
			}
		}

		reapEm();

		%mini.resetCount++;
		TD_ShapeGroup.deleteAll();
		TowerDefenseGroup.deleteAll();
		%mini.giveWeapons = 0;
		%mini.oldQueue = 0;
		%mini.failed = 0;
		%mini.healers = 0;
		%mini.message = 0;
		%mini.lastReset = $Sim::Time;
		%mini.botQueue = 0;
		%mini.refundPercent = $Pref::Server::TD_RefundPercent;
		for(%i = 0; %i < getWordCount($TD::Towers); %i++)
		{
			%brick = getWord($TD::Towers, %i);
			if(isObject(%brick))
				%brick.killBrick();
		}
		$TD::Towers = "";

		if(%mini.numMembers > 0)
		{
			for(%i = 0; %i < %mini.numMembers; %i++)
			{
				%member = %mini.member[%i];
				if(isObject(%member) && %member.getClassName() $= "GameConnection")
				{
					%member.isBuilder = 0;
					%member.canBuild = 0;
					%member.setTower("None");
				}
			}
		}
		%mini.isChangingRound = 1;
		%mini.TD_Resources = 0;
		%mini.TD_Round = 1;
		cancel(%mini.spawnRound);
		cancel(%mini.TD_Loop);
		TowerDefenseBotGroup.deleteAll();

		cancel(%mini.resetSch);
		%mini.resetSch = %mini.schedule(8000, TD_Pick);
	}
}

function TD::preMinigameReset(%this)
{
	%mini = %this.minigame;
	if(isObject(%mini))
	{
		cancel(%mini.TD_Loop);
		cancel(%mini.spawnRound);
		for(%i = 0; %i < %mini.numMembers; %i++)
		{
			%member = %mini.member[%i];
			if(isObject(%member) && %member.getClassName() $= "GameConnection")
			{
				//%member.isTowerBot = 1; //Prevent being killed for now
				if(isObject(%player = %member.player))
					if(isObject(%temp = %player.tempBrick))
						%temp.delete();
			}
		}

		reapEm();

		%mini.resetCount++;
		TD_ShapeGroup.deleteAll();
		TowerDefenseGroup.deleteAll();
		%mini.giveWeapons = 0;
		%mini.oldQueue = 0;
		%mini.failed = 0;
		%mini.healers = 0;
		%mini.message = 0;
		%mini.lastReset = $Sim::Time;
		%mini.botQueue = 0;
		%mini.refundPercent = $Pref::Server::TD_RefundPercent;
		for(%i = 0; %i < getWordCount($TD::Towers); %i++)
		{
			%brick = getWord($TD::Towers, %i);
			if(isObject(%brick))
				%brick.killBrick();
		}
		$TD::Towers = "";

		if(%mini.numMembers > 0)
		{
			for(%i = 0; %i < %mini.numMembers; %i++)
			{
				%member = %mini.member[%i];
				if(isObject(%member) && %member.getClassName() $= "GameConnection")
				{
					%member.isBuilder = 0;
					%member.canBuild = 0;
					%member.setTower("None");
				}
			}
		}
		%mini.isChangingRound = 1;
		%mini.TD_Resources = 0;
		%mini.TD_Round = 1;
		cancel(%mini.spawnRound);
		cancel(%mini.TD_Loop);
		TowerDefenseBotGroup.deleteAll();

		cancel(%mini.resetSch);
		%mini.resetSch = %mini.schedule(8000, TD_Pick);
	}
}

function SimObject::TD_CanDamage(%this, %target, %debug)
{
	if(%debug $= "")
		%debug = $TD::Debug;

	if(!isObject(%this) || !isObject(%target))
	{
		if($TD::Debug)
			talk("TD_CanDamage(1) - Main/target are non-existant.");

		return false;
	}

	if($TD::Debug)
		talk("TD_CanDamage() - Init");

	switch$(%mainClass = %this.getClassName())
	{
		case "Player" or "AIPlayer":
			%mainObj = %this;
			%mainClient = %this.client;

		case "Projectile":
			%mainObj = %this.sourceObject;
			%mainClient = %this.client;

		case "GameConnection":
			%mainObj = %this.player;
			%mainClient = %this;
	}

	switch$(%targetClass = %target.getClassName())
	{
		case "Player" or "AIPlayer":
			%targetObj = %target;
			%targetClient = %target.client;

		case "Projectile":
			%targetObj = %target.sourceObject;
			%targetClient = %target.client;

		case "GameConnection":
			%targetObj = %target.player;
			%targetClient = %target;
	}

	if(!isObject(%mainObj) || !isObject(%targetObj))
	{
		if($TD::Debug)
			talk("TD_CanDamage(2) - Main/target are non-existant.");

		return false;
	}

	if(%mainObj.friendlyfire || %mainClient.friendlyfire)
	{
		if($TD::Debug)
			talk("  TD_CanDamage() - Friendlyfire init");

		if((%mainClient.isEnemyBot || %mainObj.isEnemyBot) && (%targetClient.isEnemyBot || %targetObj.isEnemyBot))
		{
			if($TD::Debug)
				talk("  TD_CanDamage() - Main and target are both enemies");

			return false;
		}

		if((%mainClient.isTowerBot || %mainObj.isTowerBot) && (%targetClient.isTowerBot || %targetObj.isTowerBot))
		{
			if($TD::Debug)
				talk("  TD_CanDamage() - Main and target are both towers");

			return false;
		}

		if((%mainClient.isTowerBot || %mainObj.isTowerBot) && (isObject(%targetClient) && %targetClient.getClassName() $= "GameConnection" && !%targetClient.isEnemyBot))
		{
			if($TD::Debug)
				talk("  TD_CanDamage() - Tower->Target is a client");

			return false;
		}

		if((%targetClient.isTowerBot || %targetObj.isTowerBot) && (isObject(%mainClient) && %mainClient.getClassName() $= "GameConnection"))
		{
			if($TD::Debug)
				talk("  TD_CanDamage() - Client->Target is a tower bot");

			return false;
		}

		return true;
	}

	if((isObject(%mainClient) && %mainClient.getClassName() $= "GameConnection") && (isObject(%targetClient) && %targetClient.getClassName() $= "GameConnection"))
	{
		if($TD::Debug)
			talk("TD_CanDamage() - Main and target are clients");

		return false;
	}

	if((isObject(%mainObj) && %mainObj.getClassName() $= "Player") && (isObject(%targetObj) && %targetObj.getClassName() $= "Player"))
	{
		if($TD::Debug)
			talk("TD_CanDamage() - Main and target are players");

		return false;
	}


	if((isObject(%mainClient) && %mainClient.getClassName() $= "GameConnection") && (%targetObj.isTowerBot || %targetClient.isTowerBot))
	{
		if($TD::Debug)
			talk("TD_CanDamage() - Client->Target is a tower bot.");

		return false;
	}

	if((isObject(%targetClient) && %targetClient.getClassName() $= "GameConnection") && (%mainObj.isTowerBot || %mainClient.isTowerBot))
	{
		if($TD::Debug)
			talk("TD_CanDamage() - Target->Object is a tower bot.");

		return false;
	}

	if(%targetObj.getState() $= "dead")
		return false;

	if($TD::Debug)
		talk("TD_CanDamage() - Set to true");

	return true;
}

function serverCmdConfirmDestroy(%this)
{
	if(isObject(%brick = %this.killTower))
	{
		if(%brick.isBaseTowerBrick)
		{
			if(%brick.tower_cost > 0)
				if(isObject(%minigame = %brick.minigame))
					if(%minigame.refundPercent > 0 && %brick.bot.getHealth() >= %brick.bot.getMaxHealth())
					{
						%amt = %brick.tower_cost * (%minigame.refundPercent / 100);
						%minigame.messageAll('', "<font:arial:" @ $Pref::Server::TD_FontSize @ ">\c3" @ %this.getPlayerName() @ "\c6 refunded a \c3" @ %brick.tower_name @ " \c6tower for \c3$" @ %amt @ "\c6!");
						%minigame.TD_AddResources(%amt);
					}

			if(isObject(%shield = %brick.brickShield))
			{
				if(isObject(%shape = %shield.shape_name))
					%shape.delete();
			}

			$TD::Towers = removeItemFromList($TD::Towers, nameToID(%brick));
		}
		%brick.killBrick();
	}
}

function Player::SetPlayerShapeName(%this,%name){%this.setShapeName(%name,"8564862");}
function AIPlayer::SetPlayerShapeName(%this,%name,%tog){if(%tog && trim(%name) !$= "") %name = "(AI) " @ %name; %this.setShapeName(%name,"8564862");}

function serverCmdCreateMedic(%this)
{
	if(!%this.isBuilder && !%this.canSummonMedics)
	{
		%this.chatMessage("Must be a builder or can summon medics!");
		return;
	}

	if(!isObject(%minigame = %this.minigame))
	{
		%this.chatMessage("Must be in the minigame!");
		return;
	}

	if(!isObject(%player = %this.player))
	{
		%this.chatMessage("Must exist!");
		return;
	}

	if(%minigame.noHealers)
	{
		%this.chatMessage("\c5Sorry, no healers are allowed to be summoned.");
		return;
	}

	if(TD_Healers() > 4)
	{
		%this.chatMessage("\c5Sorry, max healers reached. Hammer healers if you need to spawn one differently.");
		return;
	}

	%pts = 400;
	if(%minigame.TD_Resources < %pts && !%this.canSummonMedics)
	{
		%this.chatMessage("\c5Sorry, you need at least \c3" @ %pts @ " resource" @ (%pts == 1 ? "" : "s") @ " \c5to summon a healer.");
		return;
	}

	if(!%this.canSummonMedics)
		%minigame.TD_AddResources(-%pts);

	%datablock = nameToID(PlayerNoJet);

	%bot = new AIPlayer()
	{
		position = %player.getPosition();
		rotation = %player.rotation;
		dataBlock = %datablock;
		isTowerBot = 1;
		minigame = %minigame;
		isTF2Healer = 1;
		isHealer = 1;
		range = 100;
		uiName = "Healer";
	};
	%bot.setMaxHealth(400);
	%bot.setShapeNameDistance(20);

	%bot.Accent = "0";
	%bot.AccentColor = "0.900 0 0 1.000";
	%bot.Chest = "0";
	%bot.ChestColor = "0.9 0 0 1";
	%bot.DecalName = "Hoodie";
	%bot.FaceName = "smiley";
	%bot.Hat = "4";
	%bot.HatColor = "0.2 0.2 0.2 1";
	%bot.HeadColor = "1 0.878 0.611 1";
	%bot.Hip = "0";
	%bot.HipColor = "0.9 0 0 1";
	%bot.LArm = "0";
	%bot.LArmColor = "1 1 1 1";
	%bot.LHand = "0";
	%bot.LHandColor = "1 0.878 0.611 1";
	%bot.LLeg = "0";
	%bot.LLegColor = "0.2 0.2 0.2 1";
	%bot.Pack = "0";
	%bot.PackColor = "0.392 0.196 0 1";
	%bot.RArm = "0";
	%bot.RArmColor = "1 1 1 1";
	%bot.RHand = "0";
	%bot.RHandColor = "1 0.878 0.611 1";
	%bot.RLeg = "0";
	%bot.RLegColor = "0.2 0.2 0.2 1";
	%bot.SecondPack = "0";
	%bot.SecondPackColor = "0.2 0.2 0.2 1";
	%bot.player = %bot;
	GameConnection::ApplyBodyParts(%bot);
	GameConnection::ApplyBodyColors(%bot);

	%bot.TD_Loop();
	%bot.setWeapon(findItemByName("TF2 Medigun"));
	%bot.setRunSpeed(0.75);
	%bot.setShapeName(%bot.uiName @ ", " @ mClampF(%bot.getHealth(), 0, %bot.getMaxHealth()) @ "/" @ %bot.getMaxHealth() @ "HP", "8564862");
	%minigame.healers++;
	TowerDefenseGroup.add(%bot);
	%minigame.messageAll('', "\c3" @ %this.getPlayerName() @ " \c5has summoned a medic for \c3$" @ %pts @ " of resource" @ (%pts == 1 ? "" : "s") @ "\c5!");
}

function serverCmdCreateHealer(%this)
{
	if(!%this.isBuilder)
		return;

	if(!isObject(%minigame = %this.minigame))
		return;

	if(!isObject(%player = %this.player))
		return;

	if(%minigame.noHealers)
	{
		%this.chatMessage("\c5Sorry, no healers are allowed to be summoned.");
		return;
	}

	if(TD_Healers() > 4)
	{
		%this.chatMessage("\c5Sorry, max healers reached. Hammer healers if you need to spawn one differently.");
		return;
	}

	%pts = 400;
	if(%minigame.TD_Resources < %pts)
	{
		%this.chatMessage("\c5Sorry, you need at least \c3" @ %pts @ " resource" @ (%pts == 1 ? "" : "s") @ " \c5to summon a healer.");
		return;
	}

	%minigame.TD_AddResources(-%pts);

	%datablock = nameToID(PlayerNoJet);

	%bot = new AIPlayer()
	{
		position = %player.getPosition();
		rotation = %player.rotation;
		dataBlock = %datablock;
		isTowerBot = 1;
		minigame = %minigame;
		isHealer = 1;
		range = 100;
		uiName = "Healer";
	};
	%bot.setMaxHealth(300);
	%bot.setShapeNameDistance(20);

	%bot.Accent = "0";
	%bot.AccentColor = "0.900 0 0 1.000";
	%bot.Chest = "0";
	%bot.ChestColor = "0.9 0 0 1";
	%bot.DecalName = "Hoodie";
	%bot.FaceName = "smiley";
	%bot.Hat = "4";
	%bot.HatColor = "0.2 0.2 0.2 1";
	%bot.HeadColor = "1 0.878 0.611 1";
	%bot.Hip = "0";
	%bot.HipColor = "0.9 0 0 1";
	%bot.LArm = "0";
	%bot.LArmColor = "1 1 1 1";
	%bot.LHand = "0";
	%bot.LHandColor = "1 0.878 0.611 1";
	%bot.LLeg = "0";
	%bot.LLegColor = "0.2 0.2 0.2 1";
	%bot.Pack = "0";
	%bot.PackColor = "0.392 0.196 0 1";
	%bot.RArm = "0";
	%bot.RArmColor = "1 1 1 1";
	%bot.RHand = "0";
	%bot.RHandColor = "1 0.878 0.611 1";
	%bot.RLeg = "0";
	%bot.RLegColor = "0.2 0.2 0.2 1";
	%bot.SecondPack = "0";
	%bot.SecondPackColor = "0.2 0.2 0.2 1";
	%bot.player = %bot;
	GameConnection::ApplyBodyParts(%bot);
	GameConnection::ApplyBodyColors(%bot);

	%bot.TD_Loop();
	%bot.setWeapon(findItemByName("Tower Healer"));
	%bot.setRunSpeed(0.75);
	%bot.setShapeName(%bot.uiName @ ", " @ mClampF(%bot.getHealth(), 0, %bot.getMaxHealth()) @ "/" @ %bot.getMaxHealth() @ "HP", "8564862");
	%minigame.healers++;
	TowerDefenseGroup.add(%bot);
	%minigame.messageAll('', "\c3" @ %this.getPlayerName() @ " \c5has summoned a healer for \c3$" @ %pts @ " of resource" @ (%pts == 1 ? "" : "s") @ "\c5!");
}

//Bots have avatars:
//Medic: http://i.imgur.com/aoWYhga.png
//Spammer Tower as an example: http://i.imgur.com/cfyHTiA.png

function TD_Healers()
{
	%count = 0;
	for(%i=0;%i<TowerDefenseGroup.getCount();%i++)
		%count += mFloor(TowerDefenseGroup.getObject(%i).isHealer);
	return %count;
}

function GameConnection::ToggleLight(%this)
{
	%this.useLight = 1;
	serverCmdLight(%this);
}

function GameConnection::TD_Apply(%this)
{
	if(isObject(%player = %this.player))
	{
		%player.isEnemyBot = %this.isEnemyBot; 
		if(isObject(%this.minigame))
			%this.TD_AddItems();

		%this.isForcedToBuild = 0;
		%this.isBuilder = 0;
		%this.canBuild = 0;

		if(%this.getScore() < 0)
			%this.setScore(0);

		if(isFunction(MapChanger_LoadTrack) && $Server::MapChanger::CurrentMap $= "" && !$Server::MapChanger::Changing)
		{
			messageAll('', "No bricks detected. Loading a random map..");
			MapChanger_LoadTrack(findFirstFile($Pref::Server::MapChanger::Path @ "*.bls"));
		}
	}
}

$Pref::Server::TD_Offset = 2;
$Pref::Server::TD_RefundPercent = 30;

function TD_ClientLoop()
{
	cancel($TD::ClientLoop);
	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		if(!%client.isForcedToBuild)
		{
			if(!isObject(%minigame = %client.minigame))
				continue;
		}

		if(isObject(%minigame))
		{
			%diff = %minigame.TD_Difficult;
			if(%diff $= "")
				%diff = "unknown";

			if(%client.isBuilder)
				%msg[%i] = "<font:Arial:18><just:center>\c6Tower: \c3" @ %client.towerName @ " \c7| \c6Building Resources: \c4$" @ mFloor(%minigame.TD_Resources) 
				@ "<br>\c6Round:\c4 " @ %minigame.TD_Round @ "\c6 on " @ %minigame.TD_DifficultColor @ strUpr(%diff)
				@ " \c7| \c6Enemies left: \c3" @ TowerDefenseBotGroup.getLiving() + %minigame.botQueue;
			else if(%client.canUpgradeTowers)
				%msg[%i] = "<font:Arial:18><just:center>\c6Building Resources: \c4$" @ mFloor(%minigame.TD_Resources) 
				@ "<br>\c6Round:\c4 " @ %minigame.TD_Round @ "\c6 on " @ %minigame.TD_DifficultColor @ strUpr(%diff)
				@ " \c7| \c6Enemies left: \c3" @ TowerDefenseBotGroup.getLiving() + %minigame.botQueue;
			else
				%msg[%i] = "<font:Arial:18><just:center>\c6Round:\c4 " @ %minigame.TD_Round @ "\c6 on " @ %minigame.TD_DifficultColor @ strUpr(%diff)
				@ " \c7| \c6Enemies left: \c3" @ TowerDefenseBotGroup.getLiving() + %minigame.botQueue;
		}

		if(%msg[%i] !$= %client.lastUpdateMsg && %msg[%i] !$= "")
		{
			%client.lastUpdateMsg = %msg[%i];
			%client.bottomPrint(%msg[%i], -1, 1);
		}

		if(isObject(%player = %client.player))
		{
			if(%player.getControllingClient() != %client)
			{
				if(isObject(%controlObj = %client.getControlObject()))
					if(%controlObj.getClassName() $= "AIPlayer" && %controlObj.isTowerBot)
					{
						%healthBot[%i] = mFloor(%controlObj.getHealth() / %controlObj.getMaxHealth() * 100);
						if(%controlObj.useControlTime)
						{
							if($Sim::Time - %controlObj.startControlTime > %controlObj.controlTimeEnd)
							{
								%client.setControlObject(%player);
								%controlObj.setCrouching(0);
								%controlObj.lastControlExitTime = $Sim::Time;
								continue;
							}

							%timePrint[%i] = " \c6- Time left: \c4" @ getTimeString(mFloor(%controlObj.controlTimeEnd - ($Sim::Time - %controlObj.startControlTime))) @ "s";
						}

						%msg[%i] = "<font:Arial:18><just:left>\c3" @ %controlObj.uiName @ "<br>\c6Health: \c4" @ %healthBot[%i] @ "%<br>\c7(\c4Press JUMP key to exit control mode\c7)" @ %timePrint[%i];
						if(%msg[%i] !$= %client.lastUpdateCenterMsg && %msg[%i] !$= "")
						{
							%client.lastUpdateCenterMsg = %msg[%i];
							%client.centerPrint(%msg[%i], -1, 1);
						}
					}

				continue;
			}

			if(%client.lastUpdateCenterMsg !$= "")
			{
				%client.lastUpdateCenterMsg = "";
				%client.centerPrint("", -1, 1);
			}

			if(%player.lastTowerMsg !$= "")
			{
				%player.lastTowerMsg = "";
				%client.CenterPrint("", 0.1);
			}

			%mask = $TypeMasks::FxBrickObjectType;
			%col = containerRayCast(%player.getEyePoint(),
				vectorAdd(vectorScale(vectorNormalize(%player.getEyeVector()), 10),
					%player.getEyePoint()),
					%mask,
					%player);

			%hit = firstWord(%col);
			%pos = getWords(%col, 1, 3);

			if(%client.isBuilder)
			{
				if(!isObject(%hit))
				{
					if(isObject(%temp = %player.tempbrick))
					{
						%temp.delete();
						%client.CenterPrint("", 0.1);
						if(!%client.TD_ClientData["DisableHUD"])
							commandToClient(%client, 'TDCMD', "Close", "HUD");
					}

					if(%player.lastTowerMsg !$= "")
					{
						%player.lastTowerMsg = "";
						%client.CenterPrint("", 0.1);
						if(!%client.TD_ClientData["DisableHUD"])
							commandToClient(%client, 'TDCMD', "Close", "HUD");
					}

					continue;
				}

				if(%hit.isTowerBrick && %hit.isBaseTowerBrick && !isObject(%player.getMountedImage(0)))
				{
					if(isObject(%temp = %player.tempbrick))
					{
						%temp.delete();
						%client.CenterPrint("", 0.1);
					}

					%client.TD_SendTowerInfo(%hit);
					continue;
				}

				if(%hit.isTowerBrick)
				{
					if(isObject(%temp = %player.tempbrick))
					{
						%temp.delete();
						%client.CenterPrint("", 0.1);
					}

					continue;
				}

				if(%client.canBuild)
				{
					%data = %client.getTowerDB();
					%do = ($Sim::Time - %client.lastModeTime < 0.5);
					%canPlant = true;
					%tower = %client.getTower();

					%minLevel = %tower.minLevel;
					if(%tower.minLevel[%minigame.TD_Difficult] > 0)
						%minLevel = %tower.minLevel[%minigame.TD_Difficult];

					if(%minigame.TD_Round < %minLevel)
						%canPlant = false;
					else if(TowerDefenseGroup.getTowerCount(%tower.uiName) > %tower.maxTowers && %tower.maxTowers > 0)
						%canPlant = false;

					%cost = %tower.cost * %minigame.TD_CostMultiplier;

					if(%cost > %minigame.TD_Resources && !%this.canBuildInfiniteTowers)
						%canPlant = false;

					if(%tower.adminLevel > %adminLevel)
						%canPlant = false;

					if(!isObject(%temp = %player.tempbrick))
					{
						if($Pref::Server::TD_UseGrid)
							%position = mFloatLength(mFloatLength(getWord(%pos, 0) / (%data.brickSizeX / 2), 0) * (%data.brickSizeX / 2), 1) SPC 
								mFloatLength(mFloatLength(getWord(%pos, 1) / (%data.brickSizeY / 2), 0) * (%data.brickSizeY / 2), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);
						else if($Pref::Server::TD_SnapBricks && %hit.TD_isTowerPlate())
							%position = mFloatLength(getWord(%hit.getPosition(), 0), 1) SPC 
								mFloatLength(getWord(%hit.getPosition(), 1), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);
						else
							%position = mFloatLength(getWord(%pos, 0), 1) SPC 
								mFloatLength(getWord(%pos, 1), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);

						%brick = new fxDtsBrick()
						{
							position = %position;
							rotation = "1 0 0 0";
							//angleID = %this.angleID;
							colorID = 0;
							colorFXID = 0;
							shapeFXID = 0;
							//printID = %this.printID;
							dataBlock = %data;
							isTowerBrick = 1;
							isPlanted = 0;
							plantClient = %client;
						};
						%client.brickgroup.add(%brick);
						%player.tempbrick = %brick;

						%brick.setTransform(%position SPC %brick.rotation);
						%brick.lastPos = %position;

						if(%brick.canPlantTower(%client, %do) && %brick.isOnGrid() && %canPlant)
							%brick.setColor(findclosestcolor("0 255 0 255"));
						else
							%brick.setColor(findclosestcolor("255 0 0 255"));
					}
					else if(%temp.getDatablock() == %data)
					{
						if($Pref::Server::TD_UseGrid)
							%position = mFloatLength(mFloatLength(getWord(%pos, 0) / (%data.brickSizeX / 2), 0) * (%data.brickSizeX / 2), 1) SPC 
								mFloatLength(mFloatLength(getWord(%pos, 1) / (%data.brickSizeY / 2), 0) * (%data.brickSizeY / 2), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);
						else if($Pref::Server::TD_SnapBricks && %hit.TD_isTowerPlate())
							%position = mFloatLength(getWord(%hit.getPosition(), 0), 1) SPC 
								mFloatLength(getWord(%hit.getPosition(), 1), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);
						else
							%position = mFloatLength(getWord(%pos, 0), 1) SPC 
								mFloatLength(getWord(%pos, 1), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);

						//%position = TD_setPos(%position);

						if(%position !$= %temp.lastPos)
						{
							%temp.setTransform(%position SPC %temp.rotation);
							%temp.lastPos = %position;

							if(%temp.canPlantTower(%client, %do) && %temp.isOnGrid() && %canPlant)
								%temp.setColor(findclosestcolor("0 255 0 255"));
							else
								%temp.setColor(findclosestcolor("255 0 0 255"));
						}
					}
					else if(%temp.getDatablock() != %data && %temp.isTowerBrick)
					{
						%temp.setDatablock(%data);
						if($Pref::Server::TD_UseGrid)
							%position = mFloatLength(mFloatLength(getWord(%pos, 0) / (%data.brickSizeX / 2), 0) * (%data.brickSizeX / 2), 1) SPC 
								mFloatLength(mFloatLength(getWord(%pos, 1) / (%data.brickSizeY / 2), 0) * (%data.brickSizeY / 2), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);
						else if($Pref::Server::TD_SnapBricks && %hit.TD_isTowerPlate())
							%position = mFloatLength(getWord(%hit.getPosition(), 0), 1) SPC 
								mFloatLength(getWord(%hit.getPosition(), 1), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);
						else
							%position = mFloatLength(getWord(%pos, 0), 1) SPC 
								mFloatLength(getWord(%pos, 1), 1) SPC 
								mFloatLength(getWord(%pos, 2) + (%data.brickSizeZ / 10), 1);

						//%position = TD_setPos(%position);

						if(%position !$= %temp.lastPos)
						{
							%temp.setTransform(%position SPC %temp.rotation);
							%temp.lastPos = %position;

							if(%temp.canPlantTower(%client, %do) && %temp.isOnGrid() && %canPlant)
								%temp.setColor(findclosestcolor("0 255 0 255"));
							else
								%temp.setColor(findclosestcolor("255 0 0 255"));
						}
					}
					//%client.centerPrint(%nPos NL %data.brickSizeZ, 2);
				}
			}
			else if(isObject(%hit))
				%client.TD_SendTowerInfo(%hit);
		}
	}
	$TD::ClientLoop = schedule(50, 0, TD_ClientLoop);
}
schedule(0, 0, TD_ClientLoop);

function serverCmdControl(%this)
{
	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "dead")
		return;

	//If we are spying on someone, or controlling something else
	if(!isObject(%player.getControllingClient()))
		return;

	%mask = $TypeMasks::FxBrickObjectType;
	%col = containerRayCast(%player.getEyePoint(),
		vectorAdd(vectorScale(vectorNormalize(%player.getEyeVector()), 10),
			%player.getEyePoint()),
			%mask,
			%player);

	%hit = firstWord(%col);
	%pos = getWords(%col, 1, 3);

	if(isObject(%bot = %hit.bot))
	{
		if(%bot.isTowerBot && %bot.getClassName() $= "AIPlayer")
		{
			if(!isObject(%controlClient = %bot.getControllingClient()))
			{
				%cost = mClampF(mFloor($Pref::Server::TD_ControlCost * (isObject(%minigame = %this.minigame) ? %minigame.TD_CostMultiplier * %bot.cost * 0.001 : (%bot.cost * 0.01))), 1, 10000);
				if(%this.getScore() >= %cost)
				{
					if(%bot.lastControlExitTime > 0)
						if($Sim::Time - %bot.lastControlExitTime < $Pref::Server::TD_ControlTime / 2)
							%noTime = 1;

					if(!%noTime)
					{
						%time = $Pref::Server::TD_ControlTime;
						if(%bot.tower_maxControlTime > 0)
							%time = %bot.tower_maxControlTime;

						%hit.TD_ClientControl(%this, %time);
						%this.incScore(-%cost);
					}
					else
					{
						%time = mFloatLength($Pref::Server::TD_ControlTime / 2 - ($Sim::Time - %bot.lastControlExitTime), 1);
						%this.chatMessage("\c5You need at least " @ %time @ " second" @ (%time != 1 ? "s" : "") @ " to control this tower again.");
					}
				}
				else
					%this.chatMessage("\c5You need at least " @ %cost @ " point" @ (%cost != 1 ? "s" : "") @ " to control this tower.");
			}
			else
				%this.chatMessage("\c5Sorry, \c3" @ %controlClient.getPlayerName() @ " \c5is already controlling this tower.");
		}
		else
			%this.chatMessage("\c5That's not a tower!");
	}
	else
		%this.chatMessage("\c5You need to look at a tower!");
}

if($Pref::Server::TD_ControlCost <= 0)
	$Pref::Server::TD_ControlCost = 8;

if($Pref::Server::TD_ControlTime <= 0)
	$Pref::Server::TD_ControlTime = 60;

function GameConnection::TD_SendTowerInfo(%this, %tower)
{
	if(!isObject(%player = %this.player))
		return;

	if(%player.getState() $= "dead")
		return;

	//If we are spying on someone, or controlling something else
	if(!isObject(%player.getControllingClient()))
		return;

	if(isObject(%player.getMountedImage(0)))
		return;

	if(!isObject(%tower))
		return;

	%health = 0;
	%player.lastTower = -1;

	if(%tower.getClassName() $= "AIPlayer")
	{
		%bot = %tower;
		%tower = %bot.baseTower;
	}
	else if(isObject(%bot = %tower.bot))
		%bot = %tower.bot;
	else
		return;

	//if($TD::ClientLink !$= "" && %this.isBuilder)
	//	%guiPrint = "\c0Sorry, you need the GUI to configure this tower";

	//if(%this.TD_Client && %this.isBuilder)
	//	%guiPrint = "\c6(\c4Use your sit key (or /sit) to open the Tower GUI\c6)";

	if(!%bot.doNotUpgrade)
	{
		if(%bot.TDData["Path"] $= "")
			%guiPrint = "\c6(/TD Upgrade \c3Path\c3(" @ %bot.TD_UpgradePathsString() @ ")\c6)";
		else
			%guiPrint = "\c6(/TD Upgrade - Upgrade this tower\c6)";
	}
	else
		%guiPrint = "\c6(/TD Upgrade - \c0Upgrades not available\c6)";

	if(%guiPrint $= "")
	{
		if(!isObject(%controlClient = %bot.getControllingClient()))
		{
			if(%bot.lastControlExitTime > 0)
				if($Sim::Time - %bot.lastControlExitTime < 15)
					%noTime = 1;

			if(!%noTime)
			{
				%cost = mClampF(mFloor($Pref::Server::TD_ControlCost * (isObject(%minigame = %this.minigame) ? %minigame.TD_CostMultiplier * %bot.cost * 0.001 : (%bot.cost * 0.01))), 1, 10000);
				%controlHint = "\c7(\c4Look at this tower and say /Control to control this tower \c6- \c2Costs " @ %cost @ " point" @ (%cost != 1 ? "s" : "") @ "\c7)";
			}
			else
			{
				%time = mFloatLength(15 - ($Sim::Time - %bot.lastControlExitTime), 1);
				%controlHint = "\c7(\c5You need at least " @ %time @ " second" @ (%time != 1 ? "s" : "") @ " to control this tower again\c7)";
			}
		}
		else
			%controlHint = "\c7(\c0" @ %controlClient.getPlayerName() @ " is controlling this tower\c7)";
	}
	else
	{
		if(!isObject(%controlClient = %bot.getControllingClient()))
		{
			if(%bot.lastControlExitTime > 0)
				if($Sim::Time - %bot.lastControlExitTime < 15)
					%noTime = 1;

			if(!%noTime)
			{
				%cost = mClampF(mFloor($Pref::Server::TD_ControlCost * (isObject(%minigame = %this.minigame) ? %minigame.TD_CostMultiplier * %bot.cost * 0.001 : (%bot.cost * 0.01))), 1, 10000);
				%controlHint = "<br>\c7(\c4Look at this tower and say /Control to control this tower \c6- \c2Costs " @ %cost @ " point" @ (%cost != 1 ? "s" : "") @ "\c7)";
			}
			else
			{
				%time = mFloatLength(15 - ($Sim::Time - %bot.lastControlExitTime), 1);
				%controlHint = "<br>\c7(\c5You need at least " @ %time @ " second" @ (%time != 1 ? "s" : "") @ " to control this tower again\c7)";
			}
		}
		else
			%controlHint = "<br>\c7(\c0" @ %controlClient.getPlayerName() @ " is controlling this tower\c7)";
	}

	%health = mFloor(%bot.getHealth() / %bot.getMaxHealth() * 100);
	%msg = "<font:arial:" @ $Pref::Server::TD_FontSize @ "><just:left>\c6Tower: \c4" @ %tower.tower_name @ " \c7(\c4" @ %health @ "\c6%\c7)<br>" @ %guiPrint @ %controlHint;
	if(%msg !$= %player.lastTowerMsg)
	{
		%player.lastTowerMsgTime = $Sim::Time;
		%player.lastTowerMsg = %msg;
		%this.centerPrint(%msg, -1);
	}
	else if($Sim::Time - %player.lastTowerMsgTime > 1)
	{
		%player.lastTowerMsgTime = $Sim::Time;
		%player.lastTowerMsg = %msg;
		%this.centerPrint(%msg, -1);
	}

	%player.lastTower = %bot;
}

//For creating new towers
function GameConnection::TD_SaveAvatar(%this, %name, %mode)
{
	%io = new FileObject();
	%io.openForWrite("config/server/TD/Cache/Avatar" @ %this.getBLID() @ "_" @ getSafeVariableName(%name) @ ".txt");
	switch(%mode)
	{
		case 1: //Coding stuff
			%io.writeLine("%bot.Accent = \"" @ %this.Accent @ "\";");
			%io.writeLine("%bot.AccentColor = \"" @ %this.AccentColor @ "\";");
			%io.writeLine("%bot.Chest = \"" @ %this.Chest @ "\";");
			%io.writeLine("%bot.ChestColor = \"" @ %this.ChestColor @ "\";");
			%io.writeLine("%bot.DecalName = \"" @ %this.DecalName @ "\";");
			%io.writeLine("%bot.FaceName = \"" @ %this.FaceName @ "\";");
			%io.writeLine("%bot.Hat = \"" @ %this.Hat @ "\";");
			%io.writeLine("%bot.HatColor = \"" @ %this.HatColor @ "\";");
			%io.writeLine("%bot.HeadColor = \"" @ %this.HeadColor @ "\";");
			%io.writeLine("%bot.Hip = \"" @ %this.Hip @ "\";");
			%io.writeLine("%bot.HipColor = \"" @ %this.HipColor @ "\";");
			%io.writeLine("%bot.LArm = \"" @ %this.LArm @ "\";");
			%io.writeLine("%bot.LArmColor = \"" @ %this.LArmColor @ "\";");
			%io.writeLine("%bot.LHand = \"" @ %this.LHand @ "\";");
			%io.writeLine("%bot.LHandColor = \"" @ %this.LHandColor @ "\";");
			%io.writeLine("%bot.LLeg = \"" @ %this.LLeg @ "\";");
			%io.writeLine("%bot.LLegColor = \"" @ %this.LLegColor @ "\";");
			%io.writeLine("%bot.Pack = \"" @ %this.Pack @ "\";");
			%io.writeLine("%bot.PackColor = \"" @ %this.PackColor @ "\";");
			%io.writeLine("%bot.RArm = \"" @ %this.RArm @ "\";");
			%io.writeLine("%bot.RArmColor = \"" @ %this.RArmColor @ "\";");
			%io.writeLine("%bot.RHand = \"" @ %this.RHand @ "\";");
			%io.writeLine("%bot.RHandColor = \"" @ %this.RHandColor @ "\";");
			%io.writeLine("%bot.RLeg = \"" @ %this.RLeg @ "\";");
			%io.writeLine("%bot.RLegColor = \"" @ %this.RLegColor @ "\";");
			%io.writeLine("%bot.SecondPack = \"" @ %this.SecondPack @ "\";");
			%io.writeLine("%bot.SecondPackColor = \"" @ %this.SecondPackColor @ "\";");

		default: //Applying
			%io.writeLine("\"customAvatar 1\" TAB ");
			%io.writeLine("\"Accent " @ %this.Accent @ "\" TAB ");
			%io.writeLine("\"AccentColor " @ %this.AccentColor @ "\" TAB ");
			%io.writeLine("\"Chest " @ %this.Chest @ "\" TAB ");
			%io.writeLine("\"ChestColor " @ %this.ChestColor @ "\" TAB ");
			%io.writeLine("\"DecalName " @ %this.DecalName @ "\" TAB ");
			%io.writeLine("\"FaceName " @ %this.FaceName @ "\" TAB ");
			%io.writeLine("\"Hat " @ %this.Hat @ "\" TAB ");
			%io.writeLine("\"HatColor " @ %this.HatColor @ "\" TAB ");
			%io.writeLine("\"HeadColor " @ %this.HeadColor @ "\" TAB ");
			%io.writeLine("\"Hip " @ %this.Hip @ "\" TAB ");
			%io.writeLine("\"HipColor " @ %this.HipColor @ "\" TAB ");
			%io.writeLine("\"LArm " @ %this.LArm @ "\" TAB ");
			%io.writeLine("\"LArmColor " @ %this.LArmColor @ "\" TAB ");
			%io.writeLine("\"LHand " @ %this.LHand @ "\" TAB ");
			%io.writeLine("\"LHandColor " @ %this.LHandColor @ "\" TAB ");
			%io.writeLine("\"LLeg " @ %this.LLeg @ "\" TAB ");
			%io.writeLine("\"LLegColor " @ %this.LLegColor @ "\" TAB ");
			%io.writeLine("\"Pack " @ %this.Pack @ "\" TAB ");
			%io.writeLine("\"PackColor " @ %this.PackColor @ "\" TAB ");
			%io.writeLine("\"RArm " @ %this.RArm @ "\" TAB ");
			%io.writeLine("\"RArmColor " @ %this.RArmColor @ "\" TAB ");
			%io.writeLine("\"RHand " @ %this.RHand @ "\" TAB ");
			%io.writeLine("\"RHandColor " @ %this.RHandColor @ "\" TAB ");
			%io.writeLine("\"RLeg " @ %this.RLeg @ "\" TAB ");
			%io.writeLine("\"RLegColor " @ %this.RLegColor @ "\" TAB ");
			%io.writeLine("\"SecondPack " @ %this.SecondPack @ "\" TAB ");
			%io.writeLine("\"SecondPackColor " @ %this.SecondPackColor @ "\"");
	}
	%io.close();
	%io.delete();
}

function TD_isPos(%position)
{
	if(getWordCount(%position) != 3)
		return 0;

	%x = mAbs(getWord(%position, 0));
	%y = mAbs(getWord(%position, 1));
	%z = mAbs(getWord(%position, 2));

	if(%x - mFloor(%x) != 0.5 && %x - mFloor(%x) != 0)
		return 0;

	if(%y - mFloor(%y) != 0.5 && %y - mFloor(%y) != 0)
		return 0;

	if(%z - mFloor(%z) != 0.5 && %z - mFloor(%z) != 0)
		return 0;

	return 1;
}

function GameConnection::setBuilderOption(%this, %type)
{
	if($Sim::Time - %this.lastBuilderOption < 0.1)
		return;

	%this.lastBuilderOption = $Sim::Time;
	%minigame = getMinigameFromObject(%this.minigame);
	if(!isObject(%minigame))
		return;

	switch$(%type)
	{
		case "Left":
			if(mClampF(%this.buildNum - 1, 0, TowerGroup.getCount()-1) <= 0)
				%this.buildNum = TowerGroup.getCount()-1;
			else
				%this.buildNum = mClampF(%this.buildNum--, 1, TowerGroup.getCount()-1);

		case "Right":
			if(mClampF(%this.buildNum + 1, 0, TowerGroup.getCount()) >= TowerGroup.getCount())
				%this.buildNum = 1;
			else
				%this.buildNum = mClampF(%this.buildNum++, 1, TowerGroup.getCount()-1);

		default:
			doNothing();
	}
	%tower = TowerGroup.getObject(mFloor(%this.buildNum));
	if(!isObject(%tower))
		return;

	if(mClampF(%this.buildNum - 1, 0, TowerGroup.getCount()-1) <= 0)
		%prevTower = TowerGroup.getObject(TowerGroup.getCount() - 1);
	else
		%prevTower = TowerGroup.getObject(mClampF(%this.buildNum - 1, 1, TowerGroup.getCount() - 1));

	if(mClampF(%this.buildNum + 1, 0, TowerGroup.getCount()) >= TowerGroup.getCount())
		%nextTower = TowerGroup.getObject(1);
	else
		%nextTower = TowerGroup.getObject(mClampF(%this.buildNum + 1, 1, TowerGroup.getCount() - 1));

	%this.setTower(%tower);
	if(%this.TD_Client)
		if(!%this.TD_ClientData["DisableHUD"])
		{
			commandToClient(%this, 'TDCMD', "Update", "TowerName", %tower.uiName, %nextTower.uiName, %prevTower.uiName);
			commandToClient(%this, 'TDCMD', "Update", "TowerText", %tower.description);
			commandToClient(%this, 'TDCMD', "Update", "TowerCost", mFloatLength(%tower.cost * %minigame.TD_CostMultiplier, 2), %minigame.TD_Resources);
			commandToClient(%this, 'TDCMD', "Update", "TowerRange", %tower.range @ " meters"); //Just numbers for now
			commandToClient(%this, 'TDCMD', "Update", "Tower_SetMisc", %tower.TD_Damage, "\c6Unknown", %tower.shootWaitTime); //Just numbers for now
			commandToClient(%this, 'TDCMD', "Update", "Tower_SetHeight", %tower.height); //Just numbers for now
			commandToClient(%this, 'TDCMD', "Open", "HUD");
			return;
		}

	%minLevel = %tower.minLevel;
	if(%tower.minLevel[%minigame.TD_Difficult] > 0)
		%minLevel = %tower.minLevel[%minigame.TD_Difficult];

	%msg = "<\c4Selection\cr><br>";
	%msg = %msg @ " <\c4-- \c7" @ %prevTower.uiName @ " \c6" @ %tower.uiName @ " \c7" @ %nextTower.uiName @ " \c4--\cr><br>";

	if(%minigame.TD_Round < %minLevel && !%this.allTowersUnlocked)
	{
		%isLocked = 1;
		%msg = %msg @ "\c0This is locked until \c3level " @ %minLevel @ "\c0.";	
	}
	else if(TowerDefenseGroup.getTowerCount(%tower.uiName) > %tower.maxTowers && %tower.maxTowers > 0)
	{
		%isLocked = 1;
		%msg = %msg @ "\c0Maximum towers reached.";	
	}
	else
	{
		%msg = %msg @ "\c6" @ %tower.description;
		if(%tower.cost * %minigame.TD_CostMultiplier > 0)
		{
			if(%this.canBuildAnyTower)
				%msg = %msg @ "<br>\c6Cost: \c2FREE";
			else
				%msg = %msg @ "<br>\c6Cost: \c3" @ mFloatLength(%tower.cost * %minigame.TD_CostMultiplier, 2);
		}
	}

	%msg = trim(%msg);
	%this.centerPrint(%msg, 10);
	%this.lastModeTime = $Sim::Time;
	if(isObject(%player = %this.player))
	{
		if(isObject(%temp = %player.tempBrick))
		{
			if(%temp.canPlantTower(%this, 1) && %temp.isOnGrid() && !%isLocked)
				%temp.setColor(findclosestcolor("0 255 0 255"));
			else
				%temp.setColor(findclosestcolor("255 0 0 255"));
		}
	}
}

//End

$Pref::Server::TD_Debug = 0;

function TD_Debug(%msg)
{
	if($Pref::Server::TD_Debug == 1)
		echo("TD::Debug - " @ %msg);
	if($Pref::Server::TD_Debug >= 1)
		announce("\c6TD::Debug - \c3" @ %msg);
}

function Player::lookAtPosition(%pl, %pos)
{
	if(getWordCount(%pos) == 1)
	{
		if(isObject(%pos))
			%pos = %pos.getPosition();
		else
			return;
	}
	else if(getWordCount(%pos) != 3)
		%pos = "0 0 0";

	%loc = %pl.getPosition();
	%delta = vectorSub(%pos, %loc);
	%deltaX = getWord(%delta, 0);
	%deltaY = getWord(%delta, 1);
	%deltaZ = getWord(%delta, 2);
	%deltaXYHyp = vectorLen(%deltaX SPC %deltaY SPC 0);

	%rotZ = mAtan(%deltaX, %deltaY) * -1; 
	%rotX = mAtan(%deltaZ, %deltaXYHyp);

	%ang = eulerRadToMatrix(%rotX SPC 0 SPC %rotZ); //this function should be called eulerToAngleAxis...
	%main = %loc SPC %ang;
	//%loc = %pl.getPosition();
	//%diff = vectorSub(%pos, %pl.getPosition());
	//%ang = mAtan(getWord(%diff, "0"), getWord(%diff, "1"));
	//%main = %loc @ " 0 0 1 " @ %ang;
	%pl.setTransform(%main);
}

function Player::lookAtBrick(%this, %brickName)
{
	if(!isObject(%client = %this.client))
		return;

	%brick = nameToID("_" @ getSafeVariableName(%brickName));
	if(!isObject(%brick))
	{
		%client.chatMessage("Invalid brick: " @ getSafeVariableName(%brickName));
		return;
	}

	%this.lookAtPosition(%brick);
}
registerOutputEvent("Player", "lookAtBrick", "string 50 50");

//Support

function findclosestcolor(%x)
{
	%x = getColorF(%x);
	for(%a=0; %a<64; %a++)
	{
		%match = mabs(getword(getcoloridtable(%a),0) - getword(%x,0)) + mabs(getword(getcoloridtable(%a),1) - getword(%x,1)) + mabs(getword(getcoloridtable(%a),2) - getword(%x,2)) + mabs(getword(getcoloridtable(%a),3) - getword(%x,3));

		if(%match < %bestmatch || %bestmatch $= "")
		{
			%bestmatch = %match;
			%bestid = %a;
		}
	}
	return %bestid;
}

function mRound(%num,%dec)
{
	%ten = mPow(10,%dec);
	%five = 5 / (%ten * 10);
	return (mFloor((%num + %five) * %ten) / %ten);
}

function Player::turnToFace(%pl, %pos)
{
	if(!isObject(%pl))
		return;
	
	%pl.lookAtPosition(%pos);
}

function Player::lookAtPosition(%pl, %pos)
{
	if(!isObject(%pl))
		return;

	if(getWordCount(%pos) == 1)
	{
		if(isObject(%pos))
			%pos = %pos.getPosition();
		else
			return;
	}
	else if(getWordCount(%pos) != 3)
		%pos = "0 0 0";

	%loc = %pl.getPosition();
	%delta = vectorSub(%pos, %loc);
	%deltaX = getWord(%delta, 0);
	%deltaY = getWord(%delta, 1);
	%deltaZ = getWord(%delta, 2);
	%deltaXYHyp = vectorLen(%deltaX SPC %deltaY SPC 0);

	%rotZ = mAtan(%deltaX, %deltaY) * -1; 
	%rotX = mAtan(%deltaZ, %deltaXYHyp);

	%ang = eulerRadToMatrix(%rotX SPC 0 SPC %rotZ); //this function should be called eulerToAngleAxis...
	%main = %loc SPC %ang;
	//%loc = %pl.getPosition();
	//%diff = vectorSub(%pos, %pl.getPosition());
	//%ang = mAtan(getWord(%diff, "0"), getWord(%diff, "1"));
	//%main = %loc @ " 0 0 1 " @ %ang;
	%pl.setTransform(%main);
}

if(!$TD::LoadedCommon)
{
	$TD::LoadedCommon = 1;
	TD_loadFilePath("add-ons/Gamemode_Tower_Defense/Common/*");
}

function TD_LoadCore()
{
	exec("add-ons/Gamemode_Tower_Defense/server.cs");
}

function TD_LoadAll()
{
	exec("add-ons/Gamemode_Tower_Defense/server.cs");
	TD_loadFilePath("add-ons/Gamemode_Tower_Defense/Common/*");
	TD_loadFilePath("add-ons/Gamemode_Tower_Defense/Support/*");
}

function TD_LoadSupport()
{
	TD_loadFilePath("add-ons/Gamemode_Tower_Defense/Support/*");
}

function TD_LoadCommon()
{
	TD_loadFilePath("add-ons/Gamemode_Tower_Defense/Common/*");
}
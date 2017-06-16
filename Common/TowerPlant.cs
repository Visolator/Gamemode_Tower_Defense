//Useful

if(!isObject(BrickTextEmptyShape))
	datablock StaticShapeData(BrickTextEmptyShape)
	{
		shapefile = "base/data/shapes/empty.dts";
	};

function fxDTSBrick::TP_UpBrick(%this)
{
	%BrPosXYZ = %this.getPosition();
	%BrWrldBox = %this.getWorldBox();
					
	//Determine brick size to use for container search below
	%BrWBx = getWord(%BrWrldBox,3) - getWord(%BrWrldBox,0);
	%BrWBy = getWord(%BrWrldBox,4) - getWord(%BrWrldBox,1);
	%BrWBz = getWord(%BrWrldBox,5) - getWord(%BrWrldBox,2);
					
	//Run a container search to see if brick is within a ModTer brick
	%BrSizeXYZ = %BrWBx - 0.1 SPC %BrWBy - 0.1 SPC %BrWBz - 0.1;
	initContainerBoxSearch(%BrPosXYZ,vectorAdd(%BrSizeXYZ, "0 0 -0.2"),$TypeMasks::FxBrickAlwaysObjectType);
	
	%failSafe = 0;
	//Check all bricks within container search area (failsafe is just a precaution, but not necessary)
	while(isObject(%TmpBrObj = containerSearchNext()) && %failSafe++ <= 1000)
	{
		//If brick has the ModTer print aspect ratio, it's probably a ModTer brick ("%TmpBrObj != %brick" prevents issues with planting ModTer bricks)
		if((%TmpBrObj.isRaycasting() || %TmpBrObj.isRendering()) && %TmpBrObj != %this)
			return %TmpBrObj;
	}

	return -1;
}

function fxDTSBrick::TP_DownBrick(%this)
{
	%BrPosXYZ = %this.getPosition();
	%BrWrldBox = %this.getWorldBox();
					
	//Determine brick size to use for container search below
	%BrWBx = getWord(%BrWrldBox,3) - getWord(%BrWrldBox,0);
	%BrWBy = getWord(%BrWrldBox,4) - getWord(%BrWrldBox,1);
	%BrWBz = getWord(%BrWrldBox,5) - getWord(%BrWrldBox,2);
					
	//Run a container search to see if brick is within a ModTer brick
	%BrSizeXYZ = %BrWBx - 0.1 SPC %BrWBy - 0.1 SPC %BrWBz - 0.1;
	initContainerBoxSearch(%BrPosXYZ,vectorAdd(%BrSizeXYZ, "0 0 0.2"),$TypeMasks::FxBrickAlwaysObjectType);
	
	%failSafe = 0;
	//Check all bricks within container search area (failsafe is just a precaution, but not necessary)
	while(isObject(%TmpBrObj = containerSearchNext()) && %failSafe++ <= 1000)
	{
		//If brick has the ModTer print aspect ratio, it's probably a ModTer brick ("%TmpBrObj != %brick" prevents issues with planting ModTer bricks)
		if((%TmpBrObj.isRaycasting() || %TmpBrObj.isRendering()) && %TmpBrObj != %this)
			return %TmpBrObj;
	}

	return -1;
}

function fxDTSBrick::TD_ChainKill(%this)
{
	if(isObject(%brick = %this.TP_UpBrick()))
	{
		%brick.TD_ChainKill();
		%brick.schedule(0, killBrick);
	}

	if(isObject(%brick = %this.TP_DownBrick()))
	{
		%brick.TD_ChainKill();
		%brick.schedule(0, killBrick);
	}
}

//End

//DESERT $Pref::Server::TD_AntiPlant = "135 131 126 255" TAB "186 137 66 255";
$Pref::Server::TD_AntiPlant = "66 63 61 255" TAB "249 249 249 255";
$Pref::Server::TD_PlantOnly = "20 20 18 255";

$Pref::Server::TD_PlantMode = "Only";

function fxDTSBrick::TD_isTowerPlate(%this, %client)
{
	switch$($Pref::Server::TD_PlantMode)
	{
		case "Only":
			for(%i = 0; %i < getFieldCount($Pref::Server::TD_PlantOnly); %i++)
			{
				%colorID = findclosestcolor(getField($Pref::Server::TD_PlantOnly, %i));
				if(%colorID == %this.getColorID())
					%colorFound = 1;
			}

			if(!%colorFound)
				return false;

		case "Anti":
			for(%i = 0; %i < getFieldCount($Pref::Server::TD_AntiPlant); %i++)
			{
				%colorID = findclosestcolor(getField($Pref::Server::TD_AntiPlant, %i));
				if(%colorID == %this.getColorID())
					return false;
			}

		default:
			//Allow it
	}

	return true;
}

function fxDTSBrick::canPlantTower(%this, %client, %ignore)
{
	if(!isObject(%client))
		return false;

	if(!isObject(%player = %client.player))
		return false;

	if(isObject(%downBrick = %this.TP_DownBrick()))
	{
		if(%downBrick.isTowerBrick)
			return false;

		switch$($Pref::Server::TD_PlantMode)
		{
			case "Only":
				for(%i = 0; %i < getFieldCount($Pref::Server::TD_PlantOnly); %i++)
				{
					%colorID = findclosestcolor(getField($Pref::Server::TD_PlantOnly, %i));
					if(%colorID == %downBrick.getColorID())
						%colorFound = 1;
				}

				if(!%colorFound)
				{
					if(!%ignore)
						%client.TD_PlantError("\c5Can't plant here.<br>\c6Invalid area.", 3);
					return false;
				}

			case "Anti":
				for(%i = 0; %i < getFieldCount($Pref::Server::TD_AntiPlant); %i++)
				{
					%colorID = findclosestcolor(getField($Pref::Server::TD_AntiPlant, %i));
					if(%colorID == %downBrick.getColorID())
					{
						if(!%ignore)
							%client.TD_PlantError("\c5Can't plant here.<br>\c6Invalid area.", 3);
						return false;
					}
				}

			default:
				//Allow it
		}

		if(TD_PathHandler.nodes.isMember(%downBrick))
		{
			if(!%ignore)
				%client.TD_PlantError("\c5Can't plant here.<br>\c6You cannot plant on a node.", 3);
			return false;
		}
	}
	else
	{
		if(!%ignore)
			%client.TD_PlantError("\c5Can't plant here.<br>\c6Invalid area.", 3);
		return false;
	}

	if(isObject(%upBrick = %this.TP_UpBrick()))
		return false;

	//Next search is to see if anything is already there

	%BrPosXYZ = %this.getPosition();
	%BrWrldBox = %this.getWorldBox();
					
	//Determine brick size to use for container search below
	%BrWBx = getWord(%BrWrldBox,3) - getWord(%BrWrldBox,0);
	%BrWBy = getWord(%BrWrldBox,4) - getWord(%BrWrldBox,1);
	%BrWBz = getWord(%BrWrldBox,5) - getWord(%BrWrldBox,2);
					
	//Run a container search to see if brick is within a ModTer brick
	%BrSizeXYZ = %BrWBx - 0.1 SPC %BrWBy - 0.1 SPC %BrWBz - 0.1;
	initContainerBoxSearch(%BrPosXYZ, %BrSizeXYZ, $TypeMasks::FxBrickAlwaysObjectType | $TypeMasks::PlayerObjectType);
	
	%failSafe = 0;
	//Check all bricks within container search area (failsafe is just a precaution, but not necessary)
	%ye = 1;
	while(isObject(%TmpBrObj = containerSearchNext()) && %failSafe++ <= 1000)
	{
		//If brick has the ModTer print aspect ratio, it's probably a ModTer brick ("%TmpBrObj != %brick" prevents issues with planting ModTer bricks)
		if(%TmpBrObj != %this && %TmpBrObj != %player)
		{
			if(%TmpBrObj.getClassName() $= "AIPlayer" && %TmpBrObj.isHealer)
			{
				if(!%ignore)
					%client.TD_PlantError("\c5Can't plant here.<br>\c6A medic is in the way.", 3);
			}
			else if(%TmpBrObj.getClassName() $= "AIPlayer")
			{
				if(!%ignore)
					%client.TD_PlantError("\c5Can't plant here.<br>\c6Some stupid bot is in the way.", 3);
			}
			else
				%ye = 0;

			if(%ye)
				return false;
		}
	}
					
	//Run a container search to see if brick is within a ModTer brick
	%BrSizeXYZ = %BrWBx - 0.1 SPC %BrWBy - 0.1 SPC %BrWBz - 0.1;
	%BrPosXYZ = vectorSub(%this.getPosition(), "0 0 0.1");
	initContainerBoxSearch(%BrPosXYZ, %BrSizeXYZ, $TypeMasks::FxBrickAlwaysObjectType);
	
	%failSafe = 0;
	//Check all bricks within container search area (failsafe is just a precaution, but not necessary)
	while(isObject(%TmpBrObj = containerSearchNext()) && %failSafe++ <= 1000)
	{
		//If brick has the ModTer print aspect ratio, it's probably a ModTer brick ("%TmpBrObj != %brick" prevents issues with planting ModTer bricks)
		if(%TmpBrObj != %this)
		{
			%groundBrick = %TmpBrObj;
			%bricks++;
		}
	}

	if(%bricks > 1 || %bricks <= 0)
	{
		if(!%ignore)
			%client.TD_PlantError("\c5Can't plant here.<br>\c6Cannot be in between grids.<br>\c5Found " @ %bricks @ " brick(s)", 3);
		return false;
	}

	if(isObject(%groundBrick))
	{
		if(vectorDist(getWords(%groundBrick.getPosition(), 0, 1) @ " 0", getWords(%this.getPosition(), 0, 1) @ " 0") != 0)
		{
			if(!%ignore)
				%client.TD_PlantError("\c5Can't plant here.<br>\c6Grid isn't correct.", 3);
			return false;
		}
	}

	%tower = %client.TD_Tower();
	if(isObject(%tower))
	{
		if(%tower.height >= 0 && %tower.maxTowers > 0)
		{
			if(TowerDefenseGroup.getTowerCount(%tower.uiName) >= %tower.maxTowers)
			{
				if(!%ignore)
					%client.TD_PlantError("\c3" @ %tower.uiName @ "<br>\c5Can't plant this tower.<br>\c6Max towers reached.", 3);
				return false;
			}
		}
		else if(%tower.height <= -1)
		{
			if(!%ignore)
				%client.TD_PlantError("\c2You can plant here.<br>\c6Although you need to pick a tower<br>\c3Use your left/right build keys!", 3);
			return false;
		}
	}

	if(!%ignore)
		%client.TD_PlantError("\c2You can plant here.<br>\c6Nothing is blocking you!<br>\c4Press your plant key!", 3);

	return true;
}

function GameConnection::TD_PlantError(%this, %msg, %time)
{
	if(%this.TD_Client)
	{
		if(%this.TD_ClientData["DisableHUD"])
			%this.centerPrint(%msg, %time);
		else
			commandToClient(%this, 'TDCMD', "Close", "HUD");
	}
	else
		%this.centerPrint(%msg, %time);
}

function fxDTSBrick::TD_ClientControl(%this, %client, %time)
{
	if(!isObject(%client))
		return;

	if(%client.getClassName() !$= "GameConnection")
		return;

	if(!isObject(%bot = %this.bot))
		return;

	if(!%bot.isTowerBot)
		return;

	if(%time <= 0)
		%bot.useControlTime = 0;
	else
	{
		%bot.useControlTime = 1;
		%bot.startControlTime = $Sim::Time;
		%bot.controlTimeEnd = %time;
		%timePrint = " \c0You have " @ getTimeString(%time) @ " second" @ (%time != 1 ? "s" : "") @ " \c5before your expiration time runs out.";
	}

	if(isObject(%bot.getControllingClient()))
	{
		%client.chatMessage("\c5Sorry, someone is already controlling this tower.");
		return;
	}

	%client.setControlObject(%bot);
	%client.chatMessage("\c5You are now controlling this tower!" @ %timePrint);
	%client.chatMessage("   \c3To exit, press your jump key!");
}

if(!isObject(TD_ShapeGroup))
	new SimGroup(TD_ShapeGroup);

function fxDTSBrick::TD_SpawnBot(%this, %client)
{
	if(isObject(%this.bot))
		return;

	%lscale = 0.1 * %this.getdatablock().brickSizeZ;				
	%botBrick_Pos = vectorAdd(%this.getPosition(), "0 0 " @ %lscale + 2);
	%tower = TowerGroup.findScript(%this.tower_name);
	%datablock = nameToID(NoMoveArmor);
	if(%tower.datablockName !$= "")
		if(isObject(%data = findPlayertypeByName(%tower.datablockName)))
			%datablock = nameToID(%data);

	%bot = new AIPlayer()
	{
		position = %botBrick_Pos;
		//spawnBrick = %this;
		dataBlock = %datablock;
		isTowerBot = 1;
		isBot = 1; //Enable minigame damage
		friendlyfire = 1;
		holdTime = mClampF(%tower.holdTime, 0.01, 9999);
		damageMultiplier = mClampF((%tower.damageMultiplier $= "" ? 1 : mFloatLength(%tower.damageMultiplier, 3)), 0.1, 9999);
		range = mClampF(%tower.range, 5, 1000);
		tooClose = mClampF(%tower.tooClose, 0, 1000);
		minigame = %this.minigame;
		height = mClampF(%tower.height, 0, 5);
		shootWaitTime = mClampF(%tower.shootWaitTime, 0, 1000);
		alwaysOnTrigger = mClampF(%tower.alwaysOnTrigger, 0, 1);
		hatefulTower = mClampF(%tower.hatefulTower, 0, 1);
		uiName = %tower.uiName;
		TD_Type = %tower.type;
		cost = %tower.cost;
		script = %tower;
		ownerClient = %client;
		tower_maxControlTime = %tower.controlTime;
	};
	%bot.player = %bot;
	
	if(%tower.customAvatar)
	{
		%bot.llegColor = %tower.llegColor;
		%bot.secondPackColor = %tower.secondPackColor;
		%bot.lhand = %tower.lhand;
		%bot.hip = %tower.hip;
		%bot.faceName = %tower.faceName;
		%bot.rarmColor = %tower.rarmColor;
		%bot.hatColor = %tower.hatColor;
		%bot.hipColor = %tower.hipColor;
		%bot.chest = %tower.chest;
		%bot.rarm = %tower.rarm;
		%bot.packColor = %tower.packColor;
		%bot.pack = %tower.pack;
		%bot.decalName = %tower.decalName;
		%bot.larmColor = %tower.larmColor;
		%bot.secondPack = %tower.secondPack;
		%bot.larm = %tower.larm;
		%bot.chestColor = %tower.chestColor;
		%bot.accentColor = %tower.accentColor;
		%bot.rhandColor = %tower.rhandColor;
		%bot.rleg = %tower.rleg;
		%bot.rlegColor = %tower.rlegColor;
		%bot.accent = %tower.accent;
		%bot.headColor = %tower.headColor;
		%bot.rhand = %tower.rhand;
		%bot.lleg = %tower.lleg;
		%bot.lhandColor = %tower.lhandColor;
		%bot.hat = %tower.hat;

		GameConnection::ApplyBodyParts(%bot);
		GameConnection::ApplyBodyColors(%bot);
	}
	else
		applyDefaultCharacterPrefs(%bot);

	if(isObject(%shield = %this.brickShield))
	{
		%shield.shape_name = new StaticShape()
		{
			datablock = BrickTextEmptyShape;
			Position = vectorAdd(%shield.getPosition(), "0 0" SPC %shield.getDatablock().brickSizeZ * 0.1);
			scale = "0.1 0.1 0.1";
		};
		%shield.shape_name.setShapeName(strReplace(%shield.tower_ownerName @ "'s " @ %shield.tower_name, "s's", "'s"));
		%shield.shape_name.setShapeNameColor("1 1 1 1");
		%shield.shape_name.setShapeNameDistance(30);
		TD_ShapeGroup.add(%shield.shape_name);
	}

	%bot.setMaxHealth(%this.tower_maxhealth);
	%bot.setInvulnerbilityTime(3);
	%bot.TD_SetShapeName();
	%bot.setShapeNameDistance(20);

	%this.bot = %bot;
	%bot.brickShield = %shield;
	TowerDefenseGroup.add(%bot);
	%bot.tower = %this;
	%bot.baseTower = %this.baseTower;
	%bot.baseTower.bot = %bot;
	if(isObject(%wep = findItemByName(%this.tower_weapon)))
		%bot.setWeapon(%wep);
	%bot.TD_Loop();
}

//Support

function fxDTSBrick::getColorI(%this)
{
	if(!isObject(%this))
		return -1;

	return getColorI(getColorIDTable(%this.getColorID()));
}

function fxDTSBrick::getColorF(%this)
{
	if(!isObject(%this))
		return -1;

	return getColorF(getColorIDTable(%this.getColorID()));
}

function fxDTSBrick::isOnGrid(%this)
{
	if(!isObject(%this))
		return;

	if(!$Pref::Server::TD_UseGrid)
		return true;

	%data = %this.getDatablock();
	%pos = %this.getPosition();

	%gridPos = mFloatLength(getWord(%pos, 0) / (%data.brickSizeX / 2), 0) * (%data.brickSizeX / 2) SPC 
		mFloatLength(getWord(%pos, 1) / (%data.brickSizeY / 2), 0) * (%data.brickSizeY / 2) SPC 
		mFloatLength(getWord(%pos, 2) / (%data.brickSizeZ / 2), 0) * (%data.brickSizeZ / 2) - $Pref::Server::TD_Offset * 2;

	if(getWords(%gridPos, 0, 1) !$= getWords(%pos, 0, 1))
		return;

	return true;
}

function getNTBrickcount(%name, %brickgroup)
{
	%name = stripChars(%name, "!@#$%^&*(){}[]=;\':\"\\,./<>?`~");
	%name = strReplace(%name, "-", "DASH");
	%name = strReplace(%name, " ", "_");

	%bl_id = %brickgroup;
	if(isObject(%bl_idBrickgroup = "BrickGroup_" @ %bl_id))
		%brickgroup = %bl_idBrickgroup;

	if(isObject(%brickgroup))
		return %brickgroup.NTObjectCount_[%name];
	else
	{
		%count = 0;
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%brickgroup = MainBrickGroup.getObject(%i);
			%count += mFloor(%brickgroup.NTObjectCount_[%name]);
		}
	}

	return %count;
}


function getNTBrick(%name, %num, %brickgroup)
{
	%name = stripChars(%name, "!@#$%^&*(){}[]=;\':\"\\,./<>?`~");
	%name = strReplace(%name, "-", "DASH");
	%name = strReplace(%name, " ", "_");

	%num = mFloor(%num);

	%bl_id = %brickgroup;
	if(isObject(%bl_idBrickgroup = "BrickGroup_" @ %bl_id))
		%brickgroup = %bl_idBrickgroup;
	
	if(isObject(%brickgroup))
		return %brickgroup.NTObject_[%name, %num];
	else
	{
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%brickgroup = MainBrickGroup.getObject(%i);
			if(%brickgroup.NTObjectCount_[%name] > 0)
				return %brickgroup.NTObject_[%name, %num];
		}
	}
	return -1;
}

function fxDTSBrick::getTopPosition(%this)
{
	%pos = %this.getPosition();
	return getWord(%pos, 0) SPC getWord(%pos, 1) SPC getWord(%pos, 2) + 0.1 * %this.getdatablock().bricksizez;
}

function FindBrick_Object(%name, %brickgroup)
{
	%mBrickgroup = %brickgroup;
	if(!isObject(%mBrickgroup))
	{
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%brickgroup = MainBrickGroup.getObject(%i);
			if(%brickgroup.NTObjectCount_[%name] > 0)
			{
				%mBrickgroup = %brickgroup;
				break;
			}
		}
	}

	if(isObject(%brick = getNTBrick(%name, getRandom(0, getNTBrickcount(%name)-1), %mBrickgroup)))
		return nameToID(%brick);

	return -1;
}

function FindBrick_Position(%name, %brickgroup)
{
	%mBrickgroup = %brickgroup;
	if(!isObject(%mBrickgroup))
	{
		for(%i = 0; %i < MainBrickGroup.getCount(); %i++)
		{
			%brickgroup = MainBrickGroup.getObject(%i);
			if(%brickgroup.NTObjectCount_[%name] > 0)
			{
				%mBrickgroup = %brickgroup;
				break;
			}
		}
	}

	if(isObject(%brick = getNTBrick(%name, getRandom(0, getNTBrickcount(%name)-1), %mBrickgroup)))
		return %brick.getTopPosition();

	return -1;
}
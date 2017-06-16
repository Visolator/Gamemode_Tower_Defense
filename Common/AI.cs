function AIPlayer::chatMessage(){}
function AIPlayer::play2D(){}
function AIPlayer::onDeath(){}
function AIPlayer::incScore(){}
function AIPlayer::getTeam(){}

datablock PlayerData(NoMoveArmor : PlayerStandardArmor)
{
	runForce = 0;
	runEnergyDrain = 0;
	minRunEnergy = 0;
	maxForwardSpeed = 0;
	maxBackwardSpeed = 0;
	maxSideSpeed = 0;

	maxForwardCrouchSpeed = 0;
	maxBackwardCrouchSpeed = 0;
	maxSideCrouchSpeed = 0;

	jumpForce = 0;
	jumpEnergyDrain = 0;
	minJumpEnergy = 0;
	jumpDelay = 0;

	minJetEnergy = 0;
	jetEnergyDrain = 0;
	canJet = 0;

	uiName = "Frozen player";
	showEnergyBar = false;

	runSurfaceAngle  = 0;
	jumpSurfaceAngle = 0;
};

datablock StaticShapeData(CubeStaticShape)
{
	shapeFile = "./Cube.dts";
};

datablock ParticleData(TD_HealParticleA)
{
	textureName          = "base/data/particles/dot";
	dragCoefficient      = 10.0;
	gravityCoefficient   = 0;
	inheritedVelFactor   = 0.0;
	windCoefficient      = 0;
	constantAcceleration = 0;
	lifetimeMS           = 100;
	lifetimeVarianceMS   = 0;
	spinSpeed     = 0;
	spinRandomMin = 0.0;
	spinRandomMax = 0.0;
	useInvAlpha   = false;

	colors[0]	= "0.0 0.9 0.9 0.0";
	colors[1]	= "0.0 0.8 0.9 0.3";
	colors[2]	= "0.0 0.9 0.8 0.3";
	colors[3]	= "0.0 0.9 0.7 0.0";

	sizes[0]	= 0.2;
	sizes[1]	= 0.4;
	sizes[2]	= 0.4;
	sizes[3]	= 0.4;

	times[0]	= 0.0;
	times[1]	= 0.3;
	times[2]	= 0.7;
	times[3]	= 1.0;
};

datablock ParticleEmitterData(TD_HealEmitterA)
{
	ejectionPeriodMS = 8;
	periodVarianceMS = 0;
	ejectionVelocity = 0.0;
	ejectionOffset   = 0.0;
	velocityVariance = 0.0;
	thetaMin         = 0;
	thetaMax         = 0;
	phiReferenceVel  = 0;
	phiVariance      = 0;
	overrideAdvance = false;
	particles = TD_HealParticleA;   
	
	useEmitterColors = true;
	
	uiName = "";
};

datablock ProjectileData(TD_HealProjectile)
{
	shapeFile = "base/data/shapes/empty.dts";
	directDamage = 0;
	directDamageType = $DamageType::TD_Heal;
	radiusDamageType = $DamageType::TD_Heal;

	brickExplosionRadius = 0;
	brickExplosionImpact = false;
	brickExplosionForce = 0;
	brickExplosionMaxVolume = 0;
	brickExplosionMaxVolumeFloating = 0;

	impactImpulse = 0;
	verticalImpulse = 0;
	explosion = "";
	particleEmitter = TD_HealEmitterA;

	muzzleVelocity = 60;
	velInheritFactor = 1;

	armingDelay = 0;
	lifetime = 2000;
	fadeDelay = 1500;
	bounceElasticity = 0.5;
	bounceFriction = 0.20;
	isBallistic = true;
	gravityMod = 1;

	hasLight = false;
	lightRadius = 5;
	lightColor = "0 1 1";

	uiName = "";
};

datablock ItemData(TD_HealItem)
{
	category = "Weapon";
	className = "Weapon";

	shapeFile = "base/data/shapes/empty.dts";
	rotate = false;
	mass = 0.5;
	density = 0.7;
	elasticity = 0.6;
	friction = 0.6;
	emap = true;

	uiName = "Tower Healer";
	iconName = "";
	doColorShift = false;
	colorShiftColor = "0 1 1 1";

	image = TD_HealImage;
	canDrop = true;
};

datablock ShapeBaseImageData(TD_HealImage)
{
	shapeFile = "base/data/shapes/empty.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	eyeOffset = 0;
	rotation = "0 0 0";

	correctMuzzleVector = true;

	className = "WeaponImage";

	item = TD_HealItem;
	ammo = " ";
	projectile = TD_HealProjectile;
	projectileType = Projectile;

	melee = true;
	armReady = false;

	doColorShift = false;
	colorShiftColor = TD_HealItem.colorShiftColor;

	stateName[0]                   = "Activate";
	stateTimeoutValue[0]           = 0.15;
	stateTransitionOnTimeout[0]    = "Ready";

	stateName[1]                   = "Ready";
	stateScript[1]                 = "onReady";
	stateTransitionOnTriggerDown[1]= "Fire";
	stateAllowImageChange[1]       = true;

	stateName[2]                   = "Fire";
	stateTransitionOnTimeout[2]    = "Fire";
	stateTransitionOnTriggerUp[2]  = "Ready";
	stateTimeoutValue[2]           = 0.1;
	stateFire[2]                   = true;
	stateAllowImageChange[2]       = false;
	stateScript[2]                 = "onFire";
	stateWaitForTimeout[2]         = false;
	stateEmitter[2]                = TD_HealEmitterA;
	stateEmitterTime[2]            = 0.1;
	stateSound[2]                  = ""; //Unknown
};

function TD_HealImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, "root");
	%obj.TD_Heal["Target"] = -1;
}

function TD_HealImage::onReady(%this, %obj, %slot)
{
	%obj.playThread(1, "root");
	%obj.TD_HealModeThread = "root";
	%obj.TD_Heal["Target"] = -1;
}

if($Pref::Server::TD_HealerRadius <= 0)
	$Pref::Server::TD_HealerRadius = 20;

if($Pref::Server::TD_HealerRate <= 0)
	$Pref::Server::TD_HealerRate = 0.96;

$Server::TD_TooClose = 1.5;

function TD_HealImage::onFire(%this, %obj, %slot)
{
	if(%obj.TD_HealModeThread !$= "ArmReadyRight")
	{
		%obj.TD_HealModeThread = "ArmReadyRight";
		%obj.playThread(1, "ArmReadyRight");
	}
	if(!isObject(%target = %obj.TD_Heal["Target"]))
	{
		%newRange = 999999;
		initContainerRadiusSearch(%obj.getPosition(), $Pref::Server::TD_HealerRadius, $TypeMasks::PlayerObjectType);
		while(isObject(%target = containerSearchNext()))
		{
			%newDist = vectorDist(%target.getPosition(), %obj.getPosition());
			if(%target.isTowerBot && isObject(%target.baseTower) && %target.getClassname() $= "AIPlayer" && %target != %obj && 
				%target.getState() !$= "Dead" && %newDist < %newRange)
			{
				%obj.TD_Heal["Target"] = %target;
				%newRange = %newDist;
			}
		}
	}
	else
	{
		if(vectorDist(%obj.getPosition(), %target.getPosition()) > $Pref::Server::TD_HealerRadius)
		{
			%obj.TD_Heal["Target"] = -1;
			return;
		}

		if(isObject(%client = %obj.client))
		{
			%healColor = "\c3";
			%healMsg = " \c2Healing " @ $Pref::Server::TD_HealerRate * 10 @ "HP/s";
			%health = mFloatLength(%target.getHealth() / %target.getMaxHealth() * 100, 1);
			if(%health >= 100)
			{
				%healColor = "\c6";
				%healMsg = "";
			}
			else if(isObject(%minigame = %client.minigame))
			{
				%minigame.TD_AddResources(($Pref::Server::TD_HealerRate * 10) / 8);
				%client.incScore($Pref::Server::TD_HealerRate / 10);
			}

			%client.centerPrint("<just:left>\c5Target: \c3" @ %target.uiName @ " \c5(" @ %healColor @ %health @ "\c5%)" @ %healMsg, 0.2);
		}

		%target.addHealth($Pref::Server::TD_HealerRate);
		%target.TD_SetShapeName();
	}
}

//Done

if($Pref::Server::TD_HealthRate <= 0)
	$Pref::Server::TD_HealthRate = 0.24;

function AIPlayer::TD_Loop(%this)
{
	cancel(%this.TD_Loop);
	%tooClose = mClampF(%this.tooClose, 0, 100);
	%range = mClampF(%this.range, 1, 100);
	%minigame = %this.minigame;

	%foundRange = 9999;

	%pos = %this.getPosition();

	if(isObject(%client = %this.getControllingClient()))
	{
		if(!isObject(%this.tower))
		{
			%this.kill();
			if(isObject(%player = %client.player))
				%client.setControlObject(%player);

			return;
		}

		if(%this.lastControlled != %client)
		{
			%this.lastControlled = %client;
			%this.setShapeNameColor("0 0.5 1 1");
			%this.TD_SetShapeName();
		}

		%this.TD_Loop = %this.schedule(100, TD_Loop);
		return;
	}
	else if(%this.lastControlled != 0)
	{
		%this.lastControlled = 0;
		%this.setShapeNameColor("1 1 1 1");
		%this.TD_SetShapeName();
	}

	if(%this.isTF2Healer)
	{
		if(!isObject(%target = %this.TD_Heal["Target"]))
		{
			if(%this.getImageTrigger(0))
				%this.setImageTrigger(0, 0);

			%this.clearMoveDestination();
			%this.setJetting(0);
			%this.setJumping(0);
			%this.clearMoveY();
			%this.clearMoveX();

			%newRange = 999999;
			initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
			while(isObject(%target = containerSearchNext()))
			{
				%newDist = vectorDist(%target.getPosition(), %this.getPosition());
				if(%target.getClassname() $= "Player" && %target != %this && 
					%target.getState() !$= "Dead" && %newDist < %newRange && (%target.getHealth() < %target.getMaxHealth() || $Sim::Time - %target.lastMedicCall < 1))
				{
					%this.lastMedicCallTime = $Sim::Time;
					%this.healTarget = %target;
					%this.TD_Heal["Target"] = %target;
					%newRange = %newDist;
				}
			}
		}
		
		if(isObject(%target = %this.TD_Heal["Target"]) && (%target.getHealth() < %target.getMaxHealth() || $Sim::Time - %target.lastMedicCall < 4))
		{
			%this.healTarget = %target;
			%distance = vectorDist(%pos, %target.getPosition());

			if(%distance >= 5)
			{
				%this.clearMoveY();
				%this.clearMoveX();
				%this.setMoveDestination(%target.getPosition());
				%this.clearAim();
			}
			else if(%distance < 5 && %distance >= 4.25)
			{
				if(!%this.getImageTrigger(0))
					%this.setImageTrigger(0, 1);

				//talk("Too close");
				%this.setAimLocation(%target.getEyePoint());
				%this.clearMoveDestination();
				%this.setJetting(0);
				%this.setJumping(0);
				%this.clearMoveY();
				%this.clearMoveX();
			}
			else if(%distance <= 4)
			{
				if(!%this.getImageTrigger(0))
					%this.setImageTrigger(0, 1);

				//talk("Way too close");
				%this.setAimLocation(%target.getEyePoint());
				%this.setMoveObject(0);
				%this.setMoveY(-5);
			}
		}
		else
		{
			if(%this.getImageTrigger(0))
				%this.setImageTrigger(0, 0);

			%this.clearAim();
			%this.clearMoveDestination();
			%this.setJetting(0);
			%this.setJumping(0);
			%this.clearMoveY();
			%this.clearMoveX();
			serverCmdSit(%this);
		}

		%this.addHealth($Pref::Server::TD_HealthRate);
		%this.TD_SetShapeName();

		%this.TD_Loop = %this.schedule(100, TD_Loop);
		return;
	}

	if(%this.isHealer)
	{
		if(!isObject(%target = %this.TD_Heal["Target"]))
		{
			if(%this.getImageTrigger(0))
				%this.setImageTrigger(0, 0);

			%this.clearMoveDestination();
			%this.setJetting(0);
			%this.setJumping(0);
			%this.clearMoveY();
			%this.clearMoveX();

			%newRange = 999999;
			initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
			while(isObject(%target = containerSearchNext()))
			{
				%newDist = vectorDist(%target.getPosition(), %this.getPosition());
				if(%target.isTowerBot && isObject(%base = %target.baseTower) && %target.getClassname() $= "AIPlayer" && %target != %this && 
					%target.getState() !$= "Dead" && %newDist < %newRange)
				{
					%this.TD_Heal["Target"] = %target;
					%newRange = %newDist;
				}
			}
		}
		
		if(isObject(%target = %this.TD_Heal["Target"]) && isObject(%tower = %target.baseTower))
		{
			%distance = vectorDist(%pos, %tower.getPosition());

			if(%distance >= 6 && %distance <= $Pref::Server::TD_HealerRadius)
			{
				%this.clearMoveY();
				%this.clearMoveX();
				%this.setMoveDestination(%tower.getPosition());
				%this.clearAim();	
			}
			else if(%distance >= 6)
			{
				%this.clearMoveY();
				%this.clearMoveX();
				%this.setMoveDestination(%tower.getPosition());
				%this.clearAim();
			}
			else if(%distance < 6 && %distance >= 4.25)
			{
				if(!%this.getImageTrigger(0))
					%this.setImageTrigger(0, 1);

				//talk("Too close");
				%this.setAimLocation(%target.getEyePoint());
				%this.clearMoveDestination();
				%this.setJetting(0);
				%this.setJumping(0);
				%this.clearMoveY();
				%this.clearMoveX();
			}
			else if(%distance <= 4)
			{
				if(!%this.getImageTrigger(0))
					%this.setImageTrigger(0, 1);

				//talk("Way too close");
				%this.setAimLocation(%target.getEyePoint());
				%this.setMoveObject(0);
				%this.setMoveY(-5);
			}
		}
		else
		{
			%this.clearAim();
			%this.clearMoveDestination();
			%this.setJetting(0);
			%this.setJumping(0);
			%this.clearMoveY();
			%this.clearMoveX();
		}

		%this.addHealth($Pref::Server::TD_HealthRate);
		%this.TD_SetShapeName();

		%this.TD_Loop = %this.schedule(100, TD_Loop);
		return;
	}

	if(!isObject(%this.killTarg))
	{
		initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
		while(isObject(%target = containerSearchNext()))
		{
			if(%target.isEnemyBot && vectorDist(%target.getPosition(), %pos) <= %foundRange 
				&& %target != %this && %target.getState() !$= "Dead" && vectorDist(%target.getPosition(), %pos) > %tooClose 
				&& %this.canSeeObject(%target, %this.brickShield, %range) && !%target.isInvincible)
			{
				%foundRange = vectorDist(%target.getPosition(), %pos);
				%killTarg = %target;
				continue;
			}
		}

		if(%minigame.botQueue <= 0 && !isObject(%killTarg) && %this.hatefulTower)
		{
			initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
			while(isObject(%target = containerSearchNext()))
			{
				if(%target.getClassname() $= "Player" && vectorDist(%target.getPosition(), %pos) <= %foundRange 
					&& %target != %this && %target.getState() !$= "Dead" && vectorDist(%target.getPosition(), %pos) >= %tooClose 
					&& !%target.client.isBuilder && %this.canSeeObject(%target, %this.brickShield, %range, $TypeMasks::FxBrickObjectType) && !%target.isInvincible)
				{
					%foundRange = vectorDist(%target.getPosition(), %pos);
					%killTarg = %target;
					continue;
				}
			}
		}

		if(isObject(%killTarg))
		{
			%this.clearMoveYaw();
			%this.killTarg = %killTarg;
			//%this.spawnProjectile(0, AlarmProjectile, "0 0 0", 1);
			%this.setAimLocation(vectoradd(%killTarg.getEyePoint(), vectorScale(%killTarg.getVelocity(), 0.25)));
			%this.maxYawSpeed = 10;
			%this.maxPitchSpeed = 10;
		}
		else
		{
			%this.clearAim();
			%r = getRandom(1, 2);
			%way = -0.05 * getRandom(1, 4);
			if(%r == 1)
				%way = 0.05 * getRandom(1, 4);
			if(!isObject(%this.TD_Target) && $Sim::Time - %this.waitTime > %this.waitTimeS + %this.waitTimeG)
			{
				if(%this.clearYaw)
				{
					%this.clearYaw = 0;
					%this.clearMoveYaw();
				}

				%this.waitTimeS = getRandom(2, 6);
				%this.waitTimeG = getRandom(2, 4);
				%this.waitTime = $Sim::Time;
				%this.setMoveYaw(%way);
				if(!isObject(%this.getMountedImage(0)))
				{
					%this.getDatablock().onTrigger(%this, 0, 0);
				}
				else
				{
					%this.setImageTrigger(0, 0);
					%this.setImageTrigger(1, 0);
				}
				%this.isShooting = 0;
			}
			else if($Sim::Time - %this.waitTime > %this.waitTimeS/6)
			{
				%this.clearMoveYaw();
				if(!isObject(%this.getMountedImage(0)))
				{
					%this.getDatablock().onTrigger(%this, 0, 0);
				}
				else
				{
					%this.setImageTrigger(0, 0);
					%this.setImageTrigger(1, 0);
				}
				%this.isShooting = 0;
			}
		}
	}
	else if(isObject(%killTarg = %this.killTarg))
	{
		%killPos = %killTarg.getPosition();
		%thisPos = %this.getPosition();
		%muzzleVelo = 0.5;
		//if(isObject(%image = %this.getMountedImage(0)))
		//	if(isObject(%proj = %image.projectile))
		//		if(%proj.muzzleVelocity > 0 && $Pref::Server::TD_Muzzle > 0)
		//			%muzzleVelo = mClampF(1 - %proj.muzzleVelocity / $Pref::Server::TD_Muzzle, 0, 100);

		if(%killTarg.getState() $= "dead" || vectorDist(%killTarg.getPosition(), %pos) <= %tooClose 
			|| !%this.canSeeObject(%killTarg, %this.brickShield, %range, $TypeMasks::FxBrickObjectType) || vectorDist(%killPos, %thisPos) > %range || %killTarg.isInvincible)
		{
			%this.clearYaw = 1;
			%this.killTarg = 0;
			%this.TD_Loop = %this.schedule(100, TD_Loop);
			return;
		}

		%this.setAimLocation(vectoradd(%killTarg.getEyePoint(), vectorScale(%killTarg.getVelocity(), %muzzleVelo)));

		if($Sim::Time - %this.lastShoot > %this.shootWaitTime)
		{
			if(!%this.isShooting)
			{
				%this.isShooting = 1;
				%this.lastShoot = $Sim::Time;
				if(!isObject(%this.getMountedImage(0)))
					%this.getDatablock().onTrigger(%this, 0, 1);
				else
					%this.setImageTrigger(0, 1);
			}
		}

		if($Sim::Time - %this.lastShoot > %this.holdTime && !%this.alwaysOnTrigger)
		{
			if(%this.isShooting)
			{
				%this.isShooting = 0;
				%this.lastShoot = $Sim::Time;
				if(!isObject(%this.getMountedImage(0)))
					%this.getDatablock().onTrigger(%this, 0, 0);
				else
				{
					%this.setImageTrigger(0, 0);
					%this.setImageTrigger(1, 0);
				}
			}
		}
	}

	if(!isObject(%this.tower))
	{
		%this.kill();
		return;
	}

	if(%minigame.noHealers)
	{
		%this.addHealth($Pref::Server::TD_HealthRate / 2);
		%this.TD_SetShapeName();
	}
	
	%this.TD_Loop = %this.schedule(100, TD_Loop);
}

if($Pref::Server::TD_HealthRate < 0)
	$Pref::Server::TD_Muzzle = 90;

function AIPlayer::FindPath(%this)
{
	if(!isObject(%this))
		return -1;

	if(%this.getState() $= "Dead")
		return -1;

	if(!isObject(getNTBrick("EndNode", 0)) || !isObject(getNTBrick("StartNode", 0)))
		return -1;

	if($Sim::Time - %this.lastNodeSearch < 0.5)
		return 0;

	if(isObject(%this.nextBrick))
		return 1;

	//if(isObject(%this.killTarg))
	//	return 0;

	%this.isFindingPath = 1;

	%position = %this.getHackPosition();
	%closestNode = getWord(%this.pathNodes, 0);
	//if(!isObject(%closestNode))
	//	%closestNode = TD_PathHandler.findClosestNode(%position, false, %this.nodeList);

	if(!isObject(%closestNode))
		return 0;

	%this.lastNodeSearch = $Sim::Time;
	%this.nextBrick = %closestNode;
	%this.clearAim();
	%this.clearMoveY();
	%this.clearMoveX();
	%this.setMoveDestination(getWords(%closestNode.getPosition(), 0, 1) SPC getWord(%this.getPosition(), 2));
	%this.setImageTrigger(0, 0);
	%this.setImageTrigger(1, 0);
	%this.isAttacking = 0;
	%this.brickReached = 0;
	return 1;
}

//Enemy
function AIPlayer::TD_EnemyLoop(%this, %shapeName)
{
	cancel(%this.TD_Loop);

	if($TD::BotDebug)
	{
		if(!%this.resizeShapeDist && !%this.isTerrorist)
		{
			%this.resizeShapeDist = 1;
			%this.setShapeNameDistance(50);
		}

		if(%this.getState() $= "dead")
			%this.setPlayerShapeName("");
		else if(%shapeName !$= "" && %this.getShapeName() !$= %shapeName)
			%this.setPlayerShapeName(%shapeName);
	}

	%this.lastTDState = %shapeName;

	if(%this.getState() $= "dead")
		return;

	%minigame = %this.minigame;

	%tooClose = mClampF(%this.tooClose, 5, 100);
	%range = mClampF(%this.range, 6, 100);
	%foundRange = 9999;

	%pos = %this.getPosition();

	%shapeName = "";
	if(isObject(%killTarg = %this.killTarg))
	{
		if(%killTarg.getClassName() $= "Player")
		{
			if(vectorDist(%killTarg.getPosition(), %pos) <= %tooClose)
				%yes = 1;
		}
		else if(%killTarg.getClassName() $= "AIPlayer")
		{
			if(vectorDist(%killTarg.getPosition(), %pos) >= %tooClose)
				%yes = 1;
		}
	}
	//((vectorDist(%killTarg.getPosition(), %pos) >= %tooClose && %killTarg.getClassname() $= "AIPlayer") || (vectorDist(%killTarg.getPosition(), %pos) <= %tooClose && %killTarg.getClassname() $= "Player")) && 

	if(isObject(%killTarg) && vectorDist(%pos, %killTarg.getPosition()) <= %range && 
		%killTarg.getState() !$= "Dead" && %yes && %this.canSeeObject(%killTarg, %this, %range) && !%killTarg.isInvincible)
	{
		if(%this.isTerrorist)
		{
			%aim = %killTarg.getEyePoint();
			%this.setAimLocation(%aim);
			if(%this.isTerrorist)
				%this.setMoveObject(%killTarg);

			if(vectorDist(%killTarg.getPosition(), %pos) < %tooClose && %this.isTerrorist)
			{
				if(isObject(%killTarg.client) && %killTarg.client.getClassname() $= "GameConnection")
					%minigame.messageAll('', "\c3Suicide bomber to \c7[\c4" @ %killTarg.client.getPlayerName() @ "\c7]\c6: Kaboom!");
				%this.spawnExplosion(tankShellProjectile, 3);
				%this.schedule(0, kill);
				return;
			}

			%this.TD_Loop = %this.schedule($Server::TD_EnemyLoop, TD_EnemyLoop, "KABOOM");
			return;
		}

		initContainerRadiusSearch(%pos, $Server::TD_TooClose, $TypeMasks::PlayerObjectType);
		while(isObject(%target = containerSearchNext()))
		{
			if(%target.isEnemyBot && isObject(%kTarg = %target.killTarg) && %kTarg.isTowerBot && %target != %this && %target.getState() !$= "dead")
			{
				%this.killTarg = 0;
				%this.TD_Loop = %this.schedule($Server::TD_EnemyLoop, TD_EnemyLoop, "Ally too close");
				return;
			}
		}

		initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
		while(isObject(%target = containerSearchNext()))
		{
			if(vectorDist(%target.getPosition(), %pos) < %foundRange
				&& %target != %this && %target.getState() !$= "Dead" && %this.canSeeObject(%target, %this, %range))
			{
				if(((isObject(%target.TD_Heal["Target"]) || (%target.client.isBuilder && vectorDist(%target.getPosition(), %pos) < %tooClose)) || %this.canHitAnyRange) && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible && %target != %killTarg)
				{
					%foundRange = vectorDist(%target.getPosition(), %pos);
					%newKillTarg = %target;
				}
			}

			if(isObject(%newKillTarg) && %this.killTarg != %newKillTarg)
			{
				%this.lastTarget = $Sim::Time;
				%this.killTarg = %newKillTarg;
				%shapeName = "(Debug) Found new target";
			}
		}

		if(isObject(%killTarg = %this.killTarg))
		{
			%this.clearMoveDestination();
			%this.clearMoveY();
			%this.clearMoveX();
			%this.setJumping(0);
			%this.setJetting(0);
			%this.isAttacking = 1;
			%shapeName = "(Debug) Found target (still)";
		}

		%aim = %killTarg.getEyePoint();
		%this.setAimLocation(%aim);
		if(%this.isTerrorist)
			%this.setMoveObject(%killTarg);

		%shootWaitTime = %this.shootWaitTime;
		%holdTime = %this.holdTime;
		%alwaysTrigger = %this.alwaysOnTrigger;
		if(%killTarg.getClassName() $= "Player")
		{
			%shootWaitTime = %this.tCloseShootWaitTime;
			%holdTime = %this.tCloseHoldTime;
			if(%this.tCloseAlwaysOnTrigger !$= "")
				%alwaysTrigger = %this.tCloseAlwaysOnTrigger;
		}

		if($Sim::Time - %this.lastShoot > %shootWaitTime)
		{
			if(!%this.isShooting)
			{
				%this.isShooting = 1;
				%this.lastShoot = $Sim::Time;
				if(!isObject(%this.getMountedImage(0)))
					%this.getDatablock().onTrigger(%this, 0, 1);
				else
					%this.setImageTrigger(0, 1);

				%shapeName = "(Debug) Found target->shooting";
			}
		}

		if($Sim::Time - %this.lastShoot > %holdTime && !%alwaysTrigger)
		{
			if(%this.isShooting)
			{
				%this.isShooting = 0;
				%this.lastShoot = $Sim::Time;
				if(!isObject(%this.getMountedImage(0)))
				{
					%this.getDatablock().onTrigger(%this, 0, 0);
				}
				else
				{
					%this.setImageTrigger(0, 0);
					%this.setImageTrigger(1, 0);
				}
				%shapeName = "(Debug) Found target->shoot wait";
			}
		}
	}
	else
	{
		%this.killTarg = 0;

		if($Sim::Time - %this.lastStuck > 0.5 && %this.isStuck && vectorLen(%this.getVelocity()) > 1)
		{
			%this.isStuck = 0;
			%this.setJumping(0);
			%this.setJetting(0);
			%this.setImageTrigger(0, 0);
			%this.setImageTrigger(1, 0);

			%this.clearAim();
			if(isObject(%node = %this.nextBrick))
			{
				%dist = vectorDist(%node.getPosition(), %this.getPosition());

				if(vectorDist(%node.getPosition(), %this.getPosition()) <= 2)
				{
					if((%path = %this.pathNodes) !$= "")
					{
						//talk("Found(" @ %node @ ") - " @ %path);
						%newPath = removeWord(%path, 0);
						%this.pathNodes = %newPath;
						//talk(" -> rDone: " @ %newPath);
					}

					if(strPos(%this.nodeList, %node) == -1)
						%this.nodeList = %this.nodeList SPC %node;

					%this.nextBrick = 0;
					if(%node.getName() !$= "_EndNode")
						%this.FindPath();
					else
					{
						%this.addvelocity("0 0 100");
						%this.schedule(1000, spawnExplosion, finalVehicleExplosionProjectile, 3);
						%this.schedule(1200, kill);

						if(isObject(%minigame = %this.minigame))
						{
							if(!%minigame.failed)
							{
								%minigame.failed = 1;
								%minigame.scheduleReset();
								TD_CheckRecord(%minigame.TD_Difficult, %minigame.TD_Round);
								%minigame.messageAll('', "\c3Bots have reached the end point! You all failed!");
							}
						}
					}

					%shapeName = "(Debug) [Stuck] Moving";
				}
				else
				{
					if(isObject(%this.nextBrick))
					{
						%this.clearMoveY();
						%this.clearMoveX();
						%this.setMoveDestination(getWords(%node.getPosition(), 0, 1) SPC getWord(%this.getPosition(), 2));
					}

					%shapeName = "(Debug) [Stuck] New node";
				}
			}
			else
			{
				%shapeName = "(Debug) Finding path";
				%this.FindPath();
			}
		}
		else if(!%this.isStuck)
		{
			%newKillTarg = -1;
			initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
			while(isObject(%target = containerSearchNext()))
			{
				if(vectorDist(%target.getPosition(), %pos) < %foundRange 
					&& %target != %this && %target.getState() !$= "Dead")
				{
					if(%target.getClassName() $= "Player" && vectorDist(%target.getPosition(), %pos) >= %tooClose && (%this.isTerrorist || %this.canHitAnyRange) && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible)
					{
						%foundRange = vectorDist(%target.getPosition(), %pos);
						%newKillTarg = %target;
					}
					else if(%target.getClassName() $= "Player" && vectorDist(%target.getPosition(), %pos) <= %tooClose && !%this.isTerrorist && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible)
					{
						%foundRange = vectorDist(%target.getPosition(), %pos);
						%newKillTarg = %target;
					}
					else if(%target.isTowerBot && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible)
					{
						%foundRange = vectorDist(%target.getPosition(), %pos);
						%newKillTarg = %target;
					}
				}
			}

			if(%this.killTarg != %newKillTarg && isObject(%newKillTarg))
			{
				%this.lastTarget = $Sim::Time;
				%this.killTarg = %newKillTarg;
				%shapeName = "(Debug) Found target";
			}
			else
			{
				if(isObject(%node = %this.nextBrick))
				{
					%dist = vectorDist(%node.getPosition(), %this.getPosition());

					if(vectorDist(%node.getPosition(), %this.getPosition()) <= 2)
					{
						if((%path = %this.pathNodes) !$= "")
						{
							//talk("Found(" @ %node @ ") - " @ %path);
							%newPath = removeWord(%path, 0);
							%this.pathNodes = %newPath;
							//talk(" -> rDone: " @ %newPath);
						}

						if(strPos(%this.nodeList, %node) == -1)
							%this.nodeList = %this.nodeList SPC %node;

						%this.nextBrick = 0;
						if(%node.getName() !$= "_EndNode")
							%this.FindPath();
						else
						{
							%this.addvelocity("0 0 100");
							%this.schedule(1000, spawnExplosion, finalVehicleExplosionProjectile, 3);
							%this.schedule(1200, kill);

							if(isObject(%minigame = %this.minigame))
							{
								if(!%minigame.failed)
								{
									%minigame.failed = 1;
									%minigame.scheduleReset();
									TD_CheckRecord(%minigame.TD_Difficult, %minigame.TD_Round);
									%minigame.messageAll('', "\c3Bots have reached the end point! You all failed!");
								}
							}
						}

						%shapeName = "(Debug) [Stuck] Moving";
					}
					else
					{
						if(isObject(%this.nextBrick))
						{
							%this.clearMoveY();
							%this.clearMoveX();
							%this.setMoveDestination(getWords(%node.getPosition(), 0, 1) SPC getWord(%this.getPosition(), 2));
						}

						%shapeName = "(Debug) [Stuck] New node";
					}
				}
				else
				{
					%shapeName = "(Debug) Finding path";
					%this.FindPath();
				}
			}
		}

		if(%this.isAttacking)
		{
			%this.setJumping(0);
			%this.setJetting(0);
			%this.setImageTrigger(0, 0);
			%this.setImageTrigger(1, 0);
			if(!isObject(%this.killTarg))
				%this.clearAim();

			if(isObject(%node = %this.nextBrick))
			{
				%this.clearMoveY();
				%this.clearMoveX();
				%this.setMoveDestination(getWords(%node.getPosition(), 0, 1) SPC getWord(%this.getPosition(), 2));
			}
			else
				%this.FindPath();

			%this.isAttacking = 0;
			%shapeName = "(Debug) Lost target";
		}

		if(isObject(%killTarg = %this.killTarg))
		{
			if(%killTarg.getState() $= "dead")
				%noTarg = 1;

			if(vectorDist(%killTarg.getPosition(), %pos) > %range)
				%noTarg = 1;

			if(!%this.canSeeObject(%killTarg, %this, %range))
				%noTarg = 1;

			//if(isObject(%shield = %killTarg.brickShield))
			//	if(!%this.canSeeObject(%killTarg, %killTarg.brickShield, %range))
			//		%noTarg = 1;

			if(%killTarg.isInvincible)
				%noTarg = 1;

			if(%noTarg)
			{
				%this.clearYaw = 1;
				%this.killTarg = 0;
				%shapeName = "(Debug) Lost target";
				%this.TD_Loop = %this.schedule($Server::TD_EnemyLoop, TD_EnemyLoop, %shapeName);
				return;
			}
		}
		else
		{
			initContainerRadiusSearch(%pos, %range, $TypeMasks::PlayerObjectType);
			while(isObject(%target = containerSearchNext()))
			{
				if(vectorDist(%target.getPosition(), %pos) < %foundRange 
					&& %target != %this && %target.getState() !$= "Dead")
				{
					if(%target.getClassName() $= "Player" && vectorDist(%target.getPosition(), %pos) >= %tooClose && (%this.isTerrorist || %this.canHitAnyRange) && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible)
					{
						%foundRange = vectorDist(%target.getPosition(), %pos);
						%newKillTarg = %target;
					}
					else if(%target.getClassName() $= "Player" && vectorDist(%target.getPosition(), %pos) <= %tooClose && !%this.isTerrorist && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible)
					{
						%foundRange = vectorDist(%target.getPosition(), %pos);
						%newKillTarg = %target;
					}
					else if(%target.isTowerBot && %this.canSeeObject(%target, %this, %range) && !%target.isInvincible)
					{
						%foundRange = vectorDist(%target.getPosition(), %pos);
						%newKillTarg = %target;
					}
				}
			}
		}

		//If we seem to be stuck go in a random direction
		if(vectorLen(%this.getVelocity()) <= 1 && !%this.isStuck)
		{
			%this.lastStuckTimes++;
			if(%this.lastStuckTimes >= 2)
			{
				%this.lastStuck = $Sim::Time;
				%this.isStuck = 1;

				%r = getRandom(0, 2);
				switch(%r)
				{
					case 0:
						%this.setMoveX(1);

					case 1:
						%this.setMoveX(-1);

					case 2:
						%this.setJumping(1);

					default:
						doNothing();
				}

				%shapeName = "(Debug) Stuck (Vel: " @ vectorLen(%this.getVelocity()) @ ")";
			}
		}
		else
			%this.lastStuckTimes = 0;

		initContainerRadiusSearch(%pos, $Server::TD_TooClose, $TypeMasks::PlayerObjectType);
		while(isObject(%target = containerSearchNext()))
		{
			if(%target.isEnemyBot && isObject(%kTarg = %target.killTarg) && %kTarg.isTowerBot && %target != %this && %target.getState() !$= "dead")
				%this.killTarg = 0;
		}
	}
	
	%this.TD_Loop = %this.schedule($Server::TD_EnemyLoop, TD_EnemyLoop, %shapeName);
}

$Server::TD_EnemyLoop = 100;

function AIPlayer::canSeeObject(%this, %object, %ignore, %range, %type)
{
	if(%this.range > 0)
		%range = %this.range;

	%ev = %this.getEyeVector();  // get the player's eye vector
	%pos = %this.getEyePoint();  // check where he is
	if(!isObject(%object)) // does the object exist?
	{
		return 0;  // if it doesn't then fuck trying to find it
	}
	%ep = isFunction(%object.getClassname(), getEyePoint) ? %object.getEyePoint() : %object.getPosition(); // check if the object's eyepoint can be got and if so get it's eye point and pos
	%vd = vectorDist(%pos, %ep);  // find how far away they are from each other
	if(%vd > %this.range)  // if they're further than reasonable distance,
	{
		return 0; // sod it
	}

	if(%type $= "")
		%type = $TypeMasks::FxBrickObjectType;

	%cast = containerRaycast(%pos, %ep, %type, %ignore); // check if there are any bricks blocking the way
	%hit = firstWord(%cast);
	if(isObject(%hit)) // if you find anything blocking the view of the object,
		return 0;  // it must be unviewed.

	return 1; // conclude it can be seen if we got this far
}
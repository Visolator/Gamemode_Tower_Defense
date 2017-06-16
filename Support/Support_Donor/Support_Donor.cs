datablock ParticleData(SkullsParticle)
{
	dragCoefficient      = 0.1;
	gravityCoefficient   = -0.5;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.1;
	lifetimeMS           = 1050;
	lifetimeVarianceMS   = 1;
	textureName          = "base/client/ui/ci/skull";
	spinSpeed		= 0;
	spinRandomMin		= 0.0;
	spinRandomMax		= 0.0;
	colors[0]     = "1 1 1 1";
	colors[1]     = "1 1 1 1";
	colors[2]     = "1 1 1 1";
	colors[3]     = "1 1 1 1";
	sizes[0]      = 2;
	sizes[1]      = 0.8;
	sizes[2]      = 0.7;
	sizes[3]      = 0;
	windCoefficient = 0.0;

	times[0]      = 0.50;
	times[1]      = 0.20;
	times[2]      = 0.50;
	times[3]      = 0.1;

	useInvAlpha = false;
};
datablock ParticleEmitterData(SkullsEmitter)
{
   ejectionPeriodMS = 20;
   periodVarianceMS = 10;
   ejectionVelocity = 1.0;
   velocityVariance = 0.0;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 60;
   phiReferenceVel  = 0;
   phiVariance      = 0;
   useEmitterSizes = false;
   overrideAdvance = true;
   particles = "SkullsParticle";

    uiName = "Skulls Emitter";
};

if(isPackage("Support_Donor"))
	deactivatePackage("Support_Donor");

package Support_Donor
{
	function GameConnection::AutoAdminCheck(%this)
	{
		%this.setDonations($Donations::Client[%this.getBLID()]);
		return Parent::AutoAdminCheck(%this);
	}

	function GameConnection::SpawnPlayer(%this)
	{
		Parent::SpawnPlayer(%this);
		if(isObject(%player = %this.player) && $Pref::Server::Donor::SpawnMoreHealth)
		{
			if(getDonorLevel(%this.donations) == 3)
				%player.addMaxHealth(15);
			else if(getDonorLevel(%this.donations) == 4)
				%player.addMaxHealth(25);
			else if(getDonorLevel(%this.donations) == 5)
				%player.addMaxHealth(50);
			else if(getDonorLevel(%this.donations) > 5)
				%player.addMaxHealth(75);
		}
	}
};
activatePackage("Support_Donor");

function getDonorLevel(%money)
{
	%level = 0;

	if(%money >= 1 && %money < 5)
		%level = 1;

	if(%money >= 5 && %money < 10)
		%level = 2;

	if(%money >= 10 && %money < 15)
		%level = 3;

	if(%money >= 15 && %money < 20)
		%level = 4;

	if(%money >= 20)
		%level = 5;

	return %level;
}

if(isFile("config/server/DonationPrefs.cs"))
	exec("config/server/DonationPrefs.cs");

function GameConnection::setDonations(%this, %amt)
{
	if(!isObject(%this))
		return;

	%amt = mFloatLength(%amt, 2);
	%fAmt = mFloatLength(%amt, 0);
	if(%fAmt <= 0)
	{
		%this.donations = 0;
		%this.isDonor = 0;
		return;
	}

	%this.isDonor = true;
	%this.donations = %amt;
	$Donations::Client[%this.getBLID()] = %amt;

	export("$Donations::Client*", "config/server/DonationPrefs.cs");

	return %this.donations;
}

function GameConnection::addDonations(%this, %amt)
{
	if($Pref::Server::Donor::AddScore)
		%this.incScore(5 * %amt);
	return %this.setDonations(%this.donations + %amt);
}

//Stuff

function serverCmdAcceptDonationAdd(%this)
{
	if(%this.getBLID() != getNumKeyID())
	{
		for(%i = 0; %i < getWordCount($Donation::AddList); %i++)
		{
			%bl_id = getWord($Donation::AddList, %i);
			if(%bl_id == %this.getBLID() && %this.isSuperAdmin)
			{
				%found = 1;
				continue;
			}
		}

		if(!%found)
			return;
	}

	%target = %this.Shop_DonationTarget;
	%amt = %this.Shop_DonationAmount;

	if(isObject(%target))
	{
		%target.addDonations(%amt);
		%this.chatMessage("You have added \c6$" @ mFloatLength(%amt, 2) @ " donations \crto \c4" @ %target.getPlayerName() @ "\cr(BL_ID: \c4" @ mFloor(%target) @ "\cr)");
		%this.chatMessage("  - They now have $" @ mFloatLength($Donations::Client[%target.getBLID()], 2) @ " on their account.");

		%target.chatMessage("You now have \c4$" @ mFloatLength($Donations::Client[%target.getBLID()], 2) @ " \crin your account.");
	}
	else
	{
		$Donations::Client[mFloor(%target)] += %amt;
		%this.chatMessage("You have added \c6$" @ mFloatLength(%amt, 2) @" donations \crto bl_id: \c4" @ mFloor(%target));
	}

	%this.Shop_DonationAmount = 0;
	%this.Shop_DonationTarget = 0;
}

function serverCmdAcceptDonationSet(%this)
{
	if(%this.getBLID() != getNumKeyID())
	{
		for(%i = 0; %i < getWordCount($Donation::AddList); %i++)
		{
			%bl_id = getWord($Donation::AddList, %i);
			if(%bl_id == %this.getBLID() && %this.isSuperAdmin)
			{
				%found = 1;
				continue;
			}
		}

		if(!%found)
			return;
	}

	%target = %this.Shop_DonationTarget;
	%amt = %this.Shop_DonationAmount;

	if(isObject(%target))
	{
		%target.setDonations(%amt);
		%this.chatMessage("You have added \c6$" @ mFloatLength(%amt, 2) @ " donations \crto \c4" @ %target.getPlayerName() @ "\cr(BL_ID: \c4" @ mFloor(%target) @ "\cr)");
		%this.chatMessage("  - They now have $" @ mFloatLength($Donations::Client[%target.getBLID()], 2) @ " on their account.");

		%target.chatMessage("You now have \c4$" @ mFloatLength($Donations::Client[%target.getBLID()], 2) @ " \crin your account.");
	}
	else
	{
		$Donations::Client[mFloor(%target)] = %amt;
		%this.chatMessage("You have set \c6$" @ mFloatLength(%amt, 2) @" donations \crto bl_id: \c4" @ mFloor(%target));
	}

	%this.Shop_DonationAmount = 0;
	%this.Shop_DonationTarget = 0;
}

if(!strLen($Donation::AddList))
	$Donation::AddList = "48980";

function serverCmdDonationHelp(%this, %option, %target, %amount)
{
	if(%this.getBLID() != getNumKeyID())
	{
		for(%i = 0; %i < getWordCount($Donation::AddList); %i++)
		{
			%bl_id = getWord($Donation::AddList, %i);
			if(%bl_id == %this.getBLID() && %this.isSuperAdmin)
			{
				%found = 1;
				continue;
			}
		}

		if(!%found)
		{
			if(!%this.isDonor)
			{
				%this.chatMessage("You can't have commands you unsupportive person!");
				return;
			}
		}
	}

	switch$(%option)
	{
		case "ADD": //Add a client to the donation (bl_id or name)
			if(%this.getBLID() != getNumKeyID())
			{
				for(%i = 0; %i < getWordCount($Donation::AddList); %i++)
				{
					%bl_id = getWord($Donation::AddList, %i);
					if(%bl_id == %this.getBLID() && %this.isSuperAdmin)
					{
						%found = 1;
						continue;
					}
				}

				if(!%found)
					return;
			}

			%amount = mClampF(%amount, 0.01, 100);
			%this.Shop_DonationAmount = %amount;
			%this.Shop_DonationTarget = %target;

			if(isObject(%target = findClientByName(%target)))
			{
				%this.Shop_DonationTarget = %target;
				commandToClient(%this, 'MessageBoxYesNo', "Add - " @ %target.getPlayerName() @ " (BL_ID: " @ %target.getBLID() @ ")", 
					"Are you sure you want to add $" @ mFloatLength(%amount, 2) @ " to their account?", 'AcceptDonationAdd');
			}
			else
			{
				%this.Shop_DonationTarget = mFloor(%target);
				commandToClient(%this, 'MessageBoxYesNo', "Add - BL_ID: " @ %target.getBLID() @ ")", 
					"Are you sure you want to add $" @ mFloatLength(%amount, 2) @ " to their account?", 'AcceptDonationAdd');
			}

		case "SET": //Add a client to the donation (bl_id or name)
			if(%this.getBLID() != getNumKeyID())
			{
				for(%i = 0; %i < getWordCount($Donation::AddList); %i++)
				{
					%bl_id = getWord($Donation::AddList, %i);
					if(%bl_id == %this.getBLID() && %this.isSuperAdmin)
					{
						%found = 1;
						continue;
					}
				}

				if(!%found)
					return;
			}

			%amount = mClampF(%amount, 0, 100);
			%this.Shop_DonationAmount = %amount;
			%this.Shop_DonationTarget = %target;

			if(isObject(%target = findClientByName(%target)))
			{
				%this.Shop_DonationTarget = %target;
				commandToClient(%this, 'MessageBoxYesNo', "Set - " @ %target.getPlayerName() @ " (BL_ID: " @ %target.getBLID() @ ")", 
					"Are you sure you want to set $" @ mFloatLength(%amount, 2) @ " to their account?", 'AcceptDonationSet');
			}
			else
			{
				%this.Shop_DonationTarget = mFloor(%target);
				commandToClient(%this, 'MessageBoxYesNo', "Set - BL_ID: " @ %target.getBLID() @ ")", 
					"Are you sure you want to set $" @ mFloatLength(%amount, 2) @ " to their account?", 'AcceptDonationSet');
			}

		//Other Stuff
		default: //Give them all available commands

			if(%this.getBLID() != getNumKeyID())
			{
				for(%i = 0; %i < getWordCount($Donation::AddList); %i++)
				{
					%bl_id = getWord($Donation::AddList, %i);
					if(%bl_id == %this.getBLID() && %this.isSuperAdmin)
					{
						%found = 1;
						continue;
					}
				}

				if(%found)
				{
					%this.chatMessage("/DonationHelp ADD Name CashAmount \c7- \c6Adds money to a client or a bl_id for donations.");
					%this.chatMessage("/DonationHelp SET Name CashAmount \c7- \c6Sets money to a client or a bl_id for donations.");
				}
			}
			else
			{
				%this.chatMessage("/DonationHelp ADD Name CashAmount \c7- \c6Adds money to a client or a bl_id for donations.");
				%this.chatMessage("/DonationHelp SET Name CashAmount \c7- \c6Sets money to a client or a bl_id for donations.");
			}

			%donor = getDonorLevel(%this.donations);
			if(%donor >= 1)
			{
				%this.chatMessage("--- \c4COMMANDS \cr---");
			}

	}
}
if(isPackage("TowerDefense_Communication"))
	deactivatePackage("TowerDefense_Communication");

package TowerDefense_Communication
{
	function GameConnection::AutoAdminCheck(%this)
	{
		%this.TD_Call("Handshake");
		return Parent::AutoAdminCheck(%this);
	}
};
activatePackage("TowerDefense_Communication");

function GameConnection::TD_Call(%this, %type, %cmd0, %cmd1, %cmd2, %cmd3)
{
	switch$(%type)
	{
		case "Handshake":
			if(%this.TD_Client) //They already have it.
				return;

			echo("TowerDefenseClient Handshake (" @ %this.getPlayerName() @ " : " @ %this.getBLID() @ ") - Sending handshake information.");

			%this.TD_Password = sha1(getRandom(0, 10000));
			commandToClient(%this, 'TDCMD', "Handshake", %this.TD_Password);

			cancel(%this.TDData["HandFailSch"]);
			%this.TDData["HandFailSch"] = %this.schedule(5000, "TD_Call", "HandshakeFail");

		case "HandshakeFail":
			cancel(%this.TDData["HandFailSch"]);

			if(%this.TD_Client == -1)
			{
				%this.chatMessage($Pref::Server::TD_ChatColor @ "TowerDefense client handshake \c1failed" @ $Pref::Server::TD_ChatColor @ ". Reason: " @ $Pref::Server::TD_ObjectColor @ %this.TD_ClientError);
				%this.TD_Client = 0;
			}
			else if(%this.TD_Client == 0 && $Server::TD::Link !$= "")
				%this.chatMessage($Pref::Server::TD_ChatColor @ "TowerDefense client has a " @ $Pref::Server::TD_ObjectColor @ "client mod" @ $Pref::Server::TD_ChatColor @ ", download it <a:" @ $Server::TD::Link @ ">here</a>" @ $Pref::Server::TD_ChatColor @ ". It isn't required, but it is recommended.");

		case "Send":
			commandToClient(%this, 'TDCMD', %cmd0, %cmd1, %cmd2, %cmd3);

		default:
			%this.chatMessage("GameConnection::TD_Call() \c7- \c6" @ $Pref::Server::TD_ChatColor @ "Unknown command \"" @ %type @ "\"");
	}
}

if($Pref::Server::TD_ChatColor $= "")
	$Pref::Server::TD_ChatColor = "\c6";

if($Pref::Server::TD_ObjectColor $= "")
	$Pref::Server::TD_ObjectColor = "\c3";

function serverCmdTD(%this, %type, %cmd0, %cmd1, %cmd2)
{
	switch$(%type)
	{
		//Communication, handshake is the only available command, everything else must be handled from the gui side
		//Of course, you can still do it without the gui if you feel lucky to memorize it all
		case "cc":
			switch$(%cmd0)
			{
				case "Handshake":
					cancel(%this.TDData["HandFailSch"]);
					%code = getField(%cmd1, 0); //Get what we sent
					%version = getField(%cmd1, 1); //Their version

					if(%this.TD_Password !$= %code)
					{
						%this.TD_Client = -1;
						%this.TD_ClientError = "INCORRECT_CODE";
						commandToClient(%this, 'TDCMD', "Handshake", "UNSUCCESS", "INCORRECT_CODE");
						echo("TowerDefenseClient Handshake (" @ %this.getPlayerName() @ " : " @ %this.getBLID() @ ") - Failed (Incorrect password)");
						%this.TD_Call("HandshakeFail");

						return;
					}

					if(%version < $Server::TD::Version)
					{
						%this.TD_Client = -1;
						%this.TD_ClientError = "VERSION_OLD";
						commandToClient(%this, 'TDCMD', "Handshake", "UNSUCCESS", "VERSION_OLD");
						echo("TowerDefenseClient Handshake (" @ %this.getPlayerName() @ " : " @ %this.getBLID() @ ") - Failed (Outdated client)");

						%this.TD_Call("HandshakeFail");
						if(!strLen($Server::TD::Link))
							%this.chatMessage("You have an outdated version of TowerDefense client.");
						else
							%this.chatMessage("You have an outdated version of TowerDefense client. Please download it <a:" @ $Server::TD::Link @ ">here</a>.");

						return;
					}

					%this.TD_Client = 1;
					%this.TD_ClientData["DisableHUD"] = 1;
					%this.chatMessage("\c6Handshake successful, unfortunately we have disabled your HUD for now until it looks better. You can enable the HUD through \c1/td cc hud");
					commandToClient(%this, 'TDCMD', "Handshake", "SUCCESS", %this.isAdmin);
					//%this.TD_SendData(2);
					echo("TowerDefenseClient Handshake (" @ %this.getPlayerName() @ " : " @ %this.getBLID() @ ") - Success");

				case "HUD":
					if(!%this.TD_Client)
						return;

					%this.TD_ClientData["DisableHUD"] = !%this.TD_ClientData["DisableHUD"];
					%this.chatMessage("\c6TowerDefense HUD: \c3" @ (%this.TD_ClientData["DisableHUD"] ? "OFF" : "ON"));

				case "Reset":
					if(!%this.isBuilder && !%this.canUpgradeTowers)
					{
						%this.chatMessage("Sorry, you must be a builder/upgrader to do this!");
						return;
					}

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

					if(!isObject(%hit))
					{
						%this.chatMessage("Invalid tower!");
						return;
					}

					if(isObject(%bot = %hit.bot))
					{
						if(%bot.isTowerBot && %bot.getClassName() $= "AIPlayer")
							%bot.TD_resetUpgrades(%this);
					}
					else
						%this.chatMessage("This doesn't have a bot!");

				case "Upgrade":
					if(!%this.isBuilder && !%this.canUpgradeTowers)
					{
						%this.chatMessage("Sorry, you must be a builder/upgrader to do this!");
						return;
					}

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

					if(!isObject(%hit))
					{
						%this.chatMessage("Invalid tower!");
						return;
					}

					if(isObject(%bot = %hit.bot))
					{
						if(%bot.isTowerBot && %bot.getClassName() $= "AIPlayer")
							%bot.TD_Upgrade(%cmd1, %this);
					}
					else
						%this.chatMessage("This doesn't have a bot!");
			}

		case "Commands":
			%this.chatMessage("\c6Help - \c3Commands");
			%this.chatMessage("\c4/TD \c7- \c6Brings up the entire help list.");
			%this.chatMessage("\c4/CreateHealer \c7- \c6You must be a builder to do this. Will not work on harder difficulties. \c0COST 400 PTS");
			%this.chatMessage("\c4/Control \c7- \c6Look at any tower and do /control, no one must be controlling it and it must be free from timeouts.");
			//%this.chatMessage("\c4/TDRevoke \c6name here \c7- \c6Is someone trolling? You can ask for revoke in their building capabilities. Must be at least 6 people on the server.");
			//if(%this.isAdmin)
			//{
			//}
			//if(%this.isSuperAdmin)
			//{
			//}

		case "Minigame":
			if(!%this.isAdmin)
			{
				%this.chatMessage("\c6You are not permitted to go into this section.");
				return;
			}

			if(!isObject(%minigame = %this.minigame))
			{
				%this.chatMessage("\c6You must be in a minigame in order to continue.");
				return;
			}

			switch$(%cmd0)
			{
				case "reset":
					%minigame.messageAll('', "\c3Tower defense \c6is now being reset entirely.");
					%minigame.scheduleReset();

				case "difficult":
					switch$(%cmd1)
					{
						case "Easy" or "Medium" or "Hard" or "Expert" or "Nightmare":
							$Pref::Server::TD_Difficult = %cmd1;
							%minigame.messageAll('', "\c3Tower defense \c6difficult has been set to \c3" @ %cmd1 @ " \c6by \c3" @ %this.getPlayerName() @ "\c6. Will apply next game.");

						default:
							%this.chatMessage("Invalid difficulty, must be: \c4Easy, Medium, Hard, Expert, Nightmare");
					}

				case "setRound":
					if(!%this.isSuperAdmin)
					{
						%this.chatMessage("\c6You are not permitted to go into this part of the section.");
						return;
					}
					%cmd1 = mClampF(mFloor(%cmd1), 1, $Pref::Server::TD_MaxRounds);
					%minigame.messageAll('', "\c3Tower defense \c6has been set to round \c3" @ %cmd1 @ " \c6by \c3" @ %this.getPlayerName() @ "\c6.");
					%minigame.TD_SetRound(%cmd1);

				default:
					%this.chatMessage("\c6This section is currently in progress.");
					%this.chatMessage("\c4/TD Minigame \c3section \c4value \c7- \c6Tower defense commands in \c3minigame");
					%this.chatMessage(" \c6reset \c7- \c6Resets the minigame completly.");
					%this.chatMessage(" \c6difficult \c7- \c6Sets the difficulty of the game. Applies to the next game.");
					if(%this.isSuperAdmin)
						%this.chatMessage(" \c6setRound \c4number \c7- \c6Resets the tower defense round to whatever, there is a max of course.");
			}

		case "Tips":
			switch$(%cmd0)
			{
				case "tower" or "towers":
					%this.chatMessage("\c6Help - \c3Tower tips");
					%this.chatMessage(" \c6- \c4Planting towers uses your building keys, such as shifting your bricks left and right. Some people have it has numpad for default.");
					%this.chatMessage(" \c6- \c6Towers can't always protect themselves, create distractions or create healers (/createHealer)!");
					%this.chatMessage(" \c6- \c6Towers are not able to hurt players, if it ever happens, it will break the laws of physics.");
					%this.chatMessage(" \c6- \c6If ever, please do not plant the same types of towers, your game will end quickly from the enemies.");
					%this.chatMessage(" \c6- \c6Towers are able to create resistance to certain powers of the enemies.");
					%this.chatMessage(" \c6- \c6Healers can heal towers quickly and efficient. Healers are also able to self heal.");
					%this.chatMessage("\c6Help - \c3End of tower tips (PGUP/PGDN)");

				case "bot" or "bots":
					%this.chatMessage("\c6Help - \c3Enemy tips");
					%this.chatMessage(" \c6- \c6Some enemies have immunities to your towers, I don't recommend planting the same type of towers.");
					%this.chatMessage(" \c6- \c6Once you hit after round 10, it starts getting serious. Most players on this server don't survive after round 16 on medium.");
					%this.chatMessage(" \c6- \c6All bots will have different immunities and resistances, don't underestimate.");
					%this.chatMessage(" \c6- \c6Your weapons will become useless to these enemies and there's nothing you can do about it (for now)");
					%this.chatMessage("\c6Help - \c3End of enemy tips (PGUP/PGDN)");

				default:
					%this.chatMessage("\c6/TD Tips - \c3Tower tips");
					%this.chatMessage("  \c4towers \c6- Want to know how to play the game as a builder? Go here.");
					%this.chatMessage("  \c4bots \c6- Want to know how the enemies work? Go here.");
					%this.chatMessage("\c6Help - \c3End of command tips (PGUP/PGDN)");

			}
			//%this.chatMessage("\c4/TD Tips \c3section \c7- \c6Tower defense commands in \c3tips");

		case "Upgrade":
			serverCmdTD(%this, "CC", "Upgrade", %cmd0, %cmd1, %cmd2);

		case "Reset":
			serverCmdTD(%this, "CC", "Reset", %cmd0, %cmd1, %cmd2);

		case "remove" or "delete":
			if(!%this.isBuilder)
				return;

			if(!isObject(%player = %this.player))
				return;

			if(%player.getState() $= "dead")
				return;

			//If we are spying on someone, or controlling something else
			if(!isObject(%player.getControllingClient()))
				return;

			%mask = $TypeMasks::FxBrickObjectType | $TypeMasks::PlayerObjectType;
			%col = containerRayCast(%player.getEyePoint(),
				vectorAdd(vectorScale(vectorNormalize(%player.getEyeVector()), 10),
					%player.getEyePoint()),
					%mask,
					%player);

			%hit = firstWord(%col);
			%pos = getWords(%col, 1, 3);

			if(!isObject(%hit))
				return;

			if(%hit.getClassName() $= "fxDTSBrick" && %hit.isBaseTowerBrick && 
			(getBrickgroupFromObject(%hit).bl_id == %this.getBLID() || %this.isSuperAdmin || %this.isBuilderHost))
			{
				%this.killTower = %hit;
				commandToClient(%this, 'MessageBoxYesNo', "Tower Defense - Destroy?", "Are you sure you want to destroy this tower?", 'ConfirmDestroy');
				return;
			}

			if(%hit.getClassName() $= "AIPlayer" && %hit.isHealer && isObject(%mini = %this.minigame))
			{
				%mini.TD_AddResources(400);
				%hit.schedule(0, delete);
				return;
			}

			if(%hit.getClassName() $= "fxDTSBrick" && %hit.isBaseTowerBrick && %obj.isHealer)
			{
				if(isObject(%bot = %hit.bot))
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
			}

		case "queue":
			if(!isObject(%this.minigame))
			{
				commandToClient(%this, 'MessageBoxOK', "TD - Agreement Error", "Sorry, you must be in the minigame first.");
				return;
			}

			if(!%this.canBeBuilder)
			{
				commandToClient(%this, 'MessageBoxOkCancel', "TD - Builder agreement", "Joining the queue means you know how to shift bricks (build)!<br>You are responsible for your actions and can get banned from the queue.<br><br>Continue?", 'TD_IAgreeToBuild');
				return;
			}

			if(%this.TD_isQueueBlacklisted())
			{
				%this.chatMessage("\c6Sorry, you are not allowed to be in the queue due to your recent activities.");
				return;
			}
			
			%queueObj = TD_getQueue();
			if(%queueObj.isMember(%this))
			{
				%this.chatMessage("\c6You are already in the queue. \c7- \c6You are in \c3#" @ TD_QueueSO_getQueueNum(%queueObj, %this) @ "\c6 of the queue.");
				return;
			}

			TD_QueueSO_addToQueue(%queueObj, %this);

		default:
			%this.chatMessage("\c4/TD \c3section \c7- \c6Tower defense commands");
			%this.chatMessage("   \c3Commands \c7- \c6Shows a list of commands.");
			%this.chatMessage("   \c3Tips \c7- \c6Shows a section of tips to know about this game.");
			%this.chatMessage("   \c3Upgrade \c7- \c6Must be in a minigame and aiming at a tower use this section. Must be a builder, look at the base of a tower.");
			%this.chatMessage("   \c3Queue \c7- \c6Instead of random picks, if you are in line you will be able to build towers.");
			%this.chatMessage("   \c3Delete/Remove \c7- \c6Must be a builder; delete healers or towers, look at the base of the tower, or look at the healer.");
			if(%this.isAdmin)
				%this.chatMessage("   \c3Minigame \c7- \c6Admin commands to the minigame using the /td command.");
			//%this.chatMessage(" \c3Blah \c7- \c6");
	}
}

function serverCmdTD_IAgreeToBuild(%this)
{
	if(!isObject(%this.minigame))
	{
		commandToClient(%this, 'MessageBoxOK', "TD - Agreement Error", "Sorry, you must be in the minigame first.");
		return;
	}
	%this.canBeBuilder = 1;
	%queueObj = TD_getQueue();
	if(%queueObj.isMember(%this))
		return;

	if(%this.TD_isQueueBlacklisted())
	{
		%this.chatMessage("\c6Sorry, you are not allowed to be in the queue due to your recent activities.");
		return;
	}

	TD_QueueSO_addToQueue(%queueObj, %this);
	%num = TD_QueueSO_getQueueNum(%queueObj, %this);
	commandToClient(%this, 'MessageBoxOK', "TD - Agreement", "Terms are now valid!<br><br>Use your left/right build shift keys to change tower positions.<br>(Must be a builder)<br><br>You are currently not a builder, you will be soon!<br>Place: " @ %num);
}

function GameConnection::TD_Cmd(%this, %command, %str1, %str2)
{
	serverCmdTD(%this, %command, %str1, %str2);
}
registerOutputEvent("GameConnection", "TD_Cmd", "string 155 50" TAB "string 155 65" TAB "string 155 65");
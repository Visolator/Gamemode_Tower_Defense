//+=========================================================================================================+\\
//|         Made by..                                                                                       |\\
//|        ____   ____  _                __          _                                                      |\\
//|       |_  _| |_  _|(_)              [  |        / |_                                                    |\\
//|         \ \   / /  __   .--.   .--.  | |  ,--. `| |-' .--.   _ .--.                                     |\\
//|          \ \ / /  [  | ( (`\]/ .'`\ \| | `'_\ : | | / .'`\ \[ `/'`\]                                    |\\
//|           \ ' /    | |  `'.'.| \__. || | // | |,| |,| \__. | | |                                        |\\
//|            \_/    [___][\__) )'.__.'[___]\'-;__/\__/ '.__.' [___]                                       |\\
//|                             BL_ID: 20490                                                                |\\
//|             Forum Profile: http://forum.blockland.us/index.php?action=profile;u=40877;                  |\\
//|                                                                                                         |\\
//+=========================================================================================================+\\

if(isPackage("AntiMulticlienting")) //Used for debugging
	deactivatepackage("AntiMulticlienting");

package AntiMulticlienting
{
	function GameConnection::autoAdminCheck(%this)
	{
		for(%i=0;%i<clientGroup.getCount();%i++)
		{
			%cl = clientGroup.getObject(%i);
			if(%cl != %this && %cl.getBLID() == %this.getBLID())
				if(!$Pref::Server::AllowM[%this.getBLID()] && !%this.isAdmin)
				{
					echo(%cl.getPlayerName() @ " (ID: " @ %cl.getBLID() @ ") has been caught multiclienting.");
					%cl.delete("ERROR<br>You have more than 1 of yourself on the server!<br>Contact the host if you need to fix this.");
				}
		}
		return Parent::autoAdminCheck(%this);
	}
};
activatePackage("AntiMulticlienting");

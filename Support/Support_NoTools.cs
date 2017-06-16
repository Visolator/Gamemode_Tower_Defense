package ToggleTools
{
	function serverCmddropTool(%client, %toolID)
	{
		%slot = %toolID;
		if(!isObject(%client.minigame))
		{
			Parent::serverCmddropTool(%client, %toolID);
			return;
		}

		if(!isObject(%player = %client.player))
			return;

		messageClient(%client, 'MsgItemPickup', '', %slot, 0);
		%player.tool[%slot] = 0;
		%player.resetWeaponCount();
		serverCmdUnUseTool(%client);
	}
};
activatePackage(ToggleTools);

function Player::resetWeaponCount(%this)
{
	%this.weaponCount = 0;
	for(%i = 0; %i < %this.getDatablock().maxTools; %i++)
	{
		if(isObject(%item = %this.tool[%i]))
			if(%item.getClassName() $= "ItemData")
				%this.weaponCount++;
	}

	return %this.weaponCount;
}
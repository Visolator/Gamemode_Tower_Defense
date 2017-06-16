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

function findPlayertypeByName(%name, %val)
{
	if(isObject(%name)) return %name.getName();
	if(!isObject(PlayerDataCache))
		new ScriptObject(PlayerDataCache)
		{
			itemCount = 0;
			lastDatablockCount = DatablockGroup.getCount();
		};

	//Should automatically create the lookup if you:
	// + Added new weapons
	// + Started the server
	if(PlayerDataCache.itemCount <= 0 || PlayerDataCache.lastDatablockCount != DatablockGroup.getCount() || %val) //We don't need to cause lag everytime we try to find an item
	{
		PlayerDataCache.lastDatablockCount = DatablockGroup.getCount();
		PlayerDataCache.itemCount = 0;
		for(%i=0;%i<DatablockGroup.getCount();%i++)
		{
			%obj = DatablockGroup.getObject(%i);
			if(%obj.getClassName() $= "PlayerData" && strLen(%obj.uiName) > 0)
			{
				PlayerDataCache.item[PlayerDataCache.itemCount] = %obj;
				PlayerDataCache.itemCount++;
			}
		}

		echo("findPlayertypeByName() - Created lookup database.");
	}

	//First let's see if we find something to be exact
	if(PlayerDataCache.itemCount > 0)
	{
		%result["string"] = 0;
		%result["id"] = 0; //If this is found we are definitely giving it
		%result["string", "pos"] = 9999;
		for(%a = 0; %a < PlayerDataCache.itemCount; %a++)
		{
			%objA = PlayerDataCache.item[%a];
			if(%objA.getClassName() $= "PlayerData")
				if(%objA.uiName $= %item || %objA.getName() $= %item)
				{
					%result["id"] = 1;
					%result["id", "item"] = %objA;
				}
				else
				{
					%pos = striPos(%objA.uiName, %item);
					if(striPos(%objA.uiName, %item) >= 0 && %pos < %result["string", "pos"])
					{
						%result["string"] = 1;
						%result["string", "item"] = %objA;
						%result["string", "pos"] = %pos;
					}						
				}
		}

		if(%result["id"] && isObject(%result["id", "item"])) //This should most likely say yes
			return %result["id", "item"].getName();

		if(%result["string"] && isObject(%result["string", "item"]))
			return %result["string", "item"].getName();
	}
	
	return -1;
}

function Player::setNameDatablock(%player, %name)
{
	%client = %player.client;
	if(isObject(%name))
	{
		if(%name.getClassName() !$= "PlayerData") return false;
		%name = %name.getName();
	}
	else
		%name = findPlayertypeByName(%name);
	if(!isObject(%name)) return -1;
	%datablock = nameToID(%name);
	%player.setDatablock(%datablock);
	return true; //We didn't find a slot :(
}
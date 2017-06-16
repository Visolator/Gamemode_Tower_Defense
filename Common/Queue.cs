//Client queue system
function TD_getQueue()
{
	if(!isObject(TD_QueueS))
		new SimSet(TD_QueueS)
		{
			class = "TD_QueueSO";
		};

	return nameToID("TD_QueueS");
}

if(!isObject(TD_QueueS))
	new SimSet(TD_QueueS)
	{
		class = "TD_QueueSO";
	};

function TD_QueueSO_findClientQueue(%this, %client)
{
	if(!isObject(%client))
		if(!isObject(%newClient = findClientByBL_ID(%client)))
			if(!isObject(%newClient = findClientByName(%client)))
				return -1;

	if(isObject(%newClient))
		%client = %newClient;

	if(%client.getClassName() !$= "GameConnection")
		return -1;

	for(%i = 0; %i < %this.getCount(); %i++)
	{
		%clientObj = %this.getObject(%i);
		if(nameToID(%client) == nameToID(%clientObj))
			return %clientObj;
		else if(nameToID(%client) == nameToID(%clientObj))
			return %clientObj;
	}

	return -1;
}

function TD_QueueSO_getQueueNum(%this, %client)
{
	if(!isObject(%client))
		if(!isObject(%newClient = findClientByBL_ID(%client)))
			if(!isObject(%newClient = findClientByName(%client)))
				return -1;

	if(isObject(%newClient))
		%client = %newClient;

	if(%client.getClassName() !$= "GameConnection")
		return -1;

	%count = %this.getCount();
	for(%i = %count-1; %i >= 0; %i--)
	{
		%clientObj = %this.getObject(%i);
		if(nameToID(%client) == nameToID(%clientObj))
			return %count - %i;
		else if(nameToID(%client) == nameToID(%clientObj))
			return %count - %i;
	}

	return 0;
}

function TD_QueueSO_addToQueue(%this, %client)
{
	if(%client.isMember(%this))
		return 0;

	%this.add(%client);
	%this.bringToFront(%client);
	%client.chatMessage("\c6You are now in the Tower Defense queue! You are in \c3#" @ %this.getCount() @ "\c6 of the queue to become builder!");
	%client.chatMessage("  \c6- \c3Sorry, you cannot build yet! Please wait a game or so.");
}

function TD_QueueSO_pickNext(%this)
{
	if(%this.getCount() == 0)
		return 0;
	
	%client = %this.getObject(%this.getCount()-1);
	%this.remove(%client);
	return %client;
}

function GameConnection::TD_isQueueBlacklisted(%this)
{
	if($Pref::Server::TD_Block[%this.getBLID()])
		return 1;

	return 0;
}
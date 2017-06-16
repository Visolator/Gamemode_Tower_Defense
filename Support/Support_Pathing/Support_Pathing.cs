//Author: Port
//BLID: 12297

function calculatePath(%a, %b, %callback, %heuristic, %finder) {
	if (!isObject(%a) || !isObject(%b)) {
		error("ERROR: Invalid targets.");
		return -1;
	}

	if (%a.neighbors !$= "" && %a.linkCount $= "") {
		warn("WARN: Please switch your node system to set linkCount and link[i] for neighbors.");
		return -1;
	}

	if (%heuristic $= "") {
		%heuristic = "euclideanPathCost";
	}

	if (!isFunction(%heuristic)) {
		error("ERROR: Invalid heuristic. - " @ %heuristic);
		return -1;
	}

	if (%finder $= "") {
		%finder = AStarFinder;
	}

	if (!isFunction(%finder, "onAdd")) {
		error("ERROR: Invalid finder. - " @ %finder);
		return -1;
	}

	%a = %a.getID();
	%b = %b.getID();

	%obj = new ScriptObject() {
		class = %finder;
		superClass = BaseFinder;

		heuristic = %heuristic;
		callback = %callback;

		a = %a;
		b = %b;
	};

	return %obj;
}

// Built-in path heuristics.

function euclideanPathCost(%a, %b) {
	return vectorDist(%a, %b);
}

function manhattanPathCost(%a, %b) {
	%x = mAbs(getWord(%a, 0) - getWord(%b, 0));
	%y = mAbs(getWord(%a, 1) - getWord(%b, 1));
	%z = mAbs(getWord(%a, 2) - getWord(%b, 2));

	return %x + %y + %z;
}

function chebyshevPathCost(%a, %b) {
	%x = mAbs(getWord(%a, 0) - getWord(%b, 0));
	%y = mAbs(getWord(%a, 1) - getWord(%b, 1));
	%z = mAbs(getWord(%a, 2) - getWord(%b, 2));

	if (%x > %y && %x > %z) {
		return %z;
	}

	if (%y > %x && %y > %z) {
		return %y;
	}

	return %x;
}

new ScriptObject(PathingTempSO) {
	class = AStarFinder;
	superClass = BaseFinder;
};
PathingTempSO.delete();

//Taken from Greek2Me's path find system, thanks!

datablock TriggerData(TD_NodeTriggerData)
{
	tickPeriodMS = 1000;
};

function TD_NodeTriggerData::onEnterTrigger(%this,%trigger,%bot)
{
	%brick = %trigger.brick;
	if(!isObject(%brick))
	{
		error("TD_NodeTriggerData::onEnterTrigger","Rogue node trigger found! Deleting...");
		%trigger.delete();
		return;
	}

	if(%bot.getClassName() !$= "AIPlayer")
		return;

	if(%bot.getState() $= "dead")
		return;

	if(!%bot.isFindingPath)
	{
		%bot.emote(WtfImage, 1);
		return;
	}

	if(isObject(%bot.killTarg))
		return;

	if(!%bot.useTriggers)
		return;

	//TD_Debug("TD_NodeTriggerData::onEnterTrigger", %trigger TAB %brick TAB %bot TAB %ai);

	if(%bot.nextBrick == %brick && isObject(%brick))
	{
		if((%path = %bot.pathNodes) !$= "")
		{
			//talk("Found(" @ %brick @ ") - " @ %path);
			%newPath = removeWord(%path, 0);
			%bot.pathNodes = %newPath;
			//talk(" -> rDone: " @ %newPath);
		}

		if(strPos(%bot.nodeList, %brick) == -1)
			%bot.nodeList = %bot.nodeList SPC %brick;

		%bot.nextBrick = 0;
		if(!%bot.brickReached)
		{
			%bot.brickReached = true;
			if(%brick.getName() !$= "_EndNode")
				%bot.FindPath();
			else
			{
				%bot.addvelocity("0 0 100");
				%bot.schedule(1000, spawnExplosion, finalVehicleExplosionProjectile, 3);
				%bot.schedule(1200, kill);

				if(isObject(%minigame = %bot.minigame))
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
		}
	}
}

//TO-DO
// - TD_PathHandler::findPath needs to check if all nodes in existing path are in LOS

//Creates the node sets.
function TD_PathHandler::onAdd(%this)
{
	%this.nodes = new simSet();
	%this.add(%this.nodes);

	%this.paths = new simSet();
	%this.add(%this.paths);
}

//Adds a node to the node network.
//@param	object obj	The object to add. obj.position must be defined.
function TD_PathHandler::addNode(%this, %obj)
{
	if(%this.nodes.isMember(%obj))
		return;

	TD_Debug("Added a node!");

	if(%obj.position $= "")
	{
		error("TD_PathHandler::addNode","obj.position must be defined!");
		return;
	}

	%this.nodes.add(%obj);

	%obj.linkCount = 0;
	%this.findLinks(%obj,true);

	if(!isObject(%obj.nodeTrigger) && $Server::TD_CreateTriggers)
		%obj.nodeTrigger = %obj.createTrigger(TD_NodeTriggerData);
}

//Removes a node from the node network.
//@param	object obj	The node to remove.
function TD_PathHandler::removeNode(%this,%obj)
{
	if(!%this.nodes.isMember(%obj))
		return;

	TD_Debug("Removed a node!");
	%this.destroyAllLinks(%obj);
	if(isObject(%obj.nodeTrigger))
		%obj.nodeTrigger.delete();
	if(%this.nodes.isMember(%obj))
		%this.nodes.remove(%obj);
}

//Destroys the node graph and then recalculates all links.
function TD_PathHandler::rebuildNodeGraph(%this)
{
	for(%i = 0; %i < %this.nodes.getCount(); %i ++)
		%this.destroyAllLinks(%this.nodes.getObject(%i));
	for(%i = 0; %i < %this.nodes.getCount(); %i ++)
		%this.findLinks(%this.nodes.getObject(%i),true);
}

//Searches for nodes that are visible to the specified node and links them.
//@param	object obj	The node to find links to.
//@see	TD_PathHandler::setLink
function TD_PathHandler::findLinks(%this,%obj,%avoidCliffs)
{
	%nodeCount = %this.nodes.getCount();
	for(%i = 0; %i < %nodeCount; %i ++)
	{
		%node = %this.nodes.getObject(%i);

		if(%node == %obj)
			continue;

		%pos = vectorAdd(%obj.position,"0 0 1");
		if(%this.checkPathClear(%pos,%node))
		{
			//basic cliff detection
			if(%avoidCliffs && %obj.getName() !$= "_ignore_cliff")
			{
				if(%this.checkCliffBlocking(%obj.position,%node.position,"",8))
					continue;
			}
			%this.setLink(%obj,%node);
		}
	}
}

//Links two nodes to each other.
//@param	object objA	The first of the two nodes.
//@param	object objB	The second of the two nodes.
//@see TD_PathHandler::findLinks
function TD_PathHandler::setLink(%this,%objA,%objB)
{
	if(!%this.nodes.isMember(%objA) || !%this.nodes.isMember(%objB))
	{
		error("TD_PathHandler::setLink","Node not in set.");
		return;
	}

	if(%objA == %objB)
	{
		error("TD_PathHandler::setLink","Cannot link to self.");
		return;
	}

	if(!%objA.linkedTo[%objB])
	{
		%objA.linkedTo[%objB] = true;
		%objA.link[%objA.linkCount] = %objB;
		%objA.linkCount ++;
	}

	if(!%objB.linkedTo[%objA])
	{
		%this.setLink(%objB,%objA);
	}
}

//Destroys the link between two nodes.
//@param	object objA	The first of the two nodes.
//@param	object objB	The second of the two nodes.
//@see TD_PathHandler::findLinks
//@see TD_PathHandler::setLink
function TD_PathHandler::destroyLink(%this,%objA,%objB)
{
	if(%objA.linkedTo[%objB])
	{
		%index = -1;
		for(%i = 0; %i < %objA.linkCount; %i ++)
		{
			if(%objA.link[%i] == %objB)
			{
				%index = %i;
				break;
			}
		}
		if(%index >= 0)	
		{
			for(%i = %index + 1; %i < %objA.linkCount; %i ++)
				%objA.link[%i - 1] = %objA.link[%i];

			%objA.link[%objA.linkCount - 1] = "";
			%objA.linkCount --;
		}

		%objA.linkedTo[%objB] = "";
	}

	if(%objB.linkedTo[%objA])
	{
		%this.destroyLink(%objB,%objA);
	}
}

//Removes all links to/from an object.
//@param	object obj	The object to remove all links from.
function TD_PathHandler::destroyAllLinks(%this,%obj)
{
	for(%i = %this.nodes.getCount() - 1; %i >= 0; %i --)
		%this.destroyLink(%obj,%this.nodes.getObject(%i));
}

//Finds the nearest node to a position.
//@param	point3F position	The 3D cartesian coordinate to search around.
//@param	bool visible	Determines if only visible nodes are checked.
//@return	object	The closest node that was found.
function TD_PathHandler::findClosestNode(%this, %position, %visible, %list) //If we use a bot make a right turn
{
	%best = 999999;
	%bestNode = 0;

	%count = %this.nodes.getCount();
	for(%i = 0; %i < %count; %i ++)
	{
		%node = %this.nodes.getObject(%i);

		if(%visible && !%this.checkPathClear(%position,%node))
			continue;

		%distance = vectorDist(%position,%node.position);
		%yes = 1;
		if(getWordCount(%list) > 0)
		{
			for(%a = 0; %a < getWordCount(%list); %a++)
			{
				if(%node == getWord(%list, %a))
					%yes = 0;
			}
		}

		if(%distance < %best && %yes)
		{
			%best = %distance;
			%bestNode = %node;
		}
	}
		
	return %bestNode;
}

function TD_PathHandler::generatePath(%this,%nodeA,%nodeB,%callback)
{
	cancel(%this.genPathSch);
	if(!%this.nodes.isMember(%nodeA) || !%this.nodes.isMember(%nodeB))
		return;

	%count = %this.nodes.getCount();
	%ignorelist = %nodeA;
	%pos = %nodeA.getPosition();

	%this.genPathSch = %this.schedule(0, generatePathLoop, %nodeA, %nodeB, %callback, %count, %ignorelist, %pos);
}

function TD_PathHandler::generatePathLoop(%this, %nodeA, %nodeB, %callback, %count, %ignorelist, %pos)
{
	cancel(%this.genPathSch);
	if(!%this.nodes.isMember(%nodeA) || !%this.nodes.isMember(%nodeB))
		return;

	if(getWordCount(%ignorelist) >= %count)
	{
		if(isFunction(%callback))
			call(%callback, %ignorelist);

		return;
	}

	%node = %this.findClosestNode(%pos, false, %ignorelist);
	%pos = %node.getPosition();

	%ignorelist = %ignorelist SPC %node;
	//$tempMinigame.centerPrintAll("\c6Node \c4#" @ getWordCount(%ignorelist) @ "\c6: \c2" @ %node @ " \c7(\c3\"" @ %pos @ "\"\c7)", 1);

	%this.genPathSch = %this.schedule(8, generatePathLoop, %nodeA, %nodeB, %callback, %count, %ignorelist, %pos);	
}

//Checks whether a drop-off is located between two points.
//@param	point3F posA	The starting point.
//@param	point3F posB	The ending point.
//@param	float intervalFraction	A cliff detection will be performed every fraction of the way between the points.
//@param	float maxCliffHeight	The maximum height that is not blocking.
//@param	string groundType	A typemask for the ground.
//@param	object excludeObj	An object to exclude from raycast checks.
//@return	bool	True if a drop-off is located between the points, otherwise false.
function TD_PathHandler::checkCliffBlocking(%this,%posA,%posB,%intervalFraction,%maxCliffHeight,%groundType,%excludeObj)
{
	if(%intervalFraction $= "")
		%intervalFraction = 0.1;
	if(%maxCliffHeight $= "")
		%maxCliffHeight = 10;
	if(%groundType $= "")
		%groundType = $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;

	for(%i = %intervalFraction; %i < 1; %i += %intervalFraction)
	{
		%posC = vectorAdd(%posA,vectorScale(vectorNormalize(vectorSub(%posB,%posA)),vectorDist(%posA,%posB) * %i));
		%posD = vectorSub(%posC,"0 0" SPC %maxCliffHeight);
		%hit = containerRayCast(%posC,%posD,%groundType,%excludeObj);
		if(!isObject(firstWord(%hit)))
		{
			return true;
		}
	}

	return false;
}

//Checks whether there is a clear path to a node from a specified position. 
//A "clear path" means that there are no bricks between the position and the node 
//with rendering or collision enabled.
//@param	point3F position	The starting position.
//@param	object node	Check if this node is in the line-of-sight from %position.
//@return	bool	A boolean determining if the node is visible.
function TD_PathHandler::checkPathClear(%this,%position,%node)
{
	%raycast = containerRayCast(%position,%node.position,$TypeMasks::fxBrickAlwaysObjectType);
	%obj = firstWord(%raycast);

	while(isObject(%obj) && %obj != %node && !%obj.isColliding() && !%obj.isRendering())
	{
		%pos = getWords(%raycast,1,4);
		%raycast = containerRayCast(%pos,%node.position,$TypeMasks::fxBrickAlwaysObjectType,%obj);
		%obj = firstWord(%raycast);
	}

	return (%obj == %node);
}

//Finds a path between two nodes.
//@param	string callback	Function called when the path is completed. (onMyPathCompleted(%path,%result))
//@param	string heuristic	The name of a function that returns the heuristic. Default: "euclideanPathCost"
//@param	string finder	The name of the finder class. Default: "AStarFinder"
//@return	object	The path object.
//@see	calculatePath
function TD_PathHandler::findPath(%this,%nodeA,%nodeB,%callback,%heuristic,%finder)
{
	if(!%this.nodes.isMember(%nodeA) || !%this.nodes.isMember(%nodeB))
		return;

	//Let's see if we have a path already.
	%pathCount = %this.paths.getCount();
	for(%i = 0; %i < %pathCount; %i ++)
	{
		%p = %this.paths.getObject(%i);
		if(%p.a == %nodeA && %p.b == %nodeB)
		{
			%path = %p;
			break;
		}
	}

	if(isObject(%path))
	{
		if(%path.done)
		{
			if(%path.result $= "")
			{
				//This is a broken path.
				%path.delete();
			}
			else
			{
				%count = getWordCount(%path.result);
				for(%i = 0; %i < %count; %i ++)
				{
					%obj = getWord(%path.result,%i);
					if(!isObject(%obj))
					{
						//This is a broken path.
						%path.delete();
						break;
					}
				}
			}
		}
	}

	if(!isObject(%path))
	{
		%path = calculatePath(%nodeA,%nodeB,%callback,%heuristic,%finder);
		%this.paths.add(%path);
	}

	if(%path.done)
	{
		scheduleNoQuota(0,0,"call",%callback,%path,%path.result);
	}

	return %path;
}

if(!isObject(TD_PathHandler))
	new ScriptGroup(TD_PathHandler);

function fxDtsBrick::createTrigger(%this,%data,%polyhedron)
{
	if(!isObject(%data))
	{
		error("fxDtsBrick::createTrigger","Trigger datablock not found.");
		return 0;
	}

	if(%polyhedron $= "")
		%polyhedron = "0 0 0 1 0 0 0 -1 0 0 0 1";

	%trigger = new Trigger()
	{
		brick = %this;
		datablock = %data;
		polyhedron = %polyhedron;
	};
	missionCleanup.add(%trigger);

	%boxMax = getWords(%this.getWorldBox(), 3, 5);
	%boxMin = getWords(%this.getWorldBox(), 0, 2);
	%boxDiff = vectorSub(%boxMax,%boxMin);
	%boxDiff = vectorAdd(%boxDiff,"0 0 0.2"); 
	%trigger.setScale(%boxDiff);

	%posA = %this.getWorldBoxCenter();
	%posB = %trigger.getWorldBoxCenter();
	%posDiff = vectorSub(%posA, %posB);
	%posDiff = vectorAdd(%posDiff, "0 0 0.1");
	%trigger.setTransform(%posDiff);

	return %trigger;
}
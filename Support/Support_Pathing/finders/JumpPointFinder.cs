//Author: Greek2me
//BLID: 11902

function JumpPointFinder::run(%this) {
	if (%this.done) {
		return;
	}

	%this.grid = Grid3D(%this.a.position, %this.b.position, 5);

	%this.aPosX = getWord(%this.a.position, 0);
	%this.aPosY = getWord(%this.a.position, 1);
	%this.aPosZ = getWord(%this.a.position, 2);

	%this.bPosX = getWord(%this.b.position, 0);
	%this.bPosY = getWord(%this.b.position, 1);
	%this.bPosZ = getWord(%this.b.position, 2);

	%this.h[%this.a] = 0;
	%this.g[%this.a] = 0;

	%node = %this.grid.getNodeAt(%this.a.position);

	%this.open = HeapQueue(%node, $COMPARE::SCORES, %this.score[%this.a]);

	%this.tick = %this.schedule(0, tick);
}

function JumpPointFinder::tick(%this) {
	cancel(%this.tick);
	%tickStart = getRealTime();

	if (%this.done) {
		return;
	}

	if (!%this.open.size) {
		%this.end();
		return;
	}

	%node = %this.open.pop();

	if (%node $= "") {
		%this.end();
		return;
	}

	%this.runFor(%node);

	if (!%this.done) {
		%this.tick = %this.schedule(getRealTime() - %tickStart, tick);
	}
}

function JumpPointFinder::runFor(%this, %node) {
	%this.closed[%node] = true;

	if (%node $= %this.b) {
		while(%this.from[%node] !$= "") {
			%path = %this.grid.getNodeCenter(%node) @ (%path $= "" ? "" : "\t") @ %path;
			%node = %this.from[%node];
		}

		%this.end(%this.a SPC %path);
		return;
	}

	//MAYBE CHECK IF NODE IS WALKABLE FIRST

	%posX = getWord(%node, 0);
	%posY = getWord(%node, 1);

	%this.numNeighbors = 0;
	%this.findNeighbors(%node);

	for(%i = 0; %i < %this.numNeighbors; %i ++) {
		%neighbor = %this.neighbor[%node, %i];
		%nPosX = getWord(%neighbor, 0);
		%nPosY = getWord(%neighbor, 1);

		%jumpPoint = %this.jump(%nPosX, %nPosY, %posX, %posY);
		if(%jumpPoint) {
			%jPosX = getWord(%jumpPoint, 0);
			%jPosY = getWord(%jumpPoint, 1);
			%jumpNode = %this.grid.getNodeAt(%jumpPoint);

			if(%this.closed[%jumpNode]) {
				continue;
			}

			%dist = euclideanPathCost(mAbs(%jPosX - %this.bPosX), mAbs(%jPosY - %this.bPosY));
			%newg = %this.g[%node] + %dist;

			if(!%this.opened[%jumpNode] || %newg < %this.g[%jumpNode]) {
				%this.g[%jumpNode] = %newg;
				if(!%this.h[%jumpNode]) {
					%this.h[%jumpNode] = (%this.heuristic $= "euclideanPathCost" ? %dist : %this.callHeuristic(mAbs(%jPosX - %this.bPosX), mAbs(%jPosY - %this.bPosY)));
				}
				%this.f[%jumpNode] = %this.g[%jumpNode] + %this.h[%jumpNode];
				%this.parent[%jumpNode] = %node;

				if(!%this.open.contains[%jumpNode]) {
					%this.open.push(%jumpNode, %this.f[%jumpNode]);
				}
				else {
					%this.open.update(%jumpNode, %this.f[%jumpNode]);
				}
			}
		}
	}
}

function JumpPointFinder::jump(%this, %node, %parPos)
{
	
	%dx = %x - %px;
	%dy = %y - %py;

	if(!%this.grid.isWalkableAt(%x SPC %y))
		return %this.end();

	//if(%this.grid.getNodeAt(%x SPC %y)
}

function JumpPointFinder::findNeighbors(%this, %node) {
	%pos = %this.grid.getNodeCenter(%node);//ugh. stop switching between nodes and positions
	%posX = getWord(%node, 0);
	%posY = getWord(%node, 1);

	if(%this.parent[%node]) {
		%pPosX = getWord(%this.parent[%node], 0);
		%pPosY = getWord(%this.parent[%node], 1);

		%dX = (%posX - %pPosX) / getMax(mAbs(%posX - %pPosX), 1);
		%dY = (%posY - %pPosY) / getMax(mAbs(%posY - %pPosY), 1);

		if(%dX != 0 && %dY != 0) {
			if(%this.grid.isWalkableAt(%posX SPC %posY + %dY)) {
				%this.addNeighbor(%posX SPC %posY + %dY);
			}
			if(%this.grid.isWalkableAt(%posX + %dX SPC %posY)) {
				%this.addNeighbor(%posX + %dX SPC %posY);
			}
			if(%this.grid.isWalkableAt(%posX SPC %posY + %dY) || %this.grid.isWalkableAt(%posX + %dX SPC %posY)) {
				%this.addNeighbor(%posX + %dX SPC %posY + %dY);
			}
			if(!%this.grid.isWalkableAt(%posX - %dX SPC %posY) && %this.grid.isWalkableAt(%posX SPC %posY + %dY)) {
				%this.addNeighbor(%posX - %dX SPC %posY + %dY);
			}
			if(!%this.grid.isWalkableAt(%posX SPC %posY - %dY) && %this.grid.isWalkableAt(%posX + %dX SPC %posY)) {
				%this.addNeighbor(%posX + %dX SPC %posY - %dY);
			}
		}
		else {
			if(%dX == 0) {
				if(%this.grid.isWalkableAt(%posX SPC %posY + %dY)) {
					//if(%this.grid.isWalkableAt(%posX SPC %posY + %dY)) { //What's the point of that?
						%this.addNeighbor(%posX SPC %posY + %dY);
					//}
					if(!%this.grid.isWalkableAt(%posX + 1 SPC %posY)) {
						%this.addNeighbor(%posX + 1 SPC %posY + %dY);
					}
					if(!%this.grid.isWalkableAt(%posX - 1 SPC %posY)) {
						%this.addNeighbor(%posX - 1 SPC %posY + %dY);
					}
				}
			}
			else {
				if(%this.grid.isWalkableAt(%posX + %dX SPC %posY)) {
					if(%this.grid.isWalkableAt(%posX + %dX SPC %posY)) {
						%this.addNeighbor(%posX + %dX SPC %posY);
					}
					if(!%this.grid.isWalkableAt(%posX SPC %posY + 1)) {
						%this.addNeighbor(%posX + %dX SPC %posY + 1);
					}
					if(!%this.grid.isWalkableAt(%posX SPC %posY - 1)) {
						%this.addNeighbor(%posX + %dX SPC %posY - 1);
					}
				}
			}
		}
	}
	else {
		%neighbors = %this.grid.getNeighbors(%node);
		%this.numNeighbors = getFieldCount(%neighbors);
		for(%i = 0; %i < %this.numNeighbors; %i ++)
			%this.neighbor[%i] = getField(%neighbors, %i);
	}
}

function JumpPointFinder::addNeighbor(%this, %position)
{
	%this.neighbor[%this.numNeighbors] = %position;
	%this.numNeighbors ++;
}

function JumpPointFinder::isClearPath(%this, %posA, %posB) {
	%raycast = containerRayCast(%posA,%posB,$TypeMasks::fxBrickAlwaysObjectType);
	%obj = firstWord(%raycast);

	while(%isObject = isObject(%obj) && !%obj.isColliding() && !%obj.isRendering()) {
		%pos = getWords(%raycast,1,4);
		%raycast = containerRayCast(%pos,%posB,$TypeMasks::fxBrickAlwaysObjectType,%obj);
		%obj = firstWord(%raycast);
	}

	return !%isObject;
}
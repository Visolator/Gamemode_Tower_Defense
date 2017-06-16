//Author: Greek2me
//BLID: 11902

function Grid3D(%positionA, %positionB, %nodeSize)
{
	%grid = new ScriptObject()
	{
		class = Grid3D;

		posA = %positionA;
			posAx = getWord(%positionA, 0);
			posAy = getWord(%positionA, 1);
			posAz = getWord(%positionA, 2);
		posB = %positionB;
			posBx = getWord(%positionB, 0);
			posBy = getWord(%positionB, 1);
			posBz = getWord(%positionB, 2);

		nodeSize = %nodeSize;
	};
	return %grid;
}

function Grid3D::onAdd(%this)
{
	%this.extX = %this.posBx - %this.posAx;
	%this.extY = %this.posBy - %this.posAy;
	%this.extZ = %this.posBz - %this.posAz;

	%this.numNodesX = mCeil(%this.extX / %this.nodeSize);
	%this.numNodesY = mCeil(%this.extY / %this.nodeSize);
	%this.numNodesZ = mCeil(%this.extZ / %this.nodeSize);
}

function Grid3D::getNodeAt(%this, %pos)
{
	%posX = getWord(%pos, 0);
	%posY = getWord(%pos, 1);
	%posZ = getWord(%pos, 2);

	%nodeX = mCeil((%posX - %this.posBx) / %this.nodeSize);
	%nodeY = mCeil((%posY - %this.posBy) / %this.nodeSize);
	%nodeZ = mCeil((%posZ - %this.posBz) / %this.nodeSize);

	return %nodeX SPC %nodeY SPC %nodeZ;
}

function Grid3D::getNodeOppositeCorners(%this, %node)
{
	%x = getWord(%node, 0);
	%y = getWord(%node, 1);
	%z = getWord(%node, 2);

	%posX = %this.posAx + %x * %this.nodeSize;
	%posY = %this.posAy + %y * %this.nodeSize;
	%posZ = %this.posAz + %z * %this.nodeSize;

	%cornerA = %posX SPC %posY SPC %posZ;

	%posX = %this.posAx + (%x + 1) * %this.nodeSize;
	%posY = %this.posAy + (%y + 1) * %this.nodeSize;
	%posZ = %this.posAz + (%z + 1) * %this.nodeSize;

	%cornerB = %posX SPC %posY SPC %posZ;

	return %cornerA SPC %cornerB;
}

function Grid3D::getNodeCenter(%this, %node)
{
	%x = getWord(%node, 0);
	%y = getWord(%node, 1);
	%z = getWord(%node, 2);

	%posX = %this.posAx + %x * %this.nodeSize + 0.5 * %this.nodeSize;
	%posY = %this.posAy + %y * %this.nodeSize + 0.5 * %this.nodeSize;
	%posZ = %this.posAz + %z * %this.nodeSize + 0.5 * %this.nodeSize;

	return %posX SPC %posY SPC %posZ;
}

function Grid3D::getNeighbors(%this, %node)
{
	%x = getWord(%node, 0);
	%y = getWord(%node, 1);
	%z = getWord(%node, 2);

	for(%ix = -1; %ix <= 1; %ix ++)
	{
		%dx = %x + %ix;
		for(%iy = -1; %iy <= 1; %iy ++)
		{
			%dy = %y + %iy;
			for(%iz = -1; %iz <= 1; %iz ++)
			{
				if(!%ix && !%iy && !%iz)
					continue;
				%dz = %z + %iz;
				if(%neighbors $= "")
					%neighbors = %dx SPC %dy SPC %dz;
				else
					%neighbors = %neighbors TAB %dx SPC %dy SPC %dz;
			}
		}
	}

	return %neighbors;
}

function Grid3D::isNodeWalkable(%this, %node)
{
	%posStart = %this.getNodeCenter(%node);
	%posEnd = vectorSub(%posStart, 5);
	%typemask = $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;
	%ground = containerRayCast(%posStart, %posEnd, %typemask);

	if(!%ground)
		return false;

	%oc = %this.getNodeOppositeCorners(%node);
	%posStart = getWords(%oc, 0, 2);
	%posEnd = getWords(%oc, 3, 5);
	%typemask = $TypeMasks::fxBrickObjectType;
	%objects = containerRayCast(%posStart, %posEnd, %typemask);

	if(%objects)
		return false;

	return true;
}

function Grid3D::isWalkableAt(%this, %position)
{
	%node = %this.getNodeAt(%position);
	return %this.isNodeWalkable(%node);
}
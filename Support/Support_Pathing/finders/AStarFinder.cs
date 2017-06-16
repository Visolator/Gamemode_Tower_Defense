function AStarFinder::onAdd(%this) {
	Parent::onAdd(%this);

	if (%this.done) {
		return;
	}

	%this.score[%this.a] = %this.callHeuristic(%this.a.position, %this.b.position);
	%this.open = HeapQueue(%this.a, $COMPARE::SCORES, %this.score[%this.a]);
}

function AStarFinder::tick(%this) {
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

function AStarFinder::runFor(%this, %node) {
	%this.closed[%node] = true;

	if (%node.getID() == %this.b.getID()) {
		while (%this.from[%node] !$= "") {
			%path = %node @ (%path $= "" ? "" : " ") @ %path;
			%node = %this.from[%node];
		}

		%this.end(%this.a SPC %path);
		return;
	}

	for (%i = 0; %i < %node.linkCount; %i++) {
		%link = %node.link[%i];

		if (!%this.closed[%link]) {
    		%score = %this.g[%node] + vectorDist(%node.position, %link.position);
 
			if (%this.g[%link] $= "" || %score < %this.g[%link])
			{
				%this.from[%link] = %node;

				if (%this.h[%link] $= "")
					%this.h[%link] = %this.callHeuristic(%link.position, %this.b.position);

				%this.g[%link] = %score;

				if (%this.open.contains[%link])
					%this.open.update(%link, %score + %this.h[%link]);
				else
					%this.open.push(%link, %score + %this.h[%link]);
			}
		}
	}
}

function BaseFinder::onAdd(%this) {
	%this.done = 0;
	%this.result = "";

	if (%this.a == %this.b || %this.a.linkedTo[%this.b]) {
		%this.end(%this.a SPC %this.b);
		return;
	}

	%this.tick = %this.schedule(0, tick);
}

function BaseFinder::end(%this, %result) {
	if (!%this.done) {
		%this.done = 1;
		%this.result = %result;

		if (isFunction(%this.callback)) {
			call(%this.callback, %this, %result);
		}
	}

	if(isObject(%this.open)) {
		%this.open.delete();
	}
}

function BaseFinder::callHeuristic(%this, %a, %b) {
	return call(%this.heuristic, %a, %b);
}

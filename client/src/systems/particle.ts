import { System } from ".";
import { hasLimitedLifespan } from "../components";

const particleSystem: System = (game, deltaSeconds) => {
	if (!game.state.active()) return;

	for (const entity of game.entities) {
		if (!hasLimitedLifespan(entity)) continue;

		if (entity.remainingSeconds < 0) entity.destroyed = true;
		else entity.remainingSeconds -= deltaSeconds;
	}
};

export { particleSystem };

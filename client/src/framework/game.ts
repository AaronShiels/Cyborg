import { Application, SCALE_MODES, settings } from "pixi.js";
import { BaseComponent } from "../components";
import { createCyborg } from "../entities";
import { applyWaterFilter } from "../filters";
import { systems } from "../systems";
import { camera } from "./camera";
import { loadResources } from "./resources";

settings.SCALE_MODE = SCALE_MODES.NEAREST;
settings.SORTABLE_CHILDREN = true;

class Game extends Application {
	constructor(element: HTMLElement) {
		super({
			width: element.clientWidth,
			height: (element.clientWidth / 16) * 9,
			autoDensity: true,
			resolution: window.devicePixelRatio || 1
		});

		this.renderer.backgroundColor = parseInt("39314B", 16);
		this.stage.scale.x = this.screen.width / camera.width;
		this.stage.scale.y = this.screen.height / camera.height;

		element.appendChild(this.view);

		const { left, top } = element.getBoundingClientRect();
		this.offsetX = left;
		this.offsetY = top;
	}

	private _entities: BaseComponent[] = [];

	public offsetX: number;
	public offsetY: number;

	async load(): Promise<void> {
		await loadResources();

		const cyborg = createCyborg({ x: 80, y: 90 });
		this._entities.push(cyborg);
		this.ticker.add((delta): void => this.gameLoop(delta / 60));

		applyWaterFilter(this.stage);

		console.log(`Game started using ${this.renderer.type === 1 ? "WebGL" : "canvas"} renderer.`);
	}

	private gameLoop(deltaSeconds: number): void {
		for (const system of systems) system(this._entities, this.stage, deltaSeconds);
	}
}

const gameElement = document.getElementById("game") as HTMLDivElement | null;
if (!gameElement) throw new Error("Game element not found.");

const game: Game = new Game(gameElement);

export { game };
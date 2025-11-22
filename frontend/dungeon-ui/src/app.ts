import { DungeonApi } from './Services/dungeon-api';

export class App {
  api = new DungeonApi();

  // ------------------------
  // LOGIN
  // ------------------------
  username = '';
  password = '';
  loginError: string | null = null;
  loginSuccess: string | null = null;

  async login() {
    this.loginError = null;
    this.loginSuccess = null;
    try {
      await this.api.login(this.username, this.password);
      this.loginSuccess = 'Logged in successfully!';
    } catch (err: any) {
      this.loginError = err.message || 'Login failed';
    }
  }

  // ------------------------
  // CREATE DUNGEON
  // ------------------------
  dungeonName = '';
  width = 10;
  height = 10;
  startX = 0;
  startY = 0;
  goalX = 9;
  goalY = 9;
  obstacles: { x: number; y: number }[] = [];
  createError: string | null = null;
  createErrorDetails: string | null = null;
  createSuccess: string | null = null;

  addObstacle() {
    this.obstacles.push({ x: 0, y: 0 });
  }

  removeObstacle(obstacle: { x: number; y: number }) {
    this.obstacles.splice(this.obstacles.indexOf(obstacle), 1);
  }

  async createDungeon() {
  this.createError = null;
  this.createErrorDetails = null;
  this.createSuccess = null;

  try {
    const body = {
      name: this.dungeonName,
      width: this.width,
      height: this.height,
      start: { x: this.startX, y: this.startY },
      goal: { x: this.goalX, y: this.goalY },
      obstacles: this.obstacles
    };

    const response = await this.api.createDungeon(body);

    if (response.success) {
      this.createSuccess = `Dungeon created with ID: ${response.data}`;
    } else {
      this.createError =
        response.error?.message || response.message || 'Failed to create dungeon';

      // ✅ PARSE validation details nicely
      if (response.error?.details) {
        try {
          const parsed = JSON.parse(response.error.details);

          this.createErrorDetails = Object.entries(parsed)
            .map(([field, messages]: any) =>
              `${field}: ${messages.join(', ')}`
            )
            .join(' | ');
        } catch {
          this.createErrorDetails = response.error.details;
        }
      }
    }
  } catch (err: any) {
    this.createError = err.message || 'Failed to create dungeon';
  }
}


  // ------------------------
  // FETCH DUNGEON
  // ------------------------
  fetchDungeonId: number | null = null;
  fetchedDungeon: any = null;
  dungeonGrid: number[][] = [];
  fetchError: string | null = null;
  fetchErrorDetails: string | null = null;

  async getDungeon() {
    if (!this.fetchDungeonId) return;

    this.fetchedDungeon = null;
    this.dungeonGrid = [];
    this.fetchError = null;
    this.fetchErrorDetails = null;

    try {
      const response = await this.api.getDungeon(this.fetchDungeonId);

      if (!response.success) {
        this.fetchError = response.error?.message || response.message || 'Failed to fetch dungeon'; 
        return;
      }

      const data = response.data;

      this.fetchedDungeon = {
        id: data.id,
        name: data.name,
        width: data.width ?? 10,
        height: data.height ?? 10,
        start: data.start ?? { x: 0, y: 0 },
        goal: data.goal ?? { x: 9, y: 9 },
        obstacles: data.obstacles ?? [],
        solutions: data.solutions ?? { path: [] }
      };

      // Build grid
      for (let r = 0; r < this.fetchedDungeon.height; r++) {
        const row = Array(this.fetchedDungeon.width).fill(0);
        this.dungeonGrid.push(row);
      }
    } catch (err: any) {
      this.fetchError = err.message || 'Failed to fetch dungeon';
    }
  }

  // ------------------------
  // GRID HELPERS
  // ------------------------
  get gridColumns(): string {
    if (!this.fetchedDungeon) return '';
    return `grid-template-columns: repeat(${this.fetchedDungeon.width}, 24px)`;
  }

  getCellClass(col: number, row: number): string {
    if (!this.fetchedDungeon) return '';
    const d = this.fetchedDungeon;

    if (col === d.start.x && row === d.start.y) return 'start';
    if (col === d.goal.x && row === d.goal.y) return 'goal';
    if (d.obstacles.some((o: any) => o.x === col && o.y === row)) return 'obstacle';
    if (d.solutions.path.some((p: any) => p.x === col && p.y === row)) return 'path';

    return '';
  }
}

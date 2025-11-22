export class DungeonApi {
  private baseUrl = 'http://localhost:8088/api/v1/dungeons';
  private token: string | null = null;

  // ------------------------
  // LOGIN
  // ------------------------
  async login(username: string, password: string) {
    const res = await fetch(`${this.baseUrl}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    const data = await res.json(); // always parse JSON
    if (!res.ok) throw new Error(data.message || 'Login failed');

    this.token = data.token;
    return data;
  }

  // ------------------------
  // CREATE DUNGEON
  // ------------------------
  async createDungeon(dungeon: any) {
    const res = await fetch(`${this.baseUrl}/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(this.token ? { Authorization: `Bearer ${this.token}` } : {})
      },
      body: JSON.stringify(dungeon)
    });

    const data = await res.json(); // always parse JSON
    return data;
  }

  // ------------------------
  // GET DUNGEON BY ID
  // ------------------------
  async getDungeon(id: number) {
    const res = await fetch(`${this.baseUrl}/${id}`, {
      headers: this.token ? { Authorization: `Bearer ${this.token}` } : {}
    });

    const data = await res.json(); // always parse JSON
    return data;
  }
}

export class DungeonApi {
  //base entry endpoint
  private baseUrl = 'http://localhost:8088/api/v1/dungeons';
  private token: string | null = null;

 
  //login
  async login(username: string, password: string) {
    const res = await fetch(`${this.baseUrl}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    const data = await res.json(); 
    if (!res.ok) throw new Error(data.message || 'Login failed');

    this.token = data.token;
    return data;
  }


  // create dungeon
  async createDungeon(dungeon: any) {
    const res = await fetch(`${this.baseUrl}/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(this.token ? { Authorization: `Bearer ${this.token}` } : {})
      },
      body: JSON.stringify(dungeon)
    });

    const data = await res.json(); 
    return data;
  }


  //retrieve dungeon by id 
  async getDungeon(id: number) {
    const res = await fetch(`${this.baseUrl}/${id}`, {
      headers: this.token ? { Authorization: `Bearer ${this.token}` } : {}
    });

    const data = await res.json(); 
    return data;
  }
}

import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly KEY = 'auth_token';

  setToken(token: string): void {
    localStorage.setItem(this.KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.KEY);
  }

  removeToken(): void {
    localStorage.removeItem(this.KEY);
  }

  getPayload(): any | null {
    const t = this.getToken();
    if (!t) return null;
    try {
      const [, b] = t.split('.');
      return JSON.parse(atob(b.replace(/-/g,'+').replace(/_/g,'/')));
    } catch {
      return null;
    }
  }
}

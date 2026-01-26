import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5055/api/auth';
  private currentUser$ = new BehaviorSubject<AuthResponse | null>(null);
  private isAuthenticated$ = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) {
    this.loadUser();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        this.saveUser(response);
        this.currentUser$.next(response);
        this.isAuthenticated$.next(true);
      })
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => {
        this.saveUser(response);
        this.currentUser$.next(response);
        this.isAuthenticated$.next(true);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    this.currentUser$.next(null);
    this.isAuthenticated$.next(false);
  }

  getCurrentUser(): AuthResponse | null {
    return this.currentUser$.value;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticated$.value;
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  private saveUser(response: AuthResponse): void {
    localStorage.setItem('authToken', response.token);
    localStorage.setItem('currentUser', JSON.stringify(response));
  }

  private loadUser(): void {
    const token = localStorage.getItem('authToken');
    const userStr = localStorage.getItem('currentUser');
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUser$.next(user);
        this.isAuthenticated$.next(true);
      } catch {
        localStorage.removeItem('authToken');
        localStorage.removeItem('currentUser');
      }
    }
  }
}

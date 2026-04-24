import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { NavConstants } from '../6_Common/nav-constants';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private apiUrl = environment.apiUrl;
    private http = inject(HttpClient);
    private authService = inject(AuthService);

    Get$<T>(endpoint: string, isAnonymous: boolean = false): Observable<T> {
        if(!this.authService.isAuthenticated() && !isAnonymous){
            window.location.href = "/" + NavConstants.login;
        }
        return this.http.get<T>(`${this.apiUrl}/${endpoint}`, {
            headers: this.authService.getAuthHeaders()
        });
    }

    Post$<T>(endpoint: string, body: any, isAnonymous: boolean = false): Observable<T> {
        if(!this.authService.isAuthenticated() && !isAnonymous){
            window.location.href = "/" + NavConstants.login;
        }
        return this.http.post<T>(`${this.apiUrl}/${endpoint}`, body, {
            headers: this.authService.getAuthHeaders()
        });
    }
}
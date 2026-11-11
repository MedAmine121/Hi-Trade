import { inject, Injectable } from "@angular/core";
import { StorageService } from "./storage.service";
import { Constants } from "../6_Common/constants";
import { Context } from "../2_Models/responses/context.model";
import { Roles } from "../6_Common/roles.enum";
import { HttpHeaders } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private storageService = inject(StorageService);
    constructor() { }

    private getValidContext(): Context | null {
        const context = <Context>this.storageService.getLocalStorage(Constants.CONTEXT_KEY);
        if (!context) {
            return null;
        }
        if (new Date(context.expires) < new Date()) {
            this.storageService.removeLocalStorage(Constants.CONTEXT_KEY);
            return null;
        }
        return context;
    }

    isAuthenticated(): boolean {
        return !!this.getValidContext();
    }

    isAdmin(): boolean {
        const context = this.getValidContext();
        return !!context && (context.role === Roles.Admin || String(context.role) === 'Admin');
    }
    getAuthHeaders(): HttpHeaders {
        const context = <Context>this.storageService.getLocalStorage(Constants.CONTEXT_KEY);
        return new HttpHeaders({
            'Authorization': `Bearer ${context?.token}`,
            'Content-Type': 'application/json'
        });
    }
    getContext(): Context{
        return <Context>this.storageService.getLocalStorage(Constants.CONTEXT_KEY);
    }
}
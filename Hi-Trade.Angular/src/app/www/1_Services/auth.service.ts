import { inject, Injectable } from "@angular/core";
import { StorageService } from "./storage.service";
import { Constants } from "../6_Common/constants";
import { Context } from "../2_Models/responses/context.model";
import { HttpHeaders } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private storageService = inject(StorageService);
    constructor() { }

    isAuthenticated(): boolean {
        const context = <Context>this.storageService.getLocalStorage(Constants.CONTEXT_KEY);
        if(new Date(context?.expires) < new Date()) {
            this.storageService.removeLocalStorage(Constants.CONTEXT_KEY);
            return false;
        }
        return !!context;
    }
    getAuthHeaders(): HttpHeaders {
        const context = <Context>this.storageService.getLocalStorage(Constants.CONTEXT_KEY);
        return new HttpHeaders({
            'Authorization': `Bearer ${context.token}`,
            'Content-Type': 'application/json'
        });
    }

}
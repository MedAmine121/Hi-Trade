import { inject } from "@angular/core";
import { ApiService } from "../1_Services/api.service";
import { LoginUserRequest } from "../2_Models/requests/login-request.model";

export class BaseDALService {
    controller = '';
    private apiService = inject(ApiService);
    SendPost$(action: string, request: unknown, isAnonymous = false){
        return this.apiService.Post$(this.controller + '/' + action, request, isAnonymous);
    }
    SendGet$(action: string, isAnonymous = false){
        return this.apiService.Get$(this.controller + '/' + action, isAnonymous);
    }
}
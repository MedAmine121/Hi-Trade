import { inject, Injectable } from "@angular/core";
import { LoginUserRequest } from "../2_Models/requests/login-request.model";
import { BaseResult } from "../2_Models/common/base-result.model";
import { Context } from "../2_Models/responses/context.model";
import { Observable } from "rxjs";
import { ApiService } from "../1_Services/api.service";
import { BaseDALService } from "./base-dal.service";
import { SaveResponse } from "../2_Models/common/save-response.model";
import { CreateUserRequest } from "../2_Models/requests/create-user-request.model";

@Injectable({
    providedIn: 'root'
})
export class UserDALService extends BaseDALService {
    override readonly controller = 'user';
    
    login$(request: LoginUserRequest) : Observable<BaseResult<Context>>{
        return this.SendPost$('login', request, true);
    }
    logout$() : Observable<BaseResult<SaveResponse>>{
        return this.SendPost$('logout', null);
    }
    signup$(request: CreateUserRequest) : Observable<BaseResult<Context>>{
        return this.SendPost$('signup', request, true);
    }
}
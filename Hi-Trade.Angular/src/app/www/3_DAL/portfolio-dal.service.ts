import { inject, Injectable } from "@angular/core";
import { BaseResult } from "../2_Models/common/base-result.model";
import { Observable } from "rxjs";
import { BaseDALService } from "./base-dal.service";
import { PortfolioDTO } from "../2_Models/responses/portfolio.model";

@Injectable({
    providedIn: 'root'
})
export class PortfolioDALService extends BaseDALService {
    override readonly controller = 'portfolio';
    
    getPortfolios$() : Observable<BaseResult<PortfolioDTO[]>>{
        return this.SendGet$('get');
    }
}
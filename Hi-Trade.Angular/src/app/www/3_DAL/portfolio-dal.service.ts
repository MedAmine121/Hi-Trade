import { inject, Injectable } from "@angular/core";
import { BaseResult } from "../2_Models/common/base-result.model";
import { Observable } from "rxjs";
import { BaseDALService } from "./base-dal.service";
import { PortfolioDTO } from "../2_Models/responses/portfolio.model";
import { CreatePortfolioRequest } from "../2_Models/requests/create-portfolio-request.model";
import { BuyAssetRequest } from "../2_Models/requests/buy-asset-request.model";
import { SellAssetRequest } from "../2_Models/requests/sell-asset-request.model";
import { SaveResponse } from "../2_Models/common/save-response.model";

@Injectable({
    providedIn: 'root'
})
export class PortfolioDALService extends BaseDALService {
    override readonly controller = 'portfolio';
    
    getPortfolios$() : Observable<BaseResult<PortfolioDTO[]>>{
        return this.SendGet$('get');
    }
    createPortfolio$(request: CreatePortfolioRequest) : Observable<BaseResult<SaveResponse>>{
        return this.SendPost$('create',request);
    }
    buyAsset$(request: BuyAssetRequest) : Observable<BaseResult<SaveResponse>>{
        return this.SendPost$('buyasset',request);
    }
    sellAsset$(request: SellAssetRequest) : Observable<BaseResult<SaveResponse>>{
        return this.SendPost$('sellasset',request);
    }
}
import { inject, Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { BaseResult } from "../2_Models/common/base-result.model";
import { ResultType } from "../2_Models/common/result-type.model";
import { BaseBLLService } from "./base-bll.service";
import { PortfolioDALService } from "../3_DAL/portfolio-dal.service";
import { PortfolioDTO } from "../2_Models/responses/portfolio.model";
import { CreatePortfolioRequest } from "../2_Models/requests/create-portfolio-request.model";
import { BuyAssetRequest } from "../2_Models/requests/buy-asset-request.model";
import { SaveResponse } from "../2_Models/common/save-response.model";

@Injectable({
    providedIn: 'root'
})
export class PortfolioBLLService extends BaseBLLService{
    private portfolioDALService = inject(PortfolioDALService);
    getPortfolios$(): Observable<PortfolioDTO[] | null>{
        return this.portfolioDALService.getPortfolios$().pipe(map((response: BaseResult<PortfolioDTO[]>)=> {
            if(response && response.resultType === ResultType.Success && response.model){
                return response.model;
            }
            if(response.resultType !== ResultType.Success){
                this.handleResult(response);
            }
            return null;
        }));
    }
    createPortfolio$(request: CreatePortfolioRequest): Observable<SaveResponse | null>{
        return this.portfolioDALService.createPortfolio$(request).pipe(map((response: BaseResult<SaveResponse>)=> {
            if(response && response.resultType === ResultType.Success && response.model){
                return response.model;
            }
            if(response.resultType !== ResultType.Success){
                this.handleResult(response);
            }
            return null;
        }));
    }
    buyAsset$(request: BuyAssetRequest): Observable<SaveResponse | null>{
        return this.portfolioDALService.buyAsset$(request).pipe(map((response: BaseResult<SaveResponse>)=> {
            if(response && response.resultType === ResultType.Success && response.model){
                return response.model;
            }
            if(response.resultType !== ResultType.Success){
                this.handleResult(response);
            }
            return null;
        }));
    }
}
import { inject, Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { BaseResult } from "../2_Models/common/base-result.model";
import { ResultType } from "../2_Models/common/result-type.model";
import { BaseBLLService } from "./base-bll.service";
import { AssetDALService } from "../3_DAL/asset-dal.service";
import { PortfolioDTO } from "../2_Models/responses/portfolio.model";
import { CreatePortfolioRequest } from "../2_Models/requests/create-portfolio-request.model";
import { SaveResponse } from "../2_Models/common/save-response.model";
import { AssetDTO } from "../2_Models/responses/asset.model";

@Injectable({
    providedIn: 'root'
})
export class AssetBLLService extends BaseBLLService{
    private assetDALService = inject(AssetDALService);
    getAssets$(): Observable<AssetDTO[] | null>{
        return this.assetDALService.getAssets$().pipe(map((response: BaseResult<AssetDTO[]>)=> {
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
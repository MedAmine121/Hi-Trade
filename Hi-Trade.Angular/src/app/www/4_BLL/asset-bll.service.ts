import { inject, Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { BaseResult } from "../2_Models/common/base-result.model";
import { ResultType } from "../2_Models/common/result-type.model";
import { BaseBLLService } from "./base-bll.service";
import { AssetDALService } from "../3_DAL/asset-dal.service";
import { SaveResponse } from "../2_Models/common/save-response.model";
import { CreateAssetRequest } from "../2_Models/requests/create-asset-request.model";
import { AssetDTO } from "../2_Models/responses/asset.model";

@Injectable({
    providedIn: 'root'
})
export class AssetBLLService extends BaseBLLService {
    private assetDALService = inject(AssetDALService);

    getAssets$(): Observable<AssetDTO[] | null> {
        return this.unwrapResult(this.assetDALService.getAssets$());
    }

    getAllAssets$(search?: string, enabled?: boolean): Observable<AssetDTO[] | null> {
        return this.unwrapResult(this.assetDALService.getAllAssets$(search, enabled));
    }

    createAsset$(request: CreateAssetRequest): Observable<SaveResponse | null> {
        return this.unwrapResult(this.assetDALService.createAsset$(request));
    }

    toggleAssetStatus$(assetId: number): Observable<SaveResponse | null> {
        return this.unwrapResult(this.assetDALService.toggleAssetStatus$(assetId));
    }

    private unwrapResult<T>(request$: Observable<BaseResult<T>>): Observable<T | null> {
        return request$.pipe(
            map((response: BaseResult<T>) => {
                if (response && response.resultType === ResultType.Success && response.model !== undefined && response.model !== null) {
                    return response.model;
                }
                if (response && response.resultType !== ResultType.Success) {
                    this.handleResult(response);
                }
                return null;
            })
        );
    }
}
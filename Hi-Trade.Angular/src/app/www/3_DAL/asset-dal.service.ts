import { inject, Injectable } from "@angular/core";
import { BaseResult } from "../2_Models/common/base-result.model";
import { SaveResponse } from "../2_Models/common/save-response.model";
import { Observable } from "rxjs";
import { BaseDALService } from "./base-dal.service";
import { CreateAssetRequest } from "../2_Models/requests/create-asset-request.model";
import { AssetDTO } from "../2_Models/responses/asset.model";

@Injectable({
    providedIn: 'root'
})
export class AssetDALService extends BaseDALService {
    override readonly controller = 'asset';

    getAssets$(): Observable<BaseResult<AssetDTO[]>> {
        return this.SendGet$('get');
    }

    getAllAssets$(search?: string, enabled?: boolean): Observable<BaseResult<AssetDTO[]>> {
        const params = new URLSearchParams();
        if (search && search.trim() !== '') {
            params.set('search', search.trim());
        }
        if (enabled !== undefined && enabled !== null) {
            params.set('enabled', String(enabled));
        }
        const query = params.toString();
        return this.SendGet$(query ? `all?${query}` : 'all');
    }

    createAsset$(request: CreateAssetRequest): Observable<BaseResult<SaveResponse>> {
        return this.SendPost$('create', request);
    }

    toggleAssetStatus$(assetId: number): Observable<BaseResult<SaveResponse>> {
        return this.SendPost$(`toggle/${assetId}`, {});
    }
}
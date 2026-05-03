import { inject, Injectable } from "@angular/core";
import { BaseResult } from "../2_Models/common/base-result.model";
import { Observable } from "rxjs";
import { BaseDALService } from "./base-dal.service";
import { AssetDTO } from "../2_Models/responses/asset.model";

@Injectable({
    providedIn: 'root'
})
export class AssetDALService extends BaseDALService {
    override readonly controller = 'asset';
    
    getAssets$() : Observable<BaseResult<AssetDTO[]>>{
        return this.SendGet$('get');
    }
}
import { AssetDTO } from "./asset.model";

export interface PositionDTO {
    asset: AssetDTO;
    quantity: number;
    averagePrice: number;
}
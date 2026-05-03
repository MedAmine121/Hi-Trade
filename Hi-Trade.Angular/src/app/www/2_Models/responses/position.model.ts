import { BaseDTO } from "../common/base-dto.model";
import { AssetDTO } from "./asset.model";

export interface PositionDTO extends BaseDTO {
    asset: AssetDTO;
    quantity: number;
    averagePrice: number;
}
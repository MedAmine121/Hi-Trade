import { BaseDTO } from "../common/base-dto.model";

export interface AssetDTO extends BaseDTO{
    ticker: string;
    name: string;
    currentPrice: number;
}
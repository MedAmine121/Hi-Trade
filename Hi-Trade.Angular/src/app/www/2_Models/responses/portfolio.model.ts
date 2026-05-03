import { BaseDTO } from "../common/base-dto.model";
import { PositionDTO } from "./position.model";

export interface PortfolioDTO extends BaseDTO {
    name: string;
    currentValue: number;
    gainLoss: number;
    performance: number;
    positions: PositionDTO[];
}
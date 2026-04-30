import { PositionDTO } from "./position.model";

export interface PortfolioDTO {
    currentValue: number;
    gainLoss: number;
    performance: number;
    positions: PositionDTO[];
}
import { BaseDTO } from "../common/base-dto.model";

export interface TransactionDTO extends BaseDTO {
    assetId: number;
    assetTicker: string;
    assetName: string;
    quantity: number;
    priceAtTransaction: number;
    transactionType: string;
    createdAt: string;
    totalValue: number;
}
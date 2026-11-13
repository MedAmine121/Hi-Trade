import { BaseDTO } from '../common/base-dto.model';

export interface AdminTransactionDTO extends BaseDTO {
  transactionDate: string | Date;
  userFullName: string;
  userEmail: string;
  portfolioName: string;
  assetTicker: string;
  assetName: string;
  transactionType: string;
  quantity: number;
  priceAtPurchase: number;
  totalAmount: number;
}

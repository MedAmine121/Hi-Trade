import { AdminTransactionDTO } from './admin-transaction.model';
import { AdminUserDTO } from './admin-user.model';

export interface AdminDashboardStatsDTO {
  totalUsers: number;
  totalTraders: number;
  totalAdmins: number;
  totalPortfolios: number;
  totalAssets: number;
  enabledAssets: number;
  disabledAssets: number;
  totalTransactions: number;
  buyTransactionsCount: number;
  sellTransactionsCount: number;
  totalTradingVolume: number;
  totalUserBalances: number;
  totalDepositedAmount: number;
  recentTransactions: AdminTransactionDTO[];
  recentUsers: AdminUserDTO[];
}

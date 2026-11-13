import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import {
  debounceTime,
  distinctUntilChanged,
  finalize,
  Subject,
} from 'rxjs';
import { UserService } from '../../../1_Services/user.service';
import { AssetDTO } from '../../../2_Models/responses/asset.model';
import { AssetBLLService } from '../../../4_BLL/asset-bll.service';
import { AdminBLLService } from '../../../4_BLL/admin-bll.service';
import { AdminDashboardStatsDTO } from '../../../2_Models/responses/admin-dashboard-stats.model';
import { AdminUserDTO } from '../../../2_Models/responses/admin-user.model';
import { AdminTransactionDTO } from '../../../2_Models/responses/admin-transaction.model';
import { BaseComponent } from '../../shared/base-component/base-component';
import { CreateAssetModalComponent } from '../../shared/create-asset-modal-component/create-asset-modal-component';
import { NavBarComponent } from '../../shared/nav-bar-component/nav-bar-component';

export type AdminTab = 'overview' | 'users' | 'assets' | 'transactions';
export type AssetStatusFilter = 'all' | 'enabled' | 'disabled';
export type UserStatusFilter = 'all' | 'enabled' | 'disabled';
export type UserRoleFilter = 'all' | 'User' | 'Admin';
export type TransactionTypeFilter = 'all' | 'Buy' | 'Sell';

@Component({
  selector: 'app-admin-index-component',
  imports: [CommonModule, FormsModule, DecimalPipe, DatePipe, NavBarComponent, CreateAssetModalComponent],
  templateUrl: './admin-index-component.html',
  styleUrl: './admin-index-component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminIndexComponent extends BaseComponent implements OnInit {
  private assetBLLService = inject(AssetBLLService);
  private adminBLLService = inject(AdminBLLService);
  protected userService = inject(UserService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  @ViewChild(CreateAssetModalComponent) createAssetModal!: CreateAssetModalComponent;

  activeTab = signal<AdminTab>('overview');

  // Stats
  dashboardStats = signal<AdminDashboardStatsDTO | null>(null);
  isLoadingStats = signal<boolean>(false);

  // Users
  users = signal<AdminUserDTO[]>([]);
  isLoadingUsers = signal<boolean>(false);
  userSearchQuery = signal<string>('');
  userRoleFilter = signal<UserRoleFilter>('all');
  userStatusFilter = signal<UserStatusFilter>('all');
  togglingUserId = signal<number | null>(null);

  // Assets
  assets = signal<AssetDTO[]>([]);
  isLoadingAssets = signal<boolean>(false);
  togglingAssetId = signal<number | null>(null);
  searchQuery = signal<string>('');
  statusFilter = signal<AssetStatusFilter>('all');

  // Transactions
  transactions = signal<AdminTransactionDTO[]>([]);
  isLoadingTransactions = signal<boolean>(false);
  txSearchQuery = signal<string>('');
  txTypeFilter = signal<TransactionTypeFilter>('all');

  // Asset computed counts
  totalCount = computed(() => this.assets().length);
  enabledCount = computed(() => this.assets().filter((asset) => asset.enabled).length);
  disabledCount = computed(() => this.assets().filter((asset) => !asset.enabled).length);
  hasActiveFilters = computed(
    () => this.searchQuery().trim() !== '' || this.statusFilter() !== 'all',
  );

  // User computed counts
  totalUsersCount = computed(() => this.users().length);
  activeUsersCount = computed(() => this.users().filter((u) => u.enabled).length);
  disabledUsersCount = computed(() => this.users().filter((u) => !u.enabled).length);

  private readonly userSearchInput$ = new Subject<string>();
  private readonly assetSearchInput$ = new Subject<string>();
  private readonly txSearchInput$ = new Subject<string>();

  ngOnInit(): void {
    this.userService.fetchUser();

    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const tab = params['tab'] as AdminTab;
      if (tab && ['overview', 'users', 'assets', 'transactions'].includes(tab)) {
        this.activeTab.set(tab);
        this.onTabSwitched(tab);
      }
    });

    this.userSearchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadUsers());

    this.assetSearchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadAssets());

    this.txSearchInput$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadTransactions());

    this.loadDashboardStats();
    this.loadAssets();
  }

  setActiveTab(tab: AdminTab): void {
    this.activeTab.set(tab);
    this.router.navigate([], { queryParams: { tab }, queryParamsHandling: 'merge' });
    this.onTabSwitched(tab);
  }

  private onTabSwitched(tab: AdminTab): void {
    if (tab === 'overview') {
      this.loadDashboardStats();
    } else if (tab === 'users') {
      this.loadUsers();
    } else if (tab === 'assets') {
      this.loadAssets();
    } else if (tab === 'transactions') {
      this.loadTransactions();
    }
  }

  refreshCurrent(): void {
    this.loadDashboardStats();
    const tab = this.activeTab();
    if (tab === 'users') {
      this.loadUsers();
    } else if (tab === 'assets') {
      this.loadAssets();
    } else if (tab === 'transactions') {
      this.loadTransactions();
    }
  }

  // Dashboard Stats
  loadDashboardStats(): void {
    this.isLoadingStats.set(true);
    this.adminBLLService
      .getDashboardStats$()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingStats.set(false)),
      )
      .subscribe((stats) => {
        if (stats) {
          this.dashboardStats.set(stats);
        }
      });
  }

  // Users
  loadUsers(): void {
    this.isLoadingUsers.set(true);
    const enabledParam = this.resolveEnabledFilter(this.userStatusFilter());
    const roleParam = this.userRoleFilter() === 'all' ? undefined : this.userRoleFilter();
    this.adminBLLService
      .getUsers$(this.userSearchQuery(), roleParam, enabledParam)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingUsers.set(false)),
      )
      .subscribe((users) => {
        if (users) {
          this.users.set(users);
        }
      });
  }

  onUserSearchChange(val: string): void {
    this.userSearchQuery.set(val);
    this.userSearchInput$.next(val);
  }

  onUserRoleFilterChange(role: UserRoleFilter): void {
    this.userRoleFilter.set(role);
    this.loadUsers();
  }

  onUserStatusFilterChange(status: UserStatusFilter): void {
    this.userStatusFilter.set(status);
    this.loadUsers();
  }

  toggleUserStatus(user: AdminUserDTO): void {
    if (this.togglingUserId() !== null) return;

    const currentEmail = this.userService.user()?.email;
    if (user.email.toLowerCase() === currentEmail?.toLowerCase()) {
      this.notificationService.showErrorToast('You cannot disable your own administrator account.');
      return;
    }

    this.togglingUserId.set(user.id);
    this.adminBLLService
      .toggleUserStatus$(user.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.togglingUserId.set(null)),
      )
      .subscribe((res) => {
        if (res && res.success) {
          this.notificationService.showSuccessToast(res.message || `User ${user.email} status updated.`);
          this.loadUsers();
          this.loadDashboardStats();
        } else if (res) {
          this.notificationService.showErrorToast(res.message || 'Failed to update user status.');
        }
      });
  }

  // Assets
  loadAssets(): void {
    this.isLoadingAssets.set(true);
    const enabledParam = this.resolveEnabledFilter(this.statusFilter());
    this.assetBLLService
      .getAllAssets$(this.searchQuery(), enabledParam)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingAssets.set(false)),
      )
      .subscribe((data) => {
        if (data) {
          this.assets.set(data);
        }
      });
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
    this.assetSearchInput$.next(value);
  }

  onStatusFilterChange(filter: AssetStatusFilter): void {
    this.statusFilter.set(filter);
    this.loadAssets();
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.statusFilter.set('all');
    this.loadAssets();
  }

  openCreateAssetModal(): void {
    this.createAssetModal.openModal();
  }

  onAssetCreated(): void {
    this.loadAssets();
    this.loadDashboardStats();
  }

  toggleAssetStatus(asset: AssetDTO): void {
    if (this.togglingAssetId() !== null) return;

    this.togglingAssetId.set(asset.id);
    this.assetBLLService
      .toggleAssetStatus$(asset.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.togglingAssetId.set(null)),
      )
      .subscribe((response) => {
        if (!response) return;
        if (response.success) {
          this.notificationService.showSuccessToast(
            response.message || `Asset ${asset.ticker} status updated.`,
          );
          this.loadAssets();
          this.loadDashboardStats();
        } else {
          this.notificationService.showErrorToast(
            response.message || 'Failed to update asset status.',
          );
        }
      });
  }

  // Transactions
  loadTransactions(): void {
    this.isLoadingTransactions.set(true);
    const typeParam = this.txTypeFilter() === 'all' ? undefined : this.txTypeFilter();
    this.adminBLLService
      .getTransactions$(this.txSearchQuery(), typeParam)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingTransactions.set(false)),
      )
      .subscribe((txs) => {
        if (txs) {
          this.transactions.set(txs);
        }
      });
  }

  onTxSearchChange(val: string): void {
    this.txSearchQuery.set(val);
    this.txSearchInput$.next(val);
  }

  onTxTypeFilterChange(type: TransactionTypeFilter): void {
    this.txTypeFilter.set(type);
    this.loadTransactions();
  }

  getAvatarInitials(name: string): string {
    const initials = (name || '')
      .split(' ')
      .map((n) => n.charAt(0).toUpperCase())
      .join('');
    return initials || '👤';
  }

  private resolveEnabledFilter(filter: 'all' | 'enabled' | 'disabled'): boolean | undefined {
    if (filter === 'enabled') return true;
    if (filter === 'disabled') return false;
    return undefined;
  }
}
